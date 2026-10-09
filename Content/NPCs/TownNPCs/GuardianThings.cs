using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Waybound.Common.ModSystems;
using Waybound.Content.Items.Lore.Notes;
using Waybound.Particles;
using Waybound.UIs;
using SystemVector2 = System.Numerics.Vector2;

namespace Waybound.Content.NPCs.TownNPCs
{
    public class GuardianSystem : ModSystem
    {
        private static readonly HashSet<string> HiddenLayers = new HashSet<string>
        {
            "Vanilla: Resource Bars",
            "Vanilla: Hotbar",
            "Vanilla: Inventory",
            "Vanilla: Map / Minimap",
            "Vanilla: Info Accessories Bar",
            "Vanilla: Buffs"
        };

        private UserInterface noteInterface;
        private GuardianNoteUI noteUI;
        private GameTime lastUpdateUiGameTime;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            noteUI = new GuardianNoteUI();
            noteUI.Activate();
            noteInterface = new UserInterface();
        }

        public override void Unload()
        {
            noteUI = null;
            noteInterface = null;
        }

        public void OpenNotePanel(int npcIndex)
        {
            if (noteInterface == null || GuardianCutscene.Active)
                return;

            noteUI.Open(npcIndex);
            noteInterface.SetState(noteUI);
            Main.playerInventory = true; 
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }

        public void CloseNotePanel()
        {
            if (noteInterface?.CurrentState == null)
                return;

            noteUI.ReturnItem();
            noteInterface.SetState(null);
            SoundEngine.PlaySound(SoundID.MenuClose);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            lastUpdateUiGameTime = gameTime;

            if (noteInterface?.CurrentState != null)
                noteInterface.Update(gameTime);
        }

        public override void PostUpdateEverything() => GuardianCutscene.Update();

        public override void ModifyScreenPosition() => GuardianCutscene.ModifyCamera();

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventory = layers.FindIndex(l => l.Name.Equals("Vanilla: Inventory"));
            if (inventory != -1)
            {
                layers.Insert(inventory + 1, new LegacyGameInterfaceLayer("Waybound: Guardian Note", () =>
                {
                    if (lastUpdateUiGameTime != null && noteInterface?.CurrentState != null)
                        noteInterface.Draw(Main.spriteBatch, lastUpdateUiGameTime);
                    return true;
                }, InterfaceScaleType.UI));
            }
            // I can do UIs without Aeris now i think? So i thing i gonna have a burnout
            int index = layers.FindIndex(l => l.Name.Equals("Waybound: titleInterface"));
            if (index == -1)
                index = layers.FindIndex(l => l.Name.Equals("Waybound: dialogueInterface"));
            if (index == -1)
                index = layers.FindIndex(l => l.Name.Equals("Vanilla: Mouse Text"));

            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer("Waybound: Guardian Cutscene", () =>
                {
                    GuardianCutscene.Draw(Main.spriteBatch);
                    return true;
                }, InterfaceScaleType.None));
            }

            if (GuardianCutscene.Active)
            {
                foreach (GameInterfaceLayer layer in layers)
                {
                    if (HiddenLayers.Contains(layer.Name))
                        layer.Active = false;
                }
            }
        }
    }

    internal class GuardianNoteUI : UIState
    {
        private UIPanel panel;
        private NoteSlot slot;
        private UIText message;
        private int npcIndex = -1;

        public override void OnInitialize()
        {
            panel = new UIPanel();
            panel.Width.Set(340f, 0f);
            panel.Height.Set(220f, 0f);
            panel.HAlign = 0.5f;
            panel.VAlign = 0.3f;
            panel.BackgroundColor = new Color(8, 34, 24) * 0.95f;
            panel.BorderColor = new Color(255, 105, 150, 0);

            Append(panel);

            UIText title = new UIText("Note for the Guardian", 0.95f);
            title.HAlign = 0.5f;
            title.Top.Set(6f, 0f);
            panel.Append(title);

            slot = new NoteSlot();
            slot.HAlign = 0.5f;
            slot.Top.Set(40f, 0f);
            panel.Append(slot);

            message = new UIText("Place a note in the slot.", 0.8f);
            message.HAlign = 0.5f;
            message.Top.Set(108f, 0f);
            panel.Append(message);

            UITextPanel<string> close = new UITextPanel<string>("Close", 0.8f);
            close.Width.Set(130f, 0f);
            close.Height.Set(34f, 0f);
            close.HAlign = 0.5f;
            close.Top.Set(140f, 0f);
            close.OnLeftClick += (evt, element) => ModContent.GetInstance<GuardianSystem>().CloseNotePanel();
            close.BackgroundColor = new Color(14, 56, 40) * 0.95f;
            close.BorderColor = new Color(255, 105, 150, 0);
            panel.Append(close);
        }

        public void Open(int npc)
        {
            npcIndex = npc;
            message?.SetText("Place a note in the slot.");
        }

        // I still need to make a loc
        public void ReturnItem()
        {
            if (slot == null || slot.Item.IsAir)
                return;

            Player player = Main.LocalPlayer;
            player.QuickSpawnClonedItem(player.GetSource_Misc("GuardianNote"), slot.Item, slot.Item.stack);
            slot.Item.TurnToAir();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            Player player = Main.LocalPlayer;
            GuardianSystem system = ModContent.GetInstance<GuardianSystem>();
            NPC npc = npcIndex >= 0 && npcIndex < Main.maxNPCs ? Main.npc[npcIndex] : null;
            if (!Main.playerInventory || npc == null || !npc.active || npc.type != ModContent.NPCType<CrystalGuardian>()
                || Vector2.Distance(player.Center, npc.Center) > 260f)
            {
                system.CloseNotePanel();
                return;
            }

            if (panel.ContainsPoint(Main.MouseScreen))
                player.mouseInterface = true;

            if (GuardianCutscene.Active || slot.Item.ModItem is not LoreNote)
                return;
            int noteType = slot.Item.type;
            int guardian = npcIndex;

            system.CloseNotePanel();
            GuardianCutscene.Start(noteType, guardian);
        }
    }
    internal class NoteSlot : UIElement
    {
        public Item Item = new Item();

        public NoteSlot()
        {
            Width.Set(56f, 0f);
            Height.Set(56f, 0f);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Rectangle rect = GetDimensions().ToRectangle();
            float oldScale = Main.inventoryScale;
            Main.inventoryScale = 1f;

            if (ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;

                if (Main.mouseItem.IsAir || Main.mouseItem.ModItem is LoreNote)
                    ItemSlot.Handle(ref Item, ItemSlot.Context.ChestItem);
                else
                    Main.hoverItemName = "Only notes fit here";
            }

            ItemSlot.Draw(spriteBatch, ref Item, ItemSlot.Context.ChestItem, rect.TopLeft());
            Main.inventoryScale = oldScale;
        }
    }
    internal static class GuardianCutscene
    {
        private enum Phase { None, FadeIn, Title, Lines, FadeOut }

        private const int FadeTime = 40;
        private const int LineGap = 24;          
        private const int TitleTime = 160;      
        private const int LineBaseTime = 150;   
        private const int LineCharTime = 5;      // pause between lines (ticks)
        private const float LineScale = 0.5f;   
        private const int DialogueId = 7301;    

        private static Phase phase = Phase.None;
        private static LoreNote note;
        private static string[] lines;
        private static int npcIndex = -1;
        private static int timer;
        private static int lineIndex;
        private static int wait;
        private static bool prevLeft;
        private static bool prevRight;

        public static bool Active => phase != Phase.None;
        public static float Fade { get; private set; }

        private static NPC Guardian
        {
            get
            {
                if (npcIndex < 0 || npcIndex >= Main.maxNPCs)
                    return null;

                NPC npc = Main.npc[npcIndex];
                return npc.active && npc.type == ModContent.NPCType<CrystalGuardian>() ? npc : null;
            }
        }

        public static void Start(int noteItemType, int npc)
        {
            if (Active || ModContent.GetModItem(noteItemType) is not LoreNote lore)
                return;

            note = lore;
            lines = lore.Lines;
            npcIndex = npc;
            phase = Phase.FadeIn;
            timer = 0;
            lineIndex = 0;
            wait = 0;
            Fade = 0f;
            prevLeft = true;   
            prevRight = true;

            Main.playerInventory = false;
            SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.7f, Pitch = -0.3f });
        }

        public static void Cancel()
        {
            if (!Active)
                return;

            DialogueUI.Visible = false;
            Reset();
        }

        private static void BeginFadeOut()
        {
            DialogueUI.Visible = false;
            phase = Phase.FadeOut;
            timer = 0;
        }

        private static void Finish()
        {
            Player player = Main.LocalPlayer;

            if (player.GetModPlayer<GuardianPlayer>().MarkRead(note))
            {
                note.OnFirstRead(player);
                CombatText.NewText(player.getRect(), note.TextColor, "Story added", true);
            }

            Reset();
        }

        private static void Reset()
        {
            phase = Phase.None;
            note = null;
            lines = null;
            npcIndex = -1;
            Fade = 0f;
        }

        public static void Update()
        {
            if (!Active || Main.dedServ)
                return;

            NPC npc = Guardian;
            if (npc == null || Main.LocalPlayer.dead)
            {
                Cancel();
                return;
            }

            bool click = Main.mouseLeft && !prevLeft;
            bool skip = Main.mouseRight && !prevRight;
            prevLeft = Main.mouseLeft;
            prevRight = Main.mouseRight;

            bool key = (Main.keyState.IsKeyDown(Keys.Space) && !Main.oldKeyState.IsKeyDown(Keys.Space))
                    || (Main.keyState.IsKeyDown(Keys.Enter) && !Main.oldKeyState.IsKeyDown(Keys.Enter));
            bool next = click || key;

            timer++;
            SpawnFx(npc);

            if (skip && (phase == Phase.Title || phase == Phase.Lines))
            {
                BeginFadeOut();
                return;
            }

            switch (phase)
            {
                case Phase.FadeIn:
                    Fade = Smooth(timer / (float)FadeTime);
                    if (timer >= FadeTime)
                    {
                        phase = Phase.Title;
                        timer = 0;
                        ShowTitle();
                    }
                    break;

                case Phase.Title:
                    if (next)
                        DialogueUI.Visible = false;

                    if (!DialogueUI.Visible)
                    {
                        phase = Phase.Lines;
                        wait = 0;
                    }
                    break;

                case Phase.Lines:
                    if (next)
                        DialogueUI.Visible = false;

                    if (DialogueUI.Visible || ++wait < LineGap)
                        break;

                    wait = 0;
                    if (lineIndex >= lines.Length)
                        BeginFadeOut();
                    else
                        ShowLine(lines[lineIndex++]);
                    break;

                case Phase.FadeOut:
                    Fade = 1f - Smooth(timer / (float)FadeTime);
                    if (timer >= FadeTime)
                        Finish();
                    break;
            }
        }

        private static void ShowTitle()
        {
            SystemUI.dialogueUI.DisplayDialogue(note.NoteTitle,
                displayTime: TitleTime, fadeTime: 30, fontScale: 0.9f,
                textColor: note.TextColor, shadowColor: note.ShadowColor,
                textPosition: new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.4f),
                id: DialogueId, sound: true);
        }

        private static void ShowLine(string text)
        {
            SystemUI.dialogueUI.DisplayDialogue(text,
                displayTime: LineBaseTime + text.Length * LineCharTime, fadeTime: 28, fontScale: LineScale,
                whosespeaking: note.Speaker,
                textColor: note.TextColor, shadowColor: note.ShadowColor,
                textPosition: new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.74f),
                id: DialogueId);
        }

        private static void SpawnFx(NPC npc)
        {
            if (Main.rand.NextFloat() < 0.45f * Fade)
            {
                GuardianFx.Mote(npc.Center + Main.rand.NextVector2Circular(150f, 110f),
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.3f, 1.2f)),
                    Main.rand.NextFloat(8f, 16f), note.TextColor, Main.rand.Next(40, 70));
            }

            Lighting.AddLight(npc.Center, note.TextColor.ToVector3() * 0.6f * Fade);
        }

        public static void ModifyCamera()
        {
            NPC npc = Guardian;
            if (!Active || npc == null)
                return;

            Vector2 target = npc.Center - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f
                             + new Vector2(0f, Main.screenHeight * 0.12f);
            target.X = MathHelper.Clamp(target.X, 0f, Main.maxTilesX * 16f - Main.screenWidth);
            target.Y = MathHelper.Clamp(target.Y, 0f, Main.maxTilesY * 16f - Main.screenHeight);

            Main.screenPosition = Vector2.Lerp(Main.screenPosition, target, Fade);
        }

        public static void Draw(SpriteBatch sb)
        {
            if (!Active)
                return;

            Main.LocalPlayer.mouseInterface = true;

            int w = Main.screenWidth;
            int h = Main.screenHeight;
            Texture2D px = TextureAssets.MagicPixel.Value;
                
            sb.Draw(px, new Rectangle(0, 0, w, h), new Color(5, 28, 18) * (0.65f * Fade));

            int bar = (int)(h * 0.1f * Fade);
            Color barColor = new Color(2, 12, 8);
            sb.Draw(px, new Rectangle(0, 0, w, bar), barColor);
            sb.Draw(px, new Rectangle(0, h - bar, w, bar), barColor);

            Color edge = new Color(130, 200, 255) * (0.55f * Fade);
            sb.Draw(px, new Rectangle(0, bar - 2, w, 2), edge);
            sb.Draw(px, new Rectangle(0, h - bar, w, 2), edge);

            if (phase == Phase.Title || phase == Phase.Lines)
            {
                const string hint = "LMB / Space: next      RMB: skip";
                var font = FontAssets.MouseText.Value;
                Vector2 size = font.MeasureString(hint) * 0.8f;
                Utils.DrawBorderStringFourWay(sb, font, hint, (w - size.X) * 0.5f, h - bar * 0.5f - size.Y * 0.5f,
                    Color.White * 0.5f * Fade, Color.Black * Fade, Vector2.Zero, 0.8f);
            }
        }

        private static float Smooth(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            return t * t * (3f - 2f * t);
        }
    }

    internal static class GuardianFx
    {
        public static void Mote(Vector2 pos, Vector2 vel, float size, Color color, int life)
        {
            if (Main.dedServ || ParticleSystem.CrystalBuffer == null)
                return;

            ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new SystemVector2(size, size * Main.rand.NextFloat(0.75f, 1.05f)),
                new Color(color.R, color.G, color.B, 0) * Main.rand.NextFloat(0.85f, 1.15f),
                life
            ));
        }
    }
}