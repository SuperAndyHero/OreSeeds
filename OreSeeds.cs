using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OreSeeds
{
	public class OreSeeds : Mod
	{
        public static OreSeeds Instance { get; set; }

        public const int MaxRecursiveLoopCount = 1;//total recusion count is 1 above this

        //these are used bye tiles that speed up growth to limit how many times they can activate eachother in a single tick
        public static int GrowLoopCount = 0;
        public static bool CanStartGrowLoop = true;

        //ore plants check this and if its true they add dropped items to this list
        //in the future this could be checked by a methodswapped vanilla item create method, instead of it being built into plant drop code
        public static bool IsHarvesterCheckingTile = false;
        public static List<int> ItemDropIndexList = new List<int>();

        public const float DefaultSeedDropChance = 0.5f;
        public const float DefaultGrowthChance = 0.33f;

        //config values
        public static float GrowthSpeedMultiplier = 1f;//server-sided value
        public static bool ShowGrowthAcceledTiles = false;

        public override void Load()
        {
            Instance = this;
            SeedLoader.Load();
        }
        public override void AddRecipeGroups()/* tModPorter Note: Removed. Use ModSystem.AddRecipeGroups */
        {
            SeedLoader.AddRecipeGroups();
        }
        public override void Unload()
        {
            SeedLoader.Unload();
            Instance = null;
        }

        //may have to methodswap `NewItem_Inner` to make this work on dropped items from other modded blocks
        //only supports this mod's crops for now since there is no good way to detect plants from other mods
        //possible options:
        //just run the rclick method on tile and have `NewItem_Inner` add any new items to an array (could cause issues with ui tiles)
        //setup cross mod support like luiafk, possibly detect mods supporting laiafk and use their support
        //could run drop method instead


        //for now just calls right click if is from this mod, later will work on vanilla plants and modded
        public static void HarvestPlant(int i, int j)//originally returned List<Item> when this was planned to copy right click code
        {
            //if is a plant from this mod
            ModTile modtile = ModContent.GetModTile(Main.tile[i, j].TileType);
            if (modtile is not null && modtile is BasePlantTile)//only on this mod's plants for now
            {
                Dust.NewDustPerfect(new Vector2(i + 0.5f, j + 0.5f) * 16, DustID.BlueFairy, Vector2.Zero);
                modtile.RightClick(i, j);
            }
        }

        public static bool GrowVanillaPlant(int i, int j)//returns if a tile is a valid type //from starlight river, written by me
        {
            //be sure to send tile square when using this if it returns true;
            //NetMessage.SendTileSquare(Main.myPlayer, i, j, 2, 2, TileChangeType.None);

            int type = Main.tile[i, j].TileType;
            int baseChance = (int)(10f / (((OreSeeds.GrowthSpeedMultiplier - 1) * 0.5f) + 1));//chance when plant conditions are not met
            //these chances are not really based on much 

            int successChance = (int)(3 / OreSeeds.GrowthSpeedMultiplier);//chance if successful
            //may run into issues if growthmult is ever higher than 4 which will cause this to be zero, requiring a MathMax check here


            switch (type)
            {
                //vanilla herb plants have seperate tiles for each stage, but share the same tile for each plant type
                case 82://vanilla plant first stage
                    {
                        if (Main.rand.NextBool(successChance))//check can be removed if vanilla herb plant growing is too slow
                        {
                            Main.tile[i, j].TileType = 83;
                            NetMessage.SendTileSquare(Main.myPlayer, i, j, 1, 1, TileChangeType.None);
                        }
                    }
                    return true;
                case 83://last stage before bloom
                    {
                        int tileFrameX = Main.tile[i, j].TileFrameX;
                        switch (tileFrameX)
                        {
                            case 0://daybloom
                                if (Main.IsItDay())
                                {
                                    if (Main.rand.NextBool(successChance))
                                        Main.tile[i, j].TileType = 84;
                                }
                                else if (Main.rand.NextBool(baseChance))
                                {
                                    Main.tile[i, j].TileType = 84;
                                }
                                break;

                            case 18://moonglow
                                if (!Main.IsItDay())
                                {
                                    if (Main.rand.NextBool(successChance))
                                        Main.tile[i, j].TileType = 84;
                                }
                                else if (Main.rand.NextBool(baseChance))
                                {
                                    Main.tile[i, j].TileType = 84;
                                }
                                break;

                            case 36://blinkroot
                                if (Main.rand.NextBool(successChance * 2))
                                    Main.tile[i, j].TileType = 84;
                                break;

                            case 54://deathweed
                                if (Main.bloodMoon || Main.GetMoonPhase() == Terraria.Enums.MoonPhase.Full)
                                {
                                    if (Main.rand.NextBool(Math.Max((int)(successChance / 1.5f), 1)))
                                        Main.tile[i, j].TileType = 84;
                                }
                                else if (Main.rand.NextBool(baseChance))
                                {
                                    Main.tile[i, j].TileType = 84;
                                }
                                break;

                            case 72://waterleaf
                                if (Main.raining || Main.tile[i, j].LiquidType == LiquidID.Water && Main.tile[i, j].LiquidAmount > 0)
                                {
                                    if (Main.rand.NextBool(successChance))
                                        Main.tile[i, j].TileType = 84;
                                }
                                else if (Main.rand.NextBool(baseChance))
                                {
                                    Main.tile[i, j].TileType = 84;
                                }
                                break;

                            case 90://fireblossom
                                if (Main.tile[i, j].LiquidType == LiquidID.Lava && Main.tile[i, j].LiquidAmount > 0)//vanilla does not use the lava check anymore
                                {
                                    if (Main.rand.NextBool(Math.Max((int)(successChance / 1.5f), 1)))
                                        Main.tile[i, j].TileType = 84;
                                }
                                else if (!Main.raining && Main.IsItDay())//vanilla uses sunset instead of daytime
                                {
                                    if (Main.rand.NextBool((int)(successChance / 0.75f)))//this divide will only result in zero in chance is below 1
                                        Main.tile[i, j].TileType = 84;
                                }
                                else if (Main.rand.NextBool(baseChance))
                                {
                                    Main.tile[i, j].TileType = 84;
                                }
                                break;

                            case 108://shiverthorn
                                if (Main.rand.NextBool(successChance * 3))//wiki just says 'after enough time has passed', so chance here is lower than blinkroot which says 'at random'
                                    Main.tile[i, j].TileType = 84;
                                break;

                            //default://should never be called, and if a type here was missed its best that it does not grow so it can be spotted and fixed
                            //    Main.tile[i, j].TileType = 84;
                            //    return true;
                        }

                        NetMessage.SendTileSquare(Main.myPlayer, i, j, 1, 1, TileChangeType.None);
                    }
                    return true;

                case TileID.Bamboo: //assumes top block of bamboo /broken
                    {
                        if (Main.rand.NextBool(successChance))
                        {
                            if (!Main.tile[i, j - 1].HasTile)
                            {
                                //WorldGen.PlaceTile(i, j - 1, TileID.Bamboo, true, true);//does not work for some reason
                                Main.tile[i, j - 1].Get<TileWallWireStateData>().HasTile = true;
                                Main.tile[i, j - 1].Get<TileTypeData>().Type = TileID.Bamboo;
                                WorldGen.SquareTileFrame(i, j - 1, true);
                                NetMessage.SendTileSquare(Main.myPlayer, i, j, 2, 2, TileChangeType.None);
                            }
                        }
                    }
                    return true;

                case TileID.Pumpkins: //does not assume top block
                    {
                        if (Main.rand.NextBool(successChance))
                        {
                            int sheetFrameX = Main.tile[i, j].TileFrameX / 18;
                            int offsetX = sheetFrameX % 2;

                            if (sheetFrameX < 8)
                            {
                                int sheetFrameY = Main.tile[i, j].TileFrameY / 18;
                                int offsetY = sheetFrameY % 2;

                                for (int x = 0; x < 2; x++)
                                {
                                    for (int y = 0; y < 2; y++)
                                    {
                                        Main.tile[i + x - offsetX, j + y - offsetY].TileFrameX += 36;
                                    }
                                }

                                NetMessage.SendTileSquare(Main.myPlayer, i, j, 2, 2, TileChangeType.None);
                            }
                        }
                    }
                    return true;
            }

            return false;
        }

        //https://ore-seeds-mod.fandom.com/wiki/Ore_Seeds_Mod_Wiki old wiki link
    }
}