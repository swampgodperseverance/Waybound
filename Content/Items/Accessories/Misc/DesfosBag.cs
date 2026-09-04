using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Waybound.Common.ModSystems;

namespace Waybound.Content.Items.Accessories.Misc
{
    public class DesfosBag : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;
            Item.accessory = true;
            Item.value = Item.sellPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<DesfosBagPlayer>().equippedBag = true;
        }
    }

    public class DesfosBagPlayer : ModPlayer
    {
        public bool equippedBag = false;
        public bool bagActive = false;
        public bool isClosing = false;

        public int extraSlots = 0;
        public const int MaxSlots = 20;

        public Item[] bagItems = new Item[MaxSlots];
        public float opacity = 0f;

        public override void Initialize()
        {
            for (int i = 0; i < MaxSlots; i++)
            {
                bagItems[i] = new Item();
                bagItems[i].TurnToAir(true);
            }
        }

        public override void ResetEffects()
        {
            if (!equippedBag)
            {
                if (bagActive)
                {
                    isClosing = true;
                    bagActive = false;
                }
                extraSlots = 0;
            }
            equippedBag = false;
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (VanillaKeybinds.DesfosBagActivation.JustPressed && equippedBag)
            {
                if (!bagActive && !isClosing)
                    ActivateBag();
                else if (bagActive)
                    StartClosing();
            }
        }

        private void ActivateBag()
        {
            bagActive = true;
            isClosing = false;
            RecalculateSlots();
            Main.playerInventory = true;
        }

        private void StartClosing()
        {
            bagActive = false;
            isClosing = true;
        }

        public void RecalculateSlots()
        {
            long goldValue = GetTotalGoldValue();
            // 1 + 3 + 5 + 7... = k²
            int k = (int)Math.Floor(Math.Sqrt(goldValue));
            extraSlots = Math.Clamp(k, 0, MaxSlots);
        }

        private long GetTotalGoldValue()
        {
            long value = 0;
            for (int i = 0; i < Player.inventory.Length; i++)
            {
                Item item = Player.inventory[i];
                if (item.type == ItemID.GoldCoin)
                    value += item.stack;
                else if (item.type == ItemID.PlatinumCoin)
                    value += item.stack * 100L;
            }
            return value;
        }

        public override void PostUpdate()
        {
            if (equippedBag && bagActive)
            {
                RecalculateSlots();
                opacity = MathHelper.Clamp(opacity + 0.09f, 0f, 1f);
                isClosing = false;
            }
            else
            {
                opacity = MathHelper.Clamp(opacity - 0.09f, 0f, 1f);
                if (opacity <= 0.01f)
                    isClosing = false;
            }
        }

        public override void SaveData(TagCompound tag)
        {
            var list = new List<Item>();
            for (int i = 0; i < MaxSlots; i++)
                list.Add(bagItems[i] ?? new Item());
            tag["bagItems"] = list;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("bagItems"))
            {
                var list = tag.Get<List<Item>>("bagItems");
                for (int i = 0; i < MaxSlots; i++)
                {
                    bagItems[i] = (i < list.Count && list[i] != null) ? list[i] : new Item();
                    if (bagItems[i].IsAir)
                        bagItems[i].TurnToAir(true);
                }
            }
        }
    }

    public class DesfosBagUI : ModSystem
    {
        private UserInterface _interface;
        private DesfosBagUIState _state;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                _interface = new UserInterface();
                _state = new DesfosBagUIState();
            }
        }

        public override void Unload()
        {
            _interface = null;
            _state = null;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            var bagPlayer = Main.LocalPlayer?.GetModPlayer<DesfosBagPlayer>();
            if (bagPlayer == null) return;

            bool shouldShow = (bagPlayer.bagActive || bagPlayer.isClosing || bagPlayer.opacity > 0.01f)
                              && bagPlayer.equippedBag
                              && Main.playerInventory;

            if (shouldShow)
            {
                if (_interface.CurrentState == null)
                {
                    _state = new DesfosBagUIState();
                    _state.Activate();
                    _interface.SetState(_state);
                }
                _interface.Update(gameTime);
            }
            else
            {
                if (_interface.CurrentState != null)
                    _interface.SetState(null);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
                    "Waybound: Desfos Bag",
                    delegate
                    {
                        var bagPlayer = Main.LocalPlayer?.GetModPlayer<DesfosBagPlayer>();
                        if (bagPlayer == null) return true;

                        bool shouldDraw = (bagPlayer.bagActive || bagPlayer.isClosing || bagPlayer.opacity > 0.01f)
                                          && bagPlayer.equippedBag
                                          && Main.playerInventory;

                        if (shouldDraw)
                            _interface?.Draw(Main.spriteBatch, new GameTime());

                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
    }

    public class DesfosBagUIState : UIState
    {
        private readonly List<DesfosBagSlot> _slots = new();
        private int _currentSlots = 0;

        private float _slideOffset = -140f;
        private float _targetSlide = 0f;

        public override void OnInitialize() { }

        private void RebuildSlots(int newCount)
        {
            var bagPlayer = Main.LocalPlayer?.GetModPlayer<DesfosBagPlayer>();
            if (bagPlayer == null) return;

            float slotSize = 42f;
            float spacing = 5f;
            int slotsPerRow = 10;

            float totalWidth = Math.Min(newCount, slotsPerRow) * (slotSize + spacing) - spacing;
            float startX = Main.screenWidth / 2f - totalWidth / 2f;
            float startY = 27f; // Here is height/lower = higher

            while (_slots.Count < newCount)
            {
                int i = _slots.Count;
                int row = i / slotsPerRow;
                int col = i % slotsPerRow;

                float targetX = startX + col * (slotSize + spacing);
                float targetY = startY + row * (slotSize + spacing);

                var slot = new DesfosBagSlot(i);
                slot.Left.Set(targetX, 0f);
                slot.Top.Set(targetY + _slideOffset, 0f);
                slot.Width.Set(slotSize, 0f);
                slot.Height.Set(slotSize, 0f);

                slot.IndividualFade = 0f;
                slot.IsNew = true;

                Append(slot);
                _slots.Add(slot);
            }

            while (_slots.Count > newCount)
            {
                var last = _slots[_slots.Count - 1];
                RemoveChild(last);
                _slots.RemoveAt(_slots.Count - 1);
            }

            for (int i = 0; i < _slots.Count; i++)
            {
                int row = i / slotsPerRow;
                int col = i % slotsPerRow;

                _slots[i].TargetX = startX + col * (slotSize + spacing);
                _slots[i].TargetY = startY + row * (slotSize + spacing);
            }

            _currentSlots = newCount;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var bagPlayer = Main.LocalPlayer?.GetModPlayer<DesfosBagPlayer>();
            if (bagPlayer == null) return;

            _targetSlide = bagPlayer.bagActive ? 0f : -160f;
            _slideOffset = MathHelper.Lerp(_slideOffset, _targetSlide, 0.14f);

            int desired = bagPlayer.extraSlots;
            if (desired != _currentSlots || (_slots.Count == 0 && desired > 0))
            {
                RebuildSlots(desired);
            }

            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];

                float currentX = slot.Left.Pixels;
                float currentY = slot.Top.Pixels - _slideOffset;

                float newX = MathHelper.Lerp(currentX, slot.TargetX, 0.18f);
                float newY = MathHelper.Lerp(currentY, slot.TargetY, 0.18f);

                slot.Left.Set(newX, 0f);
                slot.Top.Set(newY + _slideOffset, 0f);

                if (slot.IsNew)
                {
                    slot.IndividualFade = Math.Min(slot.IndividualFade + 0.08f, 1f);
                    if (slot.IndividualFade >= 0.99f)
                        slot.IsNew = false;
                }
                else
                {
                    slot.IndividualFade = 1f;
                }
            }

            if (bagPlayer.opacity <= 0.01f && !bagPlayer.bagActive && _slots.Count > 0)
            {
                RemoveAllChildren();
                _slots.Clear();
                _currentSlots = 0;
                _slideOffset = -140f;
            }
        }
    }

    public class DesfosBagSlot : UIElement
    {
        public int SlotIndex;
        public float TargetX;
        public float TargetY;
        public float IndividualFade = 1f;
        public bool IsNew = false;

        private bool _hover;
        private Texture2D _pixel;

        public DesfosBagSlot(int slotIndex)
        {
            SlotIndex = slotIndex;
            Width.Set(42f, 0f);
            Height.Set(42f, 0f);
        }

        private Texture2D GetPixel()
        {
            if (_pixel == null || _pixel.IsDisposed)
            {
                _pixel = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
                _pixel.SetData(new[] { Color.White });
            }
            return _pixel;
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var bagPlayer = Main.LocalPlayer?.GetModPlayer<DesfosBagPlayer>();
            if (bagPlayer == null) return;

            float opacity = bagPlayer.opacity * IndividualFade;
            if (opacity < 0.01f) return;

            Item item = bagPlayer.bagItems[SlotIndex];
            if (item == null)
            {
                item = new Item();
                item.TurnToAir(true);
                bagPlayer.bagItems[SlotIndex] = item;
            }

            Rectangle rect = GetDimensions().ToRectangle();

            Texture2D invBack = TextureAssets.InventoryBack.Value;
            Color bg = new Color(35, 28, 12) * (0.82f * opacity);
            spriteBatch.Draw(invBack, rect, bg);

            Texture2D pixel = GetPixel();
            Color border = Color.Gold * (0.9f * opacity);
            int b = 2;
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, b), border);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - b, rect.Width, b), border);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, b, rect.Height), border);
            spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - b, rect.Y, b, rect.Height), border);

            if (!item.IsAir)
            {
                Main.instance.LoadItem(item.type);
                Texture2D itemTex = TextureAssets.Item[item.type].Value;
                Rectangle frame = Main.itemAnimations[item.type]?.GetFrame(itemTex) ?? itemTex.Frame();

                // Small items restrictment
                float maxSize = 32f;
                float scale = 1f;

                if (frame.Width > maxSize || frame.Height > maxSize)
                    scale = maxSize / Math.Max(frame.Width, frame.Height);

                scale *= 0.92f;

                Vector2 position = new Vector2(
                    rect.X + rect.Width / 2f - frame.Width * scale / 2f,
                    rect.Y + rect.Height / 2f - frame.Height * scale / 2f
                );

                Color itemColor = Color.White * opacity;
                ItemSlot.GetItemLight(ref itemColor, item);

                spriteBatch.Draw(itemTex, position, frame, itemColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

                if (item.stack > 1)
                {
                    string text = item.stack.ToString();
                    Vector2 size = FontAssets.MouseText.Value.MeasureString(text) * 0.7f;
                    Vector2 textPos = new Vector2(rect.Right - size.X - 3f, rect.Bottom - size.Y - 2f);
                    Utils.DrawBorderString(spriteBatch, text, textPos, Color.White * opacity, 0.7f);
                }

                if (item.rare > 0)
                {
                    Color rare = ItemRarity.GetColor(item.rare) * (0.18f * opacity);
                    spriteBatch.Draw(invBack, rect, rare);
                }
            }

            Utils.DrawBorderString(spriteBatch, (SlotIndex + 1).ToString(),
                new Vector2(rect.X + 3f, rect.Y + 2f), Color.White * (0.22f * opacity), 0.5f);

            if (_hover && !item.IsAir)
            {
                Main.HoverItem = item.Clone();
                Main.hoverItemName = item.Name;
            }
        }

        public override void MouseOver(UIMouseEvent evt)
        {
            base.MouseOver(evt);
            _hover = true;
            Main.LocalPlayer.mouseInterface = true;
        }

        public override void MouseOut(UIMouseEvent evt)
        {
            base.MouseOut(evt);
            _hover = false;
        }

        public override void LeftClick(UIMouseEvent evt)
        { //That was SO fucking hard to do
            var bagPlayer = Main.LocalPlayer?.GetModPlayer<DesfosBagPlayer>();
            if (bagPlayer == null || !bagPlayer.bagActive || bagPlayer.opacity < 0.7f) return;
            if (SlotIndex >= bagPlayer.extraSlots) return;

            // Blocking shit
            Main.LocalPlayer.mouseInterface = true;
            Main.mouseLeftRelease = false;
            Main.mouseRightRelease = false;
            Main.mouseLeft = false;               // NOT REMOVE THIS
            Main.LocalPlayer.releaseUseItem = false;
            Main.LocalPlayer.controlUseItem = false;

            ref Item slotItem = ref bagPlayer.bagItems[SlotIndex];

            if (slotItem == null)
            {
                slotItem = new Item();
                slotItem.TurnToAir(true);
            }

            if (Main.mouseItem.IsAir)
            {
                if (!slotItem.IsAir)
                {
                    Main.mouseItem = slotItem.Clone();
                    slotItem.TurnToAir(true);
                    SoundEngine.PlaySound(SoundID.Grab);
                }
            }
            else
            {
                if (slotItem.IsAir)
                {
                    slotItem = Main.mouseItem.Clone();
                    Main.mouseItem.TurnToAir(true);
                    Main.mouseItem.SetDefaults(0);
                    Main.mouseItem.type = 0;
                    Main.mouseItem.stack = 0;
                    Main.mouseItem.prefix = 0;

                    // use style defencing code
                    Main.LocalPlayer.itemAnimation = 0;
                    Main.LocalPlayer.itemTime = 0;
                    Main.LocalPlayer.itemAnimationMax = 0;
                    Main.LocalPlayer.releaseUseItem = false;
                    Main.LocalPlayer.controlUseItem = false;

                    SoundEngine.PlaySound(SoundID.Grab);
                }
                else
                {
                   
                    Item temp = slotItem.Clone();
                    slotItem = Main.mouseItem.Clone();
                    Main.mouseItem = temp;

                    
                    Main.LocalPlayer.itemAnimation = 0;
                    Main.LocalPlayer.itemTime = 0;

                    SoundEngine.PlaySound(SoundID.Grab);
                }
            }
        }

        public override void RightClick(UIMouseEvent evt)
        {
            var bagPlayer = Main.LocalPlayer?.GetModPlayer<DesfosBagPlayer>();
            if (bagPlayer == null || !bagPlayer.bagActive || bagPlayer.opacity < 0.7f) return;
            if (SlotIndex >= bagPlayer.extraSlots) return;

            Main.LocalPlayer.mouseInterface = true;

            ref Item slotItem = ref bagPlayer.bagItems[SlotIndex];
            if (slotItem == null || slotItem.IsAir) return;

            if (Main.mouseItem.IsAir)
            {
                Main.mouseItem = slotItem.Clone();
                Main.mouseItem.stack = 1;

                slotItem.stack--;
                if (slotItem.stack <= 0)
                    slotItem.TurnToAir(true);

                SoundEngine.PlaySound(SoundID.Grab);
            }
            else if (Main.mouseItem.type == slotItem.type && Main.mouseItem.stack < Main.mouseItem.maxStack)
            {
                Main.mouseItem.stack++;
                slotItem.stack--;
                if (slotItem.stack <= 0)
                    slotItem.TurnToAir(true);

                SoundEngine.PlaySound(SoundID.Grab);
            }
        }
    }
}