#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using Steamworks;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class VipItem
{
    public string prefabName;
    internal static List<ulong> vipPlayers = new();

    static VipItem() => VipItemConfiguration.BindConfig();

    public VipItem(string prefabName)
    {
        this.prefabName = prefabName;
        all.Add(this);
    }

    public static HashSet<VipItem> all { get; protected set; } = new();

    private static bool HaveAccessToVipItem(ItemDrop itemDrop)
    {
        if (itemDrop == null) return true;
        return HaveAccessToVipItem(itemDrop.GetPrefabName());
    }

    private static bool HaveAccessToVipItem(string itemPrefabName)
    {
        if (!itemPrefabName.IsGood()) return true;
        if (!IsVipItem(itemPrefabName)) return true;
        if (vipPlayers.Contains(GetSteamID())) return true;
        return false;
    }

    private static bool IsVipItem(ItemDrop itemDrop)
    {
        if (itemDrop == null) return false;
        var itemPrefabName = itemDrop.GetPrefabName();
        return IsVipItem(itemPrefabName);
    }

    private static bool IsVipItem(string itemPrefabName)
    {
        foreach (var vipDrop in all)
            if (itemPrefabName == vipDrop.prefabName)
                return true;
        return false;
    }

    [HarmonyPatch] private static class Patch
    {
        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.CanPickup)), HarmonyPrefix]
        private static bool VipDrop_ItemDropCanPickup(ref bool __result, ItemDrop __instance)
        {
            if (HaveAccessToVipItem(__instance)) return true;
            __result = false;
            return false;
        }

        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.GetHoverText)), HarmonyPrefix]
        private static bool VipDrop_ItemDropGetHoverText(ref string __result, ItemDrop __instance)
        {
            if (HaveAccessToVipItem(__instance)) return true;
            __result = "<color=#F8DC3C>VIP only item</color>";
            return false;
        }

        [HarmonyPatch(typeof(ZLog), nameof(ZLog.Log)), HarmonyPrefix]
        private static bool VipDrop_ZLogLog(object o)
        {
            if (o.ToString().Contains("Im still nto the owner")) return false;
            return true;
        }
    }

    private static ulong GetSteamID() => SteamUser.GetSteamID().m_SteamID;
}

public static class Extension
{
    public static bool IsGood(this string str) => !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str);
    public static string GetPrefabName(this Object go) => go ? Utils.GetPrefabName(go.name) : "NULL";
}

internal static class VipItemConfiguration
{
    internal static BaseUnityPlugin? _plugin;
    private static bool hasConfigSync = true;
    private static object? _configSync;
    private static ConfigEntry<string> vipPlayersConfig;

    internal static BaseUnityPlugin plugin
    {
        get
        {
            if (_plugin is not null) return _plugin;
            IEnumerable<TypeInfo> types;
            try
            {
                types = Assembly.GetExecutingAssembly().DefinedTypes.ToList();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null).Select(t => t.GetTypeInfo());
            }

            _plugin = (BaseUnityPlugin)Chainloader.ManagerObject.GetComponent(types.First(t =>
                t.IsClass && typeof(BaseUnityPlugin).IsAssignableFrom(t)));

            return _plugin;
        }
    }

    private static object? configSync
    {
        get
        {
            if (_configSync != null || !hasConfigSync) return _configSync;
            if (Assembly.GetExecutingAssembly().GetType("ServerSync.ConfigSync") is { } configSyncType)
            {
                _configSync = Activator.CreateInstance(configSyncType, plugin.Info.Metadata.GUID + " JF_VipItems");
                configSyncType.GetField("CurrentVersion")
                    .SetValue(_configSync, plugin.Info.Metadata.Version.ToString());
                configSyncType.GetProperty("IsLocked")!.SetValue(_configSync, true);
            } else
            {
                hasConfigSync = false;
            }

            return _configSync;
        }
    }

    private static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description)
    {
        var configEntry = plugin.Config.Bind(group, name, value, description);

        configSync?.GetType().GetMethod("AddConfigEntry")!.MakeGenericMethod(typeof(T))
            .Invoke(configSync, new object[] { configEntry });

        return configEntry;
    }

    private static ConfigEntry<T> config<T>(string group, string name, T value, string description) =>
        config(group, name, value, new ConfigDescription(description));

    internal static void BindConfig()
    {
        var SaveOnConfigSet = plugin.Config.SaveOnConfigSet;
        plugin.Config.SaveOnConfigSet = false;

        vipPlayersConfig = config("VipItems", "Vip Players", "", "Example: 0000000000, 00000000, 00000000");
        vipPlayersConfig.SettingChanged += (_, _) =>
        {
            var str = vipPlayersConfig.Value.Replace(" ", "");
            VipItem.vipPlayers.Clear();
            if (str.IsGood())
            {
                foreach (var s in str.Split(','))
                    if (ulong.TryParse(s, out var id))
                        VipItem.vipPlayers.Add(id);
            }
        };

        if (SaveOnConfigSet)
        {
            plugin.Config.SaveOnConfigSet = true;
            plugin.Config.Save();
        }
    }
}