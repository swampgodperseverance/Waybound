//using Microsoft.Xna.Framework;
//using Terraria;
//using Terraria.DataStructures;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Synergia.Content.Projectiles.Friendly;
//using Synergia.Content.Buffs;
//using Synergia.Common.Rarities;

//namespace Synergia.Content.Items.Weapons.Cogworm
//{
//    public class Pyroclast : ModItem
//    {
//        private bool hasSpawnedFireballs = false; 

//        public override void SetStaticDefaults()
//        {
//        }

//        public override void SetDefaults()
//        {
//            Item.damage = 22;
//            Item.width = 50;
//            Item.height = 50;
//            Item.useStyle = ItemUseStyleID.Shoot;
//            Item.knockBack = 4;
//            Item.value = Item.sellPrice(0, 1, 1, 29);
//            Item.rare = ModContent.RarityType<LavaGradientRarity>();

//            Item.shootSpeed = 17;
//            Item.autoReuse = true;
//            Item.DamageType = DamageClass.Ranged;
//            Item.shoot = ProjectileID.PurificationPowder;
//            Item.shootSpeed = 16f;
//            Item.useAmmo = AmmoID.Arrow;
//            Item.UseSound = SoundID.Item5;
//            Item.useAnimation = 20;
//            Item.useTime = 4; 
//            Item.reuseDelay = 70;
//            Item.consumeAmmoOnLastShotOnly = true;
//            Item.noMelee = true;
//        }

//        public override Vector2? HoldoutOffset()
//        {
//            return new Vector2(-2f, 0f);
//        }

//        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
//        {
//            if (type == ProjectileID.WoodenArrowFriendly)
//            {
//                type = ModContent.ProjectileType<PyroclastShoot>();
//            }
//        }

//        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
//        {
//            int numProjectiles = Main.rand.Next(1, 6);
//            for (int p = 0; p < numProjectiles; p++)
//            {
//                Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(10));
//                newVelocity *= 1f - Main.rand.NextFloat(0.3f);
//                Projectile.NewProjectileDirect(source, position, newVelocity, type, damage, knockback, player.whoAmI);
//            }

//            if (player.HasBuff(ModContent.BuffType<Hellborn>()) && !hasSpawnedFireballs)
//            {
//                for (int i = 0; i < 3; i++)
//                {
//                    Vector2 fireballVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(15)) * Main.rand.NextFloat(0.8f, 1.2f);

//                    Vector2 fireballPosition = position + new Vector2(
//                        Main.rand.Next(-20, 21),
//                        Main.rand.Next(-20, 21)
//                    );

//                    Projectile.NewProjectileDirect(
//                        source,
//                        fireballPosition,
//                        fireballVelocity,
//                        ModContent.ProjectileType<FireballProjectile>(),
//                        damage * 2, 
//                        knockback,
//                        player.whoAmI
//                    );
//                }

//                hasSpawnedFireballs = true; 
//            }

//            return false;
//        }

//        public override bool CanUseItem(Player player)
//        {
//            hasSpawnedFireballs = false;
//            return base.CanUseItem(player);
//        }
//    }
//}