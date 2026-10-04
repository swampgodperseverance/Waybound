using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.Biome;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Content.Dusts.DeepDusts;

namespace Waybound.Content.NPCs.CrystalGrove
{
	public class CrystalWormHead : WormHead
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Crystal Worm");

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                CustomTexturePath = "Waybound/Content/NPCs/CrystalGrove/CrystalWormHead",
                Position = new Vector2(0f, 0),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 0f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

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
		public override void SetStats()
		{
			moveSpeed = 2f;
			inertiaMax = 3f;
			hbSizeScale = 0.85f;
			hasGravity = true;
			digTimerMax = 4;
			segmentsCount = 8;
			headSize = 14;
			bodySize = 10;
			tailSize = 8;
			aiType = 0;
			idleModeMaxX = 200f;
			idleModeMaxY = 100f;
			textureHead = (Texture2D)ModContent.Request<Texture2D>("Waybound/Content/NPCs/CrystalGrove/CrystalWormHead");
			textureBody = (Texture2D)ModContent.Request<Texture2D>("Waybound/Content/NPCs/CrystalGrove/CrystalWormBody");
			textureTail = (Texture2D)ModContent.Request<Texture2D>("Waybound/Content/NPCs/CrystalGrove/CrystalWormTail");
			npcTypeHead = ModContent.NPCType<CrystalWormHead>();
			npcTypeBody = ModContent.NPCType<CrystalWormBody>();
			npcTypeTail = ModContent.NPCType<CrystalWormTail>();
			inertiaUp = 0.02f;
			inertiaDown = 0.005f;
			inertiaUp = 0.025f;
			inertiaDown = 0.01f;
			rotateCheck = 10f;
		}
		public override void ExtraAI(Player player, NPC npc)
		{

		}
		public override void SetDefaults()
		{
			NPC.lifeMax = 100;
			NPC.defense = 5;
			NPC.damage = 30;
			NPC.value = 300;
			NPC.HitSound = SoundID.Item27;
			NPC.DeathSound = SoundID.Item27;
			NPC.friendly = false;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0;
			NPC.rotation = MathHelper.ToRadians(90f);
			NPC.behindTiles = true;
			SpawnModBiomes = new int[1] { ModContent.GetInstance<CrystalDepthsBiome>().Type };
		}
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Vector2 drawOrigin = new Vector2(NPC.width / 2, NPC.height / 2);
			Main.EntitySpriteDraw(textureHead, NPC.position - Main.screenPosition + drawOrigin + new Vector2(-((textureHead.Width - NPC.width) / 2), -((textureHead.Height - NPC.height) / 2)).RotatedBy(NPC.rotation), null, drawColor, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 1);
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient && NPC.life <= 0)
			{
				for (int i = 0; i < 8; i++)
				{
					int dust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, ModContent.DustType<DeepMagicDust>(), NPC.velocity.X, NPC.velocity.Y, NPC.damage / 2, default(Color), 1.50f);
					Main.dust[dust].noGravity = false;
					Main.dust[dust].velocity *= 1.15f;
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
	}
	public class CrystalWormBody : WormBody
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Crystal Worm");

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
        }
		public override void SetStats()
		{
			segmentsCount = 8;
			headSize = 14;
			bodySize = 10;
			textureBody = (Texture2D)ModContent.Request<Texture2D>("Waybound/Content/NPCs/CrystalGrove/CrystalWormBody");
			npcTypeHead = ModContent.NPCType<CrystalWormHead>();
			npcTypeBody = ModContent.NPCType<CrystalWormBody>();
		}
		public override void ExtraAI(Player player, NPC npc)
		{

		}
		public override void SetDefaults()
		{
			NPC.lifeMax = 125;
			NPC.defense = 7;
			NPC.damage = 30;
			NPC.value = 300;
			NPC.HitSound = SoundID.Item27;
			NPC.DeathSound = SoundID.Item27;
			NPC.friendly = false;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0;
			NPC.behindTiles = true;
			NPC.rotation = MathHelper.ToRadians(90f);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Vector2 drawOrigin = new Vector2(NPC.width / 2, NPC.height / 2);
			Main.EntitySpriteDraw(textureBody, NPC.position - Main.screenPosition + drawOrigin + new Vector2(-((textureBody.Width - NPC.width) / 2), -((textureBody.Height - NPC.height) / 2)).RotatedBy(NPC.rotation), null, drawColor, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 1);
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient && NPC.life <= 0)
			{
				for (int i = 0; i < 8; i++)
				{
					int dust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, ModContent.DustType<DeepMagicDust>(), NPC.velocity.X, NPC.velocity.Y, NPC.damage / 2, default(Color), 1.50f);
					Main.dust[dust].noGravity = false;
					Main.dust[dust].velocity *= 1.15f;
				}
			}
		}
	}
	public class CrystalWormTail : WormTail
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Crystal Worm");

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
        }
		public override void SetStats()
		{
			segmentsCount = 8;
			tailSize = 8;
			textureTail = (Texture2D)ModContent.Request<Texture2D>("Waybound/Content/NPCs/CrystalGrove/CrystalWormTail");
			npcTypeHead = ModContent.NPCType<CrystalWormHead>();
			npcTypeBody = ModContent.NPCType<CrystalWormBody>();
			npcTypeTail = ModContent.NPCType<CrystalWormTail>();
		}
		public override void ExtraAI(Player player, NPC npc)
		{

		}
		public override void SetDefaults()
		{
			NPC.lifeMax = 125;
			NPC.defense = 7;
			NPC.damage = 30;
			NPC.value = 300;
			NPC.HitSound = SoundID.Item27;
			NPC.DeathSound = SoundID.Item27;
			NPC.friendly = false;
			NPC.aiStyle = -1;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0;
			NPC.behindTiles = true;
			NPC.rotation = MathHelper.ToRadians(90f);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Vector2 drawOrigin = new Vector2(NPC.width / 2, NPC.height / 2);
			Main.EntitySpriteDraw(textureTail, NPC.position - Main.screenPosition + drawOrigin + new Vector2(-((textureTail.Width - NPC.width) / 2), -((textureTail.Height - NPC.height) / 2)).RotatedBy(NPC.rotation), null, drawColor, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 1);
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient && NPC.life <= 0)
			{
				for (int i = 0; i < 8; i++)
				{
					int dust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, ModContent.DustType<DeepMagicDust>(), NPC.velocity.X, NPC.velocity.Y, NPC.damage / 2, default(Color), 1.50f);
					Main.dust[dust].noGravity = false;
					Main.dust[dust].velocity *= 1.15f;
				}
			}
		}
	}
}
