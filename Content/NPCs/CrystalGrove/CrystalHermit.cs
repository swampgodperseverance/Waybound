using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.Biome;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Items.Placeable.Furniture;
using Waybound.Content.Projectiles.Hostile;

namespace Waybound.Content.NPCs.CrystalGrove
{
	public class CrystalHermit : ModNPC
	{
		public int frame = 4;
		public int timer = 0;
		public int attackChoice = 0;
		public bool activated = false;
		public bool hiding = false;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Crystal Hermit");
			Main.npcFrameCount[NPC.type] = 5;
			NPCID.Sets.TrailCacheLength[NPC.type] = 5;
			NPCID.Sets.TrailingMode[NPC.type] = 1;

			NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			{
				SpecificallyImmuneTo = new int[]
				{
					 ModContent.BuffType<DeepFire>()
				}
			};
			// NPCID.Sets.DebuffImmunitySets/* tModPorter Removed: See the porting notes in https://github.com/tModLoader/tModLoader/pull/3453 */.Add(Type, debuffData);
			NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<DeepFire>()] = true;

        }

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.lifeMax = 450;
			NPC.defense = 15;
			NPC.damage = 30;
			NPC.width = 38;
			NPC.height = 26;
			NPC.value = 120;
			NPC.DeathSound = SoundID.Item27;
			NPC.HitSound = SoundID.Item27;
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			NPC.friendly = false;
			NPC.knockBackResist = 0.05f;
			SpawnModBiomes = new int[1] { ModContent.GetInstance<CrystalDepthsBiome>().Type };
		}

		public override void AI()
		{
			NPC.TargetClosest(true);

			Player player = Main.player[NPC.target];

			Lighting.AddLight(NPC.Center, 0.80f, 0.51f, 0.56f);

			if (hiding)
			{
				NPC.dontTakeDamage = true;
				frame = 4;
				NPC.aiStyle = -1;
				NPC.velocity.X = 0;
				timer++;
				if (timer > 120)
				{
					for (int i = 0; i < 2; i++)
					{
						float x = i == 1 ? 1 : -1;
						Vector2 direction = new Vector2(x * 4, -5) * Main.rand.NextFloat(0.9f, 1.1f);
						int type = ModContent.ProjectileType<DeepBoulder>();
						int p = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), NPC.Center + new Vector2(0, -4), direction, type, 40, 2, Main.myPlayer);
						Main.projectile[p].timeLeft = Main.rand.Next(360, 480);
						Main.projectile[p].friendly = false;
						Main.projectile[p].hostile = true;
					}
					NPC.velocity.Y = -10;
					NPC.dontTakeDamage = false;
					hiding = false;
					activated = true;
					NPC.netUpdate = true;
					timer = 0;
				}
			}

			if (Vector2.Distance(NPC.Center, player.Center) < 200 && activated == false)
			{
				frame = 0;
				activated = true;
			}

			if (!hiding && activated)
			{
				if (NPC.position.X >= player.position.X)
				{
					NPC.spriteDirection = -1;
				}
				else
				{
					NPC.spriteDirection = 1;
				}
				NPC.frameCounter++;
				if (NPC.frameCounter > 4)
				{
					frame++;
					if (frame > 3)
					{
						frame = 0;
					}
					NPC.frameCounter = 0;
				}
				NPC.aiStyle = 3;
				timer++;
				if (timer > 270)
				{
					attackChoice = Main.rand.Next(1, 4);
					switch (attackChoice)
					{
						case 1:
							hiding = true;
							break;
						case 2:
							NPC.velocity = player.position.X < NPC.position.X ? new Vector2(-6.5f, -4) : new Vector2(6.5f, -4);
							break;
						case 3:
							for (int i = 0; i < 5; i++)
							{
								Vector2 direction = new Vector2(0, -1).RotatedBy(MathHelper.ToRadians(90 - 45 * i));
								int type = ModContent.ProjectileType<DeepCrystalProj>();
								int p = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), NPC.Center + new Vector2(0, -4), direction * 2.5f, type, 20, 2, Main.myPlayer);
								Main.projectile[p].friendly = false;
								Main.projectile[p].hostile = true;
							}
							break;
						default:
							hiding = true;
							break;
					}
					NPC.netUpdate = true;
					timer = 0;
				}
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Texture2D texture = ModContent.Request<Texture2D>("Waybpound/Content/NPCs/CrystalGrove/CrystalHermit_G").Value;
			Vector2 drawOrigin = new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);
			for (int k = 0; k < NPC.oldPos.Length; k++)
			{
				Vector2 drawPos = (NPC.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(-7, -10);
				Color color = NPC.GetAlpha(new Color(255, 168, 135, 0)) * 0.65f * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
				Main.EntitySpriteDraw(texture, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
			}
			return true;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Texture2D texture = ModContent.Request<Texture2D>("Waybpound/Content/NPCs/CrystalGrove/CrystalHermit_G").Value;
			Vector2 drawOrigin = new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);
			Vector2 pos = (NPC.position - Main.screenPosition) + drawOrigin + new Vector2(-7, -10);
			Main.EntitySpriteDraw(texture, pos, NPC.frame, NPC.GetAlpha(new Color(255, 168, 135, 0)) * 0.35f, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frame.Y = frame * frameHeight;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			int deepStone = ModContent.ItemType<DeepStoneItem>();
			var deepStoneParameters = new DropOneByOne.Parameters()
			{
				ChanceNumerator = 1,
				ChanceDenominator = 2,
				MinimumStackPerChunkBase = 5,
				MaximumStackPerChunkBase = 10,
				MinimumItemDropsCount = 1,
				MaximumItemDropsCount = 1,
			};
			npcLoot.Add(new DropOneByOne(deepStone, deepStoneParameters));

			int deepCrystal = ModContent.ItemType<DeepCrystalShardItem>();
			var deepCrystalParameters = new DropOneByOne.Parameters()
			{
				ChanceNumerator = 1,
				ChanceDenominator = 2,
				MinimumStackPerChunkBase = 1,
				MaximumStackPerChunkBase = 5,
				MinimumItemDropsCount = 1,
				MaximumItemDropsCount = 1,
			};
			npcLoot.Add(new DropOneByOne(deepCrystal, deepCrystalParameters));
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if (spawnInfo.Player.InModBiome(ModContent.GetInstance<CrystalDepthsBiome>()) && NPC.downedBoss3)
			{
				return 0.05f;
			}
			else
			{
				return 0f;
			}
		}
	}
}