using IL.Terraria.GameContent.Achievements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace OreSeeds.Tiles
{
    class RecyclerUISystem : ModSystem
    {
        private UserInterface RecyclerUserInterface;
        internal RecyclerUI RecyclerUIPanel;

        public void ShowUI(Vector2 origin)
        {
            RecyclerUIPanel.OnInitialize();//debug
            RecyclerUIPanel.TilePosition = origin;
            Main.playerInventory = true;
            RecyclerUserInterface?.SetState(RecyclerUIPanel);
        }

        public void HideUI()
        {
            RecyclerUserInterface?.SetState(null);
            RecyclerUIPanel.ReturnItems();
            RecyclerUIPanel.RemoveAllChildren();//debug
        }

        public override void Load()
        {
            // All code below runs only if we're not loading on a server
            if (!Main.dedServ)
            {
                RecyclerUIPanel = new();
                RecyclerUserInterface = new();
                //RecyclerUserInterface.SetState(RecyclerUIPanel);
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            RecyclerUserInterface?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (resourceBarIndex != -1)
            {
                layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                    "OreSeeds: RecyclerUI",
                    delegate {
                        RecyclerUserInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }

    internal class RecyclerUI : UIState
    {
        // For this bar we'll be using a frame texture and then a gradient inside bar, as it's one of the more simpler approaches while still looking decent.
        // Once this is all set up make sure to go and do the required stuff for most UI's in the ModSystem class.
        private UIPanel panel;
        //private UIImage barFrame;
        public Vector2 TilePosition = Vector2.Zero;

        public Item[] InputArray;
        public Item[] OutputArray;
        public override void OnInitialize()
        {
            InputArray = new Item[4] { new Item(), new Item(), new Item(), new Item() };
            OutputArray = new Item[4] { new Item(), new Item(), new Item(), new Item() }; ;

            panel = new UIPanel();
            panel.SetPadding(0);
            panel.Width.Set(318f, 0f);//main size
            panel.Height.Set(238f, 0f);

            float panelOff = (panel.Width.Pixels * Main.UIScale) / 2;//half of panel width
            float screenSpaceOff = (((Main.screenWidth) / 2) - panelOff) / Main.UIScale;//center of screen, offset by half panel width
            screenSpaceOff = Math.Max(screenSpaceOff, 570);//offsets panel if overlaps with inventory
            panel.Left.Set(screenSpaceOff, 0f);

            panel.Top.Set(-panel.Height.Pixels / Main.UIScale, 0.5f);

            panel.BackgroundColor = new Color(30, 45, 100, 150);


            //var text = new UIText("0/0", 0.8f); // text to show stat
            //text.Width.Set(0, 0f);
            //text.Height.Set(0, 0f);
            //text.Top.Set(0, 0.5f);
            //text.Left.Set(0, 0.5f);
            //panel.Append(text);

            //30 is invisible
            //default size is 48
            for (int i = 0; i < 4; i++)
            {
                var slot1 = new CustomUIItemSlot(InputArray, i, 3);
                slot1.Width.Pixels = 48;//only effects hitbox
                slot1.Height.Pixels = 48;
                slot1.Left.Set(30 + (70 * i), 0);
                slot1.Top.Set(30, 0);
                panel.Append(slot1);
            }

            //var tex = ModContent.Request<Texture2D>("OreSeeds/Tiles/DownArrow");
            var b = new CustomButton(Language.GetText("recycle seeds"), Color.White, ModContent.Request<Texture2D>("OreSeeds/Tiles/DownArrow"), OnClick, "debug");
            b.SetColor(panel.BackgroundColor, 0.8f);
            b.Width.Pixels = 120;
            b.Height.Pixels = 40;
            b.Left.Set(-60, 0.5f);
            b.Top.Set(-20, 0.5f);
            panel.Append(b);

            for (int i = 0; i < 4; i++)
            {
                var slot1 = new CustomUIItemSlot(OutputArray, i, 2);
                slot1.Width.Pixels = 48;//only effects hitbox
                slot1.Height.Pixels = 48;
                slot1.Left.Set(30 + (70 * i), 0);
                slot1.Top.Set(160, 0);
                panel.Append(slot1);
            }

            //panel.Append(barFrame);
            Append(panel);
        }

        public new void OnClick(bool all)
        {
            foreach(var inputItem in InputArray)
            {

                if (inputItem != null && !inputItem.IsAir && inputItem.ModItem != null && inputItem.ModItem is BasePlantItem)
                {
                    bool once = false;
                    while (!once || (all && !inputItem.IsAir))
                    {
                        BasePlantItem moditem = (BasePlantItem)inputItem.ModItem;
                        int type = moditem.OreItem();
                        int amount = (int)(Main.rand.Next(moditem.OreDropRange.min, moditem.OreDropRange.max + 1));
                        if(amount == 1)
                        {
                            if (Main.rand.NextFloat() > 0.6f)//60% chance to get the item if its only 1 drop
                                amount = 0;
                        }
                        else
                            amount = (int)(amount * 0.6f);//else drop 60% less

                        bool foundSlot = false;
                        for (int i = 0; i < OutputArray.Length; i++)
                        {
                            if (OutputArray[i] == null || OutputArray[i].IsAir)
                            {
                                foundSlot = true;
                                OutputArray[i] = new Item(type, amount);
                                break;
                            }
                            else if (OutputArray[i].type == type &&
                                OutputArray[i].stack < (OutputArray[i].maxStack - moditem.OreDropRange.max))
                            {
                                foundSlot = true;
                                OutputArray[i].stack += amount;
                                break;
                            }
                        }

                        once = true;
                        if (foundSlot)
                        {
                            if (inputItem.stack > 1)
                                inputItem.stack--;
                            else
                                inputItem.TurnToAir();
                        }
                        else
                            break;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // This prevents drawing unless we are using an ExampleCustomResourceWeapon
            //if (Main.LocalPlayer.HeldItem.ModItem is not ExampleCustomResourceWeapon)
            //    return;

            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            if (panel.ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            if (!Main.playerInventory || (Main.LocalPlayer.position - TilePosition).Length() > 100f)
                ModContent.GetInstance<RecyclerUISystem>().HideUI();

            //if (Main.LocalPlayer.HeldItem.ModItem is not ExampleCustomResourceWeapon)
            //    return;

            //var modPlayer = Main.LocalPlayer.GetModPlayer<ExampleResourcePlayer>();
            //// Setting the text per tick to update and show our resource values.
            //text.SetText($"Example Resource: {modPlayer.exampleResourceCurrent} / {modPlayer.exampleResourceMax2}");
            base.Update(gameTime);
        }

        public void ReturnItems()
        {
            foreach(var item in InputArray)
            {
                Main.LocalPlayer.QuickSpawnItem(Item.GetSource_None(), item, item.stack);
            }
            foreach (var item in OutputArray)
            {
                Main.LocalPlayer.QuickSpawnItem(Item.GetSource_None(), item, item.stack);
            }
            Main.NewText("items returned");
        }
    }

    public class CustomUIItemSlot : UIElement
    {
        private Item[] _itemArray;

        private int _itemIndex;

        private int _itemSlotContext;

        public float Scale = 1f;

        public CustomUIItemSlot(Item[] itemArray, int itemIndex, int itemSlotContext)
        {
            _itemArray = itemArray;
            _itemIndex = itemIndex;
            _itemSlotContext = itemSlotContext;
            Width = new StyleDimension(48f, 0f);
            Height = new StyleDimension(48f, 0f);
        }

        private void HandleItemSlotLogic()
        {
            if (base.IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
                Item inv = _itemArray[_itemIndex];
                ItemSlot.OverrideHover(ref inv, _itemSlotContext);
                ItemSlot.LeftClick(ref inv, _itemSlotContext);
                ItemSlot.RightClick(ref inv, _itemSlotContext);
                ItemSlot.MouseHover(ref inv, _itemSlotContext);
                _itemArray[_itemIndex] = inv;
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            HandleItemSlotLogic();
            Item inv = _itemArray[_itemIndex];
            float oldscale = Main.inventoryScale;
            Main.inventoryScale = Scale;
            Vector2 position = GetDimensions().Center() + new Vector2(52f, 52f) * -0.5f * Main.inventoryScale;
            ItemSlot.Draw(spriteBatch, ref inv, 1, position);//changing the context here only changes the visuals
            Main.inventoryScale = oldscale;
        }
    }

    public class CustomButton : UIIconTextButton
    {
        private readonly Asset<Texture2D> iconTexture;
        private Color _color;
        private readonly Asset<Texture2D> _BasePanelTexture;

        private readonly Asset<Texture2D> _hoveredTexture;

        private Color _hoverColor;

        private float _whiteLerp = 0.7f;

        private float _opacity = 0.7f;

        private bool _hovered;

        private bool _soundedHover;
        private Action<bool> onClick;
        private string hoverText;
        public CustomButton(LocalizedText title, Color textColor, Asset<Texture2D> iconTexturePath, Action<bool> onClick, string hoverText, float textSize = 1f, float titleAlignmentX = 0.5f, float titleWidthReduction = 10f) : base(title, textColor, "Images\\UI\\Reforge_0", textSize, titleAlignmentX, titleWidthReduction)
        {
            this.onClick = onClick;
            this.hoverText = hoverText;

            Width = StyleDimension.FromPixels(44f);
            Height = StyleDimension.FromPixels(34f);
            _hoverColor = Color.White;
            _BasePanelTexture = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/PanelGrayscale");
            _hoveredTexture = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/CategoryPanelHighlight");

            if (iconTexturePath != null)
                iconTexture = iconTexturePath;

            SetColor(Color.Lerp(Color.Black, Colors.InventoryDefaultColor, FadeFromBlack), 1f);
            if (title != null)
            {
                SetText(title, textSize, textColor);
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (_hovered)
            {
                if (!_soundedHover)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }

                _soundedHover = true;
            }
            else
            {
                _soundedHover = false;
            }

            CalculatedStyle dimensions = GetDimensions();
            Color color = _color;
            float opacity = _opacity;
            float imageOpacity = 0.2f;
            Utils.DrawSplicedPanel(spriteBatch, _BasePanelTexture.Value, (int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height, 10, 10, 10, 10, Color.Lerp(Color.Black, color, FadeFromBlack) * opacity);
            if (iconTexture != null)
            {//new Vector2(dimensions.X + dimensions.Width - (float)iconTexture.Width() - 5f, dimensions.Center().Y - (float)(iconTexture.Height() / 2))
                Color color2 = Color.Lerp(color, Color.White, _whiteLerp) * imageOpacity;
                Vector2 size = iconTexture.Size();
                spriteBatch.Draw(iconTexture.Value, new Vector2(dimensions.Center().X - (size.X / 2), dimensions.Center().Y - (size.Y / 2)), color2);
            }
        }

        public override void MouseDown(UIMouseEvent evt)
        {
            onClick(false);
            //SoundEngine.PlaySound(SoundID.MenuTick);
            base.MouseDown(evt);
        }

        public override void RightMouseDown(UIMouseEvent evt)
        {
            onClick(true);
            base.RightMouseDown(evt);
        }
        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            SetColor(Color.Lerp(Colors.InventoryDefaultColor, Color.White, _whiteLerp), 0.7f);
            _hovered = true;
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            base.MouseOut(evt);
            SetColor(Color.Lerp(Color.Black, Colors.InventoryDefaultColor, FadeFromBlack), 1f);
            _hovered = false;
        }

        public new void SetColor(Color color, float opacity)
        {
            _color = color;
            _opacity = opacity;
        }

        public new void SetHoverColor(Color color)
        {
            _hoverColor = color;
        }
    }
}
