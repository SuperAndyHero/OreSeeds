using Terraria.ModLoader;

namespace OreSeeds
{
	public class OreSeeds : Mod
	{
        //Make sure the workshop page is updated and has colors removed
        //REMOVE REF OF LUIAFK FROM DESC
        //Add planter boxes
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