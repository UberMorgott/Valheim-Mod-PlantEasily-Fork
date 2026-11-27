namespace Advize_PlantEverything;

using System;
using System.Globalization;
using HarmonyLib;
using static PluginUtils;
using static StaticMembers;

[HarmonyPatch(typeof(Piece), nameof(Piece.SetCreator))]
static class PieceCreationPatches
{
    static readonly object[] PickedTrueArgs = [Convert.ChangeType(bool.TrueString, typeof(bool), CultureInfo.InvariantCulture)];

    static void Postfix(Piece __instance)
    {
        if (!IsModdedPrefabOrSapling(__instance.m_name)) return;

        if (config.ResourcesSpawnEmpty && __instance.GetComponent<Pickable>() && __instance.m_name != "Pickable_Stone")
        {
            __instance.m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetPicked", PickedTrueArgs);
        }

        if (config.PlaceAnywhere && __instance.TryGetComponent(out StaticPhysics sp))
        {
            sp.m_fall = false;
            __instance.m_nview.GetZDO().Set(PlaceAnywhereHash, true);
        }
    }
}
