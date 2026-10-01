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
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Melee.Pans
{
    public class Gideyon : SpinnablePanItem
    {
        public override int SpinProjectileType => ModContent.ProjectileType<GideyonProjectile>();
        public override int ThrowProjectileType => ModContent.ProjectileType<GideyonThrownProjectile>();
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(gold: 1);
        }
        public override int BaseDamage => 21;
        public override int SpinUseTime => 19;
        public override int ThrowUseTime => 28;
        public override int ThrowDebuffType => BuffID.Dazed;
        public override int ThrowDebuffDuration => 200;
        public override float ThrowSpeed => 15f;
        public override float Knockback => 6.5f;
    }

    public class GideyonProjectile : SpinnablePanProjectile
    {
        public override int ProjWidth => 32;
        public override int ProjHeight => 26;
        public override float OrbitRadius => 8f;
        public override float SpriteLength => 26f;
        public override float MaxSpinTime => 280f;
        public override float ParticleStart => 50f;
        public override float OutlineStart => 70f;
        public override float MinRotationSpeed => 0.02f;
        public override float MaxRotationSpeed => 0.3f;
        public override float ScaleStart => 1f;
        public override float ScalePeak => 1.18f;
        public override float ScaleAtFade => 0.85f;

        public override int BaseBuffType => BuffID.WellFed;
        public override int BaseBuffDuration => 420;
        public override int BuffInterval => 35;
        public override int BuffStartTime => 80;

        public override Color ParticleColor => new Color(90, 200, 255, 220);
        public override float ParticleSizeMin => 12f;
        public override float ParticleSizeMax => 22f;
        public override float ParticleSizeBonus => 8f;
        public override int ParticleLifeMin => 16;
        public override int ParticleLifeMax => 28;


        public override bool CanShoot => true;
        public override int ShootProjectileType => ModContent.ProjectileType<GideyonProj>();
        public override int ShootInterval => 30;
        public override float ShootSpeed => 14f;
        public override int ShootDamageDivider => 2;
        public override float ShootMinProgress => 0.5f;
        public override float ShootMinSpinSpeed => 0.15f;
    }

    public class GideyonThrownProjectile : ThrownPanProjectile
    {
        public override int ProjWidth => 32;
        public override int ProjHeight => 26;

        public override float MaxOutTime => 45f;
        public override float MaxRange => 420f;
        public override float ReturnAccel => 0.9f;
        public override float ReturnMaxSpeed => 22f;
        public override float SpinSpeed => 0.35f;
        public override int AirTime => 600;

        public override Color ParticleColor => new Color(90, 200, 255, 220);
        public override float ParticleSizeMin => 10f;
        public override float ParticleSizeMax => 18f;
        public override int ParticleLifeMin => 14;
        public override int ParticleLifeMax => 24;

        public override string[] HitWords => new[] { "OMG", "BOOM", "DAMN" };
        public override Color HitTextColor => new Color(90, 200, 255);
    }

    public class GideyonProj : ModProjectile
    {
        private Vector2 oldPos = Vector2.Zero;
        private Vector2 startPos = Vector2.Zero;
        private readonly VertexStrip vertexStrip = new VertexStrip();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 18;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.extraUpdates = 1;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            oldPos = Projectile.Center;
            startPos = Projectile.Center;
        }

        public override void AI()
        {
            if (Projectile.velocity.Length() > 0.1f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
            else if (!Main.gamePaused)
                oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.2f);

            Projectile.ai[0]++;

            float returnTime = 40f;

            if (Projectile.ai[0] < returnTime)
            {
                Projectile.velocity *= 0.985f;
                Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero) * 0.05f;
            }
            else
            {
                Vector2 toStart = startPos - Projectile.Center;
                float dist = toStart.Length();

                if (dist < 28f)
                {
                    Projectile.Kill();
                    return;
                }

                toStart.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toStart * 18f, 0.08f);
            }

            if (Main.rand.NextBool(2))
            {
                Vector2 vel = Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.6f, 0.6f);
                float size = Main.rand.NextFloat(12f, 22f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.15f)),
                    new Color(90, 200, 255, 200) * Main.rand.NextFloat(0.9f, 1.2f),
                    Main.rand.Next(16, 28)
                ));
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0.55f, 0.75f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                float size = Main.rand.NextFloat(16f, 28f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.15f)),
                    new Color(90, 200, 255, 210) * Main.rand.NextFloat(0.9f, 1.2f),
                    Main.rand.Next(18, 32)
                ));
            }

            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.6f, Pitch = 0.1f }, Projectile.Center);
        }

        public override Color? GetAlpha(Color lightColor) => new Color(160, 230, 255, 180);

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
                    progress => Color.Lerp(new Color(90, 200, 255, 200), new Color(160, 235, 255, 60), progress),
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
                        new Color(90, 200, 255, 0) * fade * 0.5f, Projectile.oldRot[k], origin, Projectile.scale * (1f - k * 0.03f), SpriteEffects.None, 0);
                }
            }

            if (oldPos != Vector2.Zero && oldPos != Projectile.Center)
            {
                Texture2D trailTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;
                Color trailColor = new Color(80, 190, 255, 0) * 0.65f;
                float trailLength = Vector2.Distance(Projectile.Center, oldPos);
                float trailScaleY = trailLength / trailTex.Height * 4.2f;

                Main.EntitySpriteDraw(trailTex, Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor, (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.85f, trailScaleY), SpriteEffects.None, 0f);

                Main.EntitySpriteDraw(trailTex, Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor * 0.4f, (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.4f, trailScaleY * 1.35f), SpriteEffects.None, 0f);
            }

            Color outline = new Color(90, 200, 255) * 0.55f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.6f, 0f).RotatedBy(MathHelper.TwoPi * i / 4f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, outline, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, new Color(180, 235, 255, 200), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}