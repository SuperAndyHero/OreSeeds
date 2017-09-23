using Terraria.ModLoader;

namespace OreSeeds
{
	class OreSeeds : Mod
	{
		public OreSeeds()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
				AutoloadGores = true,
				AutoloadSounds = true
			};
		}
	}
}
