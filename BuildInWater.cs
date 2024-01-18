namespace DevUtils;

[HarmonyPatch]
public static class BuildInWater
{
    private static readonly List<string> pieces = new();

    [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
    [HarmonyPostfix]
    private static void Patch(Player __instance)
    {
        if (!Player.m_localPlayer || Player.m_localPlayer != __instance) return;
        var piece = __instance.m_placementGhost?.GetComponent<Piece>();
        if (!piece) return;

        if (!pieces.Contains(Utils.GetPrefabName(piece.gameObject))) return;
        if (piece.transform.position.y < ZoneSystem.instance.m_waterLevel) return;
        __instance.m_placementStatus = Player.PlacementStatus.Invalid;
        __instance.SetPlacementGhostValid(false);
    }

    public static void RegisterOnlyInWaterPiece(string prefabName) { pieces.Add(Utils.GetPrefabName(prefabName)); }
}