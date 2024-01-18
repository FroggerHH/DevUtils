using System.Threading.Tasks;
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
    }
}