//using Terraria;
//using Terraria.ID;
//using Terraria.ModLoader;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using ReLogic.Content;
//using Synergia.Content.Projectiles.Friendly;
//using Synergia.Common.Rarities;
//using System.Collections.Generic;

//namespace Waybound.Content.Items.Weapons.Ranged
//{
//    public class Impact : ModItem
//    {
//        private int shotCounter = 0;
//        private static Dictionary<int, Asset<Texture2D>> projectileTextures;
//        private int currentProjectileType = -1;

//        public override void SetStaticDefaults()
//        {
//            if (Main.dedServ)
//                return;

//            projectileTextures = new Dictionary<int, Asset<Texture2D>>
//            {
//                [0] = ModContent.Request<Texture2D>("Synergia/Content/Projectiles/Friendly/CogwormProj1"),
//                [1] = ModContent.Request<Texture2D>("Synergia/Content/Projectiles/Friendly/CogwormProj2"),
//                [2] = ModContent.Request<Texture2D>("Synergia/Content/Projectiles/Friendly/CogwormProj3"),
//                [3] = ModContent.Request<Texture2D>("Synergia/Content/Projectiles/Friendly/CogwormProj4"),
//                [4] = ModContent.Request<Texture2D>("Synergia/Content/Projectiles/Friendly/CogwormProj5")
//            };
//        }

//        public override void SetDefaults()
//        {
//            Item.width = 40;
//            Item.height = 20;
//            Item.noMelee = true;
//            Item.noUseGraphic = true;
//            Item.useStyle = ItemUseStyleID.Swing;
//            Item.UseSound = SoundID.Item1;
//            Item.DamageType = DamageClass.Throwing;
//            Item.damage = 150;
//            Item.knockBack = 4f;
//            Item.autoReuse = true;
//            Item.shootSpeed = 12f;
//            Item.useAnimation = 25;
//            Item.useTime = 25;
//            Item.rare = ModContent.RarityType<LavaGradientRarity>();

//            Item.value = Item.sellPrice(0, 5, 0, 0);
//            Item.shoot = ModContent.ProjectileType<CogwormProj1>();
//        }

//        public override bool CanUseItem(Player player)
//        {
//            if (player.HasBuff(ModContent.BuffType<Buffs.Hellborn>()))
//                shotCounter = 4;
//            return base.CanUseItem(player);
//        }

//        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
//        {
//            int projectileType;
//            int projectileCount;

//            int currentShot = shotCounter % 5;
//            currentProjectileType = currentShot;

//            switch (currentShot)
//            {
//                case 0:
//                    projectileType = ModContent.ProjectileType<CogwormProj1>();
//                    projectileCount = 1;
//                    break;
//                case 1:
//                    projectileType = ModContent.ProjectileType<CogwormProj2>();
//                    projectileCount = 2;
//                    break;
//                case 2:
//                    projectileType = ModContent.ProjectileType<CogwormProj3>();
//                    projectileCount = 3;
//                    break;
//                case 3:
//                    projectileType = ModContent.ProjectileType<CogwormProj4>();
//                    projectileCount = 4;
//                    break;
//                default: // case 4
//                    projectileType = ModContent.ProjectileType<CogwormProj5>();
//                    projectileCount = 5;
//                    break;
//            }

//            for (int i = 0; i < projectileCount; i++)
//            {
//                Vector2 spreadVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(5f)) * Main.rand.NextFloat(0.9f, 1.1f);

//                Projectile.NewProjectile(
//                    source,
//                    position,
//                    spreadVelocity,
//                    projectileType,
//                    damage,
//                    knockback,
//                    player.whoAmI
//                );
//            }

//            shotCounter++;

//            if (shotCounter >= 5)
//                shotCounter = 0;

//            return false;
//        }

//        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
//        {
//            if (projectileTextures == null || currentProjectileType == -1)
//                return true;

//            if (projectileTextures.TryGetValue(currentProjectileType, out Asset<Texture2D> textureAsset))
//            {
//                Texture2D texture = textureAsset.Value;

//                if (currentProjectileType == 4)
//                {
//                    Color outlineColor = new Color(255, 100, 0) * 0.5f; 
//                    float outlineScale = scale * 1.05f;

//                    for (int i = -1; i <= 1; i++)
//                    {
//                        for (int j = -1; j <= 1; j++)
//                        {
//                            if (i == 0 && j == 0) continue;

//                            Vector2 outlinePosition = position + new Vector2(i * 2, j * 2);
//                            spriteBatch.Draw(
//                                texture,
//                                outlinePosition,
//                                null,
//                                outlineColor,
//                                0f,
//                                texture.Size() / 2f,
//                                outlineScale,
//                                SpriteEffects.None,
//                                0f
//                            );
//                        }
//                    }

//                    Color glowColor = new Color(255, 200, 0) * 0.3f;
//                    spriteBatch.Draw(
//                        texture,
//                        position,
//                        null,
//                        glowColor,
//                        0f,
//                        texture.Size() / 2f,
//                        scale * 1.02f,
//                        SpriteEffects.None,
//                        0f
//                    );

//                    spriteBatch.Draw(
//                        texture,
//                        position,
//                        null,
//                        Color.White,
//                        0f,
//                        texture.Size() / 2f,
//                        scale,
//                        SpriteEffects.None,
//                        0f
//                    );

//                    return false;
//                }

//                spriteBatch.Draw(
//                    texture,
//                    position,
//                    null,
//                    Color.White,
//                    0f,
//                    texture.Size() / 2f,
//                    scale,
//                    SpriteEffects.None,
//                    0f
//                );

//                return false;
//            }

//            return true;
//        }

//        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
//        {
//            if (projectileTextures == null || currentProjectileType == -1)
//                return true;

//            if (projectileTextures.TryGetValue(currentProjectileType, out Asset<Texture2D> textureAsset))
//            {
//                Texture2D texture = textureAsset.Value;
//                Vector2 position = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height / 2f);

//                if (currentProjectileType == 4)
//                {
//                    // Аутлайн для мира
//                    Color outlineColor = new Color(255, 100, 0) * 0.5f;
//                    float outlineScale = scale * 1.05f;

//                    for (int i = -1; i <= 1; i++)
//                    {
//                        for (int j = -1; j <= 1; j++)
//                        {
//                            if (i == 0 && j == 0) continue;

//                            Vector2 outlinePosition = position + new Vector2(i * 2, j * 2);
//                            spriteBatch.Draw(
//                                texture,
//                                outlinePosition,
//                                null,
//                                outlineColor,
//                                rotation,
//                                texture.Size() / 2f,
//                                outlineScale,
//                                SpriteEffects.None,
//                                0f
//                            );
//                        }
//                    }

//                    Color glowColor = new Color(255, 200, 0) * 0.3f;
//                    spriteBatch.Draw(
//                        texture,
//                        position,
//                        null,
//                        glowColor,
//                        rotation,
//                        texture.Size() / 2f,
//                        scale * 1.02f,
//                        SpriteEffects.None,
//                        0f
//                    );

//                    spriteBatch.Draw(
//                        texture,
//                        position,
//                        null,
//                        lightColor,
//                        rotation,
//                        texture.Size() / 2f,
//                        scale,
//                        SpriteEffects.None,
//                        0f
//                    );

//                    return false;
//                }

//                spriteBatch.Draw(
//                    texture,
//                    position,
//                    null,
//                    lightColor,
//                    rotation,
//                    texture.Size() / 2f,
//                    scale,
//                    SpriteEffects.None,
//                    0f
//                );

//                return false;
//            }

//            return true;
//        }

//        public override void UpdateInventory(Player player)
//        {
//            base.UpdateInventory(player);
//        }
//    }
//}