using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Pans
{
    public class Ecosystem : SpinnablePanItem
    {
        public override int SpinProjectileType => ModContent.ProjectileType<EcosystemProjectile>();
        public override int ThrowProjectileType => ModContent.ProjectileType<EcosystemThrownProjectile>();

        public override int BaseDamage => 15;
        public override int SpinUseTime => 18;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(silver: 90);
        }
        public override int ThrowUseTime => 26;
        public override int ThrowDebuffType => BuffID.Poisoned;
        public override int ThrowDebuffDuration => 200;
        public override float ThrowSpeed => 16f;
        public override float Knockback => 7f;
    }

    public class EcosystemProjectile : SpinnablePanProjectile
    {
        public override int ProjWidth => 32;
        public override int ProjHeight => 26;
        public override float OrbitRadius => 8f;
        public override float SpriteLength => 26f;
        public override float MaxSpinTime => 300f;
        public override float ParticleStart => 50f;
        public override float OutlineStart => 70f;
        public override float MinRotationSpeed => 0.02f;
        public override float MaxRotationSpeed => 0.32f;
        public override float ScaleStart => 1f;
        public override float ScalePeak => 1.2f;
        public override float ScaleAtFade => 0.85f;

        public override int BaseBuffType => BuffID.WellFed;
        public override int BaseBuffDuration => 480;
        public override int BuffInterval => 35;
        public override int BuffStartTime => 80;

        public override Color ParticleColor => new Color(80, 220, 90, 220);
        public override float ParticleSizeMin => 12f;
        public override float ParticleSizeMax => 22f;
        public override float ParticleSizeBonus => 8f;
        public override int ParticleLifeMin => 18;
        public override int ParticleLifeMax => 30;

        public override bool CanShoot => true;
        public override int ShootProjectileType => ModContent.ProjectileType<EcosystemShoot>();
        public override int ShootInterval => 30;
        public override float ShootSpeed => 11f;
        public override int ShootDamageDivider => 2;
    }

    public class EcosystemThrownProjectile : ThrownPanProjectile
    {
        public override int ProjWidth => 32;
        public override int ProjHeight => 26;
        public override float MaxRange => 210f;
        public override int AirTime => 600;
        public override float ParticleSizeMin => 10f;
        public override float ParticleSizeMax => 18f;
        public override int ParticleLifeMin => 14;
        public override int ParticleLifeMax => 24;
        public override Color ParticleColor => new Color(80, 220, 90, 220);
        public override Color HitTextColor => new Color(80, 220, 90);
        public override string[] HitWords => new[] { "OMG", "BOOM", "DAMN" };
    }
}