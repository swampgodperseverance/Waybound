using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Pans
{
    public class Pan : SpinnablePanItem
    {
        public override int SpinProjectileType => ModContent.ProjectileType<PanProjectile>();
        public override int ThrowProjectileType => ModContent.ProjectileType<PanThrownProjectile>();

        public override int BaseDamage => 12;
        public override int SpinUseTime => 20;
        public override int ThrowUseTime => 30;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 10);
        }
        public override float ThrowSpeed => 14f;
        public override float Knockback => 6f;
        public override int ThrowDebuffType => BuffID.OnFire;
        public override int ThrowDebuffDuration => 90;
    }

    public class PanProjectile : SpinnablePanProjectile
    {
        public override int ProjWidth => 32;
        public override int ProjHeight => 26;
        public override float OrbitRadius => 8f;
        public override float SpriteLength => 26f;
        public override float MaxSpinTime => 260f;
        public override float ParticleStart => 55f;
        public override float OutlineStart => 75f;
        public override float MinRotationSpeed => 0.02f;
        public override float MaxRotationSpeed => 0.28f;
        public override float ScaleStart => 1f;
        public override float ScalePeak => 1.15f;
        public override float ScaleAtFade => 0.85f;
        public override int BaseBuffType => BuffID.WellFed;
        public override int BaseBuffDuration => 360;
        public override int BuffInterval => 40;
        public override int BuffStartTime => 90;
        public override Color ParticleColor => new Color(255, 120, 30, 210);
    }

    public class PanThrownProjectile : ThrownPanProjectile
    {
        public override int ProjWidth => 32;
        public override int ProjHeight => 26;
        public override float MaxRange => 130f;
        public override int AirTime => 140;
        public override Color ParticleColor => new Color(255, 120, 30, 210);
        public override string[] HitWords => new[] { "OMG", "BOOM", "DAMN" };
    }
}