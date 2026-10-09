using System;
using System.Collections.Generic;
using System.Linq;
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
using Waybound.Common.WUtils;
using Waybound.Content.Race;

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

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.gameMenu || Main.LocalPlayer == null)
                return;

            var bag = Main.LocalPlayer.GetModPlayer<DesfosBagPlayer>();

            if (DesfosRace.IsDesfo(Main.LocalPlayer))
            {
                tooltips.Add(new TooltipLine(Mod, "DesfosBagRace",
                    Loc.GetTips("DesfosBag.RaceBonus"))
                {
                    OverrideColor = Color.Gold
                });
            }

            int slots = bag.SlotsFromGold();
            long gold = bag.GetTotalGoldValue();

            tooltips.Add(new TooltipLine(Mod, "DesfosBagSlots",
                string.Format(Loc.GetTips("DesfosBag.Slots"),
                    slots,
                    DesfosBagPlayer.MaxSlots,
                    DesfosBagPlayer.GoldPerSlot))
            {
                OverrideColor = new Color(255, 225, 150)
            });

            if (slots < DesfosBagPlayer.MaxSlots)
            {
                long next = (slots - DesfosBagPlayer.BaseSlots + 1L) * DesfosBagPlayer.GoldPerSlot;

                tooltips.Add(new TooltipLine(Mod, "DesfosBagNext",
                    string.Format(Loc.GetTips("DesfosBag.NextSlot"),
                        next,
                        gold))
                {
                    OverrideColor = new Color(190, 190, 190)
                });
            }
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
        public float convertDuration = 60f;                       // depends on a queue btw
        public Color convertColor = new Color(255, 225, 150);   
        public Dictionary<string, int> pityRare = new();
        public Dictionary<string, int> pityJackpot = new();
        public HashSet<string> obtained = new();

        public override void Initialize()
        {
            for (int i = 0; i < MaxSlots; i++)
            {
                bagItems[i] = new Item();
                bagItems[i].TurnToAir(true);
            }
            pityRare = new();
            pityJackpot = new();
            obtained = new();
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

        public const bool DesfoNeedsBagItem = true;

        public override void PostUpdateEquips()
        {
            if (!DesfosRace.IsDesfo(Player)) { return; }
            if (!DesfoNeedsBagItem || HasBagInInventory()) { equippedBag = true; }
        }

        bool HasBagInInventory()
        {
            int bagType = ModContent.ItemType<DesfosBag>();
            for (int i = 0; i < Player.inventory.Length; i++)
            {
                Item it = Player.inventory[i];
                if (it != null && !it.IsAir && it.type == bagType) { return true; }
            }
            return false;
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

        public const int BaseSlots = 5;    // слотов сразу
        public const int GoldPerSlot = 5;  // каждые 5 золотых в инвентаре = +1 слот (до MaxSlots)

        /// <summary>Сколько слотов положено по золоту: 5 + золото/5, максимум 20 (т.е. 75 золотых = максимум).</summary>
        public int SlotsFromGold()
        {
            long gold = GetTotalGoldValue();
            int bonus = (int)Math.Min(MaxSlots, gold / GoldPerSlot);
            return Math.Clamp(BaseSlots + bonus, BaseSlots, MaxSlots);
        }

        public void RecalculateSlots()
        {
            // ВАЖНО: слоты никогда не сжимаются ниже последнего занятого, иначе предметы "пропадают" из виду,
            // когда игрок тратит деньги.
            int highest = -1;
            for (int i = 0; i < MaxSlots; i++)
            {
                if (bagItems[i] != null && !bagItems[i].IsAir) { highest = i; }
            }
            extraSlots = Math.Clamp(Math.Max(SlotsFromGold(), highest + 1), BaseSlots, MaxSlots);
        }

        public long GetTotalGoldValue()
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
                if (!DesfosPools.TryGet(item.type, out DesfosPool pool) || item.stack < pool.Required) continue;
                if (!pool.CanRoll(this)) continue; 

                int pending = item.stack / pool.Required;
                convertDuration = pending >= 4 ? 18f : pending >= 2 ? 35f : 60f;
                convertTimer = convertDuration;
                convertingSlot = i;

                item.stack -= pool.Required;
                if (item.stack <= 0)
                    item.TurnToAir(true);

                Item reward = pool.Roll(this, out DesfosTier tier);
                convertColor = DesfosPools.TierColor(tier);
                DeliverReward(reward, i);
                PlayConvertEffects(tier, reward);
                break;
            }
        }

        private void DeliverReward(Item reward, int preferredSlot)
        {
            if (reward == null || reward.IsAir) return;

            for (int i = 0; i < extraSlots && reward.stack > 0; i++)
            {
                Item s = bagItems[i];
                if (s == null || s.IsAir || s.type != reward.type || s.prefix != reward.prefix || s.stack >= s.maxStack) continue;
                int move = Math.Min(reward.stack, s.maxStack - s.stack);
                s.stack += move;
                reward.stack -= move;
            }

            while (reward.stack > 0)
            {
                int slot = -1;
                if (preferredSlot < extraSlots && (bagItems[preferredSlot] == null || bagItems[preferredSlot].IsAir))
                    slot = preferredSlot;
                else
                {
                    for (int j = 0; j < extraSlots; j++)
                    {
                        if (bagItems[j] == null || bagItems[j].IsAir) { slot = j; break; }
                    }
                }

                if (slot == -1)
                {
                    Player.QuickSpawnItem(Player.GetSource_Misc("DesfosBag"), reward, reward.stack);
                    return;
                }

                Item part = reward.Clone();
                part.stack = Math.Min(reward.stack, reward.maxStack);
                bagItems[slot] = part;
                reward.stack -= part.stack;
            }
        }

        private void PlayConvertEffects(DesfosTier tier, Item reward)
        {
            switch (tier)
            {
                case DesfosTier.Jackpot:
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.2f, Volume = 0.9f }, Player.Center);
                    SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.7f, Volume = 0.6f }, Player.Center);
                    break;
                case DesfosTier.Rare:
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.5f, Volume = 0.7f }, Player.Center);
                    break;
                default:
                    SoundEngine.PlaySound(SoundID.Item37 with { Pitch = 0.35f, Volume = 0.75f }, Player.Center);
                    SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.6f, Volume = 0.4f }, Player.Center);
                    break;
            }

            if (tier >= DesfosTier.Rare && !reward.IsAir)
            {
                int dusts = tier == DesfosTier.Jackpot ? 36 : 20;
                for (int i = 0; i < dusts; i++)
                {
                    Dust d = Dust.NewDustPerfect(Player.Center, DustID.GoldFlame,
                        (MathHelper.TwoPi * i / dusts).ToRotationVector2() * Main.rand.NextFloat(2f, 5f),
                        120, convertColor, 1.4f);
                    d.noGravity = true;
                }
                string text = reward.stack > 1 ? $"{reward.Name} x{reward.stack}" : reward.Name;
                CombatText.NewText(Player.getRect(), convertColor, text, tier == DesfosTier.Jackpot);
            }
        }

        public override void SaveData(TagCompound tag)
        {
            var list = new List<Item>();
            for (int i = 0; i < MaxSlots; i++)
                list.Add(bagItems[i] ?? new Item());
            tag["bagItems"] = list;

            tag["pityRareKeys"] = pityRare.Keys.ToList();
            tag["pityRareVals"] = pityRare.Values.ToList();
            tag["pityJackKeys"] = pityJackpot.Keys.ToList();
            tag["pityJackVals"] = pityJackpot.Values.ToList();
            tag["obtained"] = obtained.ToList();
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

            pityRare = LoadDict(tag, "pityRareKeys", "pityRareVals");
            pityJackpot = LoadDict(tag, "pityJackKeys", "pityJackVals");
            obtained = tag.ContainsKey("obtained") ? new HashSet<string>(tag.GetList<string>("obtained")) : new HashSet<string>();
        }

        private static Dictionary<string, int> LoadDict(TagCompound tag, string keysName, string valsName)
        {
            var dict = new Dictionary<string, int>();
            if (!tag.ContainsKey(keysName) || !tag.ContainsKey(valsName)) return dict;
            var keys = tag.GetList<string>(keysName);
            var vals = tag.GetList<int>(valsName);
            for (int i = 0; i < Math.Min(keys.Count, vals.Count); i++)
                dict[keys[i]] = vals[i];
            return dict;
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

            // стандартный пиксель вместо создания Texture2D на каждый слот (утечка видеопамяти в старой версии)
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Color border = Color.Gold * (0.9f * opacity);
            int b = 2;
            Rectangle px = new Rectangle(0, 0, 1, 1);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, b), px, border);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - b, rect.Width, b), px, border);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, b, rect.Height), px, border);
            spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - b, rect.Y, b, rect.Height), px, border);

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

                // тонкая полоска прогресса для предметов, которые сумка умеет конвертировать
                if (DesfosPools.TryGet(item.type, out DesfosPool pool))
                {
                    float fill = MathHelper.Clamp(item.stack / (float)pool.Required, 0f, 1f);
                    Rectangle bar = new Rectangle(rect.X + 4, rect.Bottom - 6, rect.Width - 8, 3);
                    spriteBatch.Draw(pixel, bar, px, new Color(0, 0, 0) * (0.55f * opacity));
                    Color barColor = fill >= 1f ? new Color(255, 225, 120) : new Color(200, 160, 70);
                    spriteBatch.Draw(pixel, new Rectangle(bar.X, bar.Y, (int)(bar.Width * fill), bar.Height), px, barColor * opacity);
                }
            }

            if (bagPlayer.convertingSlot == SlotIndex && bagPlayer.convertTimer > 0)
            {
                float progress = 1f - (bagPlayer.convertTimer / bagPlayer.convertDuration);
                float intensity = (float)Math.Sin(progress * MathHelper.Pi);
                intensity = MathHelper.SmoothStep(0f, 1f, intensity);

                Color tint = bagPlayer.convertColor; // цвет зависит от тира награды

                spriteBatch.Draw(invBack, rect, tint * (0.2f * intensity * opacity));
                spriteBatch.Draw(invBack, rect, tint * (0.22f * intensity * opacity));

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
                        Color rayColor = Color.Lerp(tint, Color.White, 0.35f) * (0.38f * intensity);

                        Main.EntitySpriteDraw(ray, center, null, rayColor, angle, origin,
                            new Vector2(scaleX, scaleY), SpriteEffects.None, 0);
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
                        140, tint, 0.55f + intensity * 0.25f);
                    d.noGravity = true;
                    d.fadeIn = 0.9f;
                }
            }

            Utils.DrawBorderString(spriteBatch, (SlotIndex + 1).ToString(),
                new Vector2(rect.X + 3f, rect.Y + 2f), Color.White * (0.22f * opacity), 0.5f);

            HandleVanillaSlot(bagPlayer);
        }

        // Весь ввод идёт через ванильный ItemSlot (как у сундука): слияние стаков, частичный перенос,
        // замена предметов, правый клик с удержанием. Вручную больше ничего не двигаем - значит и дюпать нечему.
        const int SlotContext = ItemSlot.Context.ChestItem;

        void HandleVanillaSlot(DesfosBagPlayer bag)
        {
            if (!bag.bagActive || bag.opacity < 0.7f) { return; }
            if (SlotIndex >= bag.extraSlots) { return; }
            if (PlayerInput.IgnoreMouseInterface) { return; }
            if (!ContainsPoint(Main.MouseScreen)) { return; }

            // каждый кадр, пока курсор над слотом: иначе ваниль решит, что клик "мимо интерфейса"
            // (использует предмет / выбрасывает его из руки), а мы ещё и положим его в слот
            Main.LocalPlayer.mouseInterface = true;

            ref Item slot = ref bag.bagItems[SlotIndex];
            ItemSlot.OverrideHover(ref slot, SlotContext);

            if (Main.mouseLeftRelease && Main.mouseLeft)
            {
                ItemSlot.LeftClick(ref slot, SlotContext);
                Recipe.FindRecipes();
            }
            else
            {
                ItemSlot.RightClick(ref slot, SlotContext);
            }

            ItemSlot.MouseHover(ref slot, SlotContext);
        }
    }
}