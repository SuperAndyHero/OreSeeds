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
            bool IsStartingUpdate = OreSeeds.CanStartGrowLoop;

            if (IsStartingUpdate)
            {
                OreSeeds.GrowLoopCount = 0;
                OreSeeds.CanStartGrowLoop = !OreSeeds.CanStartGrowLoop;
            }
            else if (OreSeeds.GrowLoopCount > OreSeeds.MaxRecursiveLoopCount)
                return;
            else
                OreSeeds.GrowLoopCount++;

            Main.NewText("Current update count: " + OreSeeds.GrowLoopCount + " | IsStart: " + IsStartingUpdate, !IsStartingUpdate ? null : Color.GreenYellow);

            Tile tile = Main.tile[i, j];

            int offsetX = tile.TileFrameX / 18;
            int offsetY = tile.TileFrameY / 18;

            int blockRadius = 7;
            float chance = 9 * (((OreSeeds.GrowthSpeedMultiplier - 1) * 0.3f) + 1);//needs tweaking

            for (int r = -blockRadius; r < blockRadius + 2; r++)
            {
                for (int f = -blockRadius; f < blockRadius + 3; f++)
                {
                    if ((r == 0 || r == 1) && f >= 0 && f < 3)
                        continue;

                    if (Main.rand.NextFloat(0, 100) < chance)
                    {
                        int posX = i - offsetX + r;
                        int posY = j - offsetY + f;

                        ModTile modtile = ModContent.GetModTile(Main.tile[posX, posY].TileType);//works on any modded tile...
                        if (modtile is not null)
                        {

                            for (int p = -2; p < 2 + 1; p++)//shows valid tiles (this should become a config option with better gfx)
                            {
                                for (int s = -2; s < 2 + 1; s++)
                                {
                                    Dust.NewDustPerfect(new Vector2(posX + 0.5f, posY + 0.5f) * 16 + new Vector2(p, s) * 3, DustID.GreenFairy, Vector2.Zero);
                                }
                            }

                            ModContent.GetModTile(Main.tile[posX, posY].TileType)?.RandomUpdate(posX, posY);
                            //NetMessage.SendTileSquare(Main.myPlayer, posX, posY, 2, 2, TileChangeType.None);//may be needed
                        }
                        else//vanilla grow check is seperate
                        {
                            //for (int p = -2; p < 2 + 1; p++)//shows all selected tiles
                            //{
                            //    for (int s = -2; s < 2 + 1; s++)
                            //    {
                            //        Dust.NewDustPerfect(new Vector2(posX + 0.5f, posY + 0.5f) * 16 + new Vector2(p, s) * 3, DustID.PinkFairy, Vector2.Zero);
                            //    }
                            //}
                            //GrowVanillaPlant(i, j + 1 + m);
                        }

                    }
                }
            }

            if (IsStartingUpdate)
            {
                OreSeeds.CanStartGrowLoop = !OreSeeds.CanStartGrowLoop;
                OreSeeds.GrowLoopCount = 0;
            }
        }

        public override bool RightClick(int i, int j)
        {
            RandomUpdate(i, j);

            return base.RightClick(i, j);
        }
    }
}
