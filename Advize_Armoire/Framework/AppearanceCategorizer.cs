namespace Advize_Armoire;

using System.Collections.Generic;
using static ItemDrop.ItemData;
using static Skills;
using static StaticMembers;

static class AppearanceCategorizer
{
    private static readonly HashSet<ItemType> RelevantTypes =
    [
        ItemType.Helmet, ItemType.Chest, ItemType.Legs, ItemType.Shoulder,
        ItemType.Utility, ItemType.Trinket, ItemType.Bow, ItemType.Shield,
        ItemType.OneHandedWeapon, ItemType.TwoHandedWeapon/*, ItemType.TwoHandedWeaponLeft*/
    ];

    private static readonly HashSet<ItemType> ArmorTypes =
    [
        ItemType.Helmet, ItemType.Chest, ItemType.Legs,
        ItemType.Shoulder, ItemType.Utility, ItemType.Trinket
    ];

    internal static void CategorizeAppearances(Player player)
    {
        // Get all relevant items potentially eligible to be available appearances, filter out disabled ones
        // If excludeDLCItems is false, the DLC condition is ignored. If true, it filters them out.
        List<ItemDrop> allRelevantItems = GetAllItemsByTypes(RelevantTypes);

        // Categorize items
        Dictionary<AppearanceSlotType, List<ItemDrop>> categorized = [];

        foreach (ItemDrop item in allRelevantItems)
        {
            if (item.m_itemData.m_shared.m_icons.Length <= 0 || config.DisabledAppearanceNames.Contains(item.name) ||
                (config.ExcludeDLCItems && !string.IsNullOrEmpty(item.m_itemData.m_shared.m_dlc)) ||
                (config.ExcludeNPCItems && HasExcludedPrefix(item.name)))
            {
                continue;
            }

            ItemType type = item.m_itemData.m_shared.m_itemType;
            SkillType skill = item.m_itemData.m_shared.m_skillType;

            AddToCategory(categorized, MapAppearanceSlot(type, skill), item);
        }

        // Account for variants and filter to items discovered by the player
        Dictionary<AppearanceSlotType, Dictionary<ItemDrop, int>> allAppearances = [];
        Dictionary<AppearanceSlotType, Dictionary<ItemDrop, int>> unlockedAppearances = [];

        foreach (KeyValuePair<AppearanceSlotType, List<ItemDrop>> kvp in categorized)
        {
            Dictionary<ItemDrop, int> allMap = new(kvp.Value.Count);
            Dictionary<ItemDrop, int> unlockedMap = [];

            foreach (ItemDrop item in kvp.Value)
            {
                int iconCount = item.m_itemData.m_shared.m_icons.Length;
                allMap.Add(item, iconCount);

                if (player.m_knownMaterial.Contains(item.m_itemData.m_shared.m_name))
                    unlockedMap.Add(item, iconCount);
            }

            allAppearances.Add(kvp.Key, allMap);
            unlockedAppearances.Add(kvp.Key, unlockedMap);
        }

        AllAppearances = allAppearances;
        UnlockedAppearances = unlockedAppearances;
    }

    private static void AddToCategory(Dictionary<AppearanceSlotType, List<ItemDrop>> dict, AppearanceSlotType? slotType, ItemDrop item)
    {
        if (slotType is not AppearanceSlotType slot) return;

        if (!dict.ContainsKey(slot))
            dict[slot] = [];
        dict[slot].Add(item);
    }

    private static AppearanceSlotType? MapAppearanceSlot(ItemType type, SkillType skill)
    {
        if (ArmorTypes.Contains(type))
            return type switch
            {
                ItemType.Helmet => AppearanceSlotType.Helmet,
                ItemType.Chest => AppearanceSlotType.Chest,
                ItemType.Legs => AppearanceSlotType.Legs,
                ItemType.Shoulder => AppearanceSlotType.Shoulder,
                ItemType.Utility when skill == SkillType.Swords => AppearanceSlotType.Utility,
                ItemType.Trinket => AppearanceSlotType.Trinket,
                _ => null
            };

        return type switch
        {
            ItemType.OneHandedWeapon => skill switch
            {
                SkillType.Axes => AppearanceSlotType.OneHandedAxe,
                SkillType.Clubs => AppearanceSlotType.OneHandedClub,
                SkillType.Swords => AppearanceSlotType.OneHandedSword,
                SkillType.Knives => AppearanceSlotType.Knife,
                SkillType.Spears => AppearanceSlotType.Spear,
                _ => null
            },
            ItemType.TwoHandedWeapon => skill switch
            {
                SkillType.Axes => AppearanceSlotType.TwoHandedAxe,
                SkillType.Clubs => AppearanceSlotType.TwoHandedClub,
                SkillType.Swords => AppearanceSlotType.TwoHandedSword,
                SkillType.Polearms => AppearanceSlotType.Polearm,
                SkillType.Pickaxes => AppearanceSlotType.Pickaxe,
                SkillType.ElementalMagic or SkillType.BloodMagic => AppearanceSlotType.Staff,
                SkillType.Knives => AppearanceSlotType.Knife,
                SkillType.Unarmed => AppearanceSlotType.Fist,
                _ => null
            },
            //ItemType.TwoHandedWeaponLeft => skill switch
            //{
            //    SkillType.ElementalMagic or SkillType.BloodMagic => AppearanceSlotType.Staff,
            //    _ => null
            //},
            ItemType.Bow => skill switch
            {
                SkillType.Bows => AppearanceSlotType.Bow,
                SkillType.Crossbows => AppearanceSlotType.Crossbow,
                _ => null
            },
            ItemType.Shield when skill == SkillType.Blocking => AppearanceSlotType.Shield,
            _ => null
        };
    }

    private static List<ItemDrop> GetAllItemsByTypes(HashSet<ItemType> types)
    {
        List<ItemDrop> items = [];
        foreach (UnityEngine.GameObject go in ObjectDB.instance.m_items)
        {
            ItemDrop item = go.GetComponent<ItemDrop>();
            if (item && types.Contains(item.m_itemData.m_shared.m_itemType))
                items.Add(item);
        }
        return items;
    }

    private static bool HasExcludedPrefix(string name)
    {
        if (name.Length < 3)
            return false;

        if (name[0] == 'F' && name[1] == 'W' && name[2] == '_')
            return true;

        if (name[0] == 'S' && name[1] == 'P' && name[2] == '_')
            return true;

        return false;
    }

    internal static void RecalculateAppearances(Player player)
    {
        CategorizeAppearances(player);
        AppearanceTracker.UpdateTotal();
    }
}
