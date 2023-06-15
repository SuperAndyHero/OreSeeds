using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OreSeeds.Items;
using OreSeeds.UI;
using System;
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

        //public const int 

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

            //Main.NewText("Current update count: " + OreSeeds.GrowLoopCount + " | IsStart: " + IsStartingUpdate, !IsStartingUpdate ? null : Color.GreenYellow);

            Tile tile = Main.tile[i, j];
            int offsetX = tile.TileFrameX / 18;
            int offsetY = tile.TileFrameY / 18;

            const int blockRadius = 5;
            float chance = 7 * (((OreSeeds.GrowthSpeedMultiplier - 1) * 0.1f) + 1);//needs tweaking

            for (int r = -blockRadius; r <= blockRadius + 1; r++)
            {
                for (int f = -blockRadius; f <= blockRadius + 2; f++)
                {
                    if ((r == 0 || r == 1) && f >= 0 && f < 3)
                        continue;

                    if (Main.rand.NextFloat(0, 100) < chance)
                    {
                        int posX = i - offsetX + r;
                        int posY = j - offsetY + f;

                        ModTile modtile = ModContent.GetModTile(Main.tile[posX, posY].TileType);//works on any modded tile...
                        bool isValidTile;

                        if (modtile is not null)//does not check if is a ore plant, so that it works on modded plants
                        {
                            isValidTile = true;//may need a tile type check here so not every modded tile shows up

                            ModContent.GetModTile(Main.tile[posX, posY].TileType)?.RandomUpdate(posX, posY);
                            //NetMessage.SendTileSquare(Main.myPlayer, posX, posY, 1, 1, TileChangeType.None);//may be needed
                        }
                        else//vanilla grow check is seperate
                        {
                            isValidTile = OreSeeds.GrowVanillaPlant(posX, posY);//returns true if tile is correct
                            //GrowVanillaPlant handles SendTileSquare
                        }

                        if (isValidTile && OreSeeds.ShowGrowthAcceledTiles)
                        {
                            for (int p = -1; p <= 1; p++)
                            {
                                Dust.NewDustPerfect(
                                    new Vector2(posX + 0.5f, posY + 0.75f) * 16 + new Vector2(p * 4, 0),
                                    DustID.ShimmerSpark,
                                    new Vector2(p * 0.05f, Main.rand.NextFloat(-0.7f, -0.25f)),
                                    0,
                                    Color.White, 1.5f);
                            }

                            for (int p = -3; p <= 3; p++)
                            {
                                Dust.NewDustPerfect(
                                    new Vector2(posX + 0.5f, posY + 0.75f) * 16 + new Vector2(p * 2, 0),
                                    DustID.SteampunkSteam,
                                    new Vector2(p * 0.05f, Main.rand.NextFloat(-0.5f, 0.1f)),
                                    0,
                                    new Color(Main.rand.Next(0, 32), Main.rand.Next(228, 256), Main.rand.Next(100, 228)));
                            }

                            //workable dusts
                            //DustID.ManaRegeneration
                            //enchanted gold
                            //shimmer spark
                            //DustID.GreenFairy

                            //for (int p = -2; p < 2 + 1; p++)//old dust
                            //{
                            //    for (int s = -2; s < 2 + 1; s++)
                            //    {
                            //        Dust.NewDustPerfect(new Vector2(posX + 0.5f, posY + 0.5f) * 16 + new Vector2(p, s) * 3, DustID.GreenFairy, Vector2.Zero);
                            //    }
                            //}

                            //for (int p = -2; p < 2 + 1; p++)//old unscuccesful dust
                            //{
                            //    for (int s = -2; s < 2 + 1; s++)
                            //    {
                            //        Dust.NewDustPerfect(new Vector2(posX + 0.5f, posY + 0.5f) * 16 + new Vector2(p, s) * 3, DustID.PinkFairy, Vector2.Zero);
                            //    }
                            //}
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
            //RandomUpdate(i, j);
            const int tempblockRadius = 5;

            Tile tile = Main.tile[i, j];
            int offsetX = tile.TileFrameX / 18;
            int offsetY = tile.TileFrameY / 18;

            for (int r = -tempblockRadius; r <= tempblockRadius + 2; r++)
            {
                for (int f = -tempblockRadius; f <= tempblockRadius + 3; f++)
                {
                    bool left = r == -tempblockRadius;
                    bool right = r == tempblockRadius + 2;
                    bool top = f == -tempblockRadius;
                    bool bottom = f == tempblockRadius + 3;

                    if (top || bottom || left || right)
                    {
                        int posX = i - offsetX + r;
                        int posY = j - offsetY + f;

                        for (int p = -1; p <= 1; p++)
                        {
                            Dust.NewDustPerfect(
                                   new Vector2(posX, posY) * 16 + new Vector2(((top || bottom) && !(left || right)) ? p : 0, ((left || right) && !(top || bottom)) ? p : 0) * 5.33f,
                                   DustID.SteampunkSteam,
                                   Vector2.Zero,
                                   0,
                                   new Color(80, 255, 255));
                        }
                    }
                }
            }


                    return base.RightClick(i, j);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (Main.tile[i, j].TileFrameX == 0 && Main.tile[i, j].TileFrameY == 0)
            {
                Texture2D tex = ModContent.Request<Texture2D>("OreSeeds/Tiles/GrowCrystalGlow").Value;

                Vector2 lightingOffset = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
                Vector2 center = tex.Size() / 2;

                spriteBatch.Draw(tex,
                    new Vector2(i + 1f, j + 1.5f) * 16 + new Vector2(0, 0) - Main.screenPosition + lightingOffset,
                    null, new Color(10, 10, 10, 0),//Lighting.GetColor(i, j),
                    0f,
                     center, ((float)Math.Sin(Main.GameUpdateCount / 30f + i * 0.3333f + j * 2.6666f) + 10) / 10.5f, SpriteEffects.None, 0f);
            }

            return base.PreDraw(i, j, spriteBatch);
        }
    }
}
