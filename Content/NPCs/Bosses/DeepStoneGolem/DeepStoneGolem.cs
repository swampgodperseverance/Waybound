using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.Systems;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Items.Accessories.Shields;
using Waybound.Content.Items.Materials.Misc;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Items.Placeable.Bosses;
using Waybound.Content.Items.Placeable.Furniture;
using Waybound.Content.Items.Vanity.BossMasks;
using Waybound.Content.Items.Weapons.Melee.Flails;
using Waybound.Content.Projectiles.Hostile;
using Waybound.Helpers;

namespace Waybound.Content.NPCs.Bosses.DeepStoneGolem
{
    [AutoloadBossHead]
    public class DeepStoneGolem : ModNPC
	{
		private int frame = -1;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deepstone Golem");
			Main.npcFrameCount[NPC.type] = 13;
			NPCID.Sets.TrailCacheLength[NPC.type] = 5;
			NPCID.Sets.TrailingMode[NPC.type] = 0;

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                CustomTexturePath = "Waybound/Content/NPCs/Bosses/DeepStoneGolem/DeepStoneGolem_Bestiary",
                Position = new Vector2(0f, 0),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 0f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

   //          NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			// {
			// 	SpecificallyImmuneTo = new int[]
			// 	{
			// 		BuffID.Poisoned,
			// 		BuffID.Confused,
			// 		BuffID.OnFire
			// 	}
			// };
			// NPCID.Sets.DebuffImmunitySets/* tModPorter Removed: See the porting notes in https://github.com/tModLoader/tModLoader/pull/3453 */.Add(Type, debuffData);
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;

		}

		public override void SetDefaults()
		{
            //NPC.aiStyle = 3;
            NPC.aiStyle = -1;
            NPC.lifeMax = 800;
			NPC.defense = 8;
			NPC.damage = 21;
			NPC.width = 40;
			NPC.height = 58;
			NPC.value = 750;
			NPC.knockBackResist = 0f;
			NPC.DeathSound = SoundID.Tink;
			NPC.HitSound = SoundID.Tink;
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			NPC.friendly = false;
			NPC.boss = true;
			NPC.dontTakeDamage = false;
            NPC.netAlways = true;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<Common.Biome.CrystalDepthsBiome>().Type };
			if (!Main.dedServ)
			{
				Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/CrystalDepths");
			}
		}

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)/* tModPorter Note: bossLifeScale -> balance (bossAdjustment is different, see the docs for details) */
		{
			NPC.lifeMax = (int)(NPC.lifeMax * 0.825f * balance);
		}

        public override void OnSpawn(IEntitySource source)
        {
            NPC.ai[1] = 600;
            base.OnSpawn(source);
        }

        public override void AI()
		{
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];
            Lighting.AddLight(NPC.Center, 0.80f, 0.51f, 0.56f);

            if (!player.active || player.dead)
            {
                for (int i = 0; i < 60; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(4f, 4f);
                    Dust dust = Dust.NewDustPerfect(NPC.Center + speed * 5, ModContent.DustType<DeepMagicDust>(), speed, Scale: 1.5f);
                    dust.noGravity = true;
                }
                NPC.netUpdate = true;
                NPC.active = false;
                return;
            }

            if (frame == -1)
            {
                NPC.Center = player.Center + new Vector2(0, -180);
                frame = 0;
            }

            if (Main.expertMode)
            {
				ExpertAttack(player);
            }

            if (Main.rand.Next(100) < 75 && NPC.HasTileOnSide(4, new Vector2(0, 5), false) && Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
                NPC.ai[0]++;
            if(Main.rand.Next(100) < 75 || NPC.ai[1] < 0)
                NPC.ai[1]++;
            if(NPC.ai[1] >= 0 && !Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
                NPC.ai[1] += 3;

            if (NPC.ai[0] >= 240)
            {
                if (Main.rand.Next(100) < 66 || frame > 9)
                {
                    NPC.velocity.X *= 0.9f;
                    NPC.velocity.Y += 0.15f;
                    if (NPC.ai[0] == 240)
                    {
                        NPC.frameCounter = 0;
                        frame = 10;
                    }
                    if (NPC.frameCounter > 7)
                    {
                        if (frame == 12)
                        {
                            frame = 1;
                            NPC.ai[0] = 0;
                        }
                        else
                            frame++;
                        if (frame == 11)
                        {
                            Vector2 direction1 = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                            direction1 = direction1.RotatedByRandom(MathHelper.ToRadians(7.5f)) * 2.75f;
                            int type = ModContent.ProjectileType<DeepCrystalBall>();
                            Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), new Vector2(NPC.Center.X, NPC.Center.Y), direction1, type, 16, 3, Main.myPlayer);
                        }
                        NPC.frameCounter = 0;
                    }
                }
                else
                    NPC.ai[0] = -180;
            }
            else if(NPC.ai[1] > -25)
            {
                Helpers.BaseHelper.FighterAI(NPC, 0.05f, 1.5f, 10, 0.15f, false);
                if (NPC.ai[1] < 0)
                    NPC.noTileCollide = true;
                if (NPC.frameCounter > 7)
                {
                    if (!NPC.HasTileOnSide(4, new Vector2(0, 1f)) || NPC.noTileCollide == true)
                        frame = 9;
                    else
                    {
                        if (frame < 1 || frame > 9)
                        {
                            frame = 1;
                        }
                        frame++;
                        if (frame >= 9)
                        {
                            frame = 1;
                        }
                    }
                    NPC.frameCounter = 0;
                }
            }
            else
            {
                NPC.velocity.X = 0;
                NPC.velocity.Y = 0;
                for (int i = 0; i < 15; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                    Dust dust = Dust.NewDustPerfect(NPC.Center + speed * 60, ModContent.DustType<DeepMagicDust>(), speed, Scale: 1.5f);
                    dust.noGravity = true;
                }
                if(NPC.ai[1] == -25)
                {
                    NPC.dontTakeDamage = false;
                    NPC.noGravity = false;
                }
            }

            if((NPC.ai[1] >= 600 || Vector2.Distance(NPC.Center, player.Center) > 450) && Vector2.Distance(NPC.Center, player.Center) < 900)
            {
                NPC.ai[0] = 1;
                NPC.Center = player.Center + new Vector2(0, -180);
                frame = 9;
                NPC.noTileCollide = true;
                NPC.noGravity = true;
                NPC.dontTakeDamage = true;
                NPC.ai[1] = -45;
            }

            if (NPC.ai[0] < 0)
            {
                NPC.dontTakeDamage = true;
                for (int i = 0; i < 5; i++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(0.1f, 0.1f);
                    Dust dust = Dust.NewDustPerfect(NPC.Center + speed * 600, ModContent.DustType<DeepMagicDust>(), speed, Scale: 1.5f);
                    dust.noGravity = true;
                    dust.velocity = new Vector2(NPC.velocity.X * 1.25f, 0);
                }
            }
            else if(NPC.ai[0] == 0)
            {
                NPC.ai[0] += 90;
                NPC.dontTakeDamage = false;
            }
        }

		private void ExpertAttack(Player player)
		{
            NPC.localAI[0]++;

            if (NPC.localAI[0] > 180)
            {
                if (NPC.ai[0] <= 240 && NPC.ai[1] >= 0)
                {
                    Vector2 pos = player.Center + new Vector2(Main.rand.NextFloat(300, 350), 0).RotatedByRandom(MathHelper.ToRadians(360));
                    Vector2 vel = (player.Center - pos).SafeNormalize(Vector2.UnitX);
                    vel = vel.RotatedByRandom(MathHelper.ToRadians(7.5f)) * 2f;
                    int type = ModContent.ProjectileType<DeepCrystalProj>();
                    int proj = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), pos, vel, type, 15, 3, Main.myPlayer);
                    Main.projectile[proj].tileCollide = false;
                    Main.projectile[proj].scale = 1.15f;
                    Main.projectile[proj].timeLeft = 180;
                    Main.projectile[proj].friendly = false;
                    Main.projectile[proj].hostile = true;

                    for (int i = 0; i < 40; i++)
                    {
                        Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
                        Dust dust = Dust.NewDustPerfect(pos, ModContent.DustType<DeepMagicDust>(), speed * 5f, Scale: 1.5f);
                        dust.noGravity = true;
                    }
                }
                NPC.localAI[0] = 0;
            }
        }

		public override void FindFrame(int frameHeight)
		{
            NPC.frame.Y = frame * frameHeight;
            NPC.spriteDirection = NPC.direction;
            NPC.frameCounter++;
        }

		public override void OnKill()
		{
			NPC.SetEventFlagCleared(ref DownedBossSystem.DownedDeepStoneGolem, -1);
		}

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Texture2D texture = ModContent.Request<Texture2D>("Waybound/Content/NPCs/Bosses/DeepStoneGolem/DeepStoneGolem").Value;
			Texture2D textureG = ModContent.Request<Texture2D>("Waybound/Content/NPCs/Bosses/DeepStoneGolem/DeepStoneGolemG").Value;
			Vector2 drawOrigin = new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);
			Vector2 pos = (NPC.position - Main.screenPosition) + drawOrigin + new Vector2(-10, 2);
			for (int k = 0; k < NPC.oldPos.Length; k++)
			{
				Vector2 drawPos = (NPC.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(-10, 2);
				Color color = NPC.GetAlpha(new Color(255, 168, 135, 0)) * 0.65f * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
				Main.EntitySpriteDraw(textureG, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale - 0.05f, effects, 0);
			}
			Main.EntitySpriteDraw(texture, pos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, drawOrigin, NPC.scale, effects, 0);
			Main.EntitySpriteDraw(textureG, pos, NPC.frame, NPC.GetAlpha(new Color(255, 168, 135, 0)) * 0.35f, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
			return false;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{

			int DeepStone = ModContent.ItemType<DeepStoneItem>();
			var DeepStoneParameters = new DropOneByOne.Parameters()
			{
				ChanceNumerator = 1,
				ChanceDenominator = 1,
				MinimumStackPerChunkBase = 5,
				MaximumStackPerChunkBase = 10,
				MinimumItemDropsCount = 1,
				MaximumItemDropsCount = 1,
			};
			npcLoot.Add(new DropOneByOne(DeepStone, DeepStoneParameters));

			int DeepCrystal = ModContent.ItemType<DeepCrystalShardItem>();
			var DeepCrystalParameters = new DropOneByOne.Parameters()
			{
				ChanceNumerator = 1,
				ChanceDenominator = 2,
				MinimumStackPerChunkBase = 1,
				MaximumStackPerChunkBase = 5,
				MinimumItemDropsCount = 1,
				MaximumItemDropsCount = 1,
			};
			npcLoot.Add(new DropOneByOne(DeepCrystal, DeepCrystalParameters));

			int core = ModContent.ItemType<DeepGolemCore>();
			var coreParameters = new DropOneByOne.Parameters()
			{
				ChanceNumerator = 1,
				ChanceDenominator = 1,
				MinimumStackPerChunkBase = 1,
				MaximumStackPerChunkBase = 1,
				MinimumItemDropsCount = 1,
				MaximumItemDropsCount = 1,
			};
			npcLoot.Add(new DropOneByOne(core, coreParameters));

			int DeepShield = ModContent.ItemType<DeepStoneShield>();
			var DeepShieldParameters = new DropOneByOne.Parameters()
			{
				ChanceNumerator = 1,
				ChanceDenominator = 3,
				MinimumStackPerChunkBase = 1,
				MaximumStackPerChunkBase = 1,
				MinimumItemDropsCount = 1,
				MaximumItemDropsCount = 1,
			};
			npcLoot.Add(new DropOneByOne(DeepShield, DeepShieldParameters));

            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DeepStoneGolemMask>(), 7));

            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DeepStoneGolemTrophyI>(), 10));

            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<DeepStoneGolemRelicI>()));

            npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<DeepFlail>(), 10));
        }
	}
}