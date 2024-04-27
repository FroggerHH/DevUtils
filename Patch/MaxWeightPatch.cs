namespace DevUtils.Patch;

[HarmonyPatch]
file static class MaxWeightPatch
{
    [HarmonyPatch(typeof(Game), nameof(Game.SpawnPlayer))] [HarmonyPostfix]
    public static void ApplyCarryWeightEffect()
    {
        if (!Game.instance.m_firstSpawn) return;
        m_localPlayer.m_maxCarryWeight = 999999;
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))] [HarmonyPostfix]
    private static void _(InventoryGui __instance)
    {
        __instance.transform.Find("root/Player/Weight")?.gameObject.SetActive(false);
    }
}