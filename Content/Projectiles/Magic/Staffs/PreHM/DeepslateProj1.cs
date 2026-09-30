using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Particles;

namespace Waybound.Content.Projectiles.Magic.Staffs.PreHM
{
    public class DeepslateProj1 : ModProjectile
    {
        private Vector2 oldPos = Vector2.Zero;
        private float spin = 0f;

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
            Projectile.scale = 1.1f;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            spin += 0.28f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 + spin * 0.4f;

            Lighting.AddLight(Projectile.Center, 0.9f, 0.35f, 0.55f);

            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    ModContent.DustType<DeepMagicDust>(),
                    Projectile.velocity * 0.08f,
                    100,
                    new Color(255, 140, 180),
                    Main.rand.NextFloat(0.7f, 1.15f)
                );
                d.noGravity = true;
                d.velocity *= 0.4f;
                d.alpha = 80;
            }

            if (oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
        }

        //public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        //{
        //    if (Main.rand.NextBool(2) && Projectile.owner == Main.myPlayer)
        //    {
        //        Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
        //        Projectile.NewProjectile(
        //            Projectile.GetSource_FromThis(),
        //            Projectile.Center,
        //            vel,
        //            ModContent.ProjectileType<DeepslateProj2>(),
        //            (int)(Projectile.damage * 0.45f),
        //            1.2f,
        //            Projectile.owner
        //        );
        //    }
        //}

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 7; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(2.8f, 2.8f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.CrystalPulse, vel, 70,
                        new Color(255, 130, 175), 1.1f);
                    d.noGravity = true;
                }
            }
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                    Dust d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<DeepGlowDust>(), vel, 50,
                        new Color(255, 150, 190), Main.rand.NextFloat(0.3f, 0.9f));
                    d.noGravity = true;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            if (oldPos.HasNaNs() || oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
            else if (!Main.gamePaused)
                oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.4f);

            if (oldPos != Projectile.Center)
            {
                Texture2D trailTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;
                float trailLength = Vector2.Distance(Projectile.Center, oldPos);
                float trailScaleY = trailLength / trailTex.Height * 4.5f;

                Color trailColor = new Color(255, 100, 160, 0) * 0.7f;
                Main.EntitySpriteDraw(trailTex, drawPos, new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor, (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.5f, trailScaleY), SpriteEffects.None, 0f);
            }

            Color glow = new Color(255, 130, 175) * 0.55f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2.2f, 0f).RotatedBy(MathHelper.TwoPi / 4f * i + spin);
                Main.EntitySpriteDraw(tex, drawPos + offset, null, glow, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }

            Main.EntitySpriteDraw(tex, drawPos, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }

    public class DeepslateProj2 : ModProjectile
    {
        private readonly VertexStrip vertexStrip = new VertexStrip();
        private Vector2 targetPos;
        private bool hasTarget;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 14;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.scale = 0.85f;
            Projectile.aiStyle = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            targetPos = Main.MouseWorld;
            hasTarget = false;
        }

        public override void AI()
        {
            Projectile.ai[0]++;

            if (Projectile.ai[0] < 20)
            {
                Projectile.velocity *= 0.98f;
            }
            else
            {
                if (!hasTarget)
                {
                    targetPos = Main.MouseWorld;
                    hasTarget = true;
                }

                Vector2 toTarget = targetPos - Projectile.Center;
                float dist = toTarget.Length();

                if (dist < 20f)
                {
                    Projectile.Kill();
                    return;
                }

                Vector2 desired = toTarget.SafeNormalize(Vector2.UnitX) * 14f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.12f);
            }

            if (Projectile.velocity.LengthSquared() > 0.01f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Lighting.AddLight(Projectile.Center, 0.6f, 0.25f, 0.4f);

            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
            {
                Vector2 vel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                float size = Main.rand.NextFloat(8f, 15f);

                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.6f, 1.1f)),
                    new Color(255, 150, 210, 200) * Main.rand.NextFloat(0.9f, 1.2f),
                    Main.rand.Next(12, 20)
                ));
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                    float size = Main.rand.NextFloat(14f, 26f);

                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        Projectile.Center.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.6f, 1.1f)),
                        new Color(255, 130, 200, 210) * Main.rand.NextFloat(0.9f, 1.2f),
                        Main.rand.Next(16, 28)
                    ));
                }

                for (int i = 0; i < 6; i++)
                {
                    float size = Main.rand.NextFloat(24f, 38f);
                    ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                        Projectile.Center.ToNumerics(),
                        System.Numerics.Vector2.Zero,
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(size),
                        new Color(255, 160, 220, 200),
                        Main.rand.Next(12, 18)
                    ));
                }
            }

            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.6f, Pitch = 0.15f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            try
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                GameShaders.Misc["MagicMissile"].Apply(null);

                vertexStrip.PrepareStripWithProceduralPadding(
                    Projectile.oldPos,
                    Projectile.oldRot,
                    progress => Color.Lerp(new Color(255, 130, 200, 200), new Color(255, 180, 235, 60), progress),
                    progress => 42f * Projectile.scale * (1f - progress * 0.85f),
                    -Main.screenPosition + Projectile.Size / 2f,
                    true
                );
                vertexStrip.DrawTrail();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            catch
            {
                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    if (Projectile.oldPos[k] == Vector2.Zero)
                        continue;
                    float fade = 1f - k / (float)Projectile.oldPos.Length;
                    Main.EntitySpriteDraw(texture, Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition, null,
                        new Color(255, 140, 210, 0) * fade * 0.5f, Projectile.oldRot[k], origin, Projectile.scale * (1f - k * 0.03f), SpriteEffects.None, 0);
                }
            }

            Color outline = new Color(255, 140, 210) * 0.55f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.6f, 0f).RotatedBy(MathHelper.TwoPi * i / 4f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, outline, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, new Color(255, 200, 235, 220), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}