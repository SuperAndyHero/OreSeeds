using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI.Elements;
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
        public override void OnInitialize()
        {
            panel = new UIPanel();
            panel.SetPadding(0);
            panel.Width.Set(300f, 0f);//main size
            panel.Height.Set(200f, 0f);

            float panelOff = (panel.Width.Pixels * Main.UIScale) / 2;//half of panel width
            float screenSpaceOff = (((Main.screenWidth) / 2) - panelOff) / Main.UIScale;//center of screen, offset by half panel width
            screenSpaceOff = Math.Max(screenSpaceOff, 570);//offsets panel if overlaps with inventory
            panel.Left.Set(screenSpaceOff, 0f);

            panel.Top.Set(-panel.Height.Pixels / Main.UIScale, 0.5f);

            panel.BackgroundColor = new Color(30, 45, 100, 150);


            var text = new UIText("0/0", 0.8f); // text to show stat
            text.Width.Set(0, 0f);
            text.Height.Set(0, 0f);
            text.Top.Set(0, 0.5f);
            text.Left.Set(0, 0.5f);
            panel.Append(text);

            //30 is invisible
            //default size is 48
            var a = new CustomUIItemSlot(new Item(), 3);
            a.Width.Pixels = 80;
            a.Height.Pixels = 80;
            a.Top.Set(-24, 0.5f);
            a.Left.Set(-24, 0.5f);
            panel.Append(a);


            //panel.Append(barFrame);
            Append(panel);
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
            Main.NewText("items returned");
        }
    }

    //this is a copy of the itemslot class because the vanilla one does not allow
    //you to change how it appears without also changing functionality
    public class CustomUIItemSlot : UIElement
    {
        private Item _item;

        private int _itemSlotContext;

        public CustomUIItemSlot(Item item, int itemSlotContext)
        {
            _item = item;
            _itemSlotContext = itemSlotContext;
            Width = new StyleDimension(48f, 0f);
            Height = new StyleDimension(48f, 0f);
        }

        private void HandleItemSlotLogic()
        {
            if (base.IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
                Item inv = _item;
                ItemSlot.OverrideHover(ref inv, _itemSlotContext);
                ItemSlot.LeftClick(ref inv, _itemSlotContext);
                ItemSlot.RightClick(ref inv, _itemSlotContext);
                ItemSlot.MouseHover(ref inv, _itemSlotContext);
                _item = inv;
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            HandleItemSlotLogic();
            Vector2 position = GetDimensions().Center() + new Vector2(52f, 52f) * -0.5f * Main.inventoryScale;
            ItemSlot.Draw(spriteBatch, ref _item, 2, position);//color value is item color
        }
    }
}
