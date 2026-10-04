using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class LeadRapier : ModItem
    {
        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 42;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 4);
            Item.DamageType = DamageClass.Melee;
            Item.damage = 12;
            Item.knockBack = 4.2f;
            Item.crit = 5;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<LeadRapierHoldout>();
            Item.shootSpeed = 1f;
            Item.UseSound = SoundID.Item1;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<LeadRapierHoldout>()] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class LeadRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/LeadRapier";
        public override int MaxCharge => 57;
        public override float MinLungeDistance => 86f;
        public override float MaxLungeDistance => 178f;
        public override float LungeOutSpeed => 0.28f;
        public override float LungeReturnSpeed => 0.24f;
        public override bool UseGreenPulse => false;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<LeadRapier>();

        protected override Color GetLungeParticleColor() => new Color(120, 125, 140, 255);
        protected override Color GetTipParticleColor() => new Color(105, 110, 125, 255);
        protected override Color GetTrailParticleColor() => new Color(110, 115, 130, 200);
        protected override Color GetGlowColor() => new Color(140, 145, 160, 180);
    }
}