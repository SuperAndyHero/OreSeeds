using OreSeeds;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace OreSeeds.Configs
{
	public class GeneralConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

        [Range(0.25f, 3f)]
        [Increment(0.50f)]
        [DrawTicks]
        [Slider]
        [DefaultValue(1f)]
        public float GrowthSpeedMultiplier { get { return OreSeeds.GrowthSpeedMultiplier; } set { OreSeeds.GrowthSpeedMultiplier = value; } }

        public bool ShowGrowthAcceledTiles { get { return OreSeeds.ShowGrowthAcceledTiles; } set { OreSeeds.ShowGrowthAcceledTiles = value; } }
    }
}