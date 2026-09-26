using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Helpers;

namespace Waybound.Content.Projectiles
{
    public class DesertExplosion : ModProjectile
    {
        public override string Texture => "Waybound/Assets/Textures/Pixel";

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
            Projectile.timeLeft = 5;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 1.5f, 1f, 0.5f);
            if (Projectile.ai[0] > 0)
                return;

            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            Player player = Main.player[Projectile.owner];
            player.PlayerScreen().fastScreenShake = 7 * (1000 - Vector2.Distance(player.Center, Projectile.Center)) / 1000;

            for (int i = 0; i < 75; i++)
            {
                float angle = MathHelper.TwoPi * i / 75f + Main.rand.NextFloat(-0.15f, 0.15f);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3.5f, 11f);
                int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.Torch, 0, 0, 80, default, Main.rand.NextFloat(1.1f, 2.1f));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = vel;
                Main.dust[dust].position = Projectile.Center;
            }

            for (int i = 0; i < 15; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 7f);
                int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.Smoke, 0, 0, 120, default, Main.rand.NextFloat(1.1f, 2f));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = vel;
                Main.dust[dust].position = Projectile.Center;
            }

            for (int i = 0; i < Main.rand.Next(6, 9); i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1.5f, 4f);
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, vel, Main.rand.Next(61, 64));
            }

            Projectile.ai[0] = 1;
        }
    }
}