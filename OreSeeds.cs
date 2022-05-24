using Terraria.ModLoader;

namespace OreSeeds
{
	public class OreSeeds : Mod
	{
        public static OreSeeds Instance { get; set; }
        public override void Load()
        {
            Instance = this;
            SeedLoader.Load();
        }
        public override void AddRecipeGroups()
        {
            SeedLoader.AddRecipeGroups();
        }
    }
}