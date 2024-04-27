using System.Threading.Tasks;
using DevUtils.Patch;
using JetBrains.Annotations;

namespace DevUtils;

[HarmonyPatch]
[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal)), HarmonyWrapSafe]
file static class TerminalCommands
{
    [UsedImplicitly]
    private static void Postfix()
    {
        new ConsoleCommand("showAllLocations", "",
            _ =>
            {
                foreach (var location in ZoneSystem.instance.m_locationInstances.Values)
                    location.m_location.m_iconAlways = true;
            }, true);
        new ConsoleCommand("learnAll", "",
            _ =>
            {
                foreach (var item_ in ObjectDB.instance.m_items)
                {
                    var itemData = item_.GetComponent<ItemDrop>().m_itemData;
                    m_localPlayer.m_trophies.Add(item_.name);
                    m_localPlayer.m_knownMaterial.Add(itemData.m_shared.m_name);
                    Gogan.LogEvent("Game", "ItemFound", itemData.m_shared.m_name, 0L);
                }

                m_localPlayer.UpdateKnownRecipesList();
                m_localPlayer.UpdateEvents();
            }, true);
        new ConsoleCommand("generateLocation", "", args =>
        {
            try
            {
                if (ZoneSystem.instance == null)
                    throw new Exception("ZoneSystem is not initialized yet. Try run command later.");
                if (args.Args.Length == 1)
                    throw new Exception("First argument must be a location name (string)");

                var locName = args[1];
                var location = ZoneSystem.instance.GetLocation(locName);
                if (location == null)
                    throw new Exception($"Can not find location with name '{locName}'");
                ZoneSystem.instance.GenerateLocations(location);

                args.Context.AddString("Done");
            }
            catch (Exception e)
            {
                args.Context.AddString("<color=red>Error: " + e.Message + "</color>");
            }
        }, true);

        new ConsoleCommand("GoThroughHeightmap", "", _ => GoThroughHeightmap(), true);
        new ConsoleCommand("GroundPointInfo", "", _ => groundPointInfoEnabled = !groundPointInfoEnabled, true);
        new ConsoleCommand("ForceDebug", "", _ => DoDebugModePatch.Logic(), true);

        new ConsoleCommand("UpgradeAll", "", _ =>
        {
            if (!m_localPlayer) return;
            var list = m_localPlayer.GetInventory().GetAllItems().Where(x => x.m_shared.m_maxQuality > 1).ToList();
            for (var i = 0; i < list.Count; i++)
            {
                var item = list[i];
                item.m_quality = item.m_shared.m_maxQuality;
                item.m_durability = item.m_shared.m_maxDurability;
            }
        }, true);
    }
}