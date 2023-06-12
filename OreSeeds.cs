using System.Collections.Generic;
using Terraria.ModLoader;

namespace OreSeeds
{
	public class OreSeeds : Mod
	{
        public static OreSeeds Instance { get; set; }

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
        //https://ore-seeds-mod.fandom.com/wiki/Ore_Seeds_Mod_Wiki old wiki link
    }
}