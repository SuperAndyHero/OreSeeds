using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.UI.Chat;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace OreSeeds.UI
{
    class RecyclerUISystem : ModSystem
    {
        private UserInterface RecyclerUserInterface;
        internal RecyclerUI RecyclerUIPanel;

        public void ShowUI(Vector2 origin)
        {
            if (RecyclerUserInterface != null && RecyclerUserInterface.CurrentState != RecyclerUIPanel)
            {
                SoundEngine.PlaySound(SoundID.MenuOpen);
                RecyclerUIPanel.OnInitialize();//builds window and initalizes item arrays
                RecyclerUIPanel.TilePosition = origin;
                Main.playerInventory = true;
                RecyclerUserInterface?.SetState(RecyclerUIPanel);
            }
        }

        public void HideUI()
        {
            RecyclerUserInterface?.SetState(null);
            RecyclerUIPanel.ReturnItems();//returns items and sets the item arrays to null
            RecyclerUIPanel.RemoveAllChildren();//removes all elements, prevents instances of the window from building up
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
        const int RecycleRate = 40;//percent

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

            panel.BackgroundColor = new Color(25, 45, 80, 150);


            //var text = new UIText("0/0", 0.8f); // text to show stat
            //text.Width.Set(0, 0f);
            //text.Height.Set(0, 0f);
            //text.Top.Set(0, 0.5f);
            //text.Left.Set(0, 0.5f);
            //panel.Append(text);

            var c = new UIHoverImage(ModContent.Request<Texture2D>("OreSeeds/UI/Info"), 
                Language.GetTextValue("Mods.OreSeeds.DescriptionInfo.RecycleInfo").Replace("*", RecycleRate.ToString()) + 
                '\n' + 
                Language.GetTextValue("Mods.OreSeeds.DescriptionInfo.RecycleInfo2"), new Color(150, 150, 150, 150));
            c.Width.Pixels = 20;
            c.Height.Pixels = 28;
            c.Left.Set(-10, 0.83f);
            c.Top.Set(-14, 0.5f);
            panel.Append(c);

            //context 30 is invisible
            //context 3 is for chests and breaks in multiplayer
            //default size is 48
            for (int i = 0; i < 4; i++)
            {
                var slot1 = new CustomUIItemSlot(InputArray, i, 0);
                slot1.Width.Pixels = 48;//only effects hitbox
                slot1.Height.Pixels = 48;
                slot1.Left.Set(30 + (70 * i), 0);
                slot1.Top.Set(30, 0);
                panel.Append(slot1);
            }

            //var tex = ModContent.Request<Texture2D>("OreSeeds/Tiles/DownArrow");
            var b = new CustomButton(Language.GetText(Language.GetTextValue("Mods.OreSeeds.DescriptionInfo.RecycleButton")), Color.White, ModContent.Request<Texture2D>("OreSeeds/UI/DownArrow"), OnClick, "");
            b.SetColor(panel.BackgroundColor, 0.8f);
            b.Width.Pixels = 128;
            b.Height.Pixels = 40;
            b.Left.Set(-64, 0.5f);
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

        public new void OnClick(bool all)//recycle seeds button
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
                            if (Main.rand.NextFloat() > (RecycleRate * 0.01f))//RecycleRate% chance to get the item if its only 1 drop
                                amount = 0;
                        }
                        else
                            amount = (int)(amount * (RecycleRate * 0.01f));//else drop RecycleRate% less

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
                if(!item.IsAir)
                    Main.LocalPlayer.QuickSpawnItem(Item.GetSource_None(), item, item.stack);
            }
            foreach (var item in OutputArray)
            {
                if (!item.IsAir)
                    Main.LocalPlayer.QuickSpawnItem(Item.GetSource_None(), item, item.stack);
            }
            InputArray = null;
            OutputArray = null;
            //Main.NewText("items returned");
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
                ItemSlot.OverrideHover(ref inv, _itemSlotContext);//?
                //TempHelper.LeftClick(ref inv, _itemSlotContext);//debug version
                ItemSlot.LeftClick(ref inv, _itemSlotContext);//placing items in slot
                ItemSlot.RightClick(ref inv, _itemSlotContext);//taking 1 item from slot
                ItemSlot.MouseHover(ref inv, _itemSlotContext);//?
                _itemArray[_itemIndex] = inv;
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            HandleItemSlotLogic();//does not work in update or mouse event hooks
            Item inv = _itemArray[_itemIndex];
            float oldscale = Main.inventoryScale;
            Main.inventoryScale = Scale;
            Vector2 position = GetDimensions().Center() + new Vector2(52f, 52f) * -0.5f * Main.inventoryScale;
            ItemSlot.Draw(spriteBatch, ref inv, 1, position);//changing the context here only changes the visuals
            Main.inventoryScale = oldscale;
        }

    }

    //class for debugging items slots
    //todo: remove later
    /*public static class TempHelper
    {
        internal static Item[] singleSlotArray;

        static TempHelper()
        {
            singleSlotArray = new Item[1];
        }

        public static bool IsTheSameAs(this Item baseItem, Item compareItem)
        {
            if (baseItem.netID == compareItem.netID)
            {
                return baseItem.type == compareItem.type;
            }

            return false;
        }

        public static bool IsNotTheSameAs(this Item baseItem, Item compareItem)
        {
            if (baseItem.netID == compareItem.netID && baseItem.stack == compareItem.stack)
            {
                return baseItem.prefix != compareItem.prefix;
            }

            return true;
        }

        public static bool IsAccessoryContext(int context)
        {
            int num = Math.Abs(context);
            return num == 10 || num == 11;
        }

        public static bool LeftClick_SellOrTrash(Item[] inv, int context, int slot)
        {
            bool flag = false;
            bool result = false;
            if (ItemSlot.NotUsingGamepad && ItemSlot.Options.DisableLeftShiftTrashCan)
            {
                if (!ItemSlot.Options.DisableQuickTrash)
                {
                    if (((uint)context <= 4u && context >= 0) || context == 7 || context == 32)
                    {
                        flag = true;
                    }

                    if (ItemSlot.ControlInUse && flag)
                    {
                        ItemSlot.SellOrTrash(inv, context, slot);
                        result = true;
                    }
                }
            }
            else
            {
                if (((uint)context <= 4u && context >= 0) || context == 32)
                {
                    flag = Main.player[Main.myPlayer].chest == -1;
                }

                if (ItemSlot.ShiftInUse && flag && (!ItemSlot.NotUsingGamepad || !ItemSlot.Options.DisableQuickTrash))
                {
                    ItemSlot.SellOrTrash(inv, context, slot);
                    result = true;
                }
            }

            return result;
        }

        public static bool OverrideLeftClick(Item[] inv, int context = 0, int slot = 0)
        {
            if (Math.Abs(context) == 10 && ItemSlot.isEquipLocked(inv[slot].type))
            {
                return true;
            }

            if (Main.LocalPlayer.tileEntityAnchor.IsInValidUseTileEntity() && Main.LocalPlayer.tileEntityAnchor.GetTileEntity().OverrideItemSlotLeftClick(inv, context, slot))
            {
                return true;
            }

            Item item = inv[slot];
            if (ItemSlot.ShiftInUse && PlayerLoader.ShiftClickSlot(Main.player[Main.myPlayer], inv, context, slot))
            {
                return true;
            }

            if (Main.cursorOverride == 2)
            {
                if (ChatManager.AddChatText(FontAssets.MouseText.Value, ItemTagHandler.GenerateTag(item), Vector2.One))
                {
                    //SoundEngine.PlaySound(12);
                }

                return true;
            }

            if (Main.cursorOverride == 3)
            {
                //if (!canFavoriteAt[Math.Abs(context)])
                //{
                //    return false;
                //}

                item.favorited = !item.favorited;
                //SoundEngine.PlaySound(12);
                return true;
            }

            if (Main.cursorOverride == 7)
            {
                if (context == 29)
                {
                    Item item2 = inv[slot].Clone();
                    item2.stack = item2.maxStack;
                    item2.OnCreated(new JourneyDuplicationItemCreationContext());
                    item2 = Main.player[Main.myPlayer].GetItem(Main.myPlayer, item2, GetItemSettings.InventoryEntityToPlayerInventorySettings);
                    //SoundEngine.PlaySound(12);
                    return true;
                }

                inv[slot] = Main.player[Main.myPlayer].GetItem(Main.myPlayer, inv[slot], GetItemSettings.InventoryEntityToPlayerInventorySettings);
                //SoundEngine.PlaySound(12);
                return true;
            }

            if (Main.cursorOverride == 8)
            {
                inv[slot] = Main.player[Main.myPlayer].GetItem(Main.myPlayer, inv[slot], GetItemSettings.InventoryEntityToPlayerInventorySettings);
                if (Main.player[Main.myPlayer].chest > -1)
                {
                    NetMessage.SendData(32, -1, -1, null, Main.player[Main.myPlayer].chest, slot);
                }

                return true;
            }

            if (Main.cursorOverride == 9)
            {
                if (Main.CreativeMenu.IsShowingResearchMenu())
                {
                    Main.CreativeMenu.SwapItem(ref inv[slot]);
                    //SoundEngine.PlaySound(7);
                    Main.CreativeMenu.SacrificeItemInSacrificeSlot();
                }
                else if (Main.InReforgeMenu)
                {
                    Utils.Swap(ref inv[slot], ref Main.reforgeItem);
                    //SoundEngine.PlaySound(7);
                }
                else if (Main.InGuideCraftMenu)
                {
                    Utils.Swap(ref inv[slot], ref Main.guideItem);
                    Recipe.FindRecipes();
                    //SoundEngine.PlaySound(7);
                }
                else
                {
                    ChestUI.TryPlacingInChest(inv[slot], justCheck: false, context);
                }

                return true;
            }

            return false;
        }

        public static void LeftClick(ref Item inv, int context = 0)
        {
            singleSlotArray[0] = inv;
            TempHelper.LeftClick(singleSlotArray, context);
            inv = singleSlotArray[0];
        }

        public static void LeftClick(Item[] inv, int context = 0, int slot = 0)
        {
            Player player = Main.player[Main.myPlayer];
            bool flag = Main.mouseLeftRelease && Main.mouseLeft;
            if (flag)
            {
                if (TempHelper.OverrideLeftClick(inv, context, slot))
                {
                    return;
                }

                inv[slot].newAndShiny = false;
                if (TempHelper.LeftClick_SellOrTrash(inv, context, slot) || player.itemAnimation != 0 || player.itemTime != 0)
                {
                    return;
                }
            }

            int num = ItemSlot.PickItemMovementAction(inv, context, slot, Main.mouseItem);
            if (num != 3 && !flag)//if mouse is not actually clicking
            {
                return;
            }

            switch (num)
            {
                case 0:
                    {
                        if (context == 6 && Main.mouseItem.type != 0)
                        {
                            inv[slot].SetDefaults();
                        }

                        if ((TempHelper.IsAccessoryContext(context) && !ItemLoader.CanEquipAccessory(inv[slot], slot, context < 0)) || (context == 11 && !inv[slot].FitsAccessoryVanitySlot) || (context < 0 && !LoaderManager.Get<AccessorySlotLoader>().CanAcceptItem(slot, inv[slot], context)))
                        {
                            break;
                        }

                        if (Main.mouseItem.maxStack <= 1 || inv[slot].type != Main.mouseItem.type || inv[slot].stack == inv[slot].maxStack || Main.mouseItem.stack == Main.mouseItem.maxStack)
                        {
                            Utils.Swap(ref inv[slot], ref Main.mouseItem);
                        }

                        if (inv[slot].stack > 0)
                        {
                            ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(inv[slot], 21, context, inv[slot].stack));
                        }
                        else
                        {
                            ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(Main.mouseItem, context, 21, Main.mouseItem.stack));
                        }

                        if (inv[slot].stack > 0)
                        {
                            switch (Math.Abs(context))
                            {
                                case 0:
                                    AchievementsHelper.NotifyItemPickup(player, inv[slot]);
                                    break;
                                case 8:
                                case 9:
                                case 10:
                                case 11:
                                case 12:
                                case 16:
                                case 17:
                                case 25:
                                case 27:
                                case 33:
                                    AchievementsHelper.HandleOnEquip(player, inv[slot], context);
                                    break;
                            }
                        }

                        if (inv[slot].type == ItemID.None || inv[slot].stack < 1)
                        {
                            inv[slot] = new Item();
                        }

                        if (Main.mouseItem.IsTheSameAs(inv[slot]) && inv[slot].stack != inv[slot].maxStack && Main.mouseItem.stack != Main.mouseItem.maxStack && ItemLoader.TryStackItems(inv[slot], Main.mouseItem, out var numTransferred))
                        {
                            ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(inv[slot], 21, context, numTransferred));
                        }

                        if (Main.mouseItem.type == ItemID.None || Main.mouseItem.stack < 1)
                        {
                            Main.mouseItem = new Item();
                        }

                        if (Main.mouseItem.type > 0 || inv[slot].type > ItemID.None)
                        {
                            Recipe.FindRecipes();
                            //SoundEngine.PlaySound(7);
                        }

                        if (context == 3 && Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            //NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, player.chest, slot);//crash
                        }

                        break;
                    }
                case 1:
                    if (Main.mouseItem.stack == 1 && Main.mouseItem.type > 0 && inv[slot].type > 0 && inv[slot].IsNotTheSameAs(Main.mouseItem) && (context != 11 || Main.mouseItem.FitsAccessoryVanitySlot))
                    {
                        if ((TempHelper.IsAccessoryContext(context) && !ItemLoader.CanEquipAccessory(Main.mouseItem, slot, context < 0)) || (Math.Abs(context) == 11 && !Main.mouseItem.FitsAccessoryVanitySlot) || (context < 0 && !LoaderManager.Get<AccessorySlotLoader>().CanAcceptItem(slot, Main.mouseItem, context)))
                        {
                            break;
                        }

                        Utils.Swap(ref inv[slot], ref Main.mouseItem);
                        //SoundEngine.PlaySound(7);
                        if (inv[slot].stack > 0)
                        {
                            switch (Math.Abs(context))
                            {
                                case 0:
                                    AchievementsHelper.NotifyItemPickup(player, inv[slot]);
                                    break;
                                case 8:
                                case 9:
                                case 10:
                                case 11:
                                case 12:
                                case 16:
                                case 17:
                                case 25:
                                case 27:
                                case 33:
                                    AchievementsHelper.HandleOnEquip(player, inv[slot], context);
                                    break;
                            }
                        }
                    }
                    else if (Main.mouseItem.type == 0 && inv[slot].type > 0)
                    {
                        Utils.Swap(ref inv[slot], ref Main.mouseItem);
                        if (inv[slot].type == 0 || inv[slot].stack < 1)
                        {
                            inv[slot] = new Item();
                        }

                        if (Main.mouseItem.type == 0 || Main.mouseItem.stack < 1)
                        {
                            Main.mouseItem = new Item();
                        }

                        if (Main.mouseItem.type > 0 || inv[slot].type > 0)
                        {
                            Recipe.FindRecipes();
                            //SoundEngine.PlaySound(7);
                        }
                    }
                    else if (Main.mouseItem.type > 0 && inv[slot].type == 0 && (context != 11 || Main.mouseItem.FitsAccessoryVanitySlot))
                    {
                        if ((TempHelper.IsAccessoryContext(context) && !ItemLoader.CanEquipAccessory(Main.mouseItem, slot, context < 0)) || (Math.Abs(context) == 11 && !Main.mouseItem.FitsAccessoryVanitySlot) || (context < 0 && !LoaderManager.Get<AccessorySlotLoader>().CanAcceptItem(slot, Main.mouseItem, context)))
                        {
                            break;
                        }

                        inv[slot] = ItemLoader.TransferWithLimit(Main.mouseItem, 1);
                        Recipe.FindRecipes();
                        //SoundEngine.PlaySound(7);
                        if (inv[slot].stack > 0)
                        {
                            switch (Math.Abs(context))
                            {
                                case 0:
                                    AchievementsHelper.NotifyItemPickup(player, inv[slot]);
                                    break;
                                case 8:
                                case 9:
                                case 10:
                                case 11:
                                case 12:
                                case 16:
                                case 17:
                                case 25:
                                case 27:
                                case 33:
                                    AchievementsHelper.HandleOnEquip(player, inv[slot], context);
                                    break;
                            }
                        }
                    }

                    if ((context == 23 || context == 24) && Main.netMode == 1)
                    {
                        NetMessage.SendData(121, -1, -1, null, Main.myPlayer, player.tileEntityAnchor.interactEntityID, slot);
                    }

                    if (context == 26 && Main.netMode == 1)
                    {
                        NetMessage.SendData(124, -1, -1, null, Main.myPlayer, player.tileEntityAnchor.interactEntityID, slot);
                    }

                    break;
                case 2:
                    if (Main.mouseItem.stack == 1 && Main.mouseItem.dye > 0 && inv[slot].type > 0 && inv[slot].type != Main.mouseItem.type)
                    {
                        Utils.Swap(ref inv[slot], ref Main.mouseItem);
                        //SoundEngine.PlaySound(7);
                        if (inv[slot].stack > 0)
                        {
                            switch (Math.Abs(context))
                            {
                                case 0:
                                    AchievementsHelper.NotifyItemPickup(player, inv[slot]);
                                    break;
                                case 8:
                                case 9:
                                case 10:
                                case 11:
                                case 12:
                                case 16:
                                case 17:
                                case 25:
                                case 27:
                                case 33:
                                    AchievementsHelper.HandleOnEquip(player, inv[slot], context);
                                    break;
                            }
                        }
                    }
                    else if (Main.mouseItem.type == 0 && inv[slot].type > 0)
                    {
                        Utils.Swap(ref inv[slot], ref Main.mouseItem);
                        if (inv[slot].type == 0 || inv[slot].stack < 1)
                        {
                            inv[slot] = new Item();
                        }

                        if (Main.mouseItem.type == 0 || Main.mouseItem.stack < 1)
                        {
                            Main.mouseItem = new Item();
                        }

                        if (Main.mouseItem.type > 0 || inv[slot].type > 0)
                        {
                            Recipe.FindRecipes();
                            //SoundEngine.PlaySound(7);
                        }
                    }
                    else if (Main.mouseItem.dye > 0 && inv[slot].type == 0)
                    {
                        inv[slot] = ItemLoader.TransferWithLimit(Main.mouseItem, 1);
                        Recipe.FindRecipes();
                        //SoundEngine.PlaySound(7);
                        if (inv[slot].stack > 0)
                        {
                            switch (Math.Abs(context))
                            {
                                case 0:
                                    AchievementsHelper.NotifyItemPickup(player, inv[slot]);
                                    break;
                                case 8:
                                case 9:
                                case 10:
                                case 11:
                                case 12:
                                case 16:
                                case 17:
                                case 25:
                                case 27:
                                case 33:
                                    AchievementsHelper.HandleOnEquip(player, inv[slot], context);
                                    break;
                            }
                        }
                    }

                    if (context == 25 && Main.netMode == 1)
                    {
                        NetMessage.SendData(121, -1, -1, null, Main.myPlayer, player.tileEntityAnchor.interactEntityID, slot, 1f);
                    }

                    if (context == 27 && Main.netMode == 1)
                    {
                        NetMessage.SendData(124, -1, -1, null, Main.myPlayer, player.tileEntityAnchor.interactEntityID, slot, 1f);
                    }

                    break;
                //case 3:
                //    ItemSlot.HandleShopSlot(inv, slot, rightClickIsValid: false, leftClickIsValid: true);
                //    break;
                case 4:
                    if (PlayerLoader.CanSellItem(player, player.TalkNPC, inv, Main.mouseItem))
                    {
                        Chest chest = Main.instance.shop[Main.npcShop];
                        if (player.SellItem(Main.mouseItem))
                        {
                            int num2 = chest.AddItemToShop(Main.mouseItem);
                            Main.mouseItem.SetDefaults();
                            //SoundEngine.PlaySound(18);
                            ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(inv[slot], 21, 15));
                            PlayerLoader.PostSellItem(player, player.TalkNPC, chest.item, chest.item[num2]);
                        }
                        else if (Main.mouseItem.value == 0)
                        {
                            int num3 = chest.AddItemToShop(Main.mouseItem);
                            Main.mouseItem.SetDefaults();
                            //SoundEngine.PlaySound(7);
                            ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(inv[slot], 21, 15));
                            PlayerLoader.PostSellItem(player, player.TalkNPC, chest.item, chest.item[num3]);
                        }

                        Recipe.FindRecipes();
                        Main.stackSplit = 9999;
                    }

                    break;
                case 5:
                    if (Main.mouseItem.IsAir)
                    {
                        //SoundEngine.PlaySound(7);
                        Main.mouseItem = inv[slot].Clone();
                        Main.mouseItem.stack = Main.mouseItem.maxStack;
                        Main.mouseItem.OnCreated(new JourneyDuplicationItemCreationContext());
                        ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(inv[slot], 29, 21));
                    }

                    break;
            }

            if ((uint)context > 2u && context != 5 && context != 32)
            {
                inv[slot].favorited = false;
            }
        }
    }
    */

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

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            onClick(false);
            base.LeftMouseDown(evt);
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

    public class UIHoverImage : UIElement
    {
        private Asset<Texture2D> _texture;

        public float ImageScale = 1f;

        public float Rotation;

        public bool ScaleToFit;

        public bool AllowResizingDimensions = true;

        public Color Color = Color.White;

        public Vector2 NormalizedOrigin = Vector2.Zero;

        public bool RemoveFloatingPointsFromDrawPosition;

        private Texture2D _nonReloadingTexture;

        public string HoverText;

        public UIHoverImage(Asset<Texture2D> texture, string hoverText, Color color)
        {
            HoverText = hoverText;
            Color = color;
            SetImage(texture);
        }

        public UIHoverImage(Texture2D nonReloadingTexture, string hoverText, Color color)
        {
            HoverText = hoverText;
            Color = color;
            SetImage(nonReloadingTexture);
        }

        public void SetImage(Asset<Texture2D> texture)
        {
            _texture = texture;
            _nonReloadingTexture = null;
            if (AllowResizingDimensions)
            {
                this.Width.Set(_texture.Width(), 0f);
                Height.Set(_texture.Height(), 0f);
            }
        }

        public void SetImage(Texture2D nonReloadingTexture)
        {
            _texture = null;
            _nonReloadingTexture = nonReloadingTexture;
            if (AllowResizingDimensions)
            {
                Width.Set(_nonReloadingTexture.Width, 0f);
                Height.Set(_nonReloadingTexture.Height, 0f);
            }
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            CalculatedStyle dimensions = GetDimensions();
            Texture2D texture2D = null;
            if (_texture != null)
            {
                texture2D = _texture.Value;
            }

            if (_nonReloadingTexture != null)
            {
                texture2D = _nonReloadingTexture;
            }

            if (ScaleToFit)
            {
                spriteBatch.Draw(texture2D, dimensions.ToRectangle(), Color);
                return;
            }

            Vector2 vector = texture2D.Size();
            Vector2 vector2 = dimensions.Position() + vector * (1f - ImageScale) / 2f + vector * NormalizedOrigin;
            if (RemoveFloatingPointsFromDrawPosition)
            {
                vector2 = vector2.Floor();
            }

            spriteBatch.Draw(texture2D, vector2, null, Color, Rotation, vector * NormalizedOrigin, ImageScale, SpriteEffects.None, 0f);

            if (IsMouseHovering)
            {
                Main.instance.MouseTextHackZoom(HoverText, ItemRarityID.White, 0);
            }
        }
    }
}
