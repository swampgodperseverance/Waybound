using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.Rarities;
using Waybound.Content.Projectiles.Ranged.Guns.PreHM;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class IcebornRapier : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.rare = ModContent.RarityType<IceShimer>();
            Item.value = Item.sellPrice(silver: 80);
            Item.DamageType = DamageClass.Melee;
            Item.damage = 8;
            Item.knockBack = 4f;
            Item.crit = 6;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = false;   
            Item.autoReuse = true;    
            Item.shoot = ModContent.ProjectileType<IcebornRapierHoldout>();
            Item.shootSpeed = 1f;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<IcebornRapierHoldout>()] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class IcebornRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/IcebornRapier";

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<IcebornRapier>();

        protected override Color GetLungeParticleColor() => new Color(160, 225, 255, 255);
        protected override Color GetTipParticleColor() => new Color(145, 210, 255, 255);
        protected override Color GetTrailParticleColor() => new Color(150, 215, 255, 200);
        protected override Color GetGlowColor() => new Color(120, 200, 255, 180);
        protected override Color GetGreenPulseColor() => new Color(80, 255, 140, 220);

        protected override void OnEmpoweredStrike(Player player)
        {
            int damage = (int)(Projectile.damage * 0.9f);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 vel = Projectile.velocity.RotatedBy(i * 0.14f) * 14f;
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Tip,
                    vel,
                    ModContent.ProjectileType<IcebornRifleProj>(),
                    damage,
                    Projectile.knockBack,
                    player.whoAmI);
            }
        }
    }
}