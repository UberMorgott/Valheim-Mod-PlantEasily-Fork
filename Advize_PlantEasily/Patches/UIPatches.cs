namespace Advize_PlantEasily;

using HarmonyLib;
using TMPro;
using UnityEngine;
using static ModContext;

[HarmonyPatch]
static class UIPatches
{
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.SetupRequirement)), HarmonyPriority(Priority.Last)]
    static void Postfix(Transform elementRoot, Piece.Requirement req)
    {
        if (GhostGrid.ExtraGhosts.Count < 1 || !config.ShowCost) return;

        TMP_Text component = elementRoot.transform.Find("res_amount").GetComponent<TMP_Text>();
        int totalGhosts = Mathf.Min(config.Rows * config.Columns, config.MaxConcurrentPlacements);

        string formattedCost = config.CostDisplayStyle == CostDisplayStyle.TotalCount ? config.CostDisplayLocation == CostDisplayLocation.LeftSide ?
            $"{totalGhosts.ToString()}x" : $"x{totalGhosts.ToString()}" : $"({(req.m_amount * totalGhosts).ToString()})";

        component.text = config.CostDisplayLocation == CostDisplayLocation.LeftSide ? formattedCost + component.text : component.text + formattedCost;
    }
}
