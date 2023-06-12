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
    internal class GrowCrystal : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2xX);
            TileObjectData.newTile.Height = 3;
            //TileObjectData.newTile.Origin = new Point16(2, 2);
            TileObjectData.addTile(Type);

            LocalizedText name = CreateMapEntryName();
            AddMapEntry(new Color(119, 56, 56), name);

            AnimationFrameHeight = 54;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 32, ModContent.ItemType<GrowCrystalItem>());
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            if ((frameCounter = ++frameCounter % 8) == 0)
                frame = ++frame % 4;
        }

        public override void RandomUpdate(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int offsetX = tile.TileFrameX / 18;
            int offsetY = tile.TileFrameY / 18;

            int blockRadius = 7;
            int chance = 2;//needs tweaking

            for (int r = -blockRadius; r < blockRadius + 2; r++)
            {
                for (int f = -blockRadius; f < blockRadius + 3; f++)
                {
                    if ((r == 0 || r == 1) && f >= 0 && f < 3)
                        continue;

                    if(Main.rand.NextBool(chance, 100))
                    {
                        int posX = i - offsetX + r;
                        int posY = j - offsetY + f;

                        Dust.NewDustPerfect(new Vector2(posX + 0.5f, posY + 0.5f) * 16, DustID.GreenFairy, Vector2.Zero);

                        if (Main.tile[posX, posY].TileType == Type)//can often crash if too many are too close and the chances are too high
                            continue;

                        ModTile modtile = ModContent.GetModTile(Main.tile[posX, posY].TileType);
                        if (modtile is not null)
                        {
                            //if (Main.rand.NextBool(2))//50% chance to effect modded plant
                            //{
                            ModContent.GetModTile(Main.tile[posX, posY].TileType)?.RandomUpdate(posX, posY);
                            NetMessage.SendTileSquare(Main.myPlayer, posX, posY, 2, 2, TileChangeType.None);
                            //}
                        }
                        else//vanilla grow check has seperate changes
                        {
                            //GrowVanillaPlant(i, j + 1 + m);
                        }

                    }
                }
            }
        }

        public override bool RightClick(int i, int j)
        {
            RandomUpdate(i, j);

            return base.RightClick(i, j);
        }
    }
}
