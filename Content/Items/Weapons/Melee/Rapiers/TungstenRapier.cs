using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Weapons.Melee.Rapiers;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class TungstenRapier : ModItem
    {
        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 10);
            Item.DamageType = DamageClass.Melee;
            Item.damage = 14;
            Item.knockBack = 4.7f;
            Item.crit = 6;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<TungstenRapierHoldout>();
            Item.shootSpeed = 1f;
            Item.UseSound = SoundID.Item1;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<TungstenRapierHoldout>()] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class TungstenRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/TungstenRapier";
        public override int MaxCharge => 62;
        public override float MinLungeDistance => 95f;
        public override float MaxLungeDistance => 195f;
        public override float LungeOutSpeed => 0.30f;
        public override float LungeReturnSpeed => 0.26f;
        public override bool UseGreenPulse => false;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<TungstenRapier>();
        protected override Color GetLungeParticleColor() => new Color(150, 160, 170, 255);
        protected override Color GetTipParticleColor() => new Color(135, 145, 155, 255);
        protected override Color GetTrailParticleColor() => new Color(140, 150, 160, 200);
        protected override Color GetGlowColor() => new Color(170, 180, 190, 180);
    }
}