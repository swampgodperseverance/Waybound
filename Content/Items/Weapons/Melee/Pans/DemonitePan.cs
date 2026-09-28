using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Pans
{
    public class DemonitePan : SpinnablePanItem
    {
        public override int SpinProjectileType => ModContent.ProjectileType<DemonitePanProjectile>();
        public override int ThrowProjectileType => ModContent.ProjectileType<DemonitePanThrownProjectile>();

        public override int BaseDamage => 20;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(gold: 1);
        }
        public override int SpinUseTime => 18;
        public override int ThrowUseTime => 28;
        public override int ThrowDebuffType => BuffID.Weak;
        public override int ThrowDebuffDuration => 100;
        public override float ThrowSpeed => 15f;
        public override float Knockback => 7f;
    }

    public class DemonitePanProjectile : SpinnablePanProjectile
    {
        public override int ProjWidth => 32;
        public override int ProjHeight => 26;
        public override float OrbitRadius => 8f;
        public override float SpriteLength => 26f;
        public override float MaxSpinTime => 300f;
        public override float ParticleStart => 45f;
        public override float OutlineStart => 65f;
        public override float MinRotationSpeed => 0.02f;
        public override float MaxRotationSpeed => 0.32f;

        public override float ScaleStart => 1f;
        public override float ScalePeak => 1.3f;
        public override float ScaleAtFade => 0.9f;

        public override int BaseBuffType => BuffID.WellFed;
        public override int BaseBuffDuration => 480;
        public override int BuffInterval => 35;
        public override int BuffStartTime => 75;

        public override Color ParticleColor => new Color(140, 80, 220, 220);
        public override float ParticleSizeMin => 14f;
        public override float ParticleSizeMax => 24f;
        public override float ParticleSizeBonus => 8f;
        public override int ParticleLifeMin => 16;
        public override int ParticleLifeMax => 28;

        public override bool CanShoot => false;

        protected override void UpdateLight(float eased)
        {
            if (Timer <= OutlineStart) return;
            float outlineStrength = MathHelper.Clamp((Timer - OutlineStart) / 60f, 0f, 1f);
            Vector2 lightPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * (OrbitRadius + SpriteLength * 0.5f);
            Lighting.AddLight(lightPos, 0.55f * outlineStrength * Projectile.Opacity, 0.3f * outlineStrength * Projectile.Opacity, 0.9f * outlineStrength * Projectile.Opacity);
        }

        protected override void DrawGlow(Texture2D texture, Vector2 origin, SpriteEffects effects, Vector2 drawPos, float drawRotation)
        {
            if (Timer <= OutlineStart) return;
            float strength = MathHelper.Clamp((Timer - OutlineStart) / 55f, 0f, 1f) * Projectile.Opacity;
            Color glow = new Color(140, 80, 220, 0) * strength * 0.5f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2.2f * strength, 0f).RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 3f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, glow, drawRotation, origin, Projectile.scale * 1.05f, effects, 0);
            }
        }

        protected override void DrawTrail(Texture2D texture, Vector2 origin, SpriteEffects effects)
        {
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float fade = 1f - i / (float)Projectile.oldPos.Length;
                Color trail = new Color(140, 80, 220, 0) * 0.3f * fade * Projectile.Opacity;
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

    public class DemonitePanThrownProjectile : ThrownPanProjectile
    {
        public override int ProjWidth => 32;
        public override int ProjHeight => 26;

        public override float MaxOutTime => 45f;
        public override float MaxRange => 420f;
        public override float ReturnAccel => 0.9f;
        public override float ReturnMaxSpeed => 22f;
        public override float SpinSpeed => 0.35f;
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

            Color glow = new Color(140, 80, 220, 0) * 0.6f * Projectile.Opacity;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2.2f, 0f).RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 3f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, glow, Projectile.rotation, origin, Projectile.scale * 1.05f, effects, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, origin, Projectile.scale, effects, 0);
            return false;
        }
    }
}