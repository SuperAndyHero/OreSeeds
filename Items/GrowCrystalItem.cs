using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace OreSeeds.Items
{
    public class GrowCrystalItem : ModItem
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
            Item.value = Terraria.Item.sellPrice(gold: 1, silver: 25);
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.GrowCrystal>();
            Item.placeStyle = 0;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ManaCrystal, 4)
                .AddIngredient(ItemID.LifeCrystal, 1)
                .AddIngredient(ItemID.Sunflower, 1)
                .AddIngredient(ItemID.Amethyst, 20)
                //.AddTile(TileID.WorkBenches)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.ManaCrystal, 4)
                .AddIngredient(ItemID.LifeCrystal, 1)
                .AddIngredient(ItemID.Sunflower, 1)
                .AddIngredient(ItemID.Topaz, 20)
                //.AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
