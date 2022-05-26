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
        public override void Unload()
        {
            SeedLoader.Unload();
            Instance = null;
        }
        //https://ore-seeds-mod.fandom.com/wiki/Ore_Seeds_Mod_Wiki old wiki link
    }
}