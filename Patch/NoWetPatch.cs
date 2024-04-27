namespace DevUtils.Patch;

[HarmonyPatch]
file static class NoWetPatch
{
    [HarmonyPatch(typeof(SEMan), nameof(SEMan.AddStatusEffect), typeof(int), typeof(bool), typeof(int), typeof(float))]
    [HarmonyPrefix]
    public static bool ApplyNoWetEffect(SEMan __instance, int nameHash)
    {
        if (!__instance.m_character.IsPlayer() || __instance.m_character != m_localPlayer || !ZNetScene.instance) return true;
        if (!noWet.Value) return true;
        if (nameHash == "Wet".GetStableHashCode()) return false;

        return true;
    }
}