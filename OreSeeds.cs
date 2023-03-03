using System.Collections.Generic;
using Terraria.ModLoader;
using System.IO;
using Hjson;

namespace OreSeeds
{
	public class OreSeeds : Mod
	{
        //Make sure the workshop page is updated and has colors removed
        public static OreSeeds Instance { get; set; }

        public static JsonObject ItemNames;
        public static JsonObject TileNames;
        public override void Load()
        {
            ItemNames = new();
            TileNames = new();
            Instance = this;
            SeedLoader.Load();
            JsonObject both = new JsonObject();
            both.Add("ItemNames", ItemNames);
            both.Add("TileNames", TileNames);
            File.WriteAllText("D:\\Documents\\My Games\\Terraria\\tModLoader\\ModSources\\OreSeeds\\Localization\\output.hjson",
                both.ToString(Stringify.Hjson));
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