namespace DevUtils.Patch;

[HarmonyPatch]
public class DoDebugModePatch
{
    [HarmonyPatch(typeof(Game), nameof(Game.SpawnPlayer))]
    [HarmonyWrapSafe] [HarmonyPostfix]
    public static void Logic()
    {
        if (!m_localPlayer) return;

        m_localPlayer.m_godMode = true;
        Terminal.m_cheat = true;
        Console.m_consoleEnabled = true;
        m_debugMode = true;
        m_localPlayer.SetNoPlacementCost(true);
        Console.instance.updateCommandList();
    }
}