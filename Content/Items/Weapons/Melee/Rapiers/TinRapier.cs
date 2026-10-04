using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class TinRapier : ModItem
    {
        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 4);
            Item.DamageType = DamageClass.Melee;
            Item.damage = 10;
            Item.knockBack = 3.8f;
            Item.crit = 4;
            Item.useTime = 19;
            Item.useAnimation = 19;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<TinRapierHoldout>();
            Item.shootSpeed = 1f;
            Item.UseSound = SoundID.Item1;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<TinRapierHoldout>()] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class TinRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/TinRapier";
        public override int MaxCharge => 48;
        public override float MinLungeDistance => 75f;
        public override float MaxLungeDistance => 165f;
        public override float LungeOutSpeed => 0.26f;
        public override float LungeReturnSpeed => 0.22f;
        public override bool UseGreenPulse => false;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<TinRapier>();
        protected override Color GetLungeParticleColor() => new Color(175, 170, 155, 255);
        protected override Color GetTipParticleColor() => new Color(160, 155, 140, 255);
        protected override Color GetTrailParticleColor() => new Color(165, 160, 145, 200);
        protected override Color GetGlowColor() => new Color(190, 185, 170, 160);
    }

  
}