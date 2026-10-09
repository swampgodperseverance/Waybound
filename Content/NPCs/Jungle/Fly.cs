using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.NPCs.Jungle
{
	public class Fly : ModNPC
	{
		public int frame;
		public float distance;
		public float speed = 5;
		public float inertia = 20;
		public Vector2 playerPos;
		public Vector2 vectorToPlayer;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Fly");
			Main.npcFrameCount[NPC.type] = 2;

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
        }

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.lifeMax = 20;
			NPC.defense = 0;
			NPC.damage = 15;
			NPC.width = 16;
			NPC.height = 16;
			NPC.value = 10;
			NPC.knockBackResist = 1f;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.noGravity = true;
			NPC.noTileCollide = false;
			NPC.friendly = false;
		}

		public override void AI()
		{
			NPC.TargetClosest(true);
			Player target = Main.player[NPC.target];

			NPC.rotation = NPC.velocity.X * 0.075f;

			playerPos = target.Center;
			vectorToPlayer = playerPos - NPC.Center;
			distance = vectorToPlayer.Length();

			if (distance > 400f)
			{
				if (speed < 10f)
				{
					speed += 0.25f;
				}
				else
				{
					speed = 5f;
				}
				inertia = 80f;
			}
			else
			{
				if (speed > 5f)
				{
					speed -= 0.1f;
				}
				else
				{
					speed = 5f;
				}
				inertia = 40f;
			}

			if (distance > 20f)
			{
				vectorToPlayer.Normalize();
				vectorToPlayer *= speed;
				NPC.velocity = (NPC.velocity * (inertia - 1) + vectorToPlayer) / inertia;
				NPC.netUpdate = true;
			}
			else if (NPC.velocity == Vector2.Zero)
			{
				NPC.velocity.X = -0.15f;
				NPC.velocity.Y = -0.05f;
			}
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frame.Y = frame * frameHeight;
			NPC.spriteDirection = NPC.direction;
			NPC.frameCounter++;
			if (NPC.frameCounter > 5)
			{
				frame++;
				if (frame >= Main.npcFrameCount[NPC.type])
				{
					frame = 0;
				}
				NPC.frameCounter = 0;
			}
		}

		public override void OnKill()
		{
			for (int i = 0; i < 10; i++)
			{
				int dust = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, 18, NPC.velocity.X * 1.2f, NPC.velocity.Y * 1.2f, 120, default(Color), 1.5f);
				Main.dust[dust].noGravity = false;
				Main.dust[dust].velocity *= 0.5f;
			}
		}
	}
}