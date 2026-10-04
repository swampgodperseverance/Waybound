using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class CopperRapier : ModItem
    {
        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(copper: 40);
            Item.DamageType = DamageClass.Melee;
            Item.damage = 9;
            Item.knockBack = 3.5f;
            Item.crit = 4;
            Item.useTime = 19;
            Item.useAnimation = 19;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<CopperRapierHoldout>();
            Item.shootSpeed = 1f;
            Item.UseSound = SoundID.Item1;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<CopperRapierHoldout>()] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class CopperRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/CopperRapier";
        public override int MaxCharge => 50;
        public override float MinLungeDistance => 70f;
        public override float MaxLungeDistance => 145f;
        public override float LungeOutSpeed => 0.27f;
        public override float LungeReturnSpeed => 0.23f;
        public override bool UseGreenPulse => false;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<CopperRapier>();

        protected override Color GetLungeParticleColor() => new Color(190, 110, 60, 255);
        protected override Color GetTipParticleColor() => new Color(175, 95, 50, 255);
        protected override Color GetTrailParticleColor() => new Color(180, 100, 55, 200);
        protected override Color GetGlowColor() => new Color(210, 130, 70, 180);
    }
}