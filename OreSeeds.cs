using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace OreSeeds
{
	public class OreSeeds : Mod
	{
        public static OreSeeds Instance { get; set; }

        public const int MaxRecursiveLoopCount = 2;

        //these are used bye tiles that speed up growth to limit how many times they can activate eachother in a single tick
        public static int GrowLoopCount = 0;
        public static bool CanStartGrowLoop = true;

        //ore plants check this and if its true they add dropped items to this list
        //in the future this could be checked by a methodswapped vanilla item create method, instead of it being built into plant drop code
        public static bool IsHarvesterCheckingTile = false;
        public static List<int> ItemDropIndexList = new List<int>();

        public const float DefaultSeedDropChance = 0.5f;
        public const float DefaultGrowthChance = 0.33f;

        public static float GrowthSpeedMultiplier = 1f;//server-sided value
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

        //https://ore-seeds-mod.fandom.com/wiki/Ore_Seeds_Mod_Wiki old wiki link
    }
}