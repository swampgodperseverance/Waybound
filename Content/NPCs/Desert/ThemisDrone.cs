using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.ItemDropRules;
using Waybound.Content.Items.Materials.Misc;
using Waybound.Content.Items.Placeable.Banners;
using Waybound.Helpers;

namespace Waybound.Content.NPCs.Desert
{
	public class ThemisDrone : ModNPC
	{
		public int frame;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Desert Drone");
			Main.npcFrameCount[NPC.type] = 4;

			var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
			{
				Position = new Vector2(0f, 2f),
				PortraitPositionXOverride = 0f,
				PortraitPositionYOverride = -20f
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

			// NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
			// {
			// 	SpecificallyImmuneTo = new int[]
			// 	{
			// 		BuffID.Poisoned
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
				new FlavorTextBestiaryInfoElement("Mods.Waybound.Bestiary.ThemisDrone")
			});
		}

        public override void SetDefaults()
        {
            NPC.width = 36;
            NPC.height = 30;
            NPC.damage = 20;
            NPC.defense = 3;
            NPC.lifeMax = 40;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.Item14;
            NPC.knockBackResist = 0.25f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = false;
            NPC.friendly = false;
            NPC.aiStyle = -1;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<ThemisDroneBannerI>();
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];

            NPC.rotation = NPC.velocity.X * 0.15f;
            NPC.spriteDirection = NPC.direction = player.Center.X < NPC.Center.X ? 1 : -1;

            Vector2 vectorToPlayer = player.Center - NPC.Center;
            float distance = vectorToPlayer.Length();

            if (distance > 600)
            {
                if (NPC.ai[0] < 10f)
                    NPC.ai[0] += Main.rand.NextFloat(0.9f, 1.1f);
                else
                    NPC.ai[0] = 10f;
                NPC.ai[1] = 100f;
            }
            else
            {
                if (NPC.ai[0] > 5f)
                    NPC.ai[0] -= Main.rand.NextFloat(0.9f, 1.1f);
                else
                    NPC.ai[0] = 5f;
                NPC.ai[1] = 60f;
            }

            if (distance > 20f)
            {
                vectorToPlayer.Normalize();
                vectorToPlayer *= NPC.ai[0];
                NPC.velocity = (NPC.velocity * (NPC.ai[1] - 1) + vectorToPlayer) / NPC.ai[1];
            }
            else if (NPC.velocity == Vector2.Zero)
            {
                NPC.velocity.X = -0.15f;
                NPC.velocity.Y = -0.15f;
            }

            if (!NPC.HasTileOnSide(4, new Vector2(0, 4), true)
                && NPC.IsOnPlatformNPC(new Vector2(1f, 1f)))
            {
                NPC.noTileCollide = true;
                if (NPC.HasTileOnSide(1, new Vector2(4, 0), true) || NPC.HasTileOnSide(2, new Vector2(4, 0), true))
                    NPC.velocity.X = 0;
            }
            else
                NPC.noTileCollide = false;

            Lighting.AddLight(NPC.position, 1.5f, 0.75f, 0.5f);
        }

        public override void FindFrame(int frameHeight)
		{
			NPC.frame.Y = frame * frameHeight;
			NPC.spriteDirection = NPC.direction;
			NPC.frameCounter++;

			if (NPC.frameCounter > 3)
			{
				if (NPC.frameCounter > 3)
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

        public override void OnKill()
        {
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];

            player.PlayerScreen().ScreenShakeIntensity = 10;

            for (int i = 0; i < 75; i++)
            {
                int dust = Dust.NewDust(new Vector2(NPC.Center.X, NPC.Center.Y), NPC.width / 2, NPC.height / 2, DustID.Torch, Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f), 50, default(Color), Main.rand.NextFloat(1f, 3f));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 3f;
                Main.dust[dust].velocity.Y *= 3f;
            }

            for (int i = 0; i < 15; i++)
            {
                int dust = Dust.NewDust(new Vector2(NPC.Center.X, NPC.Center.Y), NPC.width / 2, NPC.height / 2, 31, Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f), 80, default(Color), Main.rand.NextFloat(1f, 2f));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1f;
            }

            for (int i = 0; i < Main.rand.Next(3, 10); i++)
            {
                Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.Center.X, NPC.Center.Y), new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f)), Main.rand.Next(61, 64));
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			int desertWreckage = ModContent.ItemType<DesertWreckage>();
			var desertWreckageParameters = new DropOneByOne.Parameters()
			{
				ChanceNumerator = 1,
				ChanceDenominator = 1,
				MinimumStackPerChunkBase = 2,
				MaximumStackPerChunkBase = 4,
				MinimumItemDropsCount = 1,
				MaximumItemDropsCount = 1,
			};
			npcLoot.Add(new DropOneByOne(desertWreckage, desertWreckageParameters));

			DownedThemis dropCondition = new DownedThemis();
			IItemDropRule conditionalRule = new LeadingConditionRule(dropCondition);
			int itemType = ModContent.ItemType<DesertCore>();
			IItemDropRule rule = ItemDropRule.Common(itemType, chanceDenominator: 2);
			conditionalRule.OnSuccess(rule);
			npcLoot.Add(conditionalRule);
		}

		/*public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
			int shader = GameShaders.Armor.GetShaderIdFromItemId(3039);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            GameShaders.Armor.ApplySecondary(shader, Main.player[Main.myPlayer], null);
			return true;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
		}*/

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
            if (spawnInfo.Player.ZoneOverworldHeight && !spawnInfo.Water)
            {
                if (spawnInfo.Player.ZoneDesert)
                    return 0.2f;
                else
                    return 0.05f;
            }
            else
                return 0f;
		}
	}
}