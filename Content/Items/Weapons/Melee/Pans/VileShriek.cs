using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Pans
{
    public class VileShriek : SpinnablePanItem
    {
        public override int SpinProjectileType => ModContent.ProjectileType<VileShriekProjectile>();
        public override int ThrowProjectileType => ModContent.ProjectileType<VileShriekThrownProjectile>();

        public override int BaseDamage => 30;
        public override int SpinUseTime => 16;
        public override int ThrowUseTime => 72;
        public override int ThrowDebuffType => BuffID.ShadowFlame;
        public override int ThrowDebuffDuration => 360;
        public override float ThrowSpeed => 11f;
        public override float Knockback => 7.5f;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 3);
        }
    }

    public class VileShriekProjectile : SpinnablePanProjectile
    {
        public override int ProjWidth => 32;
        public override int ProjHeight => 26;

        public override bool CanShoot => false;

        public override float OrbitRadius => 0;
        public override float SpriteLength => 26f;
        public override float MaxSpinTime => 280f;
        public override float ParticleStart => 45f;
        public override float OutlineStart => 65f;
        public override float MinRotationSpeed => 0.02f;
        public override float MaxRotationSpeed => 0.32f;

        public override float ScaleStart => 1f;
        public override float ScalePeak => 1.5f;
        public override float ScaleAtFade => 0.9f;

        public override int BaseBuffType => BuffID.WellFed;
        public override int BaseBuffDuration => 480;
        public override int BuffInterval => 33;
        public override int BuffStartTime => 75;

        public override Color ParticleColor => new Color(140, 80, 220, 220);
        public override float ParticleSizeMin => 14f;
        public override float ParticleSizeMax => 24f;
        public override float ParticleSizeBonus => 8f;
        public override int ParticleLifeMin => 16;
        public override int ParticleLifeMax => 28;

        public override void AI()
        {
            base.AI();

            if (!Player.channel) return;
            if (Player.immune) return;

            if (Main.rand.NextBool(3))
            {
                Player.immune = true;
                Player.immuneTime = 12;
            }
        }

        protected override void UpdateLight(float eased)
        {
            if (Timer <= OutlineStart) return;
            float outlineStrength = MathHelper.Clamp((Timer - OutlineStart) / 60f, 0f, 1f);
            Vector2 lightPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * (OrbitRadius + SpriteLength * 0.5f);
            Lighting.AddLight(lightPos, 0.55f * outlineStrength * Projectile.Opacity, 0.3f * outlineStrength * Projectile.Opacity, 0.95f * outlineStrength * Projectile.Opacity);
        }

        protected override void DrawGlow(Texture2D texture, Vector2 origin, SpriteEffects effects, Vector2 drawPos, float drawRotation)
        {
            if (Timer <= OutlineStart) return;
            float strength = MathHelper.Clamp((Timer - OutlineStart) / 50f, 0f, 1f) * Projectile.Opacity;
            Color glow = new Color(140, 80, 220, 0) * strength * 0.65f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2.5f * strength, 0f).RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 3.5f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, glow, drawRotation, origin, Projectile.scale * 1.06f, effects, 0);
            }
        }

        protected override void DrawTrail(Texture2D texture, Vector2 origin, SpriteEffects effects)
        {
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float fade = 1f - i / (float)Projectile.oldPos.Length;
                Color trail = new Color(140, 80, 220, 0) * 0.35f * fade * Projectile.Opacity;
                Vector2 oldOrbitDir = Projectile.oldRot[i].ToRotationVector2();
                Vector2 oldDrawCenter = Projectile.oldPos[i] + Projectile.Size / 2f + oldOrbitDir * OrbitRadius;

                float oldDrawRotation;
                if (Projectile.spriteDirection == 1)
                    oldDrawRotation = Projectile.oldRot[i] - MathHelper.PiOver2 + MathHelper.Pi;
                else
                    oldDrawRotation = Projectile.oldRot[i];

                Main.EntitySpriteDraw(texture, oldDrawCenter - Main.screenPosition, null, trail, oldDrawRotation, origin, Projectile.scale * (1f - i * 0.03f), effects, 0);
            }
        }
    }

    public class VileShriekThrownProjectile : ThrownPanProjectile
    {
        public override int ProjWidth => 32;
        public override int ProjHeight => 26;

        public override float MaxOutTime => 30f;
        public override float MaxRange => 260f;
        public override float ReturnAccel => 0.7f;
        public override float ReturnMaxSpeed => 14f;
        public override float SpinSpeed => 0.3f;
        public override int AirTime => 600;

        public override Color ParticleColor => new Color(140, 80, 220, 220);
        public override float ParticleSizeMin => 10f;
        public override float ParticleSizeMax => 18f;
        public override int ParticleLifeMin => 14;
        public override int ParticleLifeMax => 24;

        public override string[] HitWords => new[] { "OMG", "BOOM", "DAMN" };
        public override Color HitTextColor => new Color(140, 80, 220);

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 origin = new(Projectile.spriteDirection == 1 ? texture.Width : 0f, texture.Height);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color color = Color.White * Projectile.Opacity;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float fade = 1f - i / (float)Projectile.oldPos.Length;
                Color trail = new Color(140, 80, 220, 0) * 0.35f * fade * Projectile.Opacity;
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, trail, Projectile.oldRot[i], origin, Projectile.scale, effects, 0);
            }

            Color glow = new Color(140, 80, 220, 0) * 0.65f * Projectile.Opacity;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2.3f, 0f).RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 3.5f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, glow, Projectile.rotation, origin, Projectile.scale * 1.06f, effects, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, origin, Projectile.scale, effects, 0);
            return false;
        }
    }
    public class VileShriekBeam : ModProjectile
    {
        public override string Texture => "Waybound/Content/Projectiles/EmptyProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 40;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.aiStyle = 1;
            Projectile.alpha = 0;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void AI()
        {
            Projectile.velocity *= 1.02f;
            Projectile.aiStyle = 0;

            if (Projectile.timeLeft < 597)
            {
                int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.Shadowflame, 0f, 0f, 0, default, 1.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0f;
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.2f, 0.8f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.timeLeft >= 597)
                return false;

            Texture2D texture = ModContent.Request<Texture2D>("Waybound/Assets/Textures/Glow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 origin = texture.Size() / 2f;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float fade = 1f - i / (float)Projectile.oldPos.Length;
                float scale = MathHelper.Lerp(0.3f, 0.02f, fade);
                Color color = Color.Lerp(new Color(60, 10, 100), new Color(180, 60, 255), fade);
                color *= (1f - fade) * 0.9f;

                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0);
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item20, target.position);
            target.AddBuff(BuffID.ShadowFlame, 240);
            base.OnHitNPC(target, hit, damageDone);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 15; i++)
            {
                int dust = Dust.NewDust(Projectile.Center, 1, 1, DustID.Shadowflame, 0f, 0f, 0, default, 1.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 4f;
            }
        }
    }
}