using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace OreSeeds.Items
{
    public class RecyclerItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            //ItemID.Sets.SortingPriorityMaterials[Item.type] = 59; // Influences the inventory sort order. 59 is PlatinumBar, higher is more valuable.
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.value = Terraria.Item.sellPrice(silver: 5);
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Recycler>();
            Item.placeStyle = 0;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Sawmill, 1)
                .AddIngredient(ItemID.TinBar, 25)
                .AddIngredient(ItemID.StoneBlock, 50)
                .AddRecipeGroup(RecipeGroupID.IronBar, 20)
                .AddTile(TileID.WorkBenches)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.Sawmill, 1)
                .AddIngredient(ItemID.CopperBar, 25)
                .AddIngredient(ItemID.StoneBlock, 50)
                .AddRecipeGroup(RecipeGroupID.IronBar, 20)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
