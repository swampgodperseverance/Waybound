using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Waybound.Content.Items.Accessories.PreHardmode;
using Waybound.Content.Items.Ammo.Arrows;
using Waybound.Content.Items.Lore.Notes;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Items.Placeable.Furniture;
using Waybound.Content.Items.Weapons.Melee.YoYos.GemYoyos;
using Waybound.Content.Items.Weapons.Ranged.LaserGuns.GemLaserGuns;

namespace Waybound.Content.NPCs.TownNPCs
{
    [AutoloadHead]
    public class CrystalGuardian : ModNPC
    {
        private const int IdleFrames = 4;     
        private const int IdleFrameTime = 16;

        private static readonly Color GlowColor = new Color(150, 215, 255, 0);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = IdleFrames;

            NPCID.Sets.NoTownNPCHappiness[Type] = true;
            NPCID.Sets.ActsLikeTownNPC[Type] = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.Waybound.Bestiary.CrystalGuardian")
            });
        }

        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 18;
            NPC.height = 40;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 250;
            NPC.HitSound = null;
            NPC.DeathSound = null;
            NPC.knockBackResist = 0f;

            NPC.dontTakeDamage = true;
            NPC.dontTakeDamageFromHostiles = true;
            NPC.immortal = true;
            for (int i = 0; i < NPC.buffImmune.Length; i++)
                NPC.buffImmune[i] = true;
            NPC.aiStyle = -1;
        }

        public override List<string> SetNPCNameList()
        {
            return new List<string> { "Quartz", "Prism", "Lumen", "Opal", "Aethon" };
        }

        public override bool CanTownNPCSpawn(int numTownNPCs) => false;

        public override bool CanChat() => true;

        public override void AI()
        {
            NPC.homeless = true;
            NPC.homeTileX = -1;
            NPC.homeTileY = -1;
            NPC.timeLeft = NPC.activeTime;
            NPC.velocity.X = 0f;

            Player closest = Main.player[Player.FindClosest(NPC.position, NPC.width, NPC.height)];
            if (closest.active && !closest.dead && Vector2.DistanceSquared(closest.Center, NPC.Center) < 420f * 420f)
                NPC.direction = NPC.spriteDirection = closest.Center.X >= NPC.Center.X ? 1 : -1;

            if (Main.dedServ)
                return;

            if (Main.rand.NextBool(GuardianCutscene.Active ? 8 : 22))
            {
                GuardianFx.Mote(NPC.Center + new Vector2(Main.rand.NextFloat(-16f, 16f), Main.rand.NextFloat(-24f, 12f)),
                    new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -Main.rand.NextFloat(0.2f, 0.8f)),
                    Main.rand.NextFloat(5f, 10f), GlowColor, Main.rand.Next(26, 44));
            }

            float l = 0.4f + 0.5f * GuardianCutscene.Fade;
            Lighting.AddLight(NPC.Center, 0.4f * l, 0.65f * l, 0.9f * l);
        }

        public override void FindFrame(int frameHeight)
        {
            int frames = Main.npcFrameCount[Type];
            if (frames <= 1)
                return;

            NPC.frameCounter += 1.0;
            NPC.frame.Y = ((int)(NPC.frameCounter / IdleFrameTime) % frames) * frameHeight;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[Type].Value;
            Rectangle frame = NPC.frame;
            Vector2 origin = new Vector2(frame.Width * 0.5f, frame.Height);
            Vector2 pos = NPC.Bottom - screenPos + new Vector2(0f, NPC.gfxOffY + 4f);
            SpriteEffects fx = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None; // sprite faces left

            float glow = 0.12f + 0.06f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2.2f) + 0.45f * GuardianCutscene.Fade;
            Color c = new Color((int)(GlowColor.R * glow), (int)(GlowColor.G * glow), (int)(GlowColor.B * glow), 0);

            spriteBatch.Draw(tex, pos, frame, c, 0f, origin, NPC.scale, fx, 0f);
            spriteBatch.Draw(tex, pos, frame, c * 0.5f, 0f, origin, NPC.scale * 1.05f, fx, 0f);
        }

        public override string GetChat()
        {
            GuardianPlayer gp = Main.LocalPlayer.GetModPlayer<GuardianPlayer>();

            WeightedRandom<string> chat = new WeightedRandom<string>();
            chat.Add("-");
            chat.Add("-.");
            chat.Add("-.");
            chat.Add("=.");

            if (gp.ReadCount == 0)
                chat.Add("You look like someone who picks up scraps of paper. Bring me any you find.", 3.0);
            else
                chat.Add($"I have read you {gp.ReadCount} stories so far. There are more out there.", 2.0);

            return chat;
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = Language.GetTextValue("LegacyInterface.28"); // "Shop"
            button2 = "Read Note";
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shop)
        {
            if (firstButton)
            {
                shop = "Shop";
                return;
            }

            Main.CloseNPCChatOrSign();
            ModContent.GetInstance<GuardianSystem>().OpenNotePanel(NPC.whoAmI);
        }
        private static Condition ReadNote<T>(string key, string text) where T : ModItem => new Condition(Language.GetOrRegister($"Mods.Waybound.Conditions.{key}", () => text),() => Main.LocalPlayer.GetModPlayer<GuardianPlayer>().HasRead<T>());

        private static Condition ReadGroup(string key, string text, string group, int needed)  => new Condition(Language.GetOrRegister($"Mods.Waybound.Conditions.{key}", () => text),() => Main.LocalPlayer.GetModPlayer<GuardianPlayer>().ReadInGroup(group) >= needed);
        public override void AddShops()
        {
            NPCShop shop = new NPCShop(Type, "Shop");

            shop.Add<DeepBoots>()
                   .Add<DeepTreeItem>()
                   .Add<CrystalArrow>();

            shop.Add<DeepCrystalShardItem>(
                ReadNote<DeepstoneGolemNote>("ReadDeepstone", "Read the note \"The Deepstone Hum\""));

            Condition desert3 = ReadGroup("ReadDesert3", "Read 3 desert notes", NoteGroups.Desert, 3);
            shop.Add<AmberLaserGun>(desert3)
                .Add<AmberYoYo>(desert3);

            shop.Register();
        }
    }

    public class GuardianPlayer : ModPlayer
    {
        private readonly HashSet<string> readNotes = new HashSet<string>();
        public bool HasRead<T>() where T : ModItem => readNotes.Contains(ModContent.GetInstance<T>().FullName);

        public int ReadInGroup(string group)
        {
            int count = 0;
            foreach (string name in readNotes)
            {
                if (ModContent.TryFind(name, out ModItem item) && item is LoreNote note && note.Group == group)
                    count++;
            }
            return count;
        }
        public int ReadCount => readNotes.Count;

        public bool HasRead(ModItem note) => readNotes.Contains(note.FullName);
        public bool MarkRead(ModItem note) => readNotes.Add(note.FullName);

        public override void SaveData(TagCompound tag) => tag["readNotes"] = readNotes.ToList();

        public override void LoadData(TagCompound tag)
        {
            readNotes.Clear();
            foreach (string name in tag.GetList<string>("readNotes"))
                readNotes.Add(name);
        }

        public override void SetControls()
        {
            if (Player.whoAmI != Main.myPlayer || !GuardianCutscene.Active)
                return;

            Player.controlLeft = false;
            Player.controlRight = false;
            Player.controlUp = false;
            Player.controlDown = false;
            Player.controlJump = false;
            Player.controlUseItem = false;
            Player.controlUseTile = false;
            Player.controlThrow = false;
            Player.controlHook = false;
            Player.controlMount = false;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (Player.whoAmI == Main.myPlayer && GuardianCutscene.Active)
                GuardianCutscene.Cancel();
        }
    }
}