namespace DevUtils.Patch;

[HarmonyPatch]
file static class AlwaysDay
{
    //TODO: add patch to always day
    [HarmonyPatch(typeof(Game), nameof(Game.SpawnPlayer))] [HarmonyPostfix]
    public static void ApplyAlwaysDayEffect()
    {
        if (!Game.instance.m_firstSpawn) return;
        Console.instance.TryRunCommand("tod 0.4");
    }
}