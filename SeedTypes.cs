using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static OreSeeds.SeedLoader;

namespace OreSeeds
{
    [Flags]
    public enum Tags
    {
        None = 0,
        MundaneOre  =   1 << 0,
        Gem =           1 << 1,
        Evil =          1 << 2,
        Hallowed =      1 << 3,
        Jungle =        1 << 4,
        Desert =        1 << 5,
        Ice =           1 << 6,
        Hell =          1 << 7,
        Water =         1 << 8,
        PreHardmode =   1 << 9,
        Hardmode =      1 << 10,
        PostMoonlord =  1 << 11,
        Modded =        1 << 12,
        NonOre =        1 << 13,
        MobDrop =       1 << 14,
        BossDrop =      1 << 15,
        Night =         1 << 16,
        Day =           1 << 17
    }


    #region info classes
    //todo: possible name/function change to reflect functionallity of these classes...
    //cont: eg: tile frames / tile height in the TileOnly class, recipes in the item only class, shared into in the shared class

    //item only
    public class SeedRecipe
    {
        public readonly int CraftingTileID;
        public readonly (Func<int> item, int count)? BaseSeedItem;
        public readonly (string group, int count)? BaseSeedGroup;
        public readonly (Func<int> item, int count)[] ExtraCraftItems;
        public readonly (string group, int count)[] ExtraCraftGroups;

        public SeedRecipe(int CraftingTileID = TileID.Bottles,
            (Func<int>, int count)? BaseSeedItem = null,
            (string group, int count)? BaseSeedGroup = null,
            (Func<int>, int count)[] ExtraCraftItems = null,
            (string group, int count)[] ExtraCraftGroups = null)
        {
            this.CraftingTileID = CraftingTileID;
            this.BaseSeedItem = BaseSeedItem;
            this.BaseSeedGroup = BaseSeedGroup;
            this.ExtraCraftItems = ExtraCraftItems;
            this.ExtraCraftGroups = ExtraCraftGroups;
        }
    }

    //shared
    public class TypeInfo
    {
        public Color MapColor;
        public string TileMapName;
        public string ItemName;
        public string ItemInternalName;
        public string TileInternalName;
        public TypeInfo(Color? MapColor = null,
            string TileMapName = null,
            string ItemName = null,
            string ItemInternalName = null,
            string TileInternalName = null)
        {
            this.MapColor = MapColor ?? Color.ForestGreen;
            this.TileMapName = TileMapName;
            this.ItemName = ItemName;
            this.ItemInternalName = ItemInternalName;
            this.TileInternalName = TileInternalName;
        }

        public void SetName(string OreName)
        {
            TileMapName ??= (OreName + " plant");
            ItemName ??= (OreName + " seeds");
            ItemInternalName ??= (OreName + "Seeds").Replace(" ", "");
            TileInternalName ??= (OreName + "Plant").Replace(" ", "");
        }
    }

    //shared between both, only the seeds only need it for mod compat
    public class ExtraInfo
    {
        public readonly Func<int, int, float> SeedDropChance;
        public readonly Func<int, int, float> GrowthChance;
        public readonly Func<int, int, bool> ShowHarvestIcon;
        public readonly Func<int, int, Tags, float> TagGrowthModifier;
        public readonly int FrameCount;
        public ExtraInfo(int FrameCount = 3, Func<int, int, float> SeedDropChance = null, Func<int, int, float> GrowthChance = null, Func<int, int, bool> ShowHarvestIcon = null, Func<int, int, Tags, float> TagGrowthModifier = null)
        {
            this.SeedDropChance = SeedDropChance ?? ((int i, int j) => 0.5f);
            this.GrowthChance = GrowthChance ?? ((int i, int j) => 1f);
            this.ShowHarvestIcon = ShowHarvestIcon ?? ((int i, int j) => true);
            this.TagGrowthModifier = TagGrowthModifier ?? BasePlantTile.TagGrowthModifier;
            this.FrameCount = FrameCount;
        }
    }
    #endregion

    [Autoload(false)]
    public class BasePlantItem : ModItem
    {
        public readonly Func<int> OreItem;//todo: use value for a seed recylcler
        private readonly int OreAmount;//crafting amount
        public readonly (int min, int max) OreDropRange;//drop amount
        private readonly string Description;
        public readonly Tags Tags;
        private readonly SeedRecipe RecipeInfo;
        private readonly TypeInfo TypeInfo;
        private readonly ExtraInfo ExtraInfo;
        //todo cache every seed in a list

        public BasePlantItem(Func<int> oreItem, int oreAmount, Tags tags, (int, int) oreDropRange, string description, SeedRecipe recipeInfo, TypeInfo typeInfo, ExtraInfo extraInfo)
        {
            OreItem = oreItem;
            OreAmount = oreAmount;
            Tags = tags;
            OreDropRange = oreDropRange;
            Description = description;
            RecipeInfo = recipeInfo;
            TypeInfo = typeInfo;
            ExtraInfo = extraInfo;
        }

        protected override bool CloneNewInstances => true;
        public override string Name => TypeInfo.ItemInternalName;
        public override string Texture => Mod.Name + "/Items/Seeds/" + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault(TypeInfo.ItemName ?? "Unnamed");
            string tooltip = Description;
            if (Tags.HasFlag(Tags.Water))
                tooltip += "\nMust be grown in water";
            if (Tags.HasFlag(Tags.Night))
                    tooltip += "\nGrows best at night";
            if (Tags.HasFlag(Tags.Day))
                tooltip += "\nGrows best during the day";
            if (Tags.HasFlag(Tags.Hell))
                tooltip += "\nWill only grow in or near hell";

            Tooltip.SetDefault(tooltip);
        }

        public override void SetDefaults()
        {
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.maxStack = 99;
            Item.consumable = true;
            Item.placeStyle = 0;
            Item.width = 12;
            Item.height = 14;
            //todo find the source item price
            Item.value = Item.sellPrice(0,

                (Tags.HasFlag(Tags.PostMoonlord) ? 2 : 0),

                Math.Max(
                (Tags.HasFlag(Tags.Jungle) ? 5 : 0) +
                (Tags.HasFlag(Tags.Evil) ? 10 : 0) +
                (Tags.HasFlag(Tags.Hallowed) ? 20 : 0) +
                (Tags.HasFlag(Tags.MundaneOre) ? 2 : 0) +
                (Tags.HasFlag(Tags.Hardmode) ? 75 : 0) -
                (Tags.HasFlag(Tags.NonOre) ? 10 : 0),
                (Tags.HasFlag(Tags.PostMoonlord) ? 0 : 1)),
                0);
            Item.createTile = Mod.Find<ModTile>(TypeInfo.TileInternalName).Type;
        }

        public static Rectangle GetFrame(int tileID)
        {
            if(tileID == 78)
                return new Rectangle(0, 0, 16, 16);
            if (tileID == 380)
                return new Rectangle(54, 0, 16, 16);
            else
                return new Rectangle(162, 54, 16, 16);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Tags.HasFlag(Tags.Hardmode) && !Main.hardMode)
            {
                var line = new TooltipLine(Mod, "HardmodeWarning", "Will only grow in hardmode!")
                {
                    OverrideColor = new Color(255, 150, 150)
                };
                tooltips.Add(line);
            }
            else if(Tags.HasFlag(Tags.PostMoonlord) && !NPC.downedMoonlord)
            {
                var line = new TooltipLine(Mod, "MoonlordWarning", "Will only grow once moonlord has been defeated!")
                {
                    OverrideColor = new Color(255, 150, 150)
                };
                tooltips.Add(line);
            }

            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                var line = new TooltipLine(Mod, "Tags", Tags.ToString())
                {
                    OverrideColor = new Color(137, 154, 203)
                };
                tooltips.Add(line);//tooltip += "\n[c/576899:" + Tags.ToString() + "]";

            }
            else if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
            {
                TooltipLine line;
                //if (cachedDescription == null)
                //{
                //    StringBuilder sb = new StringBuilder();
                //    for (int i = 0; i < 5; i++)
                //        sb.Append("[i:" + SeedLoader.ValidTiles[Item.createTile][i] + "]");
                //    cachedDescription = sb.ToString();
                //}

                for (int i = 0; i < SeedLoader.ValidTiles[Item.createTile].Length; i++)
                {
                    Main.instance.LoadTiles(SeedLoader.ValidTiles[Item.createTile][i]);
                }

                line = new TooltipLine(Mod, "ValidTiles", new string('M', SeedLoader.ValidTiles[Item.createTile].Length));
                tooltips.Add(line);

            }
            else
                tooltips.Add(new TooltipLine(Mod, "ItemInfo", "[c/858585:{Hold ]shift[c/858585: for tags or ]ctrl[c/858585: for valid tiles}]"));
        }

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if(line.Name == "ValidTiles")
            {
                for (int i = 0; i < SeedLoader.ValidTiles[Item.createTile].Length; i++)
                {
                    int tileID = SeedLoader.ValidTiles[Item.createTile][i];
                    Main.spriteBatch.Draw(TextureAssets.Tile[tileID].Value, new Vector2(line.X + (i * 16), line.Y), GetFrame(tileID),  Color.White);
                }
                return false;
            }
            return true;
        }

        public override void AddRecipes()
        {
            #region mod compat
            ModLoader.TryGetMod("miningcracks_take_on_luiafk", out var result);
            Mod LuiMod = result;
            if (LuiMod != null)
            {
                Func<int> func = () => { return Main.rand.Next(OreDropRange.min, OreDropRange.max + 1); };
                LuiMod.Call("plantharvest", Mod.Find<ModTile>(TypeInfo.TileInternalName).Type, 18 * 2, OreItem(), func);
            }
            #endregion

            Recipe recipe = CreateRecipe();

            bool useDefaultSeed = true;
            if (RecipeInfo.BaseSeedItem != null)
            {
                recipe.AddIngredient(RecipeInfo.BaseSeedItem.Value.item(), RecipeInfo.BaseSeedItem.Value.count);
                useDefaultSeed = false;
            }
            if (RecipeInfo.BaseSeedGroup != null)
            {
                recipe.AddRecipeGroup(RecipeInfo.BaseSeedGroup.Value.group, RecipeInfo.BaseSeedGroup.Value.count);
                useDefaultSeed = false;
            }

            if (useDefaultSeed)
                recipe.AddRecipeGroup("OreSeeds:AlchSeeds", 1);

            if (RecipeInfo.ExtraCraftGroups != null && RecipeInfo.ExtraCraftGroups.Length > 0)
            {
                foreach ((string group, int count) in RecipeInfo.ExtraCraftGroups)
                    recipe.AddRecipeGroup(group, count);
            }
            if (RecipeInfo.ExtraCraftItems != null && RecipeInfo.ExtraCraftItems.Length > 0)
            {
                foreach ((Func<int> item, int count) in RecipeInfo.ExtraCraftItems)
                    recipe.AddIngredient(item(), count);
            }

            recipe.AddIngredient(OreItem(), OreAmount);
            recipe.AddTile(RecipeInfo.CraftingTileID);
            recipe.Register();
        }
    }

    [Autoload(false)]
    public class BasePlantTile : ModTile
    {
        private readonly Func<int> OreItem;
        private readonly (int min, int max) OreAmount;//drop amount
        public readonly Tags Tags;
        private readonly TypeInfo TypeInfo;
        private readonly ExtraInfo ExtraInfo;
        public BasePlantTile(Func<int> oreItem, (int, int) oreDropRange, Tags tags, TypeInfo typeInfo, ExtraInfo extraInfo)
        {
            OreItem = oreItem;
            OreAmount = oreDropRange;
            Tags = tags;
            TypeInfo = typeInfo;
            ExtraInfo = extraInfo;
        }
        //TODO
        //location could be changed
        public static float TagGrowthModifier(int i, int j, Tags tags) 
        {
            float chance = 1;
            //if(AreaGrowthRules) (here or when growth rules is called)
            if (tags.HasFlag(Tags.Water) && !(Main.tile[i, j].LiquidType == LiquidID.Water && Main.tile[i, j].LiquidAmount > 20))
                return 0.1f;
            if (tags.HasFlag(Tags.Hell) && j < Main.maxTilesY - 400)
                return 0.1f;

            if ((tags.HasFlag(Tags.Night) && Main.dayTime) || (tags.HasFlag(Tags.Day) && !Main.dayTime))
                chance *= 0.1f;

            if (tags.HasFlag(Tags.PostMoonlord))
                chance *= NPC.downedMoonlord ? 0.7f : 0.075f;
            else if (tags.HasFlag(Tags.Hardmode))
                chance *= Main.hardMode ? 0.85f : 0.15f;

            return chance;
        }

        public override string Name => TypeInfo.TileInternalName;
        public override string Texture => Mod.Name + "/Tiles/Plants/" + Name;
        public bool IsLastFrame(int i, int j)
        {
            int stage = Main.tile[i, j].TileFrameX / 18;
            return stage == ExtraInfo.FrameCount - 1;
        }

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileSpelunker[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.AlternateTile, 1, 0);
            TileObjectData.newTile.DrawYOffset = -4;
            Main.tileLavaDeath[Type] = true;

            //these are either SolidWithTop or NonSolid, so they need a special case to be placed on.
            //They are still included in the normal valid tile list so that they appear in the tooltip.
            TileObjectData.newTile.AnchorAlternateTiles = new int[] { TileID.ClayPot, TileID.PlanterBox };

            List<int> validTiles = new List<int>
            {
                TileID.Dirt,
                TileID.Grass,
                TileID.GolfGrass,
                TileID.PlanterBox,
                TileID.ClayPot
            };

            #region special cases
            //A bit yikes
            if (Tags.HasFlag(Tags.Jungle))
            {
                validTiles.Add(TileID.JungleGrass);
                validTiles.Add(TileID.Mud);
            }
            if (Tags.HasFlag(Tags.MundaneOre) || Tags.HasFlag(Tags.Gem))
                validTiles.Add(TileID.Stone);
            if (Tags.HasFlag(Tags.Evil))
            {
                validTiles.Add(TileID.CorruptGrass);
                validTiles.Add(TileID.Ebonstone);
                validTiles.Add(TileID.CrimsonGrass);
                validTiles.Add(TileID.Crimstone);
            }
            if (Tags.HasFlag(Tags.Hallowed))
            {
                validTiles.Add(TileID.GolfGrassHallowed);
                validTiles.Add(TileID.HallowedGrass);
                validTiles.Add(TileID.Pearlstone);
            }
            if (Tags.HasFlag(Tags.Desert))
            {
                validTiles.Add(TileID.Sand);
                validTiles.Add(TileID.Sandstone);
                validTiles.Add(TileID.Ebonsand);
                validTiles.Add(TileID.Crimsand);
                validTiles.Add(TileID.Pearlsand);

                validTiles.Add(TileID.HardenedSand);
                validTiles.Add(TileID.CorruptSandstone);
                validTiles.Add(TileID.CrimsonSandstone);
                validTiles.Add(TileID.HallowSandstone);
                validTiles.Add(TileID.CorruptHardenedSand);
                validTiles.Add(TileID.CrimsonHardenedSand);
                validTiles.Add(TileID.HallowHardenedSand);
            }
            if (Tags.HasFlag(Tags.Ice))
            {
                validTiles.Add(TileID.IceBlock);
                validTiles.Add(TileID.BreakableIce);
                validTiles.Add(TileID.SnowBlock);
                validTiles.Add(TileID.FleshIce);
                validTiles.Add(TileID.CorruptIce);
                validTiles.Add(TileID.HallowedIce);
            }
            if (Tags.HasFlag(Tags.PostMoonlord))
            {
                validTiles.Add(TileID.LunarOre);
                validTiles.Add(TileID.LunarBlockStardust);
                validTiles.Add(TileID.LunarBlockVortex);
                validTiles.Add(TileID.LunarBlockSolar);
                validTiles.Add(TileID.LunarBlockNebula);
            }
            if (Tags.HasFlag(Tags.Hell))
            {
                validTiles.Add(TileID.Ash);
                Main.tileLavaDeath[Type] = false;
                Main.tileWaterDeath[Type] = true;
            }
            if (Tags.HasFlag(Tags.Water))
            {
                TileObjectData.newTile.WaterPlacement = LiquidPlacement.OnlyInLiquid;
                validTiles.Add(TileID.Sand);
                validTiles.Add(TileID.Ebonsand);
                validTiles.Add(TileID.Crimsand);
                validTiles.Add(TileID.Pearlsand);
                validTiles.Add(TileID.Coralstone);
            }
            #endregion

            int[] validTileArray = validTiles.ToArray();
            TileObjectData.newTile.AnchorValidTiles = validTileArray;
            SeedLoader.ValidTiles.Add(Type, validTileArray);
            TileObjectData.addTile(Type);

            ModTranslation name = CreateMapEntryName();
            name.SetDefault(TypeInfo.TileMapName);
            AddMapEntry(TypeInfo.MapColor, name);
        }

        public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
        {
            if (i % 2 == 1)
                spriteEffects = SpriteEffects.FlipHorizontally;
        }

        public override bool Drop(int i, int j)
        {
            if (IsLastFrame(i, j))
            {
                DropSeed(i, j, true);
                DropOre(i, j, Main.rand.Next(OreAmount.min, OreAmount.max + 1));
            }
            else
            {
                DropSeed(i, j, false);
            }
            return false;
        }

        public override bool RightClick(int i, int j)
        {
            if (IsLastFrame(i, j))
            {
                DropSeed(i, j, true, false);
                DropOre(i, j, Main.rand.Next(OreAmount.min, OreAmount.max + 1), true);
                Main.tile[i, j].TileFrameX = 0;
                WorldGen.SquareTileFrame(i, j, true);
                NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
                return true;
            }
            else
                return base.RightClick(i, j);
        }

        public override void RandomUpdate(int i, int j)
        {
            float chance = ExtraInfo.GrowthChance(i, j);
            //if(AreaGrowthRules) (here or inside the tag growth method)
            chance *= ExtraInfo.TagGrowthModifier(i, j, Tags);

            if (!IsLastFrame(i, j) && Main.rand.NextFloat(0.000001f, 0.999999f) < chance)
            {
                Main.tile[i, j].TileFrameX += 18; 
                WorldGen.SquareTileFrame(i, j, true);
                NetMessage.SendTileSquare(-1, i, j, 1, TileChangeType.None);
            }
        }

        public void DropSeed(int i, int j, bool grown, bool broken = true)
        {
            int count = broken ? 1 : 0;
            if (grown && Main.rand.NextFloat(0.000001f, 0.999999f) < ExtraInfo.SeedDropChance(i, j))
                count++;
            if (count != 0)
            {
                int itemIndex = Item.NewItem(broken ? new Terraria.DataStructures.EntitySource_TileBreak(i, j, "OrePlantBreak") :
                    new Terraria.DataStructures.EntitySource_TileInteraction(Main.LocalPlayer, i, j, "OrePlantHarvest"),
                    i * 16, j * 16, 0, 0, Mod.Find<ModItem>(TypeInfo.ItemInternalName).Type, count);

                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.SyncItem, number: itemIndex, number2: 1f);
            }
        }

        public void DropOre(int i, int j, int count, bool rightClick = false)
        {
            int itemIndex = Item.NewItem(rightClick ? new Terraria.DataStructures.EntitySource_TileInteraction(Main.LocalPlayer, i, j, "OrePlantHarvest") :
                new Terraria.DataStructures.EntitySource_TileBreak(i, j, "OrePlantBreak"),
                i * 16, j * 16, 0, 0, OreItem.Invoke(), count);

            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SyncItem, number: itemIndex, number2: 1f);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (IsLastFrame(i, j) && ExtraInfo.ShowHarvestIcon(i, j))
            {
                Texture2D tex = ModContent.Request<Texture2D>(Mod.Name + "/Tiles/MarkBack").Value;
                Texture2D tex2 = ModContent.Request<Texture2D>(Mod.Name + "/Tiles/Mark").Value;
                Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);//lighting mode correction
                if (Main.drawToScreen)
                    zero = Vector2.Zero;
                float pulseMult = ((float)Math.Sin((Main.GameUpdateCount / 30f) + i + j) / 8f) + 1.1f;
                Vector2 midPos = (new Vector2(i, j - 1.45f) * 16) + (Vector2.One * 8);
                Vector2 floatPos = midPos + new Vector2(0, -pulseMult * 20 + 20);
                float brightness = 0.1f * pulseMult;
                float disMult = -Math.Clamp((Vector2.Distance(midPos, Main.LocalPlayer.Center) - 40) * 0.005f, 0, 1) + 1;
                if (disMult > 0)
                {
                    spriteBatch.Draw(tex, (floatPos - Main.screenPosition) + zero, null, new Color(15, 15, 15, 35) * disMult, 0f, tex.Size() / 2, pulseMult, default, default);
                    spriteBatch.Draw(tex2, (floatPos - Main.screenPosition) + zero, null, new Color(brightness, brightness, brightness, 0f) * disMult, 0f, tex.Size() / 2, pulseMult, default, default);
                }
                //else
                //spriteBatch.Draw(tex, (floatPos - Main.screenPosition) + zero, null, new Color(15, 255, 15, 255), 0f, tex.Size() / 2, pulseMult, default, default);
            }
        }
    }
}
