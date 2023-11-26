using System.Reflection;
using BepInEx;
using BepInEx.Configuration;

namespace DevUtils;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class Plugin : BaseUnityPlugin
{
    public const string ModName = "DevUtils", ModVersion = "1.0.0", ModGUID = "com.Frogger." + ModName;
    public static ConfigEntry<bool> flatWorldConfig;
    public static ConfigEntry<bool> noVegetationConfig;
    public static ConfigEntry<bool> useInstantGame;
    public static ConfigEntry<bool> noPassword;
    public static Plugin _self;

    private void Awake()
    {
        _self = this;

        flatWorldConfig = Config.Bind("General", "FlatWorld", false);
        noVegetationConfig = Config.Bind("General", "NoVegetation", false);
        useInstantGame = Config.Bind("General", "UseInstantGame", false);
        noPassword = Config.Bind("General", "No server password", false);

        new CustomRecipe
        {
            amount = 5,
            itemName = "FineWood", // Item to be made
            craftingStationName = "piece_workbench",
            minStationLevel = 2,
            resources = new[]
            {
                new CustomRecipe.CustomRequirement
                {
                    amount = 1,
                    resItem = "Stone"
                },
                new CustomRecipe.CustomRequirement
                {
                    amount = 2,
                    resItem = "Mushroom"
                }
            }
        };

        BuildInWater.RegisterOnlyInWaterPiece("piece_bench01");

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGUID);
    }
}