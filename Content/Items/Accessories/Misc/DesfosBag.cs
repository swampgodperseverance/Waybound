using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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
using Waybound.Content.Items.Accessories.Hardmode;

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

        
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            var bagPlayer = Main.LocalPlayer.GetModPlayer<DesfosBagPlayer>();
            if (bagPlayer == null) return true;

            int extra = Math.Max(0, bagPlayer.extraSlots - 5);
            float intensity = MathHelper.Clamp(extra / 10f, 0.15f, 0.85f); 

            float pulse = 0.7f + 0.3f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3.2f);

            Color outlineColor = new Color(255, 210, 90) * (intensity * pulse * 0.65f);

            Texture2D tex = TextureAssets.Item[Item.type].Value;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = Vector2.UnitY.RotatedBy(MathHelper.PiOver2 * i) * 1.6f;
                spriteBatch.Draw(tex, position + offset, frame, outlineColor, 0f, origin, scale, SpriteEffects.None, 0f);
            }

            return true; 
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            var bagPlayer = Main.LocalPlayer.GetModPlayer<DesfosBagPlayer>();
            if (bagPlayer == null) return true;

            int extra = Math.Max(0, bagPlayer.extraSlots - 5);
            float intensity = MathHelper.Clamp(extra / 10f, 0.12f, 0.7f);
            float pulse = 0.65f + 0.35f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.8f);

            Color outlineColor = new Color(255, 215, 100) * (intensity * pulse * 0.55f);

            Texture2D tex = TextureAssets.Item[Item.type].Value;
            Rectangle frame = tex.Frame();
            Vector2 origin = frame.Size() / 2f;
            Vector2 drawPos = Item.Center - Main.screenPosition;

            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = Vector2.UnitY.RotatedBy(MathHelper.PiOver2 * i + Main.GlobalTimeWrappedHourly * 0.4f) * 1.8f;
                spriteBatch.Draw(tex, drawPos + offset, frame, outlineColor, rotation, origin, scale, SpriteEffects.None, 0f);
            }

            return true;
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

        public int convertingSlot = -1;
        public float convertTimer = 0f;
        public const float ConvertDuration = 60f;

        public class ConvertEntry
        {
            public int RequiredAmount;
            public Func<Item> GenerateReward;

            public ConvertEntry(int amount, Func<Item> reward)
            {
                RequiredAmount = amount;
                GenerateReward = reward;
            }
        }

        public static readonly Dictionary<int, ConvertEntry> ConvertibleItems = new()
        {
            {
                ItemID.StoneBlock,
                new ConvertEntry(150, () =>
                {
                    Item reward = new Item();
                    float roll = Main.rand.NextFloat();

                    if (roll < 0.02f)
                        reward.SetDefaults(ItemID.EnchantedSword);
                    else if (roll < 0.05f)
                        reward.SetDefaults(ItemID.EnchantedBoomerang);
                    else if (roll < 0.08f)
                        reward.SetDefaults(ItemID.Shackle);
                    else if (roll < 0.12f)
                        reward.SetDefaults(ItemID.MiningHelmet);
                    else if (roll < 0.50f)
                    {
                        int[] ores = { ItemID.CopperOre, ItemID.TinOre, ItemID.IronOre, ItemID.LeadOre, ItemID.SilverOre, ItemID.TungstenOre, ItemID.GoldOre, ItemID.PlatinumOre };
                        reward.SetDefaults(ores[Main.rand.Next(ores.Length)]);
                        reward.stack = Main.rand.Next(25, 56);
                    }
                    else
                    {
                        int[] gems = { ItemID.Ruby, ItemID.Sapphire, ItemID.Amethyst };
                        reward.SetDefaults(gems[Main.rand.Next(gems.Length)]);
                        reward.stack = Main.rand.Next(3, 7);
                    }
                    return reward;
                })
            },

            {
                ItemID.SandBlock,
                new ConvertEntry(150, () =>
                {
                    Item reward = new Item();
                    float roll = Main.rand.NextFloat();

                    if (roll < 0.15f)
                    {
                        reward.SetDefaults(ItemID.AntlionMandible);
                        reward.stack = Main.rand.Next(2, 5);
                    }
                    else if (roll < 0.35f)
                    {
                        reward.SetDefaults(ItemID.DesertFossil);
                        reward.stack = Main.rand.Next(8, 18);
                    }
                    else if (roll < 0.55f)
                    {
                        reward.SetDefaults(ItemID.HardenedSand);
                        reward.stack = Main.rand.Next(20, 40);
                    }
                    else if (roll < 0.75f)
                    {
                        reward.SetDefaults(ItemID.Sandstone);
                        reward.stack = Main.rand.Next(15, 30);
                    }
                    else
                    {
                        reward.SetDefaults(ItemID.Amber);
                        reward.stack = Main.rand.Next(2, 5);
                    }
                    return reward;
                })
            },

            {
                ItemID.Ectoplasm,
                new ConvertEntry(10, () =>
                {
                    Item reward = new Item();
                    float roll = Main.rand.NextFloat();

                    if (roll < 1f)
                    {
                        reward.SetDefaults(ModContent.ItemType<TheOriginOfSymmetry>());
                        reward.stack = Main.rand.Next(1, 1);
                    }
                    return reward;
                })
            },

            {
                ItemID.ClayBlock,
                new ConvertEntry(120, () =>
                {
                    Item reward = new Item();
                    float roll = Main.rand.NextFloat();

                    if (roll < 0.30f)
                    {
                        reward.SetDefaults(ItemID.RedBrick);
                        reward.stack = Main.rand.Next(15, 30);
                    }
                    else if (roll < 0.55f)
                    {
                        reward.SetDefaults(ItemID.Bowl);
                        reward.stack = Main.rand.Next(2, 5);
                    }
                    else if (roll < 0.75f)
                    {
                        reward.SetDefaults(ItemID.ClayPot);
                    }
                    else
                    {
                        reward.SetDefaults(ItemID.PinkVase);
                    }
                    return reward;
                })
            },

            {
                ItemID.MudBlock,
                new ConvertEntry(180, () =>
                {
                    Item reward = new Item();
                    float roll = Main.rand.NextFloat();

                    if (roll < 0.25f)
                    {
                        reward.SetDefaults(ItemID.JungleGrassSeeds);
                        reward.stack = Main.rand.Next(3, 8);
                    }
                    else if (roll < 0.45f)
                    {
                        reward.SetDefaults(ItemID.RichMahogany);
                        reward.stack = Main.rand.Next(20, 40);
                    }
                    else if (roll < 0.65f)
                    {
                        reward.SetDefaults(ItemID.Vine);
                        reward.stack = Main.rand.Next(3, 7);
                    }
                    else if (roll < 0.85f)
                    {
                        reward.SetDefaults(ItemID.JungleSpores);
                        reward.stack = Main.rand.Next(2, 5);
                    }
                    else
                    {
                        reward.SetDefaults(ItemID.Stinger);
                        reward.stack = Main.rand.Next(1, 4);
                    }
                    return reward;
                })
            },

            {
                ItemID.SnowBlock,
                new ConvertEntry(160, () =>
                {
                    Item reward = new Item();
                    float roll = Main.rand.NextFloat();

                    if (roll < 0.25f)
                    {
                        reward.SetDefaults(ItemID.IceBlock);
                        reward.stack = Main.rand.Next(20, 40);
                    }
                    else if (roll < 0.45f)
                    {
                        reward.SetDefaults(ItemID.BorealWood);
                        reward.stack = Main.rand.Next(25, 50);
                    }
                    else if (roll < 0.65f)
                    {
                        reward.SetDefaults(ItemID.Snowball);
                        reward.stack = Main.rand.Next(30, 60);
                    }
                    else if (roll < 0.85f)
                    {
                        reward.SetDefaults(ItemID.IceTorch);
                        reward.stack = Main.rand.Next(10, 25);
                    }
                    else
                    {
                        reward.SetDefaults(ItemID.FrostCore);
                    }
                    return reward;
                })
            },

            {
                ItemID.IceBlock,
                new ConvertEntry(140, () =>
                {
                    Item reward = new Item();
                    float roll = Main.rand.NextFloat();

                    if (roll < 0.20f)
                    {
                        reward.SetDefaults(ItemID.IceTorch);
                        reward.stack = Main.rand.Next(15, 30);
                    }
                    else if (roll < 0.40f)
                    {
                        reward.SetDefaults(ItemID.IceBrick);
                        reward.stack = Main.rand.Next(15, 30);
                    }
                    else if (roll < 0.60f)
                    {
                        reward.SetDefaults(ItemID.FrostDaggerfish);
                        reward.stack = Main.rand.Next(5, 12);
                    }
                    else if (roll < 0.80f)
                    {
                        reward.SetDefaults(ItemID.IceBoomerang);
                    }
                    else
                    {
                        reward.SetDefaults(ItemID.IceBlade);
                    }
                    return reward;
                })
            },

            {
                ItemID.AshBlock,
                new ConvertEntry(130, () =>
                {
                    Item reward = new Item();
                    float roll = Main.rand.NextFloat();

                    if (roll < 0.20f)
                    {
                        reward.SetDefaults(ItemID.Hellstone);
                        reward.stack = Main.rand.Next(8, 16);
                    }
                    else if (roll < 0.40f)
                    {
                        reward.SetDefaults(ItemID.Obsidian);
                        reward.stack = Main.rand.Next(10, 20);
                    }
                    else if (roll < 0.60f)
                    {
                        reward.SetDefaults(ItemID.FireblossomSeeds);
                        reward.stack = Main.rand.Next(2, 5);
                    }
                    else if (roll < 0.80f)
                    {
                        reward.SetDefaults(ItemID.AshWood);
                        reward.stack = Main.rand.Next(20, 40);
                    }
                    else
                    {
                        reward.SetDefaults(ItemID.LavaCharm);
                    }
                    return reward;
                })
            },
        };

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
            int k = (int)Math.Floor(Math.Sqrt(goldValue));
            extraSlots = Math.Clamp(5 + k, 5, MaxSlots);
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
                TryConvert();
            }
            else
            {
                opacity = MathHelper.Clamp(opacity - 0.09f, 0f, 1f);
                if (opacity <= 0.01f)
                    isClosing = false;
            }

            if (convertTimer > 0f)
            {
                convertTimer--;
                if (convertTimer <= 0f)
                    convertingSlot = -1;
            }
        }

        private void TryConvert()
        {
            if (convertingSlot != -1) return;

            for (int i = 0; i < extraSlots; i++)
            {
                Item item = bagItems[i];
                if (item == null || item.IsAir) continue;

                if (ConvertibleItems.TryGetValue(item.type, out ConvertEntry entry) && item.stack >= entry.RequiredAmount)
                {
                    convertingSlot = i;
                    convertTimer = ConvertDuration;

                    item.stack -= entry.RequiredAmount;
                    if (item.stack <= 0)
                        item.TurnToAir(true);

                    Item reward = entry.GenerateReward();

                    if (item.IsAir)
                    {
                        bagItems[i] = reward;
                    }
                    else
                    {
                        bool placed = false;
                        for (int j = 0; j < extraSlots; j++)
                        {
                            if (bagItems[j] == null || bagItems[j].IsAir)
                            {
                                bagItems[j] = reward;
                                placed = true;
                                break;
                            }
                        }
                        if (!placed)
                        {
                            Player.QuickSpawnItem(Player.GetSource_Misc("DesfosBag"), reward);
                        }
                    }

                    // Более приятный звук
                    SoundEngine.PlaySound(SoundID.Item37 with { Pitch = 0.35f, Volume = 0.75f }, Player.Center);
                    SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.6f, Volume = 0.4f }, Player.Center);
                    break;
                }
            }
        }

        // SaveData / LoadData без изменений
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
            float startY = 27f;

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
        private static Asset<Texture2D> rayTexture;

        public DesfosBagSlot(int slotIndex)
        {
            SlotIndex = slotIndex;
            Width.Set(42f, 0f);
            Height.Set(42f, 0f);
        }

        public override void OnInitialize()
        {
            if (rayTexture == null)
                rayTexture = ModContent.Request<Texture2D>("Waybound/Assets/Textures/Ray");
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
            if (bagPlayer.convertingSlot == SlotIndex && bagPlayer.convertTimer > 0)
            {
                float progress = 1f - (bagPlayer.convertTimer / DesfosBagPlayer.ConvertDuration);

                float intensity = (float)Math.Sin(progress * MathHelper.Pi);
                intensity = MathHelper.SmoothStep(0f, 1f, intensity); 

                Color flash = new Color(255, 225, 150) * (0.18f * intensity * opacity);
                spriteBatch.Draw(TextureAssets.InventoryBack.Value, rect, flash);

                Color glow = new Color(255, 210, 120) * (0.22f * intensity * opacity);
                spriteBatch.Draw(invBack, rect, glow);

                if (rayTexture != null && rayTexture.IsLoaded)
                {
                    Texture2D ray = rayTexture.Value;
                    Vector2 center = GetDimensions().Center();
                    Vector2 origin = new Vector2(ray.Width / 2f, ray.Height);

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

                    int rayCount = 6; 
                    float globalRot = Main.GlobalTimeWrappedHourly * 1.6f + progress * 2.2f; 

                    for (int i = 0; i < rayCount; i++)
                    {
                        float angle = MathHelper.TwoPi / rayCount * i + globalRot;

                        float scaleX = 0.09f + 0.04f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3.5f + i);
                        float scaleY = 0.18f + 0.28f * intensity;

                        Color rayColor = new Color(255, 235, 170) * (0.38f * intensity);

                        Main.EntitySpriteDraw(ray, center, null, rayColor,
                            angle, origin,
                            new Vector2(scaleX, scaleY),
                            SpriteEffects.None, 0);
                    }

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                }

                if (Main.rand.NextBool(5)) 
                {
                    Vector2 pos = GetDimensions().Center() + Main.rand.NextVector2Circular(11f, 11f);
                    Dust d = Dust.NewDustPerfect(pos, DustID.GoldFlame,
                        Main.rand.NextVector2Circular(0.6f, 0.6f) * (0.4f + intensity * 0.5f),
                        140,                                          
                        new Color(255, 230, 160),
                        0.55f + intensity * 0.25f);                  

                    d.noGravity = true;
                    d.fadeIn = 0.9f;
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
        {
            var bagPlayer = Main.LocalPlayer?.GetModPlayer<DesfosBagPlayer>();
            if (bagPlayer == null || !bagPlayer.bagActive || bagPlayer.opacity < 0.7f) return;
            if (SlotIndex >= bagPlayer.extraSlots) return;

            Main.LocalPlayer.mouseInterface = true;
            Main.mouseLeftRelease = false;
            Main.mouseRightRelease = false;
            Main.mouseLeft = false;
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