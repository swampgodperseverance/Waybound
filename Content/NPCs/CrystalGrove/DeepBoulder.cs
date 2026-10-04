using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;

namespace Waybound.Content.NPCs.CrystalGrove
{
	public class DeepBoulder : ModProjectile
	{
		public int tileCounter = 0;
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deep Boulder");
		}

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = true;
			Projectile.penetrate = 4;
			Projectile.timeLeft = 420;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			tileCounter += 1;
			if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
			{
				Projectile.velocity.X = -oldVelocity.X;
			}
			if (tileCounter < 4)
			{
				Collision.HitTiles(Projectile.position, Projectile.velocity * 0.75f, Projectile.width, Projectile.height);
				if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
				{
					Projectile.velocity.Y = -oldVelocity.Y;
				}
			}
			if (tileCounter < 5)
			{
				SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
			}
			Projectile.velocity *= 0.85f;
			for (int i = 0; i < (int)(10 * Math.Abs(Projectile.velocity.X) * 0.25f); i++)
			{
				Vector2 vel = new Vector2(Main.rand.NextFloat(-4, 4) * Projectile.velocity.X * 0.1f, Main.rand.NextFloat(-3, -1));
				int dust = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y + Projectile.height), Projectile.width, 2, ModContent.DustType<DeepStoneDust>(), vel.X, vel.Y, 0, default(Color), Main.rand.NextFloat(1, 1.5f));
				Main.dust[dust].noGravity = false;
				Main.dust[dust].velocity *= 1f;
			}

			return false;
		}

		public override void AI()
		{
			if (tileCounter >= 5)
			{
				Projectile.alpha++;
			}
			if (Projectile.alpha >= 255)
			{
				Projectile.Kill();
			}
			Projectile.velocity *= 0.995f;
			Projectile.rotation += Projectile.velocity.X * 0.1f;
			Projectile.velocity.Y += 0.2f;
		}

		public override bool CanHitPlayer(Player target) => tileCounter < 5 ? true : false;
	}
}