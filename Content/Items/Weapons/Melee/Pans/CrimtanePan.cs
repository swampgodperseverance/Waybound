using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Pans
{
    public class CrimtanePan : SpinnablePanItem
    {
        public override int SpinProjectileType => ModContent.ProjectileType<CrimtanePanProjectile>();
        public override int ThrowProjectileType => ModContent.ProjectileType<CrimtanePanThrownProjectile>();

        public override int BaseDamage => 20;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(gold: 1);
        }
        public override int SpinUseTime => 18;
        public override int ThrowUseTime => 28;
        public override int ThrowDebuffType => BuffID.Bleeding;
        public override int ThrowDebuffDuration => 100;
        public override float ThrowSpeed => 15f;
        public override float Knockback => 7f;
    }

    public class CrimtanePanProjectile : SpinnablePanProjectile
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

        public override Color ParticleColor => new Color(200, 40, 60, 220);
        public override float ParticleSizeMin => 14f;
        public override float ParticleSizeMax => 24f;
        public override float ParticleSizeBonus => 8f;
        public override int ParticleLifeMin => 16;
        public override int ParticleLifeMax => 28;

        public override bool CanShoot => false;
    }

    public class CrimtanePanThrownProjectile : ThrownPanProjectile
    {
        public override int ProjWidth => 32;
        public override int ProjHeight => 26;

        public override float MaxOutTime => 45f;
        public override float MaxRange => 420f;
        public override float ReturnAccel => 0.9f;
        public override float ReturnMaxSpeed => 22f;
        public override float SpinSpeed => 0.35f;
        public override int AirTime => 600;

        public override Color ParticleColor => new Color(200, 40, 60, 220);
        public override float ParticleSizeMin => 10f;
        public override float ParticleSizeMax => 18f;
        public override int ParticleLifeMin => 14;
        public override int ParticleLifeMax => 24;

        public override string[] HitWords => new[] { "OMG", "BOOM", "DAMN" };
        public override Color HitTextColor => new Color(200, 40, 60);
    }
}