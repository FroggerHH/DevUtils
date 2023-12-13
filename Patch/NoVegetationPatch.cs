using HarmonyLib;

namespace DevUtils;

[HarmonyPatch]
public class NoVegetationPatch
{
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.PlaceVegetation))]
    public static bool Prefix() { return !Plugin.noVegetationConfig.Value; }
}