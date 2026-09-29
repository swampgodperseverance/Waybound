using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    public class GoldRapier : ModItem
    {
        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Item.width = 45;
            Item.height = 45;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 18);
            Item.DamageType = DamageClass.Melee;
            Item.damage = 15;
            Item.knockBack = 4.8f;
            Item.crit = 6;
            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<GoldRapierHoldout>();
            Item.shootSpeed = 1f;
            Item.UseSound = SoundID.Item1;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<GoldRapierHoldout>()] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }

    public class GoldRapierHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/GoldRapier";
        public override int MaxCharge => 64;
        public override float MinLungeDistance => 98f;
        public override float MaxLungeDistance => 210f;
        public override float LungeOutSpeed => 0.295f;
        public override float LungeReturnSpeed => 0.255f;
        public override bool UseGreenPulse => true;

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<GoldRapier>();

        protected override Color GetLungeParticleColor() => new Color(230, 190, 60, 255);
        protected override Color GetTipParticleColor() => new Color(215, 175, 45, 255);
        protected override Color GetTrailParticleColor() => new Color(220, 180, 50, 200);
        protected override Color GetGlowColor() => new Color(250, 210, 80, 190);
        protected override Color GetGreenPulseColor() => new Color(255, 230, 80, 220);

        protected override void OnSpecialDuringGreenPulse(Player player)
        {
            Vector2 target = Main.MouseWorld;
            player.Teleport(target, 1);
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDustPerfect(target, DustID.Gold, Main.rand.NextVector2Circular(3f, 3f), 100, default, 1.4f).noGravity = true;
            }
        }
    }
}