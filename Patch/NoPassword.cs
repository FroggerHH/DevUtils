namespace DevUtils.Patch;

[HarmonyPatch]
file static class NoPassword
{
    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.IsPublicPasswordValid))] [HarmonyPrefix]
    private static bool IsPublicPasswordValid(ref bool __result)
    {
        if (noPassword.Value == false) return true;
        __result = true;
        return false;
    }

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.NeedPassword))] [HarmonyPrefix]
    private static bool No_NeedPassword(ref bool __result)
    {
        if (noPassword.Value == false) return true;
        __result = false;
        return false;
    }
}