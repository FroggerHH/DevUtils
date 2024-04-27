namespace DevUtils.Patch;

[HarmonyPatch]
file class NoVegetationPatch
{
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.PlaceVegetation))]
    public static bool Prefix() { return !noVegetationConfig.Value; }
}