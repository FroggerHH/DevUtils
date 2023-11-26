using UnityEngine.SceneManagement;

namespace DevUtils;

[HarmonyPatch]
public class FlatWorldPatch
{
    private static readonly float NewHeight = 40;

    [HarmonyPatch(typeof(Heightmap), nameof(Heightmap.GetWorldHeight))] [HarmonyPrefix]
    public static bool GetWorldHeight(ref float height, ref bool __result)
    {
        if (SceneManager.GetActiveScene().name != "main") return true;
        if (flatWorldConfig.Value)
        {
            height = NewHeight;
            __result = true;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(Heightmap), nameof(Heightmap.GetHeight), typeof(int), typeof(int))]
    public static bool Prefix(ref float __result)
    {
        if (SceneManager.GetActiveScene().name != "main") return true;
        if (flatWorldConfig.Value)
        {
            __result = NewHeight;
            return false;
        }

        return true;
    }


    [HarmonyPatch(typeof(WorldGenerator), nameof(WorldGenerator.GetBiomeHeight))] [HarmonyPrefix]
    public static bool WorldGeneratorGetBiomeHeight(ref float __result)
    {
        if (SceneManager.GetActiveScene().name != "main") return true;
        if (flatWorldConfig.Value)
        {
            __result = NewHeight;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(Player), nameof(Player.UpdateTeleport))] [HarmonyPrefix]
    public static void FastTeleport(Player __instance)
    {
        if (__instance != Player.m_localPlayer) return;
        Player.m_localPlayer.m_teleportCooldown = 999;
        Player.m_localPlayer.m_teleportTimer = 999;
    }

    [HarmonyPatch(typeof(Player), nameof(Player.ShowTeleportAnimation))] [HarmonyPrefix]
    public static bool NotShowTeleportAnimation(ref bool __result)
    {
        __result = false;
        return false;
    }
}