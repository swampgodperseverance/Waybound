using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class WoodRapier : ModItem
    {
        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(copper: 20);
            Item.DamageType = DamageClass.Melee;
            Item.damage = 4;
            Item.knockBack = 4f;
            Item.crit = 6;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<WoodRapierHoldout>();
            Item.shootSpeed = 1f;
            Item.UseSound = SoundID.Item1;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<WoodRapierHoldout>()] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class WoodRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/WoodRapier";
        public override int MaxCharge => 15;
        public override float MinLungeDistance => 80f;
        public override float MaxLungeDistance => 160f;
        public override float LungeOutSpeed => 0.28f;
        public override float LungeReturnSpeed => 0.24f;
        public override bool UseGreenPulse => false;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<WoodRapier>();

        protected override Color GetLungeParticleColor() => new Color(160, 105, 55, 255);
        protected override Color GetTipParticleColor() => new Color(145, 95, 45, 255);
        protected override Color GetTrailParticleColor() => new Color(150, 100, 50, 200);
        protected override Color GetGlowColor() => new Color(180, 130, 70, 180);
    }
}