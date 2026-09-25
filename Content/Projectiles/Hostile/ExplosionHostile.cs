using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Helpers;

namespace Waybound.Content.Projectiles.Hostile
{
	public class ExplosionHostile : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Explosion");
		}

		public override void SetDefaults()
		{
			Projectile.width = 90;
			Projectile.height = 90;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.ignoreWater = true;
			Projectile.light = 1.5f;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 5;
		}

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 1.5f, 1f, 0.5f);

            if (Projectile.ai[0] > 0)
                return;

            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            Player player = Main.player[Projectile.owner];
            player.PlayerScreen().fastScreenShake = 7 * (1000 - Vector2.Distance(player.Center, Projectile.Center)) / 1000;

            Vector2 pos = new Vector2(Projectile.Center.X - Projectile.width / 4, Projectile.Center.Y - Projectile.height / 4);
            for (int i = 0; i < 75; i++)
            {
                int dust = Dust.NewDust(pos, Projectile.width / 2, Projectile.height / 2, DustID.Torch, 0, 0, 80, default(Color), Main.rand.NextFloat(1f, 2f));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = Main.rand.NextVector2Circular(10f, 10f);
            }

            for (int i = 0; i < 15; i++)
            {
                int dust = Dust.NewDust(pos, Projectile.width / 2, Projectile.height / 2, DustID.Smoke, 0, 0, 120, default(Color), Main.rand.NextFloat(1f, 2f));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = Main.rand.NextVector2Circular(10f, 10f);
            }

            for (int i = 0; i < Main.rand.Next(6, 9); i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Gore.NewGore(Projectile.GetSource_Death(), new Vector2(Projectile.Center.X, Projectile.Center.Y), vel, Main.rand.Next(61, 64));
            }
            Projectile.ai[0] = 1;
        }
    }
}