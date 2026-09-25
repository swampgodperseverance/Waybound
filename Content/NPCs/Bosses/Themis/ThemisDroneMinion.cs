using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Helpers;

namespace Waybound.Content.NPCs.Bosses.Themis
{
	public class ThemisDroneMinion : ModNPC
	{
		public int frame;
		public int canDash = 0;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Drone of Themis");
			Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

            // NPCDebuffImmunityData debuffData = new NPCDebuffImmunityData
            // {
            //     SpecificallyImmuneTo = new int[]
            //     {
            //         BuffID.Poisoned
            //     }
            // };
            // NPCID.Sets.DebuffImmunitySets/* tModPorter Removed: See the porting notes in https://github.com/tModLoader/tModLoader/pull/3453 */.Add(Type, debuffData);
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;

        }

		public override void SetDefaults()
		{
			NPC.width = 36;
			NPC.height = 30;
			NPC.damage = 20;
			NPC.defense = 3;
			NPC.lifeMax = 30;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.Item14;
			NPC.knockBackResist = 0.25f;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.boss = false;
			NPC.friendly = false;
			NPC.aiStyle = -1;
		}

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)/* tModPorter Note: bossLifeScale -> balance (bossAdjustment is different, see the docs for details) */
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.5f * balance);
        }

        public override void OnSpawn(IEntitySource source)
		{
            NPC.TargetClosest(true);
            base.OnSpawn(source);
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
                if (NPC.ai[0] < 20f)
                    NPC.ai[0] += Main.rand.NextFloat(0.9f, 1.1f);
                else
                    NPC.ai[0] = 20f;
                NPC.ai[1] = 100f;
            }
            else
            {
                if (NPC.ai[0] > 5f)
                    NPC.ai[0] -= Main.rand.NextFloat(0.4f, 0.6f);
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

            if (NPC.ai[2] < 45)
			{
				NPC.ai[2]++;
                NPC.noTileCollide = true;
            }
			else if (!NPC.HasTileOnSide(4, new Vector2(0, 4), true)
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
			NPC.frameCounter++;
			if (NPC.frameCounter > 3)
			{
				frame++;
				if (frame >= 4)
					frame = 0;
				NPC.frameCounter = 0;
			}
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
            NPC.TargetClosest(true);
            base.HitEffect(hit);
		}

		public override void OnKill()
		{
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
	}
}