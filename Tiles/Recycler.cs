using Microsoft.Xna.Framework;
using OreSeeds.Items;
using OreSeeds.UI;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace OreSeeds.Tiles
{
    internal class Recycler : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            //Main.tileLavaDeath[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 18 };
            TileObjectData.newTile.Origin = new Point16(2, 2);
            TileObjectData.addTile(Type);

            // AddMapEntry is for setting the color and optional text associated with the Tile when viewed on the map
            LocalizedText name = CreateMapEntryName();
            //name.SetDefault("Seed Recycler");
            AddMapEntry(new Color(119, 56, 56), name);

            AnimationFrameHeight = 56;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 32, ModContent.ItemType<RecyclerItem>());
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            if ((frameCounter = ++frameCounter % 8) == 0)
                frame = ++frame % 4;
        }

        public override bool RightClick(int i, int j)
        {
            ModContent.GetInstance<RecyclerUISystem>().ShowUI(new Vector2(i, j) * 16);
            return base.RightClick(i, j);
        }
    }
}
