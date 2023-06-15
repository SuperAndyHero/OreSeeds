using OreSeeds;
using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace OreSeeds.Configs
{
	public class GeneralConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

        public enum GrowthSpeed
        {
            RealTime = 10,
            VerySlow = 25,
            Slow = 50,
            Slowish = 75,
            Normal = 100,
            Quick = 125,
            Fast = 150,
            Faster = 200,
            Rapid = 300
        }

        [DrawTicks]
        [Slider]
        [DefaultValue(GrowthSpeed.Normal)]
        public GrowthSpeed GrowthSpeedMultiplier { 
            get { return (GrowthSpeed)((int)(Math.Round(OreSeeds.GrowthSpeedMultiplier, 2) * 100)); } 
            set { OreSeeds.GrowthSpeedMultiplier = ((float)value) * 0.01f; } }

        public bool ShowGrowthAcceledTiles { get { return OreSeeds.ShowGrowthAcceledTiles; } set { OreSeeds.ShowGrowthAcceledTiles = value; } }
    }
}