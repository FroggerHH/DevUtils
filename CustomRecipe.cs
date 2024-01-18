[Serializable]
public class CustomRecipe
{
    public string itemName;
    public int amount = 1;
    public float qualityResultAmountMultiplier = 1f;
    public string craftingStationName;
    public string repairStationName;
    public int minStationLevel = 1;
    public bool requireOnlyOneIngredient;
    public CustomRequirement[] resources = new CustomRequirement[0];
    public readonly Recipe recipe;
    private bool initialized;

    public CustomRecipe()
    {
        recipe = CreateInstance<Recipe>();
        recipe.name = "CustomRecipe" + Guid.NewGuid().ToString().GetStableHashCode();
        all.Add(this);
    }

    public static List<CustomRecipe> all { get; protected set; } = new();

    public void Init()
    {
        if (initialized) return;
        initialized = true;

        if (IsGood(itemName))
        {
            var item = ObjectDB.instance.GetItemPrefab(itemName);
            if (item) recipe.m_item = item?.GetComponent<ItemDrop>();
            else Debug.LogError($"Can not find an item with name{itemName}");
        } else
        {
            Debug.LogError("itemName is not specified");
        }

        recipe.m_amount = amount;
        recipe.m_qualityResultAmountMultiplier = qualityResultAmountMultiplier;
        if (IsGood(craftingStationName))
        {
            var station = ZNetScene.instance.GetPrefab(craftingStationName);
            if (station) recipe.m_craftingStation = station.GetComponent<CraftingStation>();
            else Debug.LogError($"Can not find a CraftingStation with name {craftingStationName}");
        }

        if (IsGood(repairStationName))
        {
            var station = ZNetScene.instance.GetPrefab(repairStationName);
            if (station) recipe.m_repairStation = station.GetComponent<CraftingStation>();
            else Debug.LogError($"Can not find a CraftingStation with name {repairStationName}");
        }

        recipe.m_minStationLevel = minStationLevel;
        recipe.m_requireOnlyOneIngredient = requireOnlyOneIngredient;
        recipe.m_resources = resources.Select(x => x.ToNormal()).ToArray();

        recipe.name = "CustomRecipe " + itemName;
    }

    private static bool IsGood(string str) { return !string.IsNullOrEmpty(str) && !string.IsNullOrWhiteSpace(str); }

    [Serializable]
    public class CustomRequirement
    {
        public string resItem;
        public int amount = 1;
        public int extraAmountOnlyOneIngredient;
        public int amountPerLevel = 1;

        public Piece.Requirement ToNormal()
        {
            var result = new Piece.Requirement();
            if (IsGood(resItem))
            {
                var itemPrefab = ObjectDB.instance.GetItemPrefab(resItem);
                if (itemPrefab) result.m_resItem = itemPrefab.GetComponent<ItemDrop>();
                else Debug.LogError($"Can not find an item with name {resItem}");
            } else
            {
                Debug.LogError("resItem is not specified");
            }

            result.m_amount = amount;
            result.m_recover = true;
            result.m_amountPerLevel = amountPerLevel;
            result.m_extraAmountOnlyOneIngredient = extraAmountOnlyOneIngredient;

            return result;
        }
    }

    [HarmonyPatch] private static class Patch
    {
        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        private static void Postfix()
        {
            foreach (var customRecipe in all)
            {
                if (ObjectDB.instance.m_recipes.Contains(customRecipe.recipe)) continue;
                ObjectDB.instance.m_recipes.Add(customRecipe.recipe);

                customRecipe.Init();
            }
        }
    }
}