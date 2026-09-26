using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.Systems;
using static System.Net.Mime.MediaTypeNames;
using Waybound.Content.BossBars;
using Waybound.Helpers;
using Waybound.Common.ModSystems;

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

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Themis");
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

			// NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			// {
			// 	SpecificallyImmuneTo = new int[]
			// 	{
			// 		BuffID.Poisoned,
			// 	}
			// };
			// NPCID.Sets.DebuffImmunitySets/* tModPorter Removed: See the porting notes in https://github.com/tModLoader/tModLoader/pull/3453 */.Add(Type, debuffData);
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
			NPC.lifeMax = 3200;
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

		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)/* tModPorter Note: bossLifeScale -> balance (bossAdjustment is different, see the docs for details) */
		{
			NPC.lifeMax = (int)(NPC.lifeMax * 0.75f * balance);
		}

		public override void OnSpawn(IEntitySource source)
		{
            NPC.TargetClosest(true);
            base.OnSpawn(source);
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

		public void Fight(Player player)
		{
			if(NPC.ai[0] < 0)
                NPC.ai[0]++;
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
					NPC.localAI[0] = NPC.localAI[1];
					while (NPC.localAI[1] == NPC.localAI[0])
					{
						if (!Main.expertMode)
							NPC.localAI[1] = Main.rand.Next(1, 5);
						else
							NPC.localAI[1] = Main.rand.Next(1, 6);
					}
                    NPC.TargetClosest(true);
					NPC.netUpdate = true;
				}
            }

			switch(NPC.localAI[1])
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
            }
		}

        public void Jump(Player player)
		{
            if (NPC.ai[0] == 0)
			{
				float x = NPC.Center.X - NPC.spriteDirection * 20 < player.MountedCenter.X ? 1 : -1;
				NPC.velocity = new Vector2(Main.rand.NextFloat(7, 9) * x, -Main.rand.NextFloat(9, 11));
                frame = 4;
				NPC.netUpdate = true;
                NPC.noTileCollide = true;
                NPC.noGravity = true;
                NPC.ai[0]++;
            }
			else
			{
				NPC.velocity.Y += 0.35f + NPC.velocity.Y * 0.01f;
                NPC.velocity.X *= MathF.Min(0.99f - MathF.Abs(NPC.velocity.Y) * 0.0025f, 0.99f);
            }

			if(NPC.IsOnPlatformNPC(new Vector2(1f, 1f)) && NPC.Center.Y + NPC.height / 2 < player.MountedCenter.Y + player.height / 2)
                NPC.noTileCollide = true;
			else if (NPC.velocity.Y > 4)
			{
				if(NPC.ai[0] == 1)
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

			if((NPC.HasTileOnSide(4, new Vector2(1f, 2f), false) || NPC.IsOnPlatformNPC(new Vector2(1f, 1f))) && !NPC.noTileCollide)
			{
                for (int i = 0; i < 15; i++)
                {
                    int dust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + NPC.height - 2), NPC.width, 4, DustID.Smoke, Main.rand.Next(-8, 8), 0, 120, default(Color), Main.rand.NextFloat(1f, 1.75f));
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 1f;
                }
                if (!DownedBossSystem.DownedThemis)
                    SoundEngine.PlaySound(SoundID.Dig, NPC.Center);
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
                    int npc = NPC.NewNPC(NPC.GetSource_FromAI(), (int)player.Center.X - 1000 + 1000 * i, (int)NPC.Center.Y - 1000, ModContent.NPCType<ThemisDroneMinion>(), NPC.whoAmI);
                    NPC.netUpdate = true;
                }
                frame = 13;
			}

            NPC.ai[0]++;

            if (NPC.ai[0] == 30)
                SoundEngine.PlaySound(new("Waybound/Assets/Sounds/DroneCall"), NPC.Center);

            if (NPC.ai[0] == 60)
            {
                frame = 0;
                NPC.ai[0] = -30;
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

            if (phase == 1 && NPC.ai[0] == 60 || NPC.ai[0] == 50)
            {
                Vector2 vel = Vector2.Zero;
                Vector2 pos = Vector2.Zero;
                for (int i = 0; i < 4; i++)
                {
                    vel = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(12f)) * Main.rand.NextFloat(8.5f, 9.5f);
                    pos = NPC.Center + new Vector2(0, -5) + vel.SafeNormalize(Vector2.UnitX) * 20;
                    int proj1 = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), pos, vel, ProjectileID.BulletDeadeye, 15, 3, Main.myPlayer);
                    Main.projectile[proj1].timeLeft = 240;
                }
                vel = new Vector2(player.Center.X - NPC.Center.X, player.Center.Y - NPC.Center.Y - 50).SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(8f, 11f);
                int proj2 = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), pos, vel, ModContent.ProjectileType<ThemisBomb>(), 0, 3, Main.myPlayer);
                Main.projectile[proj2].timeLeft = Main.rand.Next(60, 90);

                player.PlayerScreen().fastScreenShake = 7 * (1000 - Vector2.Distance(player.Center, NPC.Center)) / 1000;
                Lighting.AddLight(pos, 1.5f, 0.75f, 0.5f);
                SoundEngine.PlaySound(SoundID.Item36, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item149, NPC.Center);

                NPC.netUpdate = true;
                NPC.ai[1]++;
                NPC.ai[0] = phase == 1 ? 0 : 10;
            }

            if (NPC.ai[1] == 4)
            {
                frame = 0;
                NPC.ai[0] = -45;
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

            if (NPC.ai[0] == 10 && NPC.ai[1] > 0 || NPC.ai[0] == 30)
            {
                Vector2 vel = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(5f, 7f);
                Vector2 pos = NPC.Center + new Vector2(0, -10) + vel.SafeNormalize(Vector2.UnitX) * 12f;
                int proj1 = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), pos, vel, ModContent.ProjectileType<ThemisRocket>(), 15, 3, Main.myPlayer);
                Main.projectile[proj1].timeLeft = 240;

                player.PlayerScreen().fastScreenShake = 3.5f * (1000 - Vector2.Distance(player.Center, NPC.Center)) / 1000;
                Lighting.AddLight(pos, 1.5f, 0.75f, 0.5f);
                SoundEngine.PlaySound(SoundID.Item11, NPC.Center);

                NPC.netUpdate = true;
                NPC.ai[1]++;
                NPC.ai[0] = 0;
            }

            if (NPC.ai[1] == 10)
            {
                frame = 0;
                NPC.ai[0] = -60;
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
							SystemUI.Instance.dialogueUI.DisplayDialogue("And here you are. I've been following you for a long time.\n It's time for your death!", 120, 20, 0.4f, "Themis:", 0, txtColor, null, null, NPC.Center, sound: false);
							break;
						case 1:
							SystemUI.Instance.dialogueUI.DisplayDialogue("Okay, I underestimated you...", 120, 20, 0.4f, "Themis:", 0, txtColor, null, null, NPC.Center, sound: false);
							phase = 2;
							break;
                        case 2:
                            SystemUI.Instance.dialogueUI.DisplayDialogue("You can't stop me from killing the king!", 120, 20, 0.4f, "Themis:", 0, txtColor, null, null, NPC.Center, sound: false);
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
						SystemUI.Instance.titleUI.DisplayTitle("Themis", 90, 90, 0.8f, 0, txtColor, txtColor, "the hunter of king");
                        NPC.ai[0] = -60;
                    }
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

                for (int i = 0; i < Main.rand.Next(3, 10); i++)
                {
                    Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.Center.X, NPC.Center.Y), new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f)), Main.rand.Next(61, 64));
                }
                SystemUI.Instance.titleUI.DisplayTitle("Mech Themis", 90, 90, 0.8f, 0, txtColor, txtColor, "Themis' robot");
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
			if (NPC.ai[0] == 1)
			{
                player.PlayerScreen().notHurt = true;
                player.PlayerScreen().cutscene = true;
				player.PlayerScreen().lockScreen = true;
				player.PlayerScreen().ScreenFocusPosition = NPC.position;
				NPC.noTileCollide = false;
				NPC.dontTakeDamage = true;
			}
			NPC.ai[0]++;

			if (NPC.ai[0] < 65)
			{
				if (NPC.ai[0] >= 25)
					NPC.alpha += 10;
				if (NPC.ai[0] == 25)
				{
					NPC.velocity = new Vector2(6 * NPC.spriteDirection, -16);
					NPC.noTileCollide = true;
				}
				if (NPC.ai[0] == 45)
					NPC.velocity = new Vector2(8 * NPC.spriteDirection, -12);
				if (NPC.ai[0] == 55)
					NPC.velocity = new Vector2(10 * NPC.spriteDirection, -8);
			}
			if (NPC.ai[0] == 70f)
			{
				NPC.velocity = Vector2.Zero;
				NPC.noGravity = true;
				SystemUI.Instance.dialogueUI.DisplayDialogue("This time you won... But next time you will not live!", 120, 20, 0.4f, "Themis:", 0, txtColor, null, null, NPC.Center, sound: false);
			}

			if (NPC.ai[0] >= 180f)
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
            Texture2D texture = !DownedBossSystem.DownedThemis ? TextureAssets.Npc[NPC.type].Value : ModContent.Request<Texture2D>("VictimaMod2/Content/NPCs/Bosses/Themis/themisMech").Value;
            Vector2 drawOrigin = new(texture.Width * 0.5f, texture.Height * 0.5f / Main.npcFrameCount[NPC.type]);
            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                Vector2 drawPos = (NPC.oldPos[k] - Main.screenPosition) + new Vector2(NPC.width * 0.5f, NPC.height * 0.5f) + new Vector2(0, 2);
                Color color = NPC.GetAlpha(drawColor) * 0.4f * (1 - (float)NPC.alpha / 255) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale - 0.02f * k, effects1, 0);
            }
            Vector2 pos = (NPC.position - Main.screenPosition) + new Vector2(NPC.width * 0.5f, NPC.height * 0.5f) + new Vector2(0, 2);
            Main.EntitySpriteDraw(texture, pos, NPC.frame, drawColor * (1 - (float)NPC.alpha / 255), NPC.rotation, drawOrigin, NPC.scale, effects1, 0);

            Texture2D weaponTexture = !DownedBossSystem.DownedThemis ? ModContent.Request<Texture2D>("VictimaMod2/Content/NPCs/Bosses/Themis/themisHands").Value : ModContent.Request<Texture2D>("VictimaMod2/Content/NPCs/Bosses/Themis/themisMechHands").Value;

            if (NPC.localAI[1] == 4 || NPC.localAI[1] == 5)
			{
				var effects2 = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
                Rectangle rect = NPC.localAI[1] == 4 ? new Rectangle(0, 0, 62, 24) : new Rectangle(0, 26, 62, 26);
                drawOrigin = new(rect.Width * (NPC.localAI[1] == 4 ? 0.25f : 0.33f), rect.Height * 0.5f);
                float rot = (Main.player[NPC.target].MountedCenter - NPC.Center).ToRotation();
                Main.EntitySpriteDraw(weaponTexture, pos + new Vector2(0, NPC.localAI[1] == 2 ? -6 : -10), rect, drawColor * (1 - (float)NPC.alpha / 255), rot, drawOrigin, NPC.scale, effects2, 0);
            }

			return false;
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
                for (int i = 0; i < Main.rand.Next(3, 10); i++)
                {
                    Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.Center.X, NPC.Center.Y), new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f)), Main.rand.Next(61, 64));
                }
            }
			else
				NPC.SetEventFlagCleared(ref DownedBossSystem.DownedThemis, -1);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            //npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Placeable.Trophies.themisTrophyItem>(), 10));

            LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());

           // int desertWreckage = ModContent.ItemType<Items.Other.desertWreckage>();
            var desertWreckageParameters = new DropOneByOne.Parameters()
            {
                ChanceNumerator = 1,
                ChanceDenominator = 1,
                MinimumStackPerChunkBase = 25,
                MaximumStackPerChunkBase = 35,
                MinimumItemDropsCount = 1,
                MaximumItemDropsCount = 1,
            };
           // notExpertRule.OnSuccess(new DropOneByOne(desertWreckage, desertWreckageParameters));

          //  int desertCore = ModContent.ItemType<Items.Other.desertCore>();
            var desertCoreParameters = new DropOneByOne.Parameters()
            {
                ChanceNumerator = 1,
                ChanceDenominator = 1,
                MinimumStackPerChunkBase = 2,
                MaximumStackPerChunkBase = 5,
                MinimumItemDropsCount = 1,
                MaximumItemDropsCount = 1,
            };
           // notExpertRule.OnSuccess(new DropOneByOne(desertCore, desertCoreParameters));

           // notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Items.Vanity.BossMasks.themisMask>(), 7));

            npcLoot.Add(notExpertRule);

           // npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<Items.Bags.themisBag>()));

           // npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<Items.Placeable.Relics.ThemisRelicI>()));

           // npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Items.Mount.motorcycleOfDesertHunter>(), 4));
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
               // && spawnInfo.Player.GetModPlayer<playerAccessories>().antiradar == 0)
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