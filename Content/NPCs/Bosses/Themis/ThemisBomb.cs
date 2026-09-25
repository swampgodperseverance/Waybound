using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Waybound.Content.Projectiles.Hostile;

namespace Waybound.Content.NPCs.Bosses.Themis
{
	public class ThemisBomb : ModProjectile
	{
		public int CanCollide;

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Bomb Of Themis");
		}

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 75;
		}

		public override void AI()
		{
			CanCollide++;
			if (CanCollide >= 15)
			{
				Projectile.tileCollide = true;
			}

			Projectile.ai[0] += 1f;
			if (Projectile.ai[0] >= 15f)
			{
				Projectile.ai[0] = 15f;
				Projectile.velocity.Y = Projectile.velocity.Y + 0.3f;
			}
			if (Projectile.velocity.Y > 18f)
			{
				Projectile.velocity.Y = 18f;
			}

            Lighting.AddLight(Projectile.position, 1.5f, 0.75f, 0.5f);

            if (Projectile.velocity.X >= 0)
			{
				Projectile.rotation += Projectile.direction * 0.15f;
			}
			else
			{
				Projectile.rotation -= Projectile.direction * 0.15f;
			}
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