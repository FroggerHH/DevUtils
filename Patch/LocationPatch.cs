using System.Collections.Generic;
using HarmonyLib;
using ItemManager;
using UnityEngine;
using static DevUtils.Plugin;
using static Heightmap;
using static Heightmap.Biome;
using static ZoneSystem;
using static ZoneSystem.ZoneVegetation;

namespace DevUtils;

[HarmonyPatch]
public class LocationPatch
{
    [HarmonyPatch(typeof(Location), nameof(Location.Awake)), HarmonyPostfix, HarmonyWrapSafe]
    public static void Patch(Location __instance)
    {
        if (!__instance.name.StartsWith("TestTown")) return;
        var town = NPS_Town.FindTown(__instance.transform.position);
        if (!town)
        {
            DebugError($"[DevUtils] Can't find town settings for {__instance.name}");
        }
        town.m_location = __instance;
    }
}