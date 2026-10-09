using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.ModSystems;
using Waybound.Common.Systems;
using Waybound.Content.BossBars;
using Waybound.Content.Items.Materials.Misc;
using Waybound.Content.Items.Mounts;
using Waybound.Content.Items.Placeable.Bosses;
using Waybound.Content.Items.Vanity.BossMasks;
using Waybound.Helpers;
using Waybound.Particles;
using static Waybound.Content.NPCs.Bosses.Themis.ThemisRocket;

namespace Waybound.Content.NPCs.Bosses.Themis
{
    [AutoloadBossHead]
    public class Themis : ModNPC
    {
        public int frame;
        public int phase = 1;
        public int text;
        public int cutsceneCounter = 1;
        public Color txtColor = new Color(255, 245, 219);
        public int teleportIndex;
        public float outlineOpacity;
        public Vector2 teleportTarget;

        //contur outline color idk 
        private Color outlineColor = new Color(160, 160, 160);
        private int lastAttack;

        private static readonly Color TeleportColor = new Color(120, 200, 255);
        private static readonly Color StompColor = new Color(255, 140, 40);
        private static readonly Color DefaultOutline = new Color(160, 160, 160);

        private static Asset<Texture2D> mechTexture;
        private static Asset<Texture2D> handsTexture;
        private static Asset<Texture2D> mechHandsTexture;

        public override void Load()
        {
            if (Main.dedServ) return;
            mechTexture = ModContent.Request<Texture2D>("Waybound/Content/NPCs/Bosses/Themis/ThemisMech");
            handsTexture = ModContent.Request<Texture2D>("Waybound/Content/NPCs/Bosses/Themis/ThemisHands");
            mechHandsTexture = ModContent.Request<Texture2D>("Waybound/Content/NPCs/Bosses/Themis/ThemisMechHands");
        }

        public override void Unload()
        {
            mechTexture = null;
            handsTexture = null;
            mechHandsTexture = null;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 17;
            NPCID.Sets.TrailCacheLength[NPC.type] = 5;
            NPCID.Sets.TrailingMode[NPC.type] = 0;

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                CustomTexturePath = "Waybound/Content/NPCs/Bosses/Themis/Themis_Bestiary",
                Position = new Vector2(0f, 15f),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 0f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement("Mods.Waybound.Bestiary.Themis")
            });
        }

        public override void SetDefaults()
        {
            NPC.width = 36;
            NPC.height = 70;
            NPC.damage = 30;
            NPC.defense = 4;
            NPC.lifeMax = 2800;
            NPC.HitSound = SoundID.NPCHit1;
            if (DownedBossSystem.DownedThemis)
            {
                NPC.HitSound = SoundID.NPCHit4;
                NPC.DeathSound = SoundID.Item14;
            }
            NPC.alpha = 0;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.value = Item.buyPrice(gold: 6);
            NPC.boss = true;
            NPC.friendly = false;
            NPC.aiStyle = -1;
            NPC.netAlways = true;
            NPC.BossBar = ModContent.GetInstance<ThemisBossBar>();
            if (!Main.dedServ)
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/DesertHermit");
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.75f * balance);
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.TargetClosest(true);
            base.OnSpawn(source);
        }

        private static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private static void EndAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private static void RingBurst(Vector2 center, int count, float speed, Color color,
            float sizeMin, float sizeMax, int lifeMin, int lifeMax, float startRadius = 0f)
        {
            if (Main.dedServ) return;

            float offset = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < count; i++)
            {
                float angle = offset + MathHelper.TwoPi * i / count;
                Vector2 dir = angle.ToRotationVector2();

                ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                    (center + dir * startRadius).ToNumerics(),
                    (dir * speed * Main.rand.NextFloat(0.85f, 1.15f)).ToNumerics(),
                    angle,
                    new System.Numerics.Vector2(Main.rand.NextFloat(sizeMin, sizeMax)),
                    color,
                    Main.rand.Next(lifeMin, lifeMax)
                ));
            }
        }

        private static void MuzzleFlash(Vector2 pos, Vector2 dir)
        {
            Lighting.AddLight(pos, 1.5f, 0.75f, 0.5f);
            if (Main.dedServ) return;

            dir = dir.SafeNormalize(Vector2.UnitX);

            ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                System.Numerics.Vector2.Zero,
                Main.rand.NextFloat(MathHelper.TwoPi),
                new System.Numerics.Vector2(Main.rand.NextFloat(34f, 46f)),
                new Color(255, 200, 120, 230),
                8
            ));

            for (int i = 0; i < 6; i++)
            {
                Vector2 v = dir.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 6f);
                ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                    pos.ToNumerics(),
                    v.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(Main.rand.NextFloat(8f, 14f)),
                    new Color(255, 170, 80, 200),
                    Main.rand.Next(10, 18)
                ));
            }

            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, dir.RotatedByRandom(0.6f) * Main.rand.NextFloat(2f, 5f), 100, default, Main.rand.NextFloat(1.1f, 1.7f));
                d.noGravity = true;
            }
        }

        private static void TeleportBurst(Vector2 pos)
        {
            if (Main.dedServ) return;

            for (int i = 0; i < 6; i++)
            {
                float s = 30f + i * 14f;
                ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                    position: pos.ToNumerics(),
                    velocity: System.Numerics.Vector2.Zero,
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new System.Numerics.Vector2(s + Main.rand.NextFloat(-4f, 4f)),
                    color: TeleportColor,
                    duration: 12 + i * 2
                ));
            }

            RingBurst(pos, 14, 5f, new Color(150, 220, 255, 200), 10f, 16f, 14, 22, 10f);
        }

        private void PhaseAura()
        {
            if (Main.dedServ || phase != 2 || NPC.alpha > 200) return;
            if (!Main.rand.NextBool(5)) return;

            Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.6f, NPC.height * 0.5f);
            Vector2 vel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -Main.rand.NextFloat(0.6f, 1.8f));
            Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 100, default, Main.rand.NextFloat(0.9f, 1.4f));
            d.noGravity = true;
            Lighting.AddLight(NPC.Center, 0.35f, 0.12f, 0.02f);
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            NPC.spriteDirection = player.Center.X < NPC.Center.X ? 1 : -1;

            if (cutsceneCounter == 0)
            {
                if (text == 0)
                    cutsceneCounter++;

                if (player.dead)
                {
                    NPC.velocity = new Vector2(4 * NPC.spriteDirection, -16);
                    NPC.alpha += 3;
                    NPC.noTileCollide = true;
                    NPC.noGravity = true;
                    NPC.dontTakeDamage = true;
                    NPC.damage = 0;
                    NPC.EncourageDespawn(10);
                    NPC.netUpdate = true;
                    return;
                }
                else if (NPC.damage == 0 && !DownedBossSystem.DownedThemis)
                    DeathCutscene(player);
                else
                    Fight(player);
            }
            else
            {
                if (!DownedBossSystem.DownedThemis)
                {
                    NPC.velocity = new Vector2(0, 12);
                    Cutscene(player);
                }
                else
                    MechCutscene(player);
            }
        }

        private int RollAttack()
        {
            if (!Main.expertMode)
                return Main.rand.Next(1, 5);
            if (phase == 1)
                return Main.rand.Next(1, 6);

            int roll = Main.rand.Next(1, 31);
            if (roll <= 8) return 4;
            if (roll <= 16) return 5;
            if (roll <= 21) return 2;
            if (roll <= 25) return 3;
            if (roll <= 28) return 7;
            if (roll == 29) return 1;
            return 6;
        }

        public void Fight(Player player)
        {
            if (NPC.ai[0] < 0)
                NPC.ai[0]++;

            if (cutsceneCounter == 0)
                NPC.dontTakeDamage = false;

            PhaseAura();

            if (NPC.ai[0] == 0 && NPC.localAI[1] == 0)
            {
                if (!DownedBossSystem.DownedThemis && cutsceneCounter == 0 && (NPC.life <= NPC.lifeMax * 0.66f && text == 1 || NPC.life <= NPC.lifeMax * 0.33f && text == 2))
                {
                    Cutscene(player);
                }
                else if (NPC.life <= NPC.lifeMax * 0.66f && phase == 1)
                {
                    phase = 2;
                }
                else
                {
                    int next;
                    do { next = RollAttack(); } while (next == lastAttack);

                    lastAttack = next;
                    NPC.localAI[1] = next;
                    NPC.localAI[0] = 0; 
                    NPC.TargetClosest(true);
                    NPC.netUpdate = true;
                }
            }

            switch (NPC.localAI[1])
            {
                case 1:
                    Jump(player);
                    break;
                case 2:
                    SummonDrones(player);
                    break;
                case 3:
                    ThrowSickle(player);
                    break;
                case 4:
                    ShootBullets(player);
                    break;
                case 5:
                    ShootRockets(player);
                    break;
                case 6:
                    TeleportBarrage(player);
                    break;
                case 7:
                    StompAttack(player);
                    break;         
            }
            // I do really like this attack switch code Vi that had made im pretty sure im gonna use it in my future boss AIs
        }

        public void Jump(Player player)
        {
            if (NPC.ai[0] == 0)
            {
                float x = NPC.Center.X - NPC.spriteDirection * 20 < player.MountedCenter.X ? 1 : -1;

                float horizSpeed = phase == 2 ? Main.rand.NextFloat(6f, 8f) : Main.rand.NextFloat(5f, 7f);
                float vertSpeed = phase == 2 ? Main.rand.NextFloat(13f, 15f) : Main.rand.NextFloat(11f, 13f);

                NPC.velocity = new Vector2(horizSpeed * x, -vertSpeed);
                frame = 4;
                NPC.netUpdate = true;
                NPC.noTileCollide = true;
                NPC.noGravity = true;
                NPC.ai[0]++;

                if (!Main.dedServ)
                    RingBurst(NPC.Bottom, 8, 3f, new Color(230, 200, 140, 160), 12f, 20f, 14, 22);
            }
            else
            {
                NPC.velocity.Y += 0.35f + NPC.velocity.Y * 0.01f;
                NPC.velocity.X *= 0.985f;
            }

            if (NPC.IsOnPlatformNPC(new Vector2(1f, 1f)) && NPC.Center.Y + NPC.height / 2 < player.MountedCenter.Y + player.height / 2)
                NPC.noTileCollide = true;
            else if (NPC.velocity.Y > 4)
            {
                if (NPC.ai[0] == 1)
                {
                    if (phase == 2)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 direction = new Vector2(0, -5).SafeNormalize(Vector2.UnitX);
                            direction = direction.RotatedByRandom(MathHelper.ToRadians(60f));
                            int proj = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), new Vector2(NPC.Center.X, NPC.Center.Y), direction * Main.rand.NextFloat(6.5f, 7.5f), ModContent.ProjectileType<ThemisBomb>(), 0, 3, Main.myPlayer);
                            Main.projectile[proj].timeLeft = Main.rand.Next(80, 100);
                        }
                    }
                    NPC.ai[0]++;
                    NPC.noGravity = false;
                    NPC.netUpdate = true;
                }
                NPC.noTileCollide = false;
            }

            if ((NPC.HasTileOnSide(4, new Vector2(1f, 2f), false) || NPC.IsOnPlatformNPC(new Vector2(1f, 1f))) && !NPC.noTileCollide)
            {
                for (int i = 0; i < 15; i++)
                {
                    int dust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + NPC.height - 2), NPC.width, 4, DustID.Smoke, Main.rand.Next(-8, 8), 0, 120, default(Color), Main.rand.NextFloat(1f, 1.75f));
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1f;
                }
                if (!DownedBossSystem.DownedThemis)
                {
                    SoundEngine.PlaySound(SoundID.Dig, NPC.Center);
                    RingBurst(NPC.Bottom, 10, 4f, new Color(230, 200, 140, 170), 14f, 22f, 14, 22);
                }
                else
                {
                    player.PlayerScreen().fastScreenShake = 7 * (1000 - Vector2.Distance(player.Center, NPC.Center)) / 1000;
                    for (int i = 0; i < 15; i++)
                    {
                        int dust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + NPC.height - 2), NPC.width, 4, DustID.Torch, Main.rand.Next(-8, 8), 0, 120, default(Color), Main.rand.NextFloat(1f, 1.75f));
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity *= 1f;
                    }
                    SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                    RingBurst(NPC.Bottom, 14, 6f, new Color(255, 150, 60, 210), 16f, 26f, 14, 24);
                }
                NPC.netUpdate = true;
                frame = 0;
                NPC.ai[0] = phase == 1 ? -30 : 0;
                NPC.localAI[1] = 0;
                NPC.velocity = new Vector2(0, 12);
            }
        }

        public void SummonDrones(Player player)
        {
            if (NPC.ai[0] == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    int x = (int)player.Center.X - 1000 + 1000 * i;
                    int y = (int)NPC.Center.Y - 1000;
                    NPC.NewNPC(NPC.GetSource_FromAI(), x, y, ModContent.NPCType<ThemisDroneMinion>(), NPC.whoAmI);

                    TeleportBurst(new Vector2(x, y));
                }
                NPC.netUpdate = true;
                frame = 13;
            }

            NPC.ai[0]++;

            if (NPC.ai[0] == 30)
                SoundEngine.PlaySound(new("Waybound/Assets/Sounds/DroneCall"), NPC.Center);

            if (NPC.ai[0] == 60)
            {
                frame = 0;
                NPC.ai[0] = (Main.expertMode && !Main.masterMode) ? -50 : -30;
                NPC.ai[1] = 0;
                NPC.localAI[1] = 0;
                NPC.netUpdate = true;
            }
        }

        public void ThrowSickle(Player player)
        {
            if (NPC.ai[0] == 0)
            {
                frame = 9;
            }

            NPC.ai[0]++;

            if (frame == 11 && NPC.frameCounter == 0)
            {
                SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
                if (phase == 1)
                {
                    Vector2 direction = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 10;
                    direction = direction.RotatedByRandom(MathHelper.ToRadians(30f));
                    Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), NPC.Center, direction, ModContent.ProjectileType<SickleOfThemis>(), 15, 3, Main.myPlayer);
                }
                else
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 direction = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 9;
                        direction = direction.RotatedBy(MathHelper.ToRadians(-45f + 90 * i));
                        Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), NPC.Center, direction * 2, ModContent.ProjectileType<SickleOfThemis>(), 15, 3, Main.myPlayer);
                    }
                }
                NPC.netUpdate = true;
            }

            if (NPC.ai[0] == 60)
            {
                NPC.ai[0] = 0;
                NPC.ai[1] = 0;
                NPC.localAI[1] = 0;
                NPC.netUpdate = true;
            }
        }

        public void ShootBullets(Player player)
        {
            if (NPC.ai[0] == 0 && NPC.ai[1] == 0)
                frame = 5;

            NPC.ai[0]++;

            // I Literally felt like the bullet attack was the worst thing themis have had. Such a mess
            if ((phase == 1 && NPC.ai[0] == 60) || NPC.ai[0] == 50)
            {
                Vector2 vel = new Vector2(player.Center.X - NPC.Center.X, player.Center.Y - NPC.Center.Y - 50).SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(8f, 11f);
                Vector2 pos = NPC.Center + new Vector2(0, -5) + vel.SafeNormalize(Vector2.UnitX) * 20;

                int proj2 = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), pos, vel, ModContent.ProjectileType<ThemisBomb>(), 0, 3, Main.myPlayer);
                Main.projectile[proj2].timeLeft = Main.rand.Next(60, 90);

                player.PlayerScreen().fastScreenShake = 7 * (1000 - Vector2.Distance(player.Center, NPC.Center)) / 1000;
                MuzzleFlash(pos, vel);
                SoundEngine.PlaySound(SoundID.Item36, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item149, NPC.Center);

                NPC.netUpdate = true;
                NPC.ai[1]++;
                NPC.ai[0] = phase == 1 ? 0 : 10;
            }

            if (NPC.ai[1] == 4)
            {
                frame = 0;
                NPC.ai[0] = (Main.expertMode && !Main.masterMode) ? -65 : -45;
                NPC.ai[1] = 0;
                NPC.localAI[1] = 0;
                NPC.netUpdate = true;
            }
        }

        public void ShootRockets(Player player)
        {
            if (NPC.ai[0] == 0 && NPC.ai[1] == 0)
            {
                frame = 5;
                SoundEngine.PlaySound(SoundID.Item149, NPC.Center);
            }

            NPC.ai[0]++;

            float rocketSpeedMin = 3f;
            float rocketSpeedMax = 6f;
            if (Main.expertMode && !Main.masterMode)
            {
                rocketSpeedMin = 2f;
                rocketSpeedMax = 5.5f;
            }

            if (NPC.ai[0] == 10 && NPC.ai[1] > 0 || NPC.ai[0] == 30)
            {
                Vector2 vel = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(rocketSpeedMin, rocketSpeedMax);
                Vector2 pos = NPC.Center + new Vector2(0, -10) + vel.SafeNormalize(Vector2.UnitX) * 12f;
                int proj1 = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), pos, vel, ModContent.ProjectileType<ThemisRocket>(), 15, 3, Main.myPlayer);
                Main.projectile[proj1].timeLeft = 240;
                player.PlayerScreen().fastScreenShake = 3.5f * (1000 - Vector2.Distance(player.Center, NPC.Center)) / 1000;
                MuzzleFlash(pos, vel);
                SoundEngine.PlaySound(SoundID.Item11, NPC.Center);
                NPC.netUpdate = true;
                NPC.ai[1]++;
                NPC.ai[0] = 0;
            }

            int rocketsToFire = Main.masterMode ? 6 : (Main.expertMode ? 4 : 3);

            if (NPC.ai[1] == rocketsToFire)
            {
                frame = 0;
                NPC.ai[0] = (Main.expertMode && !Main.masterMode) ? -85 : -60;
                NPC.ai[1] = 0;
                NPC.localAI[1] = 0;
                NPC.netUpdate = true;
            }
        }

        public void Cutscene(Player player)
        {
            if (text == 0 && Vector2.Distance(NPC.Center, player.Center) > 300 && cutsceneCounter < 2)
            {
                if (!Main.dedServ)
                    Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/DesertHermit");
            }
            else if (NPC.HasValidTarget)
            {
                if (!Main.dedServ && text == 0)
                    Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/TheseusAndTheMinotaur");

                if (cutsceneCounter == 1)
                {
                    switch (text)
                    {
                        case 0:
                            SystemUI.dialogueUI.DisplayDialogue("And here you are. I've been following you for a long time.\n It's time for your death!", 360, 20, 0.4f, "Themis:", 0, txtColor, null, null, NPC.Center, sound: false);
                            break;
                        case 1:
                            SystemUI.dialogueUI.DisplayDialogue("Okay, I underestimated you...", 360, 20, 0.4f, "Themis:", 0, txtColor, null, null, NPC.Center, sound: false);
                            phase = 2;
                      
                            RingBurst(NPC.Center, 20, 7f, new Color(255, 140, 40, 220), 18f, 30f, 18, 28);
                            break;
                        case 2:
                            SystemUI.dialogueUI.DisplayDialogue("You can't stop me!", 360, 20, 0.4f, "Themis:", 0, txtColor, null, null, NPC.Center, sound: false);
                            break;
                    }
                    player.PlayerScreen().notHurt = true;
                    player.PlayerScreen().cutscene = true;
                    player.PlayerScreen().lockScreen = true;
                    player.PlayerScreen().ScreenFocusPosition = NPC.position;
                    NPC.noTileCollide = false;
                    NPC.dontTakeDamage = true;
                    NPC.velocity = new Vector2(0, 12);
                    text++;
                    NPC.netUpdate = true;
                }

                cutsceneCounter++;
                if (cutsceneCounter > 150)
                {
                    CutsceneReset(player);
                    if (text == 1)
                    {
                        SystemUI.titleUI.DisplayTitle("Themis", 360, 90, 0.8f, 0, txtColor, txtColor, "The Badlands Seeker");
                        NPC.ai[0] = -60;
                    }
                }
            }
        }

        public void TeleportBarrage(Player player)
        {
            outlineColor = TeleportColor;
            int maxTeleports = Main.masterMode ? 3 : 1;

            NPC.localAI[0]++;
            if (NPC.localAI[0] > 500)
            {
                ForceEndTeleportBarrage(player);
                return;
            }

            if (NPC.ai[0] == 0 && NPC.ai[1] == 0 && teleportIndex == 0)
            {
                frame = 5;
                NPC.noGravity = true;
                NPC.noTileCollide = true;
                NPC.velocity = Vector2.Zero;
                NPC.netUpdate = true;
            }

            NPC.ai[0]++;

            if (NPC.ai[1] == 0)
            {
                NPC.velocity *= 0.85f;
                NPC.velocity.Y = MathF.Sin(NPC.ai[0] * 0.15f) * 0.3f;

                if (NPC.alpha < 220)
                    NPC.alpha += 6;

                outlineOpacity = MathHelper.Lerp(outlineOpacity, 1f, 0.08f);

                if (NPC.ai[0] % 3 == 0)
                {
                    float warningProgress = MathHelper.Clamp(NPC.ai[0] / 40f, 0f, 1f);
                    float radius = MathHelper.Lerp(120f, 40f, warningProgress);

                    for (int i = 0; i < 16; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 16f + NPC.ai[0] * 0.08f;
                        Vector2 spawn = player.Center + angle.ToRotationVector2() * radius;
                        Vector2 vel = angle.ToRotationVector2() * -1.2f;

                        ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                            position: spawn.ToNumerics(),
                            velocity: vel.ToNumerics(),
                            rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                            scale: new System.Numerics.Vector2(Main.rand.NextFloat(8f, 13f)),
                            color: new Color(120, 200, 255, 180),
                            duration: Main.rand.Next(12, 18)
                        ));
                    }
                }

                if (NPC.ai[0] == 5)
                    SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.4f, Pitch = 0.5f }, NPC.Center);

                if (NPC.ai[0] >= 40)
                {
                    Vector2 around = player.Center + Main.rand.NextVector2Circular(380f, 230f);
                    around.Y -= 220f;

                    TeleportBurst(NPC.Center); 

                    NPC.Center = around;
                    NPC.velocity = Vector2.Zero;
                    NPC.alpha = 255;

                    SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.7f, Pitch = -0.1f }, NPC.Center);

                    Vector2 rocketVel = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(10f)) * 6.5f;
                    int proj = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), NPC.Center, rocketVel, ModContent.ProjectileType<ThemisRocket>(), 15, 3, Main.myPlayer);
                    Main.projectile[proj].timeLeft = 240;
                    SoundEngine.PlaySound(SoundID.Item11, NPC.Center);
                    MuzzleFlash(NPC.Center, rocketVel);

                    TeleportBurst(NPC.Center); 

                    teleportIndex++;
                    NPC.ai[0] = 0;
                    NPC.ai[1] = 1;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[1] == 1)
            {
                if (NPC.alpha > 0)
                    NPC.alpha -= 12;
                else
                    NPC.alpha = 0;

                outlineOpacity = MathHelper.Lerp(outlineOpacity, 0f, 0.05f);

                NPC.velocity *= 0.9f;
                NPC.velocity.Y = MathF.Sin(NPC.ai[0] * 0.15f) * 0.3f;

                if (NPC.ai[0] >= 20)
                {
                    NPC.ai[0] = 0;

                    if (teleportIndex >= maxTeleports)
                    {
                        NPC.ai[1] = 2;
                    }
                    else
                    {
                        NPC.ai[1] = 0;
                    }
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[1] == 2)
            {
                NPC.noTileCollide = false;
                NPC.noGravity = false;
                NPC.velocity.Y += 0.4f;
                NPC.alpha = 0;
                outlineOpacity = MathHelper.Lerp(outlineOpacity, 0f, 0.1f);

                bool onGround = (NPC.HasTileOnSide(4, new Vector2(1f, 2f), false) || NPC.IsOnPlatformNPC(new Vector2(1f, 1f))) && !NPC.noTileCollide;
                bool fellTooFar = NPC.Center.Y > player.Center.Y + 800f;

                if (onGround || fellTooFar)
                {
                    frame = 0;
                    NPC.dontTakeDamage = false;
                    NPC.ai[0] = (Main.expertMode && !Main.masterMode) ? -60 : -40;
                    NPC.ai[1] = 0;
                    NPC.localAI[0] = 0;
                    NPC.localAI[1] = 0;
                    teleportIndex = 0;
                    outlineOpacity = 0f;
                    NPC.velocity = new Vector2(0, 12);
                    NPC.netUpdate = true;
                }
            }
        }

        private void ForceEndTeleportBarrage(Player player)
        {
            NPC.alpha = 0;
            NPC.dontTakeDamage = false;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.velocity.Y = 12f;
            NPC.ai[0] = 0;
            NPC.ai[1] = 0;
            NPC.localAI[0] = 0;
            NPC.localAI[1] = 0;
            teleportIndex = 0;
            outlineOpacity = 0f;
            frame = 0;
            NPC.netUpdate = true;
        }

        public void StompAttack(Player player)
        {
            outlineColor = StompColor;

            if (NPC.ai[0] == 0 && NPC.ai[1] == 0)
            {
                frame = 4;
                NPC.noTileCollide = true;
                NPC.noGravity = true;
                NPC.velocity = new Vector2(0, -22f);
                NPC.netUpdate = true;
            }

            NPC.ai[0]++;

            if (NPC.ai[1] == 0)
            {
                NPC.velocity.Y += 0.5f;
                NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -22f, 2f);

                if (NPC.ai[0] >= 40)
                {
                    NPC.ai[0] = 0;
                    NPC.ai[1] = 1;
                    NPC.velocity = Vector2.Zero;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[1] == 1)
            {
                float targetX = player.Center.X - NPC.Center.X;
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, MathHelper.Clamp(targetX * 0.04f, -6f, 6f), 0.08f);
                NPC.velocity.Y = MathF.Sin(NPC.ai[0] * 0.15f) * 0.5f;

                outlineOpacity = MathHelper.Lerp(outlineOpacity, 0.6f, 0.06f);

                if (!Main.dedServ && NPC.ai[0] % 3 == 0)
                {
                    float a = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 from = NPC.Center + a.ToRotationVector2() * Main.rand.NextFloat(60f, 110f);
                    Dust d = Dust.NewDustPerfect(from, DustID.Torch, (NPC.Center - from) * 0.06f, 100, default, 1.3f);
                    d.noGravity = true;
                }
                Lighting.AddLight(NPC.Center, 0.8f * outlineOpacity, 0.35f * outlineOpacity, 0.05f);

                if (NPC.ai[0] >= 45)
                {
                    NPC.ai[0] = 0;
                    NPC.ai[1] = 2;
                    NPC.velocity = new Vector2(0, 28f);
                    NPC.noTileCollide = false;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[1] == 2)
            {
                NPC.velocity.Y += 1.2f;
                if (NPC.velocity.Y > 40f)
                    NPC.velocity.Y = 40f;

                player.PlayerScreen().fastScreenShake = 5f * (1000f - Vector2.Distance(player.Center, NPC.Center)) / 1000f;

                if (Main.netMode != NetmodeID.Server && NPC.ai[0] % 2 == 0)
                {
                    Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(NPC.width, NPC.height);
                    ParticleSystem.FlameBuffer.Create(new ParticleInfo(
                        pos.ToNumerics(),
                        (-NPC.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f)).ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(18f, 28f)),
                        new Color(255, 140, 40, 220),
                        Main.rand.Next(20, 34)
                    ));
                }

                bool onGround = (NPC.collideY || NPC.collideX) || NPC.IsOnPlatformNPC(new Vector2(1f, 2f));

                if (onGround)
                {
                    player.PlayerScreen().ScreenShakeIntensity = 35;
                    SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1f, Pitch = -0.1f }, NPC.Center);

                    for (int i = 0; i < 40; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 12f);
                        ParticleSystem.FlameBuffer.Create(new ParticleInfo(
                            NPC.Center.ToNumerics(),
                            vel.ToNumerics(),
                            Main.rand.NextFloat(MathHelper.TwoPi),
                            new System.Numerics.Vector2(Main.rand.NextFloat(20f, 36f)),
                            new Color(255, 130, 30, 230),
                            Main.rand.Next(24, 42)
                        ));
                    }

                    RingBurst(NPC.Bottom, 28, 10f, new Color(255, 180, 80, 220), 26f, 40f, 16, 26);
                    RingBurst(NPC.Bottom, 16, 5f, new Color(255, 240, 200, 200), 14f, 22f, 12, 20);
                    for (int i = 0; i < 20; i++)
                    {
                        float side = i % 2 == 0 ? 1f : -1f;
                        Vector2 v = new Vector2(side * Main.rand.NextFloat(3f, 11f), -Main.rand.NextFloat(0.5f, 2.5f));
                        Dust d = Dust.NewDustPerfect(NPC.Bottom + new Vector2(0, -4), DustID.Smoke, v, 80, default, Main.rand.NextFloat(1.4f, 2.4f));
                        d.noGravity = true;
                    }

                    for (int i = 0; i < 25; i++)
                    {
                        int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Smoke, Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f), 50, default(Color), Main.rand.NextFloat(1f, 3f));
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].velocity *= 3f;
                    }

                    NPC.velocity = Vector2.Zero;
                    NPC.noTileCollide = false;
                    NPC.noGravity = false;

                    frame = 0;
                    NPC.frameCounter = 0;

                    NPC.ai[0] = 0;
                    NPC.ai[1] = 3;
                    NPC.netUpdate = true;
                }
            }
            else if (NPC.ai[1] == 3)
            {
                outlineOpacity = MathHelper.Lerp(outlineOpacity, 0f, 0.08f);

                if (NPC.ai[0] == 20)
                {
                    int rockets = Main.masterMode ? Main.rand.Next(6, 8) : (Main.expertMode ? Main.rand.Next(4, 6) : 0);

                    for (int i = 0; i < rockets; i++)
                    {
                        Vector2 spawnPos = new Vector2(
                            player.Center.X + Main.rand.NextFloat(-350f, 350f),
                            player.Center.Y - 650f - Main.rand.NextFloat(0f, 150f)
                        );
                        Vector2 vel = new Vector2(0f, 4f);
                        Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), spawnPos, vel, ModContent.ProjectileType<ThemisSkyRocket>(), 15, 3, Main.myPlayer);
                    }

                    SoundEngine.PlaySound(SoundID.Item149 with { Volume = 0.7f, Pitch = 0.2f }, player.Center);
                }

                if (NPC.ai[0] >= 40)
                {
                    frame = 0;
                    NPC.dontTakeDamage = false;
                    NPC.ai[0] = (Main.expertMode && !Main.masterMode) ? -70 : -50;
                    NPC.ai[1] = 0;
                    NPC.localAI[1] = 0;
                    outlineOpacity = 0f;
                    NPC.velocity = new Vector2(0, 12);
                    NPC.netUpdate = true;
                }
            }
        }

        public void MechCutscene(Player player)
        {
            if (cutsceneCounter == 1)
            {
                NPC.Center = player.Center + new Vector2(300 * player.direction, -400);
                frame = 4;
                NPC.velocity = new(-4 * player.direction, 12);
                NPC.dontTakeDamage = true;
                NPC.noGravity = true;
                NPC.noTileCollide = true;
                player.PlayerScreen().notHurt = true;
                player.PlayerScreen().cutscene = true;
                player.PlayerScreen().lockScreen = true;
                if (!Main.dedServ)
                    Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/theseusAndTheMinotaur");
            }

            cutsceneCounter++;
            player.PlayerScreen().ScreenFocusPosition = NPC.position;

            if (cutsceneCounter > 30 && (NPC.HasTileOnSide(4, new Vector2(1f, 10), false) || NPC.IsOnPlatformNPC(new Vector2(1f, 10))))
            {
                player.PlayerScreen().ScreenShakeIntensity = 40;
                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                NPC.velocity.X = 0;
                NPC.noGravity = false;
                NPC.noTileCollide = false;
                for (int i = 0; i < 75; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f), 50, default(Color), Main.rand.NextFloat(1f, 3f));
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 3f;

                    dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Smoke, Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f), 50, default(Color), Main.rand.NextFloat(1f, 3f));
                    Main.dust[dust].noGravity = true;
                }

                RingBurst(NPC.Bottom, 32, 11f, new Color(255, 170, 70, 220), 28f, 44f, 18, 28);

                int goreCount = Main.rand.Next(3, 10); 
                for (int i = 0; i < goreCount; i++)
                {
                    Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.Center.X, NPC.Center.Y), new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f)), Main.rand.Next(61, 64));
                }
                SystemUI.titleUI.DisplayTitle("Mech Themis", 240, 90, 0.8f, 0, txtColor, txtColor, "Themis' robot");
                NPC.dontTakeDamage = false;
                player.PlayerScreen().notHurt = false;
                player.PlayerScreen().cutscene = false;
                player.PlayerScreen().lockScreen = false;
                NPC.ai[0] = -60;
                frame = 0;
                cutsceneCounter = 0;
                text = 1;
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, NPC.velocity.X, NPC.velocity.Y, 50, default(Color), Main.rand.NextFloat(1f, 3f));
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 3f;
                }
                NPC.velocity.X *= 0.995f;
                NPC.noGravity = true;
                NPC.noTileCollide = true;
            }
        }

        public void DeathCutscene(Player player)
        {
            if (NPC.ai[0] == 0)
            {
                NPC.ai[0] = 1;
                player.PlayerScreen().notHurt = true;
                player.PlayerScreen().cutscene = true;
                player.PlayerScreen().lockScreen = true;
                player.PlayerScreen().ScreenFocusPosition = NPC.position;
                NPC.noTileCollide = true;
                NPC.noGravity = true;
                NPC.dontTakeDamage = true;
                NPC.velocity = Vector2.Zero;
                NPC.netUpdate = true;
            }

            NPC.ai[0]++;

            if (NPC.ai[0] < 40)
            {
                NPC.velocity = new Vector2(0, -0.4f);
                NPC.alpha = (int)MathHelper.Lerp(NPC.alpha, 0, 0.1f);
            }
            else if (NPC.ai[0] == 40)
            {
                SystemUI.dialogueUI.DisplayDialogue("Let’s see what you’ve got, goodbye loser!", 360, 20, 0.4f, "Themis:", 0, txtColor, null, null, NPC.Center, sound: false);
            }
            else if (NPC.ai[0] > 40 && NPC.ai[0] < 120)
            {
                float t = (NPC.ai[0] - 40) / 80f;
                NPC.velocity = new Vector2(
                    MathHelper.Lerp(NPC.velocity.X, 2f * NPC.spriteDirection, 0.05f),
                    MathHelper.Lerp(-2f, -18f, t)
                );

                NPC.alpha = (int)MathHelper.Lerp(0, 255, t);

                if (Main.netMode != NetmodeID.Server && NPC.ai[0] % 2 == 0)
                {
                    int dust = Dust.NewDust(
                        NPC.position, NPC.width, NPC.height,
                        DustID.Torch,
                        Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f),
                        50, default, Main.rand.NextFloat(1f, 2.5f));
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 2f;
                }
            }
            else if (NPC.ai[0] == 120)
            {
                NPC.alpha = 255;
                NPC.velocity = Vector2.Zero;

                RingBurst(NPC.Center, 24, 8f, new Color(255, 200, 120, 230), 24f, 38f, 16, 26);

                int bombCount = Main.rand.Next(5, 11);
                for (int i = 0; i < bombCount; i++)
                {
                    Vector2 spawnPos = new Vector2(
                        player.Center.X + Main.rand.NextFloat(-400f, 400f),
                        player.Center.Y - 700f - Main.rand.NextFloat(0f, 200f)
                    );

                    Vector2 dir = (player.Center - spawnPos).SafeNormalize(Vector2.UnitY);
                    Vector2 vel = dir * Main.rand.NextFloat(5f, 8f);

                    Projectile.NewProjectile(
                        NPC.GetSource_GiftOrReward(),
                        spawnPos, vel,
                        ModContent.ProjectileType<ThemisSkyRocket>(),
                        20, 3, Main.myPlayer);
                }

                SoundEngine.PlaySound(SoundID.Item149 with { Volume = 0.8f, Pitch = 0.2f }, player.Center);
            }
            else if (NPC.ai[0] >= 180)
            {
                NPC.alpha = 255;
                NPC.Center = player.Center;
                NPC.life = 0;

                player.PlayerScreen().notHurt = false;
                player.PlayerScreen().cutscene = false;
                player.PlayerScreen().lockScreen = false;

                NPC.HitEffect(0, 0);
                NPC.checkDead();
            }
        }

        public void CutsceneReset(Player player)
        {
            cutsceneCounter = 0;
            player.PlayerScreen().notHurt = false;
            player.PlayerScreen().cutscene = false;
            player.PlayerScreen().lockScreen = false;
            NPC.dontTakeDamage = false;
        }

        public override bool CheckDead()
        {
            if (NPC.damage != 0 && !DownedBossSystem.DownedThemis)
            {
                NPC.damage = 0;
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                NPC.netUpdate = true;
                return false;
            }
            return true;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = frame * frameHeight;

            if (frame >= 0 && frame <= 3)
            {
                NPC.frameCounter++;
                if (NPC.frameCounter > 5)
                {
                    frame++;
                    if (frame >= 4)
                        frame = 0;
                    NPC.frameCounter = 0;
                }
            }
            else if (frame >= 5 && frame <= 8)
            {
                NPC.frameCounter++;
                if (NPC.frameCounter > 5)
                {
                    frame++;
                    if (frame >= 9)
                        frame = 5;
                    NPC.frameCounter = 0;
                }
            }
            else if (frame >= 9 && frame <= 12)
            {
                NPC.frameCounter++;
                if (NPC.frameCounter > 5)
                {
                    frame++;
                    if (frame >= 13)
                        frame = 0;
                    NPC.frameCounter = 0;
                }
            }
            else if (frame >= 13 && frame <= 16)
            {
                NPC.frameCounter++;
                if (NPC.frameCounter > 5)
                {
                    frame++;
                    if (frame >= 17)
                        frame = 13;
                    NPC.frameCounter = 0;
                }
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var effects1 = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = !DownedBossSystem.DownedThemis ? TextureAssets.Npc[NPC.type].Value : mechTexture.Value;
            Vector2 drawOrigin = new(texture.Width * 0.5f, texture.Height * 0.5f / Main.npcFrameCount[NPC.type]);
            Vector2 center = new Vector2(NPC.width * 0.5f, NPC.height * 0.5f) + new Vector2(0, 2);
            float visible = 1f - NPC.alpha / 255f;
            if (outlineOpacity > 0.01f)
            {
                Vector2 outlinePos = (NPC.position - Main.screenPosition) + center;
                Color col = (outlineColor with { A = 0 }) * (outlineOpacity * 0.55f);
                float thickness = 2f + 0.6f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 8f);

                BeginAdditive(spriteBatch);
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 offset = angle.ToRotationVector2() * thickness;
                    Main.EntitySpriteDraw(texture, outlinePos + offset, NPC.frame, col, NPC.rotation, drawOrigin, NPC.scale, effects1, 0);
                }
                EndAdditive(spriteBatch);
            }

            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                Vector2 drawPos = (NPC.oldPos[k] - Main.screenPosition) + center;
                Color color = NPC.GetAlpha(drawColor) * 0.4f * visible * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale - 0.02f * k, effects1, 0);
            }

            Vector2 pos = (NPC.position - Main.screenPosition) + center;
            Main.EntitySpriteDraw(texture, pos, NPC.frame, drawColor * visible, NPC.rotation, drawOrigin, NPC.scale, effects1, 0);

            if (NPC.localAI[1] == 4 || NPC.localAI[1] == 5)
            {
                Texture2D weaponTexture = !DownedBossSystem.DownedThemis ? handsTexture.Value : mechHandsTexture.Value;
                var effects2 = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
                Rectangle rect = NPC.localAI[1] == 4 ? new Rectangle(0, 0, 62, 24) : new Rectangle(0, 26, 62, 26);
                Vector2 weaponOrigin = new(rect.Width * (NPC.localAI[1] == 4 ? 0.25f : 0.33f), rect.Height * 0.5f);
                float rot = (Main.player[NPC.target].MountedCenter - NPC.Center).ToRotation();
                Main.EntitySpriteDraw(weaponTexture, pos + new Vector2(0, -10), rect, drawColor * visible, rot, weaponOrigin, NPC.scale, effects2, 0);
            }

            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.localAI[1] != 7 || NPC.ai[1] != 1 || cutsceneCounter != 0) return;

            float t = MathHelper.Clamp(NPC.ai[0] / 45f, 0f, 1f);
            float flicker = 0.75f + 0.25f * (float)Math.Sin(NPC.ai[0] * 0.5f);

            Vector2 start = NPC.Bottom;
            float len = 0f;
            while (len < 1400f && !Collision.SolidCollision(start + new Vector2(-2f, len), 4, 4))
                len += 16f;

            Texture2D px = TextureAssets.MagicPixel.Value;
            Rectangle src = new Rectangle(0, 0, 1, 1);
            Vector2 top = start - Main.screenPosition;
            float outerWidth = MathHelper.Lerp(34f, 10f, t);

            Color outer = new Color(255, 120, 40, 0) * (0.25f * t * flicker);
            Color core = new Color(255, 230, 180, 0) * (0.55f * t * flicker);

            BeginAdditive(spriteBatch);
            spriteBatch.Draw(px, top, src, outer, 0f, new Vector2(0.5f, 0f), new Vector2(outerWidth, len), SpriteEffects.None, 0f);
            spriteBatch.Draw(px, top, src, core, 0f, new Vector2(0.5f, 0f), new Vector2(3f, len), SpriteEffects.None, 0f);

            // маркер на земле
            Vector2 hit = top + new Vector2(0f, len);
            spriteBatch.Draw(px, hit, src, outer, 0f, new Vector2(0.5f, 0.5f), new Vector2(outerWidth * 2.5f, 5f), SpriteEffects.None, 0f);
            spriteBatch.Draw(px, hit, src, core, 0f, new Vector2(0.5f, 0.5f), new Vector2(outerWidth * 1.2f, 2f), SpriteEffects.None, 0f);
            EndAdditive(spriteBatch);
        }

        public override void OnKill()
        {
            if (DownedBossSystem.DownedThemis)
            {
                for (int i = 0; i < 75; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f), 50, default(Color), Main.rand.NextFloat(1f, 3f));
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 3f;
                }
                for (int i = 0; i < 75; i++)
                {
                    int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Smoke, Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f), 50, default(Color), Main.rand.NextFloat(1f, 3f));
                    Main.dust[dust].noGravity = true;
                }

                RingBurst(NPC.Center, 36, 12f, new Color(255, 170, 70, 230), 30f, 48f, 20, 32);

                int goreCount = Main.rand.Next(3, 10);
                for (int i = 0; i < goreCount; i++)
                {
                    Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.Center.X, NPC.Center.Y), new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f)), Main.rand.Next(61, 64));
                }
            }
            else
                NPC.SetEventFlagCleared(ref DownedBossSystem.DownedThemis, -1);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ThemisTrophyItem>(), 10));

            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());

            int desertWreckage = ModContent.ItemType<DesertWreckage>();
            var desertWreckageParameters = new DropOneByOne.Parameters()
            {
                ChanceNumerator = 1,
                ChanceDenominator = 1,
                MinimumStackPerChunkBase = 25,
                MaximumStackPerChunkBase = 35,
                MinimumItemDropsCount = 1,
                MaximumItemDropsCount = 1,
            };
            notExpertRule.OnSuccess(new DropOneByOne(desertWreckage, desertWreckageParameters));

            int desertCore = ModContent.ItemType<DesertCore>();
            var desertCoreParameters = new DropOneByOne.Parameters()
            {
                ChanceNumerator = 1,
                ChanceDenominator = 1,
                MinimumStackPerChunkBase = 2,
                MaximumStackPerChunkBase = 5,
                MinimumItemDropsCount = 1,
                MaximumItemDropsCount = 1,
            };
            notExpertRule.OnSuccess(new DropOneByOne(desertCore, desertCoreParameters));

            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ThemisMask>(), 7));

            npcLoot.Add(notExpertRule);

            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<Items.Bags.ThemisBag>()));

            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<ThemisRelicI>()));

            npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<MotorcycleOfDesertHunter>(), 4));
        }

        public override void ModifyTypeName(ref string typeName)
        {
            if (!DownedBossSystem.DownedThemis)
                typeName = "Themis";
            else
                typeName = "Mech Themis";
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneDesert
                && spawnInfo.Player.ZoneOverworldHeight
                && !DownedBossSystem.DownedThemis
                && !NPC.AnyNPCs(ModContent.NPCType<Themis>())
                && NPC.downedBoss1)
            {
                return 0.1f;
            }
            else
                return 0f;
        }
    }
}
/*
		public override void AI()
		{
			if (Main.netMode != NetmodeID.MultiplayerClient && !Main.dedServ)
			{

				NPC.TargetClosest(true);
				Player player = Main.player[NPC.target];

				if (NPC.HasValidTarget && Vector2.Distance(NPC.Center, player.Center) < 300 && cutsceneCounter >= 0)
				{
					if (!Main.dedServ)
					{
						Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/theseusAndTheMinotaur");
					}
					IsActivated = true;
					NPC.dontTakeDamage = false;
					if (Text == 0)
					{
						if (cutsceneCounter == 0)
						{
							canFight = false;
							cutsceneCounter++;
							if (!DownedBossSystem.DownedThemis)
								SystemUI.Instance.dialogueUI.DisplayDialogue("And here you are. I've been following you for a long time.\n It's time for your death!", 120, 20, 0.4f, "Themis:", 0, txtColor, null, null, NPC.Center, sound: false);
						}
						player.PlayerScreen().cutscene = true;
						player.PlayerScreen().lockScreen = true;
						player.PlayerScreen().ScreenFocusPosition = NPC.position;
						NPC.noTileCollide = false;
						NPC.dontTakeDamage = true;
						NPC.velocity = new Vector2(0, 12);
					}
				}

				if (cutsceneCounter > 0 || cutsceneCounter < 0)
				{
					cutsceneCounter++;
				}

				if(cutsceneCounter < 0)
				{
					if(cutsceneCounter == -139)
					{
						NPC.Center = player.Center + new Vector2(200 * player.direction, -400);
						NPC.velocity = new (-4 * player.direction, 12);
						NPC.dontTakeDamage = true;
						if (!Main.dedServ)
							Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/theseusAndTheMinotaur");
					}
					player.PlayerScreen().cutscene = true;
					player.PlayerScreen().lockScreen = true;
					player.PlayerScreen().ScreenFocusPosition = NPC.position;
					if(cutsceneCounter < -140)
					{
						NPC.noGravity = true;
						NPC.noTileCollide = true;
					}
					else
					{
						NPC.noGravity = false;
						NPC.noTileCollide = false;
					}

					if(cutsceneCounter > -120 && (NPC.HasTileOnSide(4, new Vector2(1f, 10), false) || NPC.IsOnPlatformNPC(new Vector2(1f, 10))))
					{
						player.PlayerScreen().ScreenShakeIntensity = 40;
						SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
						NPC.velocity *= 0;
						NPC.noGravity = false;
						NPC.noTileCollide = false;
						for (int i = 0; i < 75; i++)
						{
							int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f), 50, default(Color), Main.rand.NextFloat(1f, 3f));
							Main.dust[dust].noGravity = true;
							Main.dust[dust].velocity *= 3f;
						}

						for (int i = 0; i < 75; i++)
						{
							int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Smoke, Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f), 50, default(Color), Main.rand.NextFloat(1f, 3f));
							Main.dust[dust].noGravity = true;
						}

						for (int i = 0; i < Main.rand.Next(3, 10); i++)
						{
							Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.Center.X, NPC.Center.Y), new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f)), Main.rand.Next(61, 64));
						}
						cutsceneCounter = 0;
						NPC.dontTakeDamage = false;
					}
					else
					{
						for (int i = 0; i < 10; i++)
						{
							int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, NPC.velocity.X, NPC.velocity.Y, 50, default(Color), Main.rand.NextFloat(1f, 3f));
							Main.dust[dust].noGravity = true;
							Main.dust[dust].velocity *= 3f;
						}
						NPC.velocity.X *= 0.995f;
						NPC.noGravity = true;
						NPC.noTileCollide = true;
					}
				}

				if (cutsceneCounter >= 180)
				{
					if (Text == 0)
					{
						if(!DownedBossSystem.DownedThemis)
							SystemUI.Instance.titleUI.DisplayTitle("Themis", 90, 90, 0.8f, 0, txtColor, txtColor, "the hunter of king");
						else
                            SystemUI.Instance.titleUI.DisplayTitle("Mech Themis", 90, 90, 0.8f, 0, txtColor, txtColor, "Themis' robot");
                    }
					Text += 1;
					player.PlayerScreen().cutscene = false;
					player.PlayerScreen().lockScreen = false;
					NPC.dontTakeDamage = false;
					canFight = true;
					cutsceneCounter = 0;
				}

				if (IsActivated == false && NPC.HasValidTarget && Vector2.Distance(NPC.Center, player.Center) > 300)
				{
					if (!Main.dedServ)
					{
						Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/desertHermit");
					}
				}

				if (IsActivated == false)
				{
					NPC.dontTakeDamage = true;
				}

				if (IsActivated == true && canFight == true)
				{
					BossAI();
				}

				if (player.dead)
				{
					NPC.velocity = new Vector2(4 * NPC.spriteDirection, -16);
					NPC.netUpdate = true;
					IsActivated = false;
					NPC.EncourageDespawn(12);
					return;
				}

				if (!Main.expertMode && !Main.masterMode)
				{
					if (Phase == 1)
					{
						ActionCounterMax = 150;
					}
					else
					{
						ActionCounterMax = 120;
					}
				}
				else
				{
					if (Phase == 1)
					{
						ActionCounterMax = 120;
					}
					else
					{
						ActionCounterMax = 90;
					}
				}

				if (Phase == 1)
				{
					ShootTimeMax = 45;
				}
				else
				{
					ShootTimeMax = 35;
				}

				if (NPC.life <= NPC.lifeMax * 0.66f && !DownedBossSystem.DownedThemis)
				{
					Phase = 2;
					if (Text == 1)
					{
						if (cutsceneCounter == 0)
						{
							canFight = false;
							cutsceneCounter++;
							SystemUI.Instance.dialogueUI.DisplayDialogue("Okay, I underestimated you...", 120, 20, 0.4f, "Themis:", 0, txtColor, null, null, NPC.Center, sound: false);
						}
						player.PlayerScreen().cutscene = true;
						player.PlayerScreen().lockScreen = true;
						player.PlayerScreen().ScreenFocusPosition = NPC.position;
						NPC.velocity = new Vector2(0, Main.rand.NextFloat(10.5f, 12.5f));
						NPC.noTileCollide = false;
						NPC.dontTakeDamage = true;
					}
				}
				else
				{
					Phase = 1;
				}

				if (Text == 2 && NPC.life <= NPC.lifeMax * 0.33f && !DownedBossSystem.DownedThemis)
				{
					if (cutsceneCounter == 0)
					{
						canFight = false;
						cutsceneCounter++;
						SystemUI.Instance.dialogueUI.DisplayDialogue("You can't stop me from killing king!", 120, 20, 0.4f, "Themis:", 0, txtColor, null, null, NPC.Center, sound: false);
					}
					player.PlayerScreen().cutscene = true;
					player.PlayerScreen().lockScreen = true;
					player.PlayerScreen().ScreenFocusPosition = NPC.position;
					NPC.velocity = new Vector2(0, Main.rand.NextFloat(10.5f, 12.5f));
					NPC.noTileCollide = false;
					NPC.dontTakeDamage = true;
				}

				if (NPC.position.X >= player.position.X)
				{
					NPC.spriteDirection = 1;
				}
				else
				{
					NPC.spriteDirection = -1;
				}

				if (CanSpawnDust == true)
				{
					if (NPC.collideY)
					{
						for (int i = 0; i < 15; i++)
						{
							int dust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + NPC.height), NPC.width, 4, 31, Main.rand.Next(-8, 8), 0, 120, default(Color), Main.rand.NextFloat(1f, 1.75f));
							Main.dust[dust].noGravity = true;
							Main.dust[dust].velocity *= 1f;
						}
						CanSpawnDust = false;
					}
				}

				if (Jump == true && JumpBugFix >= 180)
				{
					Jump = false;
					JumpBugFix = 0;
				}

				if (NPC.ai[3] > 0f)
				{
					if (Text == 3)
					{
						if (cutsceneCounter == 0)
						{
							canFight = false;
							cutsceneCounter++;
						}
						player.PlayerScreen().cutscene = true;
						player.PlayerScreen().lockScreen = true;
						player.PlayerScreen().ScreenFocusPosition = NPC.position;
						NPC.noTileCollide = false;
						NPC.dontTakeDamage = true;
					}
					Throw = false;
					Shoot = false;
					Jump = false;
					ActionNumber = 10;
					NPC.dontTakeDamage = true;
					NPC.ai[3] += 1f;
					if (NPC.ai[3] < 65)
					{
						if (NPC.ai[3] >= 25)
						{
							AlphaCounter++;
							if (AlphaCounter >= 1)
							{
								NPC.alpha += 10;
								AlphaCounter = 0;
							}
							NPC.noTileCollide = true;
						}
						if (NPC.ai[3] == 25)
						{
							NPC.velocity = new Vector2(6 * NPC.spriteDirection, -16);
						}
						if (NPC.ai[3] == 45)
						{
							NPC.velocity = new Vector2(8 * NPC.spriteDirection, -12);
						}
						if (NPC.ai[3] == 55)
						{
							NPC.velocity = new Vector2(10 * NPC.spriteDirection, -8);
						}
					}
					if (NPC.ai[3] == 70f)
					{
						NPC.noTileCollide = false;
						NPC.velocity = new Vector2(0, 0);
						NPC.noGravity = true;
						SystemUI.Instance.dialogueUI.DisplayDialogue("This time you won... But next time you will not live!", 120, 20, 0.4f, "Themis:", 0, txtColor, null, null, NPC.Center, sound: false);
					}

					if (NPC.ai[3] >= 180f)
					{
						NPC.alpha = 255;
						NPC.Center = player.Center;
						NPC.life = 0;
                        player.PlayerScreen().cutscene = false;
                        player.PlayerScreen().lockScreen = false;
                        NPC.HitEffect(0, 0);
						NPC.checkDead();
					}
					return;
				}
			}
			else
			{
				IsActivated = true;
				NPC.dontTakeDamage = false;
				NPC.TargetClosest(true);
				BossAI();

				Player player = Main.player[NPC.target];
				if (player.dead)
				{
					NPC.velocity = new Vector2(4 * NPC.spriteDirection, -16);
					IsActivated = false;
					NPC.EncourageDespawn(12);
					NPC.netUpdate = true;
					return;
				}
			}
		}

		public override bool CheckDead()
		{
			if (NPC.ai[3] == 0f && !DownedBossSystem.DownedThemis)
			{
				NPC.ai[3] = 1f;
				NPC.damage = 0;
				NPC.life = 1;
				NPC.dontTakeDamage = true;
				NPC.netUpdate = true;
				return false;
			}
			return true;
		}

		private void BossAI()
		{
			if (Main.netMode != NetmodeID.MultiplayerClient && NPC.HasValidTarget)
			{
				Player player = Main.player[NPC.target];

				ActionCounter++;

				if (ActionCounter > ActionCounterMax)
				{
					if (ActionNumber == 0)
					{
						Jump = true;
					}
					else if (ActionNumber == 1)
					{
						Shoot = true;
					}
					else if (ActionNumber == 2)
					{
						Throw = true;
					}
				}
				else
				{
					Jump = false;
				}

				if (Jump == true)  //JUMP
				{
					NPC.ai[0]++;
					if (NPC.ai[0] < 25)
					{
						if (player.position.X >= NPC.position.X && TurnCounter == 0)
						{
							NPC.velocity = new Vector2(Main.rand.NextFloat(10f, 13f), Main.rand.NextFloat(-10.5f, -12.5f));
							TurnCounter = 1;
							NPC.netUpdate = true;
						}
						else if (player.position.X < NPC.position.X && TurnCounter == 0)
						{
							NPC.velocity = new Vector2(Main.rand.NextFloat(-10f, -13f), Main.rand.NextFloat(-10.5f, -12.5f));
							TurnCounter = 1;
							NPC.netUpdate = true;
						}
						if (NPC.ai[0] < 24)
						{
							NPC.noTileCollide = true;
						}
						if (NPC.ai[0] >= 24)
						{
							NPC.noTileCollide = false;
						}
						NPC.netUpdate = true;
					}
					else if (NPC.ai[0] >= 25)
					{
						if (NPC.ai[0] == 25)
						{
							for (int i = 0; i < 6; i++)
							{
								Vector2 direction = new Vector2(0, -5).SafeNormalize(Vector2.UnitX);
								direction = direction.RotatedByRandom(MathHelper.ToRadians(60f));
								int proj = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), new Vector2(NPC.Center.X, NPC.Center.Y), direction * Main.rand.NextFloat(6.5f, 7.5f), ModContent.ProjectileType<themisBomb>(), 0, 3, Main.myPlayer);
								Main.projectile[proj].timeLeft = Main.rand.Next(80, 100);
								NPC.netUpdate = true;
							}
						}

						NPC.velocity.Y++;
						if (NPC.collideY || NPC.collideX)
						{
							NPC.velocity = new Vector2(0, 2.5f);
							ActionCounter = 0;
							NPC.ai[0] = 0;
							Jump = false;
							ActionNumber = Main.rand.Next(1, 3);
							TurnCounter = 0;
							NPC.TargetClosest();
							NPC.netUpdate = true;

							if (NPC.collideX && !NPC.collideY)
							{
								CanSpawnDust = true;
							}
							else
							{
								for (int i = 0; i < 15; i++)
								{
									int dust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + NPC.height - 2), NPC.width, 4, 31, Main.rand.Next(-8, 8), 0, 120, default(Color), Main.rand.NextFloat(1f, 1.75f));
									Main.dust[dust].noGravity = true;
									Main.dust[dust].velocity *= 1f;
								}
							}
						}
						else
						{
							JumpBugFix++;
						}
					}
				}
				else if (Shoot == true)  //SHOOT
				{
					NPC.ai[0]++;
					if (NPC.ai[0] > ShootTimeMax && ShootCounter != 4)
					{
						for (int i = 0; i < 4; i++)
						{
							Vector2 direction1 = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
							direction1 = direction1.RotatedByRandom(MathHelper.ToRadians(12f));
							int proj1 = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), new Vector2(NPC.Center.X - (NPC.spriteDirection * 25), NPC.Center.Y - 6), direction1 * Main.rand.NextFloat(8.5f, 9.5f), ProjectileID.BulletDeadeye, 15, 3, Main.myPlayer);
							Main.projectile[proj1].tileCollide = false;
							Main.projectile[proj1].timeLeft = 180;
						}

						Vector2 direction2 = new Vector2(player.Center.X - NPC.Center.X, player.Center.Y - NPC.Center.Y - 50).SafeNormalize(Vector2.UnitX);
						direction2 = direction2.RotatedByRandom(MathHelper.ToRadians(15f));
						int proj2 = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), new Vector2(NPC.Center.X - (NPC.spriteDirection * 25), NPC.Center.Y - 6), direction2 * Main.rand.NextFloat(8f, 11f), ModContent.ProjectileType<themisBomb>(), 0, 3, Main.myPlayer);
						Main.projectile[proj2].timeLeft = Main.rand.Next(60, 90);

						for (int i = 0; i < 10; i++)
						{
							Vector2 speed = new Vector2(-1 * NPC.spriteDirection, 0);
							int dust = Dust.NewDust(new Vector2(NPC.Center.X - (NPC.spriteDirection * 28), NPC.Center.Y - 8), 2, 2, DustID.Torch, speed.X, speed.Y, 120, default(Color), Main.rand.NextFloat(1f, 1.5f));
							Main.dust[dust].noGravity = true;
							Main.dust[dust].velocity *= 2f;
						}
						for (int i = 0; i < 5; i++)
						{
							Vector2 speed = new Vector2(-1 * NPC.spriteDirection, 0);
							int dust = Dust.NewDust(new Vector2(NPC.Center.X - (NPC.spriteDirection * 28), NPC.Center.Y - 8), 2, 2, DustID.Smoke, speed.X, speed.Y, 120, default(Color), Main.rand.NextFloat(1f, 1.5f));
							Main.dust[dust].noGravity = true;
							Main.dust[dust].velocity *= 1f;
						}

						ShootCounter += 1;
						NPC.netUpdate = true;
						NPC.ai[0] = 0;
					}
					if (ShootCounter == 4)
					{
						Shoot = false;
						ActionCounter = 0;
						NPC.ai[0] = 0;
						NPC.TargetClosest();
						if (Main.rand.NextBool(2))
						{
							ActionNumber = 2;
						}
						else
						{
							ActionNumber = 0;
						}
						NPC.netUpdate = true;
						ShootCounter = 0;
					}
				}
				else if (Throw == true)  //THROW
				{
					if (!Main.expertMode && !Main.masterMode)
					{
						ThrowRandom = 1;
					}
					else
					{
						ThrowRandom = Main.rand.Next(1, 3);
					}

					NPC.ai[0]++;
					if (NPC.ai[0] == 8)
					{
						SoundEngine.PlaySound(SoundID.Item18, NPC.position);
						if (ThrowRandom == 1)
						{
							if (Phase == 1)
							{
								Vector2 direction = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 10;
								direction = direction.RotatedByRandom(MathHelper.ToRadians(30f));
								int proj = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), NPC.Center, direction, ModContent.ProjectileType<sickleOfThemis>(), 15, 3, Main.myPlayer);
								NPC.netUpdate = true;
							}
							else
							{
								for (int i = 0; i < 2; i++)
								{
									Vector2 direction = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 9;
									direction = direction.RotatedBy(MathHelper.ToRadians(-45f + 90 * i));
									int proj = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), NPC.Center, direction * 2, ModContent.ProjectileType<sickleOfThemis>(), 15, 3, Main.myPlayer);
								}
								NPC.netUpdate = true;
							}
						}
						else if (ThrowRandom == 2)
						{
							for (int i = 0; i < 12; i++)
							{
								Vector2 direction = new Vector2(player.Center.X - NPC.Center.X, player.Center.Y - NPC.Center.Y - 200).SafeNormalize(Vector2.UnitX) * 1.5f;
								direction = direction.RotatedByRandom(MathHelper.ToRadians(30));
								int proj = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), new Vector2(NPC.Center.X, NPC.Center.Y), direction * Main.rand.NextFloat(1f, 8f), ModContent.ProjectileType<themisBomb>(), 0, 3, Main.myPlayer);
							}
							Vector2 direction2 = new Vector2(player.Center.X - NPC.Center.X, player.Center.Y - NPC.Center.Y - 200).SafeNormalize(Vector2.UnitX) * 1.5f;
							direction2 = direction2.RotatedByRandom(MathHelper.ToRadians(30));
							int proj2 = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), new Vector2(NPC.Center.X, NPC.Center.Y), direction2 * Main.rand.NextFloat(1f, 8f), ModContent.ProjectileType<turretSpawner>(), 0, 3, Main.myPlayer);
							NPC.netUpdate = true;
						}
						NPC.netUpdate = true;
					}
					if (NPC.ai[0] > 15)
					{
						NPC.ai[0] = 0;
						Throw = false;
						ActionCounter = 0;
						NPC.TargetClosest();
						NPC.netUpdate = true;

						if (Main.rand.NextBool(2))
						{
							ActionNumber = 1;
						}
						else
						{
							ActionNumber = 0;
						}
					}
				}

				if (SpawnCounterNPC < 780)
				{
					SpawnCounterNPC++;
				}
				if (SpawnCounterNPC >= 780)
				{
					if (Phase == 1)
					{
						int npc = NPC.NewNPC(NPC.GetSource_FromAI(), (int)player.Center.X, (int)NPC.Center.Y - 1000, ModContent.NPCType<themisDroneMinion>(), NPC.whoAmI);
						NPC.netUpdate = true;
						SpawnCounterNPC = 0;
					}
					else
					{
						for (int i = 0; i < 2; i++)
						{
							int npc = NPC.NewNPC(NPC.GetSource_FromAI(), (int)player.Center.X - 1000 + 1000 * i, (int)NPC.Center.Y - 1000, ModContent.NPCType<themisDroneMinion>(), NPC.whoAmI);
							NPC.netUpdate = true;
							SpawnCounterNPC = 0;
						}
					}
				}
			}
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frame.Y = frame * frameHeight;
			Player player = Main.player[NPC.target];

			if (IsActivated == true && NPC.ai[3] <= 0f && !player.dead)
			{
				if (Jump == false && Shoot == false && Throw == false)
				{
					if (frame > 4)
					{
						frame = 0;
					}
					NPC.frameCounter++;
					if (NPC.frameCounter > 5)
					{
						frame++;
						if (frame >= 4)
						{
							frame = 0;
						}
						NPC.frameCounter = 0;
					}
				}
				else if (Shoot == true)
				{
					if (frame < 5)
					{
						frame = 5;
					}
					NPC.frameCounter++;
					if (NPC.frameCounter > 5)
					{
						frame++;
						if (frame >= 9)
						{
							frame = 5;
						}
						NPC.frameCounter = 0;
					}
				}
				else if (Jump == true)
				{
					frame = 4;
				}
				else if (Throw == true)
				{
					if (frame < 9)
					{
						frame = 9;
					}
					NPC.frameCounter++;
					if (NPC.frameCounter > 5)
					{
						frame++;
						if (frame >= 13)
						{
							frame = 9;
						}
						NPC.frameCounter = 0;
					}
				}
			}
			else if (IsActivated == false && NPC.ai[3] <= 0f && !player.dead)
			{
				if (frame > 4)
				{
					frame = 0;
				}
				NPC.frameCounter++;
				if (NPC.frameCounter > 5)
				{
					frame++;
					if (frame >= 4)
					{
						frame = 0;
					}
					NPC.frameCounter = 0;
				}
			}
			else if (NPC.ai[3] > 0f || player.dead)
			{
				if (NPC.ai[3] >= 25)
				{
					frame = 4;
				}
				else
				{
					NPC.frameCounter++;
					if (NPC.frameCounter > 5)
					{
						frame++;
						if (frame >= 4)
						{
							frame = 0;
						}
						NPC.frameCounter = 0;
					}
				}
			}
			NPC.frame.Y = frameHeight * frame;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(ShootCounter);
			writer.Write(JumpBugFix);
			writer.Write(ActionCounter);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			ShootCounter = reader.ReadInt32();
			JumpBugFix = reader.ReadInt32();
			ActionCounter = reader.ReadInt32();
		}
}*/