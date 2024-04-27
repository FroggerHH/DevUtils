namespace DevUtils.Patch;

[HarmonyPatch]
file static class NoStaminaCostPatch
{
    [HarmonyPatch(typeof(Player), nameof(Player.HaveStamina))] [HarmonyPrefix]
    public static bool ApplyNoStaminaCostEffect_HaveStamina(Player __instance, ref bool __result)
    {
        if (!noStaminaCost.Value) return true;

        __result = true;
        return false;
    }
}