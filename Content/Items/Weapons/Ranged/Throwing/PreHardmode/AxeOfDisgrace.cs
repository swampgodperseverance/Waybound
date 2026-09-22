using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.Rarities;
using Waybound.Content.Projectiles.Ranged.Thrown;

namespace Waybound.Content.Items.Weapons.Ranged.Throwing.PreHardmode
{
    public class AxeOfDisgrace : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 48;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 4.5f;
            Item.value = Item.sellPrice(0, 1, 50, 0);
            Item.rare = RarityType<IceShimer>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<AxeOfDisgraceProj>();
            Item.shootSpeed = 14f;
            Item.consumable = false;
            Item.maxStack = 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 perturbedSpeed = velocity.RotatedByRandom(MathHelper.ToRadians(4f));
            Projectile.NewProjectile(
                source,
                position,
                perturbedSpeed,
                type,
                damage,
                knockback,
                player.whoAmI
            );
            return false;
        }

        //public override void AddRecipes()
        //{
        //    CreateRecipe()
        //        .AddIngredient(ModContent.ItemType<SnowCore>())
        //        .Register();
        //}
    }
}