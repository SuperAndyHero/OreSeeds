using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static OreSeeds.SeedLoader;

namespace OreSeeds
{
	public static class SeedLoader
    {
        public static Mod Mod => OreSeeds.Instance;

        private static RecipeGroup SeedRecipeGroup;
        private static RecipeGroup CobaltSeedGroup;
        private static RecipeGroup MythrilSeedGroup;
        private static RecipeGroup AdamantiteSeedGroup;
        public static void AddRecipeGroups()
        {
            //todo: Add translation for this
            SeedRecipeGroup = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} Farmable {Lang.GetItemNameValue(ItemID.Seed)}", 
                ItemID.MushroomGrassSeeds, 
                ItemID.JungleGrassSeeds, 
                ItemID.BlinkrootSeeds, 
                ItemID.DaybloomSeeds, 
                ItemID.ShiverthornSeeds,
                ItemID.FireblossomSeeds,
                ItemID.WaterleafSeeds,
                ItemID.MoonglowSeeds,
                ItemID.DeathweedSeeds
                );

            CobaltSeedGroup = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(Mod.Find<ModItem>("CobaltSeeds").Type)}",
                Mod.Find<ModItem>("CobaltSeeds").Type,
                Mod.Find<ModItem>("PalladiumSeeds").Type);

            MythrilSeedGroup = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(Mod.Find<ModItem>("MythrilSeeds").Type)}",
                Mod.Find<ModItem>("MythrilSeeds").Type,
                Mod.Find<ModItem>("OrichalcumSeeds").Type);

            AdamantiteSeedGroup = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(Mod.Find<ModItem>("AdamantiteSeeds").Type)}",
                Mod.Find<ModItem>("AdamantiteSeeds").Type,
                Mod.Find<ModItem>("TitaniumSeeds").Type);

            RecipeGroup.RegisterGroup("OreSeeds:AlchSeeds", SeedRecipeGroup);
            RecipeGroup.RegisterGroup("OreSeeds:CobaltSeeds", CobaltSeedGroup);
            RecipeGroup.RegisterGroup("OreSeeds:MythrilSeeds", MythrilSeedGroup);
            RecipeGroup.RegisterGroup("OreSeeds:AdamantiteSeeds", AdamantiteSeedGroup);
        }

        public static void Load()
        {
            #region ores
            AddPlant("Copper",
                () => ItemID.CopperOre, 15,
                Tags.PreHardmode | Tags.MundaneOre,
                (2,4));//default drop range is (1,3)

            AddPlant("Tin",
               () => ItemID.TinOre, 15,
               Tags.PreHardmode | Tags.MundaneOre,
               (2,4));

            AddPlantRange( new (string, Func<int>)[]{ 
                ("Iron", () => ItemID.IronOre), 
                ("Lead", () => ItemID.LeadOre),
                ("Silver", () => ItemID.SilverOre),
                ("Tungsten", () => ItemID.TungstenOre)}, 
                20,
                Tags.PreHardmode | Tags.MundaneOre);

            AddPlant("Gold",
               () => ItemID.GoldOre, 30,
               Tags.PreHardmode | Tags.MundaneOre);

            AddPlant("Platinum",
               () => ItemID.PlatinumOre, 30,
               Tags.PreHardmode | Tags.MundaneOre);

            AddPlant("Meteorite",
               () => ItemID.Meteorite, 30,
               Tags.PreHardmode);

            AddPlant("Demonite",
               () => ItemID.DemoniteOre, 20,
               Tags.PreHardmode | Tags.Evil, 
               description: "Crafted at a demon altar",
               recipe: new SeedRecipe(TileID.DemonAltar)); 

            AddPlant("Crimtane",
               () => ItemID.CrimtaneOre, 20,
               Tags.PreHardmode | Tags.Evil,
               description: "Crafted at a crimson altar",
               recipe: new SeedRecipe(TileID.DemonAltar));

            AddPlant("Obsidian",
               () => ItemID.Obsidian, 12,
               Tags.PreHardmode | Tags.Hell,
               (2, 4));
               //description: "Crafted near lava",
               //craftTileID: TileID.LavaLamp);//todo find how to use lava

            AddPlant("Hellstone",
               () => ItemID.Hellstone, 20,
               Tags.PreHardmode | Tags.Hell,
               description: $"Crafted at [i:{ItemID.Hellforge}]",
               recipe: new SeedRecipe(TileID.Hellforge));

            AddPlant("Cobalt",
               () => ItemID.CobaltOre, 16,
               Tags.Hardmode | Tags.MundaneOre,
               description: $"Crafted at [i:{ItemID.IronAnvil}]\nDoes not drop seeds",
               recipe: new SeedRecipe(TileID.Anvils),
               extra: new ExtraInfo(SeedDropChance: (int i, int j) => 0f));

            AddPlant("Palladium",
               () => ItemID.PalladiumOre, 16,
               Tags.Hardmode | Tags.MundaneOre,
               description: $"Crafted at [i:{ItemID.LeadAnvil}]\nDoes not drop seeds",
               recipe: new SeedRecipe(TileID.Anvils),
               extra: new ExtraInfo(SeedDropChance: (int i, int j) => 0f));

            AddPlant("Mythril",
               () => ItemID.MythrilOre, 24,
               Tags.Hardmode | Tags.MundaneOre,
               description: $"Crafted at [i:{ItemID.MythrilAnvil}]\nDoes not drop seeds",
               recipe: new SeedRecipe(TileID.MythrilAnvil,
               BaseSeedGroup: ("OreSeeds:CobaltSeeds", 2)),
               extra: new ExtraInfo(SeedDropChance: (int i, int j) => 0f));

            AddPlant("Orichalcum",
               () => ItemID.OrichalcumOre, 24,
               Tags.Hardmode | Tags.MundaneOre,
               description: $"Crafted at [i:{ItemID.OrichalcumAnvil}]\nDoes not drop seeds",
               recipe: new SeedRecipe(TileID.MythrilAnvil,
               BaseSeedGroup: ("OreSeeds:CobaltSeeds", 2)),
               extra: new ExtraInfo(SeedDropChance: (int i, int j) => 0f));

            AddPlant("Adamantite",
               () => ItemID.AdamantiteOre, 32,
               Tags.Hardmode | Tags.MundaneOre,
               description: $"Crafted at [i:{ItemID.AdamantiteForge}]\nDoes not drop seeds",
               recipe: new SeedRecipe(TileID.AdamantiteForge,
               BaseSeedGroup: ("OreSeeds:MythrilSeeds", 2)),
               extra: new ExtraInfo(SeedDropChance: (int i, int j) => 0f));

            AddPlant("Titanium",
               () => ItemID.TitaniumOre, 32,
               Tags.Hardmode | Tags.MundaneOre,
               description: $"Crafted at [i:{ItemID.TitaniumForge}]\nDoes not drop seeds",
               recipe: new SeedRecipe(TileID.AdamantiteForge,
               BaseSeedGroup: ("OreSeeds:MythrilSeeds", 2)),
               extra: new ExtraInfo(SeedDropChance: (int i, int j) => 0f));

            AddPlant("Chlorophyte",
               () => ItemID.ChlorophyteOre, 28,
               Tags.Hardmode | Tags.Jungle,
               (2, 5),
               extra: new ExtraInfo(SeedDropChance: (int i, int j) => 0.75f));//default is 0.5

            AddPlant("Hallowed",
               () => ItemID.HallowedBar, 16,
               Tags.Hardmode | Tags.Hallowed,
               (1, 2),
               description: $"Crafted at [i:{ItemID.TitaniumForge}]\nDoes not drop seeds",
               recipe: new SeedRecipe(TileID.AdamantiteForge, null,
               ("OreSeeds:AdamantiteSeeds", 1), 
               new (Func<int>, int count)[]
               {
                   (() => ItemID.SoulofMight, 3),
                   (() => ItemID.SoulofSight, 3),
                   (() => ItemID.SoulofFright, 3)
               }, 
               new (string group, int count)[] {
                   ("OreSeeds:MythrilSeeds", 1),
                   ("OreSeeds:CobaltSeeds", 1)
               }),
               extra: new ExtraInfo(SeedDropChance: (int i, int j) => 0f));

            AddPlant("Luminite",
               () => ItemID.LunarOre, 35,
               Tags.PostMoonlord,
               description: $"Crafted at [i:{ItemID.LunarCraftingStation}]\nRarely drops seeds",
               recipe: new SeedRecipe(TileID.LunarCraftingStation,
               ExtraCraftItems: new (Func<int>, int count)[]
               {
                   (() => ItemID.FragmentSolar, 1),
                   (() => ItemID.FragmentNebula, 1),
                   (() => ItemID.FragmentVortex, 1),
                   (() => ItemID.FragmentStardust, 1),
               }),
               extra: new ExtraInfo(
                   SeedDropChance: (int i, int j) => 0.01f));

            AddPlantRange(new (string, Func<int>, int)[]{
                ("Amethyst", () => ItemID.Amethyst, 12),
                ("Topaz", () => ItemID.Topaz, 12),
                ("Sapphire", () => ItemID.Sapphire, 16),
                ("Emerald", () => ItemID.Emerald, 16),
                ("Ruby", () => ItemID.Ruby, 20),
                ("Diamond", () => ItemID.Diamond, 20)}, 
                Tags.PreHardmode | Tags.Gem);

            AddPlant("Amber",
               () => ItemID.Amber, 15,
               Tags.PreHardmode | Tags.Gem | Tags.Desert);
                
            AddPlant("Fossil",
               () => ItemID.FossilOre, 25,
               Tags.PreHardmode | Tags.Desert);
            #endregion
            
            //todo pillar fragments
            #region non-ores

            AddPlant("Crystal",
               () => ItemID.CrystalShard, 15,
               Tags.Hardmode | Tags.NonOre | Tags.Hallowed,
               (2, 5));

            AddPlant("Cursed Flame",
               () => ItemID.CursedFlame, 15,
               Tags.Hardmode | Tags.NonOre | Tags.Evil | Tags.MobDrop,
               (2, 5));

            AddPlant("Star",
               () => ItemID.FallenStar, 25,
               Tags.PreHardmode | Tags.NonOre | Tags.Night,
               (1, 4),
               extra: new ExtraInfo(
                   GrowthChance: (int i, int j) => Main.dayTime ? 0.1f : 1f,
                   ShowHarvestIcon: (int i, int j) => !Main.dayTime));

            AddPlant("Bone",
               () => ItemID.Bone, 40,
               Tags.PreHardmode | Tags.NonOre | Tags.MobDrop);

            AddPlant("Soul of Light",
               () => ItemID.SoulofLight, 16,
               Tags.Hardmode | Tags.NonOre | Tags.MobDrop | Tags.Hallowed,
               (1, 2),
               extra: new ExtraInfo(
                   SeedDropChance: (int i, int j) => 0.33f));

            AddPlant("Soul of Night",
               () => ItemID.SoulofNight, 16,
               Tags.Hardmode | Tags.NonOre | Tags.MobDrop | Tags.Evil,
               (1, 2),
               extra: new ExtraInfo(
                   SeedDropChance: (int i, int j) => 0.33f));

            AddPlant("Soul of Flight",
               () => ItemID.SoulofFlight, 14,
               Tags.Hardmode | Tags.NonOre | Tags.MobDrop,
               (1, 2));

            AddPlant("Soul of Might",
               () => ItemID.SoulofMight, 12,
               Tags.Hardmode | Tags.NonOre | Tags.BossDrop,
               (1, 1),
               description: "Low seed rate",
               recipe: new SeedRecipe(
                   ExtraCraftItems: new (Func<int>, int count)[] { (() => ItemID.HallowedBar, 4) }),
               extra: new ExtraInfo(
                   SeedDropChance: (int i, int j) => 0.15f));

            AddPlant("Soul of Sight",
               () => ItemID.SoulofSight, 12,
               Tags.Hardmode | Tags.NonOre | Tags.BossDrop,
               (1, 1),
               description: "Low seed rate",
               recipe: new SeedRecipe(
                   ExtraCraftItems: new (Func<int>, int count)[] { (() => ItemID.HallowedBar, 4) }),
               extra: new ExtraInfo(
                   SeedDropChance: (int i, int j) => 0.15f));

            AddPlant("Soul of Fright",
               () => ItemID.SoulofFright, 12,
               Tags.Hardmode | Tags.NonOre | Tags.BossDrop,
               (1, 1),
               description: "Low seed rate",
               recipe: new SeedRecipe(
                   ExtraCraftItems: new (Func<int>, int count)[] { (() => ItemID.HallowedBar, 4) }),
               extra: new ExtraInfo(
                   SeedDropChance: (int i, int j) => 0.15f));
            #endregion

            #region modded

            //ores out of date
            #region thorium
            if (ModLoader.TryGetMod("ThoriumMod", out Mod thoriumMod))
            {
                AddPlant("Thorium",
                () => thoriumMod.Find<ModItem>("ThoriumOre").Type, 20,
                Tags.PreHardmode | Tags.Modded);

                AddPlantRange(new (string, Func<int>, int)[]{
                ("Smooth Coal", () => thoriumMod.Find<ModItem>("SmoothCoal").Type,  16),
                ("Opal", () => thoriumMod.Find<ModItem>("Opal").Type,               20),
                ("Life Quartz", () => thoriumMod.Find<ModItem>("LifeQuartz").Type,  24)},
                Tags.PreHardmode | Tags.Modded | Tags.Gem);

                AddPlantRange(new (string, Func<int>, int)[]{
                ("Pearl", () => thoriumMod.Find<ModItem>("Pearl").Type,             24),
                ("Aquaite", () => thoriumMod.Find<ModItem>("Aquaite").Type,         20)},
                Tags.PreHardmode | Tags.Water | Tags.Modded | Tags.Gem);

                AddPlant("Magma",
                () => thoriumMod.Find<ModItem>("MagmaOre").Type, 20,
                Tags.PreHardmode | Tags.Modded | Tags.Hardmode | Tags.Hell);//unknown if correct

                AddPlant("Lodestone",
                () => thoriumMod.Find<ModItem>("LodeStoneChunk").Type, 25,
                Tags.PreHardmode | Tags.Modded | Tags.Hardmode);

                AddPlant("Valadium",
                () => thoriumMod.Find<ModItem>("ValadiumChunk").Type, 25,
                Tags.PreHardmode | Tags.Modded | Tags.Hardmode);

                AddPlant("Illumite",
                () => thoriumMod.Find<ModItem>("IllumiteChunk").Type, 28,
                Tags.PreHardmode | Tags.Modded | Tags.Hardmode | Tags.Hallowed);//unknown if correct
            }
            #endregion

            //ores out of date
            #region calamity
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamityMod))
            {
                AddPlant("Aerialite",
                () => calamityMod.Find<ModItem>("AerialiteOre").Type, 16,
                Tags.PreHardmode | Tags.Modded);

                AddPlant("Cryonic",
                () => calamityMod.Find<ModItem>("CryonicOre").Type, 20,
                Tags.PreHardmode | Tags.Modded | Tags.Ice);//unknown if correct

                AddPlant("Perennial",
                () => calamityMod.Find<ModItem>("PerennialOre").Type, 25,
                Tags.PreHardmode | Tags.Modded | Tags.Jungle);//unknown if correct

                AddPlant("Charred",
                () => calamityMod.Find<ModItem>("CharredOre").Type, 28,
                Tags.Hardmode | Tags.Modded | Tags.Hell);//unknown if correct

                AddPlant("Astral",
                () => calamityMod.Find<ModItem>("AstralOre").Type, 30,
                Tags.Hardmode | Tags.Modded);//unknown if correct

                AddPlant("Chaotic",
                () => calamityMod.Find<ModItem>("ChaoticOre").Type, 32,
                Tags.PostMoonlord | Tags.Modded | Tags.Hell);//unknown if correct

                AddPlant("Uelibloom",
                () => calamityMod.Find<ModItem>("UelibloomOre").Type, 35,
                Tags.PostMoonlord | Tags.Modded | Tags.Jungle);//unknown if correct

                AddPlant("Chaos Essence",
                () => calamityMod.Find<ModItem>("EssenceofChaos").Type, 30,
                Tags.PostMoonlord | Tags.Modded | Tags.NonOre | Tags.MobDrop);//unknown if correct

                AddPlant("Eleum Essence",
                () => calamityMod.Find<ModItem>("EssenceofEleum").Type, 30,
                Tags.PostMoonlord | Tags.Modded | Tags.NonOre | Tags.MobDrop);//unknown if correct

                AddPlant("Sunlight Essence",
                () => calamityMod.Find<ModItem>("EssenceofCinder").Type, 30,
                Tags.PostMoonlord | Tags.Modded | Tags.NonOre | Tags.MobDrop);//unknown if correct
            }
            #endregion

            //this mod is no longer active and will likely not be updated
            #region btfa
            if (ModLoader.TryGetMod("ForgottenMemories", out Mod btfa))//unsure if this mod even exists anymore
            {
                AddPlant("Blight",
                () => btfa.Find<ModItem>("BlightOreItem").Type, 25,
                Tags.Hardmode | Tags.Modded);//unknown if correct

                AddPlant("Cosmodium",
                () => btfa.Find<ModItem>("CosmodiumOre").Type, 30,
                Tags.Hardmode | Tags.Modded);//unknown if correct

                AddPlant("Cryotine",
                () => btfa.Find<ModItem>("CryotineOreItem").Type, 30,
                Tags.Hardmode | Tags.Modded | Tags.Ice);//unknown if correct

                AddPlant("Gelatine",
                () => btfa.Find<ModItem>("GelatineOreItem").Type, 25,
                Tags.Hardmode | Tags.Modded);//unknown if correct

                //ToFix: Sprite is incorrect on this one, not sure if it its seed or plant
                AddPlant("Cosmorock",
                () => btfa.Find<ModItem>("SpaceRockFragment").Type, 25,
                Tags.Hardmode | Tags.Modded);//unknown if correct
            }
            #endregion

            //ores out of date and mod has been renamed
            #region sacred tools
            //old name
            if (ModLoader.TryGetMod("SacredTools", out Mod sacredTools))
            {
                AddPlant("Cernium",
                () => sacredTools.Find<ModItem>("CerniumItem").Type, 20,
                Tags.PreHardmode | Tags.Modded);//unknown if correct

                AddPlant("Flarium",
                () => sacredTools.Find<ModItem>("FlariumItem").Type, 25,
                Tags.Hardmode | Tags.Modded);//unknown if correct

                AddPlant("Oblivion",
                () => sacredTools.Find<ModItem>("OblivionOre").Type, 25,
                Tags.Hardmode | Tags.Modded);//unknown if correct

                AddPlant("Lapis",
                () => sacredTools.Find<ModItem>("RawLapis").Type, 20,
                Tags.Hardmode | Tags.Modded);//unknown if correct
            }
            #endregion

            //ores are out of date
            #region spirit
            if (ModLoader.TryGetMod("SpiritMod", out Mod spiritMod))
            {
                AddPlant("Enchanted Granite",
                () => spiritMod.Find<ModItem>("GraniteChunk").Type, 16,
                Tags.PreHardmode | Tags.Modded);

                AddPlant("Ancient Marble",
                () => spiritMod.Find<ModItem>("MarbleChunk").Type, 16,
                Tags.PreHardmode | Tags.Modded);

                AddPlant("Cryolite",
                () => spiritMod.Find<ModItem>("CryoliteOre").Type, 20,
                Tags.PreHardmode | Tags.Modded | Tags.Ice);

                AddPlant("Spirit",
                () => spiritMod.Find<ModItem>("SpiritOre").Type, 24,
                Tags.PreHardmode | Tags.Modded);//unknown if correct

                AddPlant("Star Piece",
                () => spiritMod.Find<ModItem>("StarPiece").Type, 20,
                Tags.Hardmode | Tags.Modded);//unknown if correct

                AddPlant("Thermite",
                () => spiritMod.Find<ModItem>("ThermiteOre").Type, 24,
                Tags.Hardmode | Tags.Modded);//unknown if correct

                AddPlant("Floran",
                () => spiritMod.Find<ModItem>("FloranOre").Type, 24,
                Tags.Hardmode | Tags.Modded);//unknown if correct
            }
            #endregion

            //this mod is no longer active
            #region blue magic
            if (ModLoader.TryGetMod("Bluemagic", out Mod blueMagic))//unsure if this mod even exists anymore
            {
                AddPlant("Purium",
                () => blueMagic.Find<ModItem>("PuriumOre").Type, 24,
                Tags.Hardmode | Tags.Modded);
            }
            #endregion

            #region EEMod
            if (ModLoader.TryGetMod("EEMod", out Mod eeMod))//unsure if this mod even exists anymore
            {
                AddPlant("Dalantinium",
                () => eeMod.Find<ModItem>("DalantiniumOre").Type, 24,
                Tags.PreHardmode | Tags.Water | Tags.Modded);

                AddPlant("Lythen",
                () => eeMod.Find<ModItem>("LythenOre").Type, 24,
                Tags.PreHardmode | Tags.Water | Tags.Modded);

                AddPlant("Aquamarine",
                () => eeMod.Find<ModItem>("Aquamarine").Type, 20,
                Tags.PreHardmode | Tags.Water | Tags.Gem | Tags.Modded);
            }
            #endregion

            #region Starlight River
            if (ModLoader.TryGetMod("StarlightRiver", out Mod slr))//unsure if this mod even exists anymore
            {
                AddPlant("Palestone",
                () => slr.Find<ModItem>("PalestoneItem").Type, 20,
                Tags.PreHardmode | Tags.MundaneOre | Tags.Modded);

                //AddPlant("Ebony",
                //() => slr.Find<ModItem>("OreEbony").Type, 25,
                //Tags.PreHardmode | Tags.MundaneOre | Tags.Modded);

                //AddPlant("Ivory",
                //() => slr.Find<ModItem>("OreIvory").Type, 25,
                //Tags.PreHardmode | Tags.Modded);

                AddPlant("Moonstone",
                () => slr.Find<ModItem>("MoonstoneOreItem").Type, 30,
                Tags.PreHardmode | Tags.Modded);

                AddPlant("Vitric",
                () => slr.Find<ModItem>("VitricOre").Type, 22,
                Tags.PreHardmode | Tags.Desert | Tags.Modded);

                AddPlant("Astroscrap",
                () => slr.Find<ModItem>("Astroscrap").Type, 24,
                Tags.PreHardmode | Tags.MobDrop | Tags.NonOre | Tags.Modded);

                AddPlant("Salt",
                () => slr.Find<ModItem>("TableSalt").Type, 16,
                Tags.PreHardmode | Tags.NonOre | Tags.Modded);

                AddPlant("Pepper",
                () => slr.Find<ModItem>("BlackPepper").Type, 16,
                Tags.PreHardmode | Tags.NonOre | Tags.Modded);

                AddPlant("Sea Salt",
                () => slr.Find<ModItem>("SeaSalt").Type, 20,
                Tags.PreHardmode | Tags.Water | Tags.NonOre | Tags.Modded);
            }
            #endregion

            //consolarria


            #endregion
        }

        public static void Unload()
        {
            SeedRecipeGroup = null;

            CobaltSeedGroup = null;
            MythrilSeedGroup = null;
            AdamantiteSeedGroup = null;
        }

        #region add plant methods
        private static void AddPlant(string oreName,
            Func<int> oreItem,
            int oreCraftAmount,
            Tags tags = 0,
            (int, int)? oreDropRange = null,
            string description = null,
            SeedRecipe recipe = null,
            TypeInfo info = null,
            ExtraInfo extra = null)
        {
            (int, int) thisDropRange = oreDropRange ?? (1, 3);
            SeedRecipe thisRecipe = recipe ?? new SeedRecipe();
            TypeInfo thisInfo = info ?? new TypeInfo();
            ExtraInfo thisExtra = extra ?? new ExtraInfo();
            thisInfo.SetName(oreName);

            Mod.AddContent(new BasePlantItem(
                oreItem, oreCraftAmount, 
                tags,
                thisDropRange,
                description,
                thisRecipe,
                thisInfo));
            Mod.AddContent(new BasePlantTile(
                oreItem,
                thisDropRange,
                tags,
                thisInfo,
                thisExtra));
        }

        private static void AddPlantRange((string name, Func<int> item)[] ores, 
            int oreCraftAmount,
            Tags tags = 0,
            (int, int)? oreDropRange = null,
            string description = null,
            SeedRecipe recipe = null,
            TypeInfo info = null,
            ExtraInfo extra = null)
        {
            foreach ((string name, Func<int> item) in ores)
                AddPlant(name, item, oreCraftAmount, tags, oreDropRange, description, recipe, info, extra);
        }

        private static void AddPlantRange((string name, Func<int> item, int count)[] ores, 
            Tags tags = 0,
            (int, int)? oreDropRange = null,
            string description = null,
            SeedRecipe recipe = null,
            TypeInfo info = null)
        {
            foreach ((string name, Func<int> item, int count) in ores)
                AddPlant(name, item, count, tags, oreDropRange, description, recipe, info);
        }
        #endregion
    }
}
