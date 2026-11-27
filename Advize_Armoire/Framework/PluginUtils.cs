namespace Advize_Armoire;

using System.Collections.Generic;
using System.Linq;
using static StaticMembers;

static class PluginUtils
{
    internal static AppearanceSlot FindMatchingSlot(ItemDrop.ItemData.SharedData sd)
    {
        foreach (AppearanceSlot slot in ActiveOverrides.Values)
            if (slot.IsMatch(sd))
                return slot;

        return null;
    }

    internal static AppearanceSlotType? GetSlotTypeForItemDrop(ItemDrop item)
    {
        foreach (KeyValuePair<AppearanceSlotType, Dictionary<ItemDrop, int>> kvp in AllAppearances)
            if (kvp.Value.ContainsKey(item)) return kvp.Key;

        return null;
    }

    // Deep clone an overrides dictionary
    internal static Dictionary<AppearanceSlotType, AppearanceSlot> CloneOverrides(Dictionary<AppearanceSlotType, AppearanceSlot> source)
    {
        return source.ToDictionary(entry => entry.Key, entry => new AppearanceSlot(entry.Value));
    }
}
