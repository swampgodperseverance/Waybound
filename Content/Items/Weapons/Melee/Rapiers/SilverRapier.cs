using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class SilverRapier : ModItem
    {
        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 9);
            Item.DamageType = DamageClass.Melee;
            Item.damage = 13;
            Item.knockBack = 4.5f;
            Item.crit = 6;
            Item.useTime = 17;
            Item.useAnimation = 17;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<SilverRapierHoldout>();
            Item.shootSpeed = 1f;
            Item.UseSound = SoundID.Item1;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<SilverRapierHoldout>()] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class SilverRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/SilverRapier";
        public override int MaxCharge => 60;
        public override float MinLungeDistance => 90f;
        public override float MaxLungeDistance => 190f;
        public override float LungeOutSpeed => 0.29f;
        public override float LungeReturnSpeed => 0.25f;
        public override bool UseGreenPulse => false;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<SilverRapier>();

        protected override Color GetLungeParticleColor() => new Color(180, 190, 205, 255);
        protected override Color GetTipParticleColor() => new Color(165, 175, 190, 255);
        protected override Color GetTrailParticleColor() => new Color(170, 180, 195, 200);
        protected override Color GetGlowColor() => new Color(200, 210, 220, 180);
    }
}