namespace DevUtils.Patch;

[HarmonyPatch]
file static class HigherStacksPatch
{
    [HarmonyPatch(typeof(Game), nameof(Game.SpawnPlayer))] [HarmonyPostfix]
    public static void ApplyHigherStacksEffect()
    {
        if (!Game.instance.m_firstSpawn) return;
        foreach (var item_ in ObjectDB.instance.m_items)
        {
            var item = item_.GetComponent<ItemDrop>();
            var size = item.m_itemData.m_shared.m_maxStackSize;
            if (size == 1) continue;
            item.m_itemData.m_shared.m_maxStackSize = 10000;
        }

        foreach (var item in m_localPlayer.GetInventory().m_inventory)
        {
            var size = item.m_shared.m_maxStackSize;
            if (size == 1) continue;
            item.m_shared.m_maxStackSize = 10000;
        }
    }

    [HarmonyPatch(typeof(HotkeyBar), nameof(HotkeyBar.UpdateIcons))] [HarmonyPostfix]
    public static void FixHotkeyBar(HotkeyBar __instance)
    {
        foreach (var el in __instance.m_elements)
        {
            el.m_amount.text = el.m_amount.text.Replace($"/{10000}", "");
        }
    }

    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))] [HarmonyPostfix]
    public static void FixInventoryGrid(InventoryGrid __instance)
    {
        foreach (var el in __instance.m_elements)
        {
            el.m_amount.text = el.m_amount.text.Replace($"/{10000}", "");
        }
    }

    // [HarmonyPatch(typeof(ItemData), nameof(ItemElement.SetDescription), [])] [HarmonyPostfix]
    // public static void FixItemElement(ItemElement __instance)
    // {
    //     __instance.Description = __instance.Description.Replace($"/{10000}", "");
    // }
}