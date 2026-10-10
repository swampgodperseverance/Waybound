using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.Biome;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Content.Items.Placeable.Furniture;
using Waybound.Content.Projectiles.Hostile;

namespace Waybound.Content.NPCs.CrystalGrove
{
	public class DeepCrystalSlime : ModNPC
	{
		private int attackCounter;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Crystal Slime");
			Main.npcFrameCount[NPC.type] = 2;
			NPCID.Sets.TrailCacheLength[NPC.type] = 5;
			NPCID.Sets.TrailingMode[NPC.type] = 1;

            // NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
            // {
            //     SpecificallyImmuneTo = new int[]
            //     {
            //         ModContent.BuffType<Buffs.Debuffs.deepFire>()
            //     }
            // };
            // NPCID.Sets.DebuffImmunitySets/* tModPorter Removed: See the porting notes in https://github.com/tModLoader/tModLoader/pull/3453 */.Add(Type, debuffData);
            NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<DeepFire>()] = true;

        }

		public override void SetDefaults()
		{
			NPC.aiStyle = 1;
			NPC.lifeMax = 100;
			NPC.defense = 10;
			NPC.damage = 30;
			NPC.width = 36;
			NPC.height = 36;
			NPC.value = 300;
			NPC.DeathSound = SoundID.Item27;
			NPC.HitSound = SoundID.Item27;
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			NPC.friendly = false;
			NPC.knockBackResist = 0;
			AnimationType = NPCID.BlueSlime;
			SpawnModBiomes = new int[1] { ModContent.GetInstance<CrystalDepthsBiome>().Type };
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
		{
			if (Main.rand.NextBool(3))
			{
				target.AddBuff(ModContent.BuffType<DeepFire>(), 240);
			}
		}

		public override void AI()
		{
			Player target = Main.player[NPC.target];

			if (NPC.HasValidTarget && Main.netMode != NetmodeID.MultiplayerClient)
			{
				if (attackCounter > 0)
				{
					attackCounter--;
				}
				if (attackCounter <= 0 && Vector2.Distance(NPC.Center, target.Center) < 200 && Collision.CanHitLine(NPC.position, NPC.width, NPC.height, target.position, target.width, target.height))
				{
					for (int i = 0; i < Main.rand.Next(3, 5); i++)
					{
						Vector2 direction = new Vector2(0, -1).RotatedByRandom(MathHelper.ToRadians(180)) * Main.rand.NextFloat(0.75f, 1f);
						int type = ModContent.ProjectileType<DeepCrystalProj>();
						int p = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), NPC.Center, direction * 3f, type, 15, 2, Main.myPlayer);
						Main.projectile[p].friendly = false;
						Main.projectile[p].hostile = true;
						attackCounter = 120;
						NPC.netUpdate = true;
					}
				}
			}

			Lighting.AddLight(NPC.Center, 0.80f, 0.51f, 0.56f);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Texture2D texture = TextureAssets.Npc[NPC.type].Value;
			Vector2 drawOrigin = new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);
			for (int k = 0; k < NPC.oldPos.Length; k++)
			{
				Vector2 drawPos = (NPC.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0, 0);
				Color color = NPC.GetAlpha(new Color(255, 168, 135, 0)) * 0.5f * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
				Main.EntitySpriteDraw(texture, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale - 0.05f, effects, 0);
			}
			return true;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Texture2D texture = TextureAssets.Npc[NPC.type].Value;
			Vector2 drawOrigin = new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);
			Vector2 pos = (NPC.position - Main.screenPosition) + drawOrigin + new Vector2(0, 0);
			Main.EntitySpriteDraw(texture, pos, NPC.frame, NPC.GetAlpha(new Color(255, 168, 135, 0)) * 0.35f, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient && NPC.life <= 0)
			{
				for (int i = 0; i < 20; i++)
				{
					int dust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, ModContent.DustType<Dusts.DeepDusts.DeepMagicDust>(), NPC.velocity.X, NPC.velocity.Y, NPC.damage / 2, default(Color), 1.50f);
					Main.dust[dust].noGravity = false;
					Main.dust[dust].velocity *= 2f;
				}
			}

			if (Main.rand.NextBool(3))
			{
				for (int i = 0; i < Main.rand.Next(3, 5); i++)
				{
					Vector2 direction = new Vector2(0, -1).RotatedByRandom(MathHelper.ToRadians(180)) * Main.rand.NextFloat(0.75f, 1f);
					int type = ModContent.ProjectileType<DeepCrystalProj>();
					int p = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), NPC.Center, direction * 3f, type, 15, 2, Main.myPlayer);
					Main.projectile[p].friendly = false;
					Main.projectile[p].hostile = true;
				}
			}
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			if (spawnInfo.Player.InModBiome(ModContent.GetInstance<CrystalDepthsBiome>()))
			{
				return 0.05f;
			}
			else
			{
				return 0f;
			}
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			int deepCrystal = ModContent.ItemType<DeepCrystalShardItem>();
			var deepCrystalParameters = new DropOneByOne.Parameters()
			{
				ChanceNumerator = 1,
				ChanceDenominator = 2,
				MinimumStackPerChunkBase = 1,
				MaximumStackPerChunkBase = 1,
				MinimumItemDropsCount = 1,
				MaximumItemDropsCount = 3,
			};
			npcLoot.Add(new DropOneByOne(deepCrystal, deepCrystalParameters));
		}
	}
}