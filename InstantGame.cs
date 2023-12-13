using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace DevUtils;

[HarmonyPatch]
public class InstantGame
{
    [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Start))]
    private static class FejdStartup_Awake_Patch
    {
        [UsedImplicitly]
        private static void Postfix(FejdStartup __instance)
        {
            if (Plugin.useInstantGame.Value == false) return;
            var mWorlds = SaveSystem.GetWorldList();
            var name = PlayerPrefs.GetString("world");
            var world = mWorlds.FirstOrDefault(x => x.m_name == name);
            if (world == null) return;
            var playerProfile = PlayerPrefs.GetString("profile");
            if (string.IsNullOrWhiteSpace(playerProfile)) return;
            ZSteamMatchmaking.instance.StopServerListing();
            ZNet.m_onlineBackend = OnlineBackendType.Steamworks;
            Game.SetProfile(playerProfile, FileHelpers.FileSource.Auto);
            ZNet.SetServer(true, true, false, name, "", world);
            __instance.LoadMainScene();
        }
    }
}