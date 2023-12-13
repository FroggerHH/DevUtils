﻿using HarmonyLib;

namespace DevUtils.Patch;

[HarmonyPatch]
public static class NoPassword
{
    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.IsPublicPasswordValid))] [HarmonyPrefix]
    private static bool IsPublicPasswordValid(ref bool __result)
    {
        if (Plugin.noPassword.Value == false) return true;
        __result = true;
        return false;
    }

    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.NeedPassword))] [HarmonyPrefix]
    private static bool No_NeedPassword(ref bool __result)
    {
        if (Plugin.noPassword.Value == false) return true;
        __result = false;
        return false;
    }
}