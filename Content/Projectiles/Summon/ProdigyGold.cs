using Waybound.Helpers;
using Waybound.Particles;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace Waybound.Content.Projectiles.Summon
{
    public class ProdigyGold : ModProjectile
    {
        private Vector2 initialVelocity;
        private float travelTime = 210f;
        private float elapsed = 0f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.tileCollide = true;
            Projectile.penetrate = 6;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 1;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            if (elapsed == 0f)
            {
                initialVelocity = Projectile.velocity;
            }

            elapsed++;

            float t = MathHelper.Clamp(elapsed / travelTime, 0f, 1f);

            Projectile.velocity = initialVelocity * (1f - EaseFunctions.EaseOutCubic(t));

            if (Projectile.velocity.LengthSquared() > 0.1f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            if (Projectile.velocity.Length() < 0.5f)
            {
                Explode();
            }
        }

        private void Explode()
        {
            for (int i = 0; i < 12; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame);
                d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                d.scale = Main.rand.NextFloat(1f, 1.5f);
                d.noGravity = true;
                d.color = new Color(255, 215, 0);
            }

            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch);
                d.velocity = Main.rand.NextVector2Circular(2f, 2f);
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
                d.noGravity = true;
            }

            var p = VanillaParticles.RequestPrettySparkleParticle();
		    p.ColorTint = new Color(255, 215, 0);
		    p.Scale = new Vector2(5f, 1.1f);
		    p.Rotation = default;
		    p.LocalPosition = Projectile.Center;
		    p.TimeToLive = 25;
		    p.FadeInEnd = 2;
		    p.FadeOutStart = 4;
		    Main.ParticleSystem_World_OverPlayers.Add(p);
            var p2 = VanillaParticles.RequestPrettySparkleParticle();
		    p2.ColorTint = Color.White;
		    p2.Scale = new Vector2(5f, 1.1f);
		    p2.Rotation = default;
		    p2.LocalPosition = Projectile.Center;
		    p2.TimeToLive = 15;
		    p2.FadeInEnd = 2;
		    p2.FadeOutStart = 4;
		    Main.ParticleSystem_World_OverPlayers.Add(p2);

            Projectile.Kill();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity = Vector2.Zero;     
            Explode();
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = texture.Size() / 2f;

            SpriteEffects effects = SpriteEffects.None;
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (Projectile.oldPos[k] == Vector2.Zero)
                    continue;

                Vector2 drawPos = Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition;

                float progress = (float)k / Projectile.oldPos.Length;
                Color color = Color.Lerp(Color.Gold, Color.Orange, progress) * (0.7f * (1f - progress));

                float rotation;
                if (k + 1 >= Projectile.oldPos.Length || Projectile.oldPos[k + 1] == Vector2.Zero)
                    rotation = (Projectile.position - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;
                else
                    rotation = (Projectile.oldPos[k + 1] - Projectile.oldPos[k]).ToRotation() + MathHelper.PiOver2;

                float scale = Projectile.scale * (0.75f + 0.35f * progress);

                spriteBatch.Draw(texture, drawPos, null, color, rotation, drawOrigin, scale, effects, 0f);
            }

            Vector2 center = Projectile.Center - Main.screenPosition;
                 Color glowColor = Color.Lerp(Color.Gold, Color.Yellow, 0.5f);
            glowColor.A = 0;

            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 offset = angle.ToRotationVector2() * 2.5f;

                spriteBatch.Draw(
                    texture,
                    center + offset,
                    null,
                    glowColor * 0.6f,
                    Projectile.rotation,
                    drawOrigin,
                    Projectile.scale * 1.2f,
                    effects,
                    0f
                );
            }   
             Color outlineColor = Color.Lerp(Color.Gold, Color.Orange, 0.5f);
            outlineColor.A = 140;

            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f;
                Vector2 offset = angle.ToRotationVector2() * 1.8f;

                spriteBatch.Draw(
                    texture,
                    center + offset,
                    null,
                    outlineColor,
                    Projectile.rotation,
                    drawOrigin,
                    Projectile.scale,
                    effects,
                    0f
                );
            }
            spriteBatch.Draw(
                texture,
                center,
                null,
                Color.White,
                Projectile.rotation,
                drawOrigin,
                Projectile.scale,
                effects,
                0f
            );

            return false;
        }
    }
}