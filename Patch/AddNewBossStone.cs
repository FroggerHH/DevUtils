using JetBrains.Annotations;

namespace DevUtils.Patch;

[HarmonyPatch]
file static class AddNewBossStone
{
    private static readonly string name = "TheMeltingKnight";
    private static readonly string prefabName = "BossStone_TheMeltingKnight";

    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetupLocations))] [HarmonyPostfix]
    private static void AddStone([NotNull] ZoneSystem __instance)
    {
        if (string.IsNullOrEmpty(prefabName) || string.IsNullOrWhiteSpace(prefabName))
        {
            Debug.LogError("AddNewBossStone: prefabName is not specified");
            return;
        }

        var bossStonePrefab = ZNetScene.instance.GetPrefab(prefabName);
        if (bossStonePrefab is null)
        {
            Debug.LogError(
                "AddNewBossStone: prefab not found: " + prefabName +
                ".\nMake sure you register it in ZNetScene");
            return;
        }

        var zs = __instance;
        var start = zs.GetLocation("StartTemplate".GetStableHashCode());
        if (start is null) return;
        if (Utils.FindChild(start.m_prefab.transform, "BossStone") is not null) return;
        var queenStoneSpawn = Utils.FindChild(start.m_prefab.transform, "StoneSpawner_TheQueen");
        var newStone = Instantiate(queenStoneSpawn.gameObject, start.m_prefab.transform)
            .GetComponent<SpawnPrefab>();
        newStone.name = "BossStone_" + name;
        var transform = newStone.transform;
        transform.localPosition = new Vector3(0, 0.5f, 0);
        newStone.m_prefab = bossStonePrefab;
    }
}