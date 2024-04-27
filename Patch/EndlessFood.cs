namespace DevUtils.Patch;

[HarmonyPatch]
file static class EndlessFood
{
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateFood)), HarmonyPostfix]
    private static void _(Player __instance)
    {
        if (__instance != m_localPlayer) return;
        foreach (var food in __instance.GetFoods()) food.m_time = 1500;
        foreach (var food in Hud.instance.m_foodTime) food.gameObject.SetActive(false);
    }
}