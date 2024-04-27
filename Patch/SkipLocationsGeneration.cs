using JetBrains.Annotations;

namespace DevUtils.Patch;

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.GenerateLocations), [typeof(ZoneLocation)])]
file static class SkipLocationsGeneration
{
    [UsedImplicitly]
    private static bool Prefix()
    {
        if (skipLocationsGeneration.Value == true)
            return false;

        return true;
    }
}