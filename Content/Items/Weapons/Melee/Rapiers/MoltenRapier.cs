using Microsoft.Xna.Framework;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Weapons.Melee.Pans;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class MoltenRapier : ModItem
    {
        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 1);
            Item.DamageType = DamageClass.Melee;
            Item.damage = 32;
            Item.knockBack = 5.5f;
            Item.crit = 8;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<MoltenRapierHoldout>();
            Item.shootSpeed = 1f;
            Item.UseSound = SoundID.Item1;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<MoltenRapierHoldout>()] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class MoltenRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/MoltenRapier";
        public override int MaxCharge => 75;
        public override float MinLungeDistance => 115f;
        public override float MaxLungeDistance => 255f;
        public override float LungeOutSpeed => 0.31f;
        public override float LungeReturnSpeed => 0.27f;
        public override bool UseGreenPulse => true;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<MoltenRapier>();

        protected override Color GetLungeParticleColor() => new Color(255, 120, 40, 255);
        protected override Color GetTipParticleColor() => new Color(255, 90, 20, 255);
        protected override Color GetTrailParticleColor() => new Color(255, 100, 30, 200);
        protected override Color GetGlowColor() => new Color(255, 160, 60, 200);
        protected override Color GetGreenPulseColor() => new Color(255, 180, 50, 220);

        protected override void OnSpecialDuringGreenPulse(Player player)
        {
            Vector2 tip = Projectile.Center;
            Vector2 baseVelocity = Projectile.velocity * 13f;
            int damage = (int)(Projectile.damage * 1.35f);
            float knockback = Projectile.knockBack;
            int projType = ModContent.ProjectileType<IncandescencedPanP>();

            int count = Main.rand.Next(2, 4);
            for (int i = 0; i < count; i++)
            {
                float angleOffset = MathHelper.ToRadians(Main.rand.NextFloat(-12f, 12f));
                Vector2 velocity = baseVelocity.RotatedBy(angleOffset);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    tip,
                    velocity,
                    projType,
                    damage,
                    knockback,
                    player.whoAmI
                );
            }

            for (int i = 0; i < 18; i++)
            {
                Vector2 pos = tip;
                Vector2 vel = Main.rand.NextVector2Circular(4.5f, 4.5f) + Projectile.velocity * Main.rand.NextFloat(2f, 6f);
                ParticleSystem.FlameBuffer.Create(new ParticleInfo(
                    pos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(Main.rand.NextFloat(18f, 30f)),
                    new Color(255, 140, 40, 220),
                    Main.rand.Next(24, 40)
                ));
            }
        }
    }
}