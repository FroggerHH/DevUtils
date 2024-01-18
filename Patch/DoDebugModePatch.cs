namespace DevUtils.Patch;

[HarmonyPatch]
public class DoDebugModePatch
{
    [HarmonyPatch(typeof(Game), nameof(Game.SpawnPlayer))]
    [HarmonyWrapSafe] [HarmonyPostfix]
    private static void Patch()
    {
        if (!Player.m_localPlayer) return;

        Player.m_localPlayer.m_godMode = true;
        Terminal.m_cheat = true;
        Console.m_consoleEnabled = true;
        Player.m_debugMode = true;
        Player.m_localPlayer.SetNoPlacementCost(true);
        Console.instance.updateCommandList();
    }
}