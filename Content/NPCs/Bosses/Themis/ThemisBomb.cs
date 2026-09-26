using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Projectiles.Hostile;

namespace Waybound.Content.NPCs.Bosses.Themis
{
    public class ThemisBomb : ModProjectile
    {
        private const int SlowStart = 30;
        private const int PulseInterval = 25;
        private int pulseCount;
        private float outlinePulse;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 200;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            if (Projectile.ai[0] < SlowStart)
            {
                if (Projectile.ai[0] >= 15f)
                {
                    Projectile.velocity.Y += 0.3f;
                    Projectile.tileCollide = true;
                }
                if (Projectile.velocity.Y > 18f)
                    Projectile.velocity.Y = 18f;

                outlinePulse = MathHelper.Lerp(outlinePulse, 0f, 0.12f);
            }
            else
            {
                Projectile.tileCollide = false;
                Projectile.velocity *= 0.90f;
                if (Projectile.velocity.Length() < 0.35f)
                    Projectile.velocity *= 0.75f;

                float sinceSlow = Projectile.ai[0] - SlowStart;
                int expectedPulses = (int)(sinceSlow / PulseInterval) + 1;

                if (expectedPulses > pulseCount && pulseCount < 3)
                {
                    pulseCount++;
                    outlinePulse = 1f;
                    Projectile.localAI[0] = 14f;

                    SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.35f, Pitch = 0.25f + pulseCount * 0.15f }, Projectile.Center);

                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            Dust d = Dust.NewDustPerfect(
                                Projectile.Center,
                                DustID.Torch,
                                Main.rand.NextVector2Circular(2.8f, 2.8f),
                                80,
                                new Color(255, 50, 35),
                                1.5f
                            );
                            d.noGravity = true;
                        }
                    }
                }

                outlinePulse = MathHelper.Lerp(outlinePulse, 0f, 0.08f);

                if (pulseCount >= 3 && sinceSlow >= PulseInterval * 3)
                {
                    Projectile.Kill();
                    return;
                }
            }

            if (Projectile.localAI[0] > 0)
                Projectile.localAI[0]--;

            Projectile.rotation += Projectile.velocity.X * 0.05f + 0.08f;
            Lighting.AddLight(Projectile.Center, 1.5f, 0.55f, 0.35f);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            if (Projectile.localAI[0] > 0)
            {
                float t = Projectile.localAI[0] / 14f;
                return Color.Lerp(lightColor, new Color(255, 40, 30), t * 0.9f);
            }
            return null;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color drawColor = GetAlpha(lightColor) ?? lightColor;

            if (outlinePulse > 0.05f)
            {
                Color glow = new Color(255, 35, 25, 0) * outlinePulse * 0.8f;
                float scaleAdd = 1f + outlinePulse * 0.25f;

                for (int i = 0; i < 6; i++)
                {
                    Vector2 offset = new Vector2(4f * outlinePulse, 0f).RotatedBy(MathHelper.TwoPi * i / 6f);
                    Main.EntitySpriteDraw(texture, drawPos + offset, null, glow, Projectile.rotation, origin, Projectile.scale * scaleAdd, SpriteEffects.None, 0);
                }

                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new Vector2(7.5f * outlinePulse, 0f).RotatedBy(MathHelper.TwoPi * i / 4f);
                    Main.EntitySpriteDraw(texture, drawPos + offset, null, glow * 0.45f, Projectile.rotation, origin, Projectile.scale * (scaleAdd + 0.08f), SpriteEffects.None, 0);
                }
            }

            Main.EntitySpriteDraw(texture, drawPos, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.InheritSource(Projectile),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<ExplosionHostile>(),
                    20, 4, Main.myPlayer
                );
            }
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        }
    }
}