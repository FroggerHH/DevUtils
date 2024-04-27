using GroundReset.Compatibility.WardIsLove;
using UnityEngine.SceneManagement;
using AzuWardZdoKeys = DevUtils.Compatibility.WardIsLove.AzuWardZdoKeys;

namespace DevUtils.Patch;

[HarmonyPatch] public class InitWardsSettings
{
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] [HarmonyPostfix] [HarmonyWrapSafe]
    static void Init(ZNetScene __instance) { RegisterWards(); }

    internal static void RegisterWards()
    {
        wardsSettingsList.Clear();

        AddWard("guard_stone");
        AddWardThorward();
    }

    private static void AddWard(string name)
    {
        var prefab = ZNetScene.instance.GetPrefab(name.GetStableHashCode());
        if (!prefab) return;

        var areaComponent = prefab.GetComponent<PrivateArea>();
        wardsSettingsList.Add(new WardSettings(name, areaComponent.m_radius));
    }

    private static void AddWardThorward()
    {
        var name = "Thorward";
        var prefab = ZNetScene.instance.GetPrefab(name.GetStableHashCode());
        if (!prefab) return;
        wardsSettingsList.Add(new WardSettings(name, zdo =>
        {
            var radius = zdo.GetFloat(AzuWardZdoKeys.wardRadius, -101);
            if (radius == -101) return WardIsLovePlugin.WardRange().Value;
            return radius;
        }));
    }
}