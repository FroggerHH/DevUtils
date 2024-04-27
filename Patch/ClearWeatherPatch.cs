namespace DevUtils.Patch;

[HarmonyPatch]
file static class ClearWeatherPatch
{
    [HarmonyPatch(typeof(Game), nameof(Game.SpawnPlayer))] [HarmonyPostfix]
    public static void ApplyNoHuginEffect(Game __instance)
    {
        if (!clearWeather.Value) return;

        EnvMan.instance.m_debugEnv = "Clear";
    }
}