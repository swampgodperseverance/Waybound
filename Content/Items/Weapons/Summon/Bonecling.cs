using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Minions;

namespace Waybound.Content.Items.Weapons.Summon
{
    public class Bonecling : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.damage = 19;
            Item.knockBack = 2f;
            Item.mana = 10;

            Item.width = 40;
            Item.height = 40;

            Item.useTime = 30;
            Item.useAnimation = 30;

            Item.useStyle = 1;
            Item.noUseGraphic = false;
            Item.staff[Item.type] = true;

            Item.UseSound = SoundID.Item44;

            Item.noMelee = true;
            Item.DamageType = DamageClass.Summon;

            Item.buffType = ModContent.BuffType<OssuaryBuff>();

            Item.shoot = ModContent.ProjectileType<Projectiles.Summon.OssuaryStaffProj>();
            Item.shootSpeed = 0f;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.buyPrice(gold: 1, silver: 50);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 3600, quiet: false);

            var spawnPos = Main.MouseWorld;

            if (Main.myPlayer == player.whoAmI)
            {
                Projectile.NewProjectile(source, spawnPos, Vector2.Zero, type, damage, knockback, player.whoAmI);
            }

            return false;
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            damage = player.GetTotalDamage(DamageClass.Summon);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Bone, 26); 
            recipe.AddTile(TileID.Anvils); 
            recipe.Register();
        }

    }
}