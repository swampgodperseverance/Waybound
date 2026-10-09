using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Waybound.Particles;
using System;

namespace Waybound.Content.NPCs.Bosses.DeepStoneGolem
{
    public class DeepStalactite : ModProjectile
    {
        private const int FormDuration = 60;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 32;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.alpha = 255;
            Projectile.scale = 0.1f;
        }

        private void SpawnCrystal(Vector2 pos, Vector2 vel, float sizeMin, float sizeMax, float colorMix)
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.CrystalBuffer == null)
                return;

            float size = Main.rand.NextFloat(sizeMin, sizeMax);

            Color baseColor = Color.Lerp(new Color(255, 168, 135), new Color(255, 200, 230), colorMix);

            ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.75f, 1.1f)),
                baseColor * Main.rand.NextFloat(0.85f, 1.2f),
                Main.rand.Next(16, 28)
            ));
        }

        public override void AI()
        {
            if (Projectile.ai[1] == 0)
            {
                Projectile.ai[0]++;
                float progress = Projectile.ai[0] / FormDuration;

                Projectile.alpha = (int)MathHelper.Lerp(255, 0, MathHelper.SmoothStep(0, 1, progress));
                Projectile.scale = MathHelper.Lerp(0.15f, 1f, MathHelper.SmoothStep(0, 1, progress));
                Projectile.rotation = 0f;

                if (Main.rand.NextBool(2))
                {
                    Vector2 offset = Main.rand.NextVector2Circular(18f, 28f) * (1.3f - progress);
                    Vector2 vel = -offset * 0.08f + Main.rand.NextVector2Circular(0.4f, 0.4f);
                    SpawnCrystal(
                        Projectile.Center + offset,
                        vel,
                        16f,
                        28f,
                        progress
                    );
                }

                if (Projectile.ai[0] >= FormDuration)
                {
                    Projectile.ai[1] = 1;
                    Projectile.ai[0] = 0;
                    Projectile.tileCollide = true;
                    Projectile.velocity = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), 1.8f);

                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 18; i++)
                        {
                            Vector2 speed = Main.rand.NextVector2CircularEdge(2.5f, 2.5f) * 3.5f;
                            SpawnCrystal(
                                Projectile.Center,
                                speed,
                                20f,
                                34f,
                                0.5f
                            );
                        }
                    }
                }
                return;
            }

            Projectile.velocity.Y += 0.32f;
            if (Projectile.velocity.Y > 18f)
                Projectile.velocity.Y = 18f;

            Projectile.rotation = 0f;

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(4f, 6f);
                    Vector2 vel = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(0.6f, 0.6f);

                    SpawnCrystal(pos, vel, 14f, 24f, 0.4f);
                }

                if (Main.rand.NextBool(3))
                {
                    Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(3f, 5f);
                    Vector2 vel = -Projectile.velocity * 0.18f + Main.rand.NextVector2Circular(1.2f, 1.2f);

                    SpawnCrystal(pos, vel, 10f, 18f, 0.7f);
                }

                if (Main.rand.NextBool(2))
                {
                    SpawnCrystal(
                        Projectile.Center,
                        Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.7f, 0.7f),
                        14f,
                        22f,
                        0.5f
                    );
                }
            }

            Lighting.AddLight(Projectile.Center, 1.1f, 0.45f, 0.75f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Texture2D glowTexture = ModContent.Request<Texture2D>("Waybound/Content/NPCs/Bosses/DeepStoneGolem/DeepStalactiteGlow").Value;

            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            if (Projectile.ai[1] == 1)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;

                    float progress = 1f - i / (float)Projectile.oldPos.Length;
                    Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                    Color trailColor = Color.White * progress * 0.45f;

                    Main.EntitySpriteDraw(texture, trailPos, null, trailColor, 0f, origin, Projectile.scale * (0.75f + progress * 0.25f), SpriteEffects.None, 0);
                }
            }

            Color drawColor = Projectile.GetAlpha(lightColor);
            Main.EntitySpriteDraw(texture, drawPos, null, drawColor, 0f, origin, Projectile.scale, SpriteEffects.None, 0);

            if (Projectile.ai[1] == 1 || Projectile.ai[0] > FormDuration * 0.65f)
            {
                float glowStrength = Projectile.ai[1] == 1 ? 0.9f : MathHelper.SmoothStep(0, 0.75f, (Projectile.ai[0] - FormDuration * 0.65f) / (FormDuration * 0.35f));
                Color glowColor = Color.White * glowStrength;

                Main.EntitySpriteDraw(glowTexture, drawPos, null, glowColor, 0f, origin, Projectile.scale, SpriteEffects.None, 0);

                for (int i = 0; i < 2; i++)
                {
                    float scaleMul = 1.08f + i * 0.07f;
                    Main.EntitySpriteDraw(glowTexture, drawPos, null, glowColor * (0.35f - i * 0.12f), 0f, origin, Projectile.scale * scaleMul, SpriteEffects.None, 0);
                }
            }

            return false;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 14; i++)
                {
                    Vector2 speed = Main.rand.NextVector2Circular(4f, 4f);
                    SpawnCrystal(
                        Projectile.Center,
                        speed,
                        20f,
                        34f,
                        0.5f
                    );
                }

                for (int i = 0; i < 10; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                    SpawnCrystal(
                        Projectile.Center,
                        vel,
                        18f,
                        28f,
                        0.6f
                    );
                }
            }

            return true;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            //lmfao i wanted to make golem even harder but i dropped this idea 
        }
    }
}