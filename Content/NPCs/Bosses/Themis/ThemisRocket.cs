using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Projectiles.Hostile;

namespace Waybound.Content.NPCs.Bosses.Themis
{
	public class ThemisRocket : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Rocket Of Themis");
		}

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 120;
			DrawOriginOffsetY = -3;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.ai[1] = MathHelper.ToRadians(Main.rand.NextFloat(-90, 90));
            base.OnSpawn(source);
		}

		public override void AI()
		{
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90);
			Projectile.ai[0]++;

            Player player = Main.player[Projectile.owner];

			if (Projectile.ai[0] == 43)
                Projectile.tileCollide = true;

            if (Projectile.ai[0] < 10)
				Projectile.velocity = (Projectile.velocity + (Projectile.velocity.RotatedBy(Projectile.ai[1])).SafeNormalize(Vector2.UnitX) * 1f).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
			else if (Projectile.ai[0] < 40)
                Projectile.velocity = (Projectile.velocity + (player.Center - Projectile.Center).SafeNormalize(Vector2.UnitX) * 1.5f).SafeNormalize(Vector2.UnitX) * Projectile.velocity.Length();
			Projectile.velocity *= 1.015f;

            Vector2 pos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(4, 6);
            Dust dust = Dust.NewDustPerfect(pos, DustID.Smoke, Vector2.Zero, Scale: Main.rand.NextFloat(1.25f, 1.75f));
			dust.noGravity = true;
			pos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(4, 6);
            dust = Dust.NewDustPerfect(pos, DustID.InfernoFork, Vector2.Zero, 0, new Color(255, 100, 50), Main.rand.NextFloat(1.25f, 1.75f));
            dust.noGravity = true;

            Lighting.AddLight(Projectile.position, 1.5f, 0.75f, 0.5f);
		}

		public override void OnKill(int timeLeft)
		{
			if (Main.myPlayer == Projectile.owner)
			{
				Vector2 perturbedSpeed = new Vector2(0, 0);
				Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, perturbedSpeed, ModContent.ProjectileType<ExplosionHostile>(), 20, 4, Main.myPlayer);
			}
		}
	}
}