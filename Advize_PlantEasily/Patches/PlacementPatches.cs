namespace Advize_PlantEasily;

using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static ModContext;
using static ModUtils;

static class PlacementPatches
{
    [HarmonyPatch(typeof(Player), nameof(Player.SetupPlacementGhost))]
    static class PlayerSetupPlacementGhost
    {
        private static int placementRotation;

        static void Prefix(Player __instance)
        {
            if (!config.ModActive || !HoldingCultivator)
                return;

            placementRotation = __instance.m_placeRotation;
        }

        static void Postfix(Player __instance)
        {
            //Dbgl("SetupPlacementGhost");
            if (PlacementController.IsPlanting)
                return;

            GhostGrid.PrepareGhostPool(__instance.m_placementGhost);

            if (!config.ModActive || !__instance.m_placementGhost || !HoldingCultivator)
                return;

            PlacementState.LastSelectedPlantName = null;

            if (!IsPlantOrPickable(__instance.m_placementGhost))
                return;

            //Dbgl("SetupPlacementGhost2");
            __instance.m_placeRotation = placementRotation;

            GhostGrid.BuildGrid(__instance.m_placementGhost);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
    static class PlayerUpdatePlacementGhost
    {
        static void Prefix(Player __instance)
        {
            if (!config.ModActive || !PlacementState.PlacementGhost)
                return;

            //If there are no extra ghosts but there is supposed to be
            if (GhostGrid.GhostPlacementStatus.Count == 0 || (GhostGrid.ExtraGhosts.Count == 0 && !(config.Rows == 1 && config.Columns == 1)))
            {
                //Dbgl($"Calling Setup from Update. placementCount:{GhostGrid.GhostPlacementStatus.Count}, ghostCount is 0? ({GhostGrid.ExtraGhosts.Count == 0})");
                __instance.SetupPlacementGhost();
            }
        }

        static void Postfix(Player __instance, bool flashGuardStone)
        {
            if (!config.ModActive || !PlacementState.PlacementGhost || PlacementController.IsPlanting)
                return;

            // Grid not built (e.g. the Cultivator was unequipped this frame, PlacementGhost is destroyed only at frame end)
            if (GhostGrid.ExtraGhosts.Count < GhostGrid.MaxActiveGhosts)
                return;

            PlacementState.Update(__instance.transform.position);
            GhostGrid.Update(__instance, press: flashGuardStone);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.TryPlacePiece))]
    static class PlayerTryPlacePiece
    {
        private static readonly Dictionary<Status, string> StatusMessages = new()
        {
            { Status.LackResources, "$msg_missingrequirement" },
            { Status.NotCultivated, "$piece_plant_notcultivated" },
            { Status.WrongBiome, "$piece_plant_wrongbiome" },
            { Status.NoSpace, "$piece_plant_nospace" },
            { Status.NoSun, "$piece_plant_nosun" },
            { Status.Invalid, "$msg_invalidplacement" },
            { Status.NoAttachPiece, "$piece_plant_nowall" },
            { Status.TooHot, "$piece_plant_toohot" },
            { Status.TooCold, "$piece_plant_toocold" }
        };

        // Cost of the whole press on the selected piece while the other TryPlacePiece prefixes run: Morgott.Patcher's
        // StationNetwork gates HaveRequirements(piece) and fetches that much from chests, and a gate covering one seed
        // left the extras (judged against chest stock it never takes) planted unpaid. Restored before the body consumes.
        private static Piece pressPiece;
        private static int pressBaseCost;
        internal static int CoveredPieces;

        [HarmonyPriority(Priority.First)]
        static bool Prefix(Player __instance, Piece piece, ref bool __result)
        {
            if (!config.ModActive || !piece || !HoldingCultivator || !IsPlantOrPickable(piece.gameObject))
                return true;

            bool place = Judge(__instance, ref __result);
            if (place && !__instance.m_noPlacementCost && piece.m_resources.Length > 0 && GhostGrid.MaxActiveGhosts > 0)
            {
                CoveredPieces = PressCost.Pieces(GhostGrid.GhostPlacementStatus, GhostGrid.MaxActiveGhosts, config.PreventInvalidPlanting);
                (pressPiece, pressBaseCost) = (piece, piece.m_resources[0].m_amount);
                piece.m_resources[0].m_amount = pressBaseCost * CoveredPieces;
            }
            return place;
        }

        internal static void RestoreCost()
        {
            if (pressPiece)
                pressPiece.m_resources[0].m_amount = pressBaseCost;
            pressPiece = null;
        }

        static System.Exception Finalizer(System.Exception __exception)
        {
            RestoreCost();
            CoveredPieces = 0;
            return __exception;
        }

        private static bool Judge(Player __instance, ref bool __result)
        {

            if (PlacementController.IsPlanting)
            {
                __result = false;
                return false;
            }

            if (config.PreventInvalidPlanting)
            {
                Status rootStatus = GhostStatus.EvaluateStatus(__instance.m_placementGhost);

                if (rootStatus != Status.Healthy)
                    return PreventPlacement(__instance, rootStatus, ref __result);
            }

            if (config.PreventPartialPlanting)
            {
                foreach (Status status in GhostGrid.GhostPlacementStatus)
                {
                    if (status == Status.Healthy)
                        continue;

                    if (status == Status.LackResources && __instance.m_noPlacementCost)
                        continue;

                    return PreventPlacement(__instance, status, ref __result);
                }
            }
            return true;
        }

        private static bool PreventPlacement(Player player, Status status, ref bool result)
        {
            if (StatusMessages.TryGetValue(status, out string message))
                player.Message(MessageHud.MessageType.Center, message);

            result = false;
            return false;
        }
    }

    // After every other prefix judged the press cost: the body (PlacePiece, then UpdatePlacement's ConsumeResources) pays per piece.
    [HarmonyPatch(typeof(Player), nameof(Player.TryPlacePiece))]
    static class PlayerTryPlacePieceRestoreCost
    {
        [HarmonyPriority(Priority.Last)]
        static void Prefix() => PlayerTryPlacePiece.RestoreCost();
    }

    [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
    static class PlayerPlacePiece
    {
        private static int placementRotation;

        static void Prefix(Player __instance) => placementRotation = __instance.m_placeRotation;

        static void Postfix(Player __instance, PieceTable ___m_buildPieces, Piece piece)
        {
            if (!config.ModActive || !piece || !HoldingCultivator || !IsPlantOrPickable(piece.gameObject))
                return;

            __instance.m_placeRotation = placementRotation;

            if (GhostGrid.MaxActiveGhosts < 1)
                return;

            // The press re-aimed after the cost gate (Player.cs:3043): never plant more extras than the gated cost covered.
            int maxExtras = PlayerTryPlacePiece.CoveredPieces > 0 ? PlayerTryPlacePiece.CoveredPieces - 1 : int.MaxValue;

            for (int i = 0; i < GhostGrid.MaxActiveGhosts && GhostGrid.ValidExtraGhosts.Count < maxExtras; i++)
            {
                if (PressCost.CanPlant(GhostGrid.GhostPlacementStatus[i + 1], __instance.m_noPlacementCost, config.PreventInvalidPlanting))
                    GhostGrid.ValidExtraGhosts.Add(GhostGrid.ExtraGhosts[i]);
            }

            int placementMultiplier = __instance.m_noPlacementCost ? 0 : GhostGrid.ValidExtraGhosts.Count;

            if (GhostGrid.ValidExtraGhosts.Count > 0)
            {
                Piece.PieceCategory category = ___m_buildPieces.GetSelectedCategory();
                Vector2Int index = ___m_buildPieces.GetSelectedIndex();
                GameObject piecePrefab = ___m_buildPieces.GetPiece(category, index).gameObject;

                PlacementController.Instance.StartBulkPlanting(piecePrefab);
            }

            ItemDrop.ItemData rightItem = __instance.GetRightItem();

            for (int i = 0; i < placementMultiplier; i++)
            {
                __instance.ConsumeResources(piece.m_resources, 0);

                if (config.UseStamina)
                    __instance.UseStamina(__instance.GetBuildStamina());

                if (config.UseDurability && rightItem.m_shared.m_useDurability)
                    rightItem.m_durability -= __instance.GetPlaceDurability(rightItem);
            }
        }
    }
}
