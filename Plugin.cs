﻿using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using DevUtils.Patch;
using Debug = UnityEngine.Debug;

namespace DevUtils;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class Plugin : BaseUnityPlugin
{
    public const string ModName = "DevUtils", ModVersion = "1.0.0", ModGUID = "com.Frogger." + ModName;
    public static ConfigEntry<bool> flatWorldConfig;
    public static ConfigEntry<bool> noVegetationConfig;
    public static ConfigEntry<bool> useInstantGame;
    public static ConfigEntry<bool> noPassword;
    public static ConfigEntry<bool> noStaminaCost;
    public static ConfigEntry<bool> noWet;
    public static ConfigEntry<bool> clearWeather;
    public static ConfigEntry<bool> skipLocationsGeneration;
    public static Plugin _self;

    internal static readonly int HeightmapWidth = 64;
    internal static readonly int HeightmapScale = 1;


    private void Awake()
    {
        _self = this;

        flatWorldConfig = Config.Bind("General", "FlatWorld", false);
        noVegetationConfig = Config.Bind("General", "NoVegetation", false);
        useInstantGame = Config.Bind("General", "UseInstantGame", false);
        noPassword = Config.Bind("General", "No server password", false);
        noStaminaCost = Config.Bind("General", "NoStaminaCost", true);
        noWet = Config.Bind("General", "NoWet", true);
        clearWeather = Config.Bind("General", "ClearWeather", true);
        skipLocationsGeneration = Config.Bind("General", "SkipLocationsGeneration", false);

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGUID);

        InvokeRepeating(nameof(UpdateGroundPointInfoCall), 1, 1);
        InvokeRepeating(nameof(FindWardsCall), 5, 20);
        InvokeRepeating(nameof(RegisterWardsCall), 5, 5);
    }

    private void RegisterWardsCall()
    {
        try
        {
            InitWardsSettings.RegisterWards();
        }
        catch
        {
            // ignored
        }
    }

    internal static async void GoThroughHeightmap()
    {
        var comp = TerrainComp.FindTerrainCompiler(m_localPlayer.transform.position);
        var zoneCenter =
            ZoneSystem.instance.GetZonePos(ZoneSystem.instance.GetZone(comp.m_nview.GetZDO().GetPosition()));
        var skyLine = new GameObject("SkyLine").AddComponent<LineRenderer>();
        skyLine.positionCount = 2;
        var skyLineMaterial = new Material(Shader.Find("Unlit/Color"));
        skyLineMaterial.name = "UnlitColor Material";
        skyLineMaterial.color = Color.white;
        skyLine.material = skyLineMaterial;

        var num = HeightmapWidth + 1;
        for (var h = 0; h < num; h++)
        for (var w = 0; w < num; w++)
        {
            var idx = h * num + w;
            var worldPos = VertexToWorld(zoneCenter, w, h);
            var inWard = PrivateArea.InsideFactionArea(worldPos, Character.Faction.Players);
            skyLine.material.color = !inWard ? Color.green : Color.red;
            skyLine.SetPosition(0, worldPos);
            skyLine.SetPosition(1, worldPos with { y = 10000 });
            await Task.Delay(10);
        }

        Destroy(skyLine.gameObject);
    }

    public static bool groundPointInfoEnabled = false;
    public static Transform groundPointObj = null;

    private void UpdateGroundPointInfoCall()
    {
        if (!groundPointInfoEnabled)
        {
            if (groundPointObj) groundPointObj.gameObject.SetActive(false);
            return;
        }

        try
        {
            UpdateGroundPointInfo();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    private static void UpdateGroundPointInfo()
    {
        if (!groundPointObj)
        {
            groundPointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            groundPointObj.name = "GroundPoint";
            groundPointObj.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }

        groundPointObj.gameObject.SetActive(true);

        if (!GetPointingPos(out var worldPos)) return;
        var vertexPos = WorldToVertex(worldPos);
        var zonePos = ZoneSystem.instance.GetZonePos(ZoneSystem.instance.GetZone(worldPos));
        var worldPosFromVertex = VertexToWorld(zonePos, vertexPos);
        groundPointObj.position = worldPosFromVertex with { y = worldPos.y + 0.1f };
        var comp = TerrainComp.FindTerrainCompiler(worldPosFromVertex);

        var message = $"---------------------------------\n"
                      + $"World:           {worldPos}\n"
                      + $"Vertex:          {vertexPos}\n"
                      + $"WorldFromVertex: {worldPosFromVertex}\n";
        if (comp)
        {
            var height_idx = (int)(vertexPos.y * (HeightmapWidth + 1) + vertexPos.x);
            if (comp.m_levelDelta.Length > height_idx)
            {
                if (comp.m_modifiedHeight[height_idx] == false)
                    message += $"Height:          000\n";
                else message += $"Height:          {comp.m_levelDelta[height_idx]}\n";
            } else
                message += $"Height:          index {height_idx} out of range\n";

            var color_idx = (int)(vertexPos.y * HeightmapWidth + vertexPos.x);
            if (comp.m_paintMask.Length > color_idx)
            {
                if (comp.m_modifiedPaint[color_idx] == false)
                    message += $"Color:           000\n";
                else
                {
                    var color = comp.m_paintMask[color_idx];
                    message += $"Color:           {color}\n";
                }
            } else
                message += $"Color:           index {color_idx} out of range\n";


            // var num = HeightmapWidth + 1;
            // for (var h = 0; h < num; h++)
            // for (var w = 0; w < num; w++)
            // {
            //     var idx = h * num + w;
            //     if (comp.m_modifiedHeight[idx] && comp.m_levelDelta[idx] != 0)
            //     {
            //         message += $"\n";
            //         //Height on idx:3395 w:15 h:52 = 2
            //         message += $"Height on idx:{idx} w:{w} h:{h} = {comp.m_levelDelta[idx]}\n";
            //     }
            // }
            //
            // num = HeightmapWidth;
            // for (var h = 0; h < num; h++)
            // for (var w = 0; w < num; w++)
            // {
            //     var idx = h * num + w;
            //     if (comp.m_modifiedPaint[idx] && comp.m_paintMask[idx] != m_paintMaskNothing)
            //     {
            //         message += $"\n";
            //         //Paint on idx:3982 w:14 h:62 = RGBA(0.000, 0.992, 0.000, 1.000)
            //         message += $"Paint on idx:{idx} w:{w} h:{h} = {comp.m_paintMask[idx]}\n";
            //     }
            // }
        }

        message += $"IsInWard:        {IsInWard(worldPosFromVertex)}\n";

        Debug.Log(message);
    }

    private static List<ZDO> wards = new();
    public static List<WardSettings> wardsSettingsList = new();
    public static Stopwatch watch = new();

    private void FindWardsCall() => FindWards();

    private static async Task FindWards()
    {
        if (!ZoneSystem.instance || !m_localPlayer) return;
        watch.Restart();
        wards.Clear();
        for (var i = 0; i < wardsSettingsList.Count; i++)
        {
            var wardsSettings = wardsSettingsList[i];
            var zdos = await ZoneSystem.instance.GetWorldObjectsAsync(wardsSettings.prefabName);
            wards = wards.Concat(zdos).ToList();
        }

        var totalSeconds = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
        DebugWarning($"Wards count: {wards.Count}. Took {totalSeconds} seconds");
        watch.Restart();
    }

    private static bool GetPointingPos(out Vector3 worldPos)
    {
        worldPos = Vector3.zero;
        var cameraTransform = GameCamera.instance.transform;
        var direction = cameraTransform.forward;
        var origin = cameraTransform.position;
        var collisions = Physics.RaycastAll(origin, direction, 50f, Character.s_groundRayMask);
        if (collisions == null || collisions.Length > 1 || collisions.Length == 0) return false;
        worldPos = collisions[0].point;
        return true;
    }

    public static Vector3 VertexToWorld(Vector3 zonePos, int x, int y)
    {
        var xPos = ((float)x - HeightmapWidth / 2) * HeightmapScale;
        var zPos = ((float)y - HeightmapWidth / 2) * HeightmapScale;
        return zonePos + new Vector3(xPos, 0f, zPos);
    }

    public static Vector3 VertexToWorld(Vector3 zonePos, Vector2 vertexPos) =>
        VertexToWorld(zonePos, (int)vertexPos.x, (int)vertexPos.y);

    public static Vector2 WorldToVertex(Vector3 worldPos, Vector3 heightmapPos)
    {
        var vector3 = worldPos - heightmapPos;
        var result = new Vector2();
        result.x = FloorToInt((float)(vector3.x / (double)HeightmapScale + 0.5)) + HeightmapWidth / 2;
        result.y = FloorToInt((float)(vector3.z / (double)HeightmapScale + 0.5)) + HeightmapWidth / 2;

        return result;
    }

    public static Vector2 WorldToVertex(Vector3 worldPos) =>
        WorldToVertex(worldPos, ZoneSystem.instance.GetZonePos(ZoneSystem.instance.GetZone(worldPos)));

    public static bool IsInWard(Vector3 pos, float checkRadius = 0) =>
        wards.Exists(searchWard =>
        {
            var wardSettings =
                wardsSettingsList.Find(s => s.prefabName.GetStableHashCode() == searchWard.GetPrefab());
            var isEnabled = searchWard.GetBool(ZDOVars.s_enabled, true);
            Debug.Log($"isEnabled={isEnabled}");
            if (!isEnabled) return false; // not enabled, skip range check
            var wardRadius = wardSettings.dynamicRadius
                ? wardSettings.getDynamicRadius(searchWard)
                : wardSettings.radius;
            Debug.Log($"wardRadius={wardRadius}");
            var inRange = pos.DistanceXZ(searchWard.GetPosition()) <= wardRadius + checkRadius;
            return inRange;
        });
}