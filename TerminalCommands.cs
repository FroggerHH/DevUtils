using System;
using HarmonyLib;

namespace DevUtils;

public static class TerminalCommands
{
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    [HarmonyWrapSafe]
    internal class AddChatCommands
    {
        private static void Postfix()
        {
            new Terminal.ConsoleCommand("showAllLocations", "",
                args =>
                {
                    foreach (var location in ZoneSystem.instance.m_locationInstances.Values)
                        location.m_location.m_iconAlways = true;
                }, true);

            new Terminal.ConsoleCommand("generateLocation", "", args =>
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
        }
    }
}