using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class PlatinumRapier : ModItem
    {
        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 27);
            Item.DamageType = DamageClass.Melee;
            Item.damage = 16;
            Item.knockBack = 5f;
            Item.crit = 6;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<PlatinumRapierHoldout>();
            Item.shootSpeed = 1f;
            Item.UseSound = SoundID.Item1;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<PlatinumRapierHoldout>()] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class PlatinumRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/PlatinumRapier";
        public override int MaxCharge => 68;
        public override float MinLungeDistance => 105f;
        public override float MaxLungeDistance => 230f;
        public override float LungeOutSpeed => 0.30f;
        public override float LungeReturnSpeed => 0.26f;
        public override bool UseGreenPulse => true;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<PlatinumRapier>();

        protected override Color GetLungeParticleColor() => new Color(210, 220, 235, 255);
        protected override Color GetTipParticleColor() => new Color(195, 205, 220, 255);
        protected override Color GetTrailParticleColor() => new Color(200, 210, 225, 200);
        protected override Color GetGlowColor() => new Color(230, 240, 250, 190);
        protected override Color GetGreenPulseColor() => new Color(180, 220, 255, 220);

        protected override void OnSpecialDuringGreenPulse(Player player)
        {
            Vector2 target = Main.MouseWorld;
            player.Teleport(target, 1);
            for (int i = 0; i < 25; i++)
            {
                Dust.NewDustPerfect(target, DustID.Platinum, Main.rand.NextVector2Circular(3.5f, 3.5f), 100, default, 1.5f).noGravity = true;
            }
        }
    }
}