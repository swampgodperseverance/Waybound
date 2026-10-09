using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Misc;
using Waybound.Helpers;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Melee.Swords
{

    //TODO
    public class BattleaxeOfDesertHunter : ModItem
    {
        public override void SetStaticDefaults()
        {
        }
        public int attackPattern;
        public override void SetDefaults()
        {
            Item.damage = 90;
            Item.DamageType = DamageClass.Melee;
            Item.width = 64;
            Item.height = 64;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<DesertHunterAxeSwing>();
            Item.shootSpeed = 1f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int dir = attackPattern == 0 ? 1 : -1;
            Projectile.NewProjectile(
                source,
                player.MountedCenter,
                velocity,
                type,
                damage,
                knockback,
                player.whoAmI,
                ai0: 0f,
                ai1: dir
            );
            attackPattern = attackPattern == 0 ? 1 : 0;
            return false;
        }
    
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient<DesertCore>(1)
                .AddIngredient<DesertWreckage>(12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class DesertHunterAxeSwing : CustomSwing
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Melee/Swords/BattleaxeOfDesertHunter";

        public override float SwordLength => 60f;
        public override float BaseScale => 1f;

        public override int TrailDrawLength => Projectile.oldPos.Length / 3;
        public override Color TrailColor => new Color(255, 190, 100);
        public override Color TrailCoreColor => new Color(255, 230, 160);

        public override Color ParticleColor => new Color(255, 140, 40);
        public override Color ParticleCoreColor => new Color(255, 200, 120);
        public override float ParticleWidth => 1.3f;
        public override float ParticleSizeMultiplier => 1.1f;
        public override float ParticleSpeedMultiplier => 1.2f;
        public override int ParticleLife => 28;
        public override int ParticleCount => 3;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.DamageType = DamageClass.Melee;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            base.AI();
            Lighting.AddLight(Projectile.Center, 0.9f, 0.5f, 0.2f);
        }

        public override void SpawnTrailParticles()
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.FlameBuffer == null)
                return;

            float swingProgress = 1f - (float)Player.itemAnimation / Player.itemAnimationMax;
            if (swingProgress < 0.15f || swingProgress > 0.95f)
                return;

            float bladeRot = Projectile.rotation - MathHelper.PiOver4;
            Vector2 tipDirection = bladeRot.ToRotationVector2();
            Vector2 perp = new Vector2(-tipDirection.Y, tipDirection.X);

            int count = (int)MathHelper.Lerp(1f, ParticleCount, swingProgress);

            for (int i = 0; i < count; i++)
            {
                float along = Main.rand.NextFloat(0.3f, 1f);
                Vector2 spawnPos = Projectile.Center + tipDirection * SwordLength * Projectile.scale * along;
                spawnPos += perp * Main.rand.NextFloat(-8f, 8f) * ParticleWidth;

                Vector2 vel = (-tipDirection * Main.rand.NextFloat(0.5f, 1.8f) + Main.rand.NextVector2Circular(0.8f, 0.8f)) * ParticleSpeedMultiplier;

                float size = Main.rand.NextFloat(18f, 34f) * (0.6f + along * 0.6f) * ParticleWidth * ParticleSizeMultiplier;

                ParticleSystem.FlameBuffer.Create(new ParticleInfo(
                    spawnPos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.7f, 1.1f)),
                    ParticleColor * Main.rand.NextFloat(0.85f, 1.15f),
                    Main.rand.Next(ParticleLife - 8, ParticleLife + 6)
                ));
            }

            if (Main.rand.NextBool(3))
            {
                float along = Main.rand.NextFloat(0.6f, 1f);
                Vector2 spawnPos = Projectile.Center + tipDirection * SwordLength * Projectile.scale * along;

                ParticleSystem.FlameBuffer.Create(new ParticleInfo(
                    spawnPos.ToNumerics(),
                    (-tipDirection * 0.6f * ParticleSpeedMultiplier).ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(Main.rand.NextFloat(26f, 44f) * ParticleWidth, Main.rand.NextFloat(20f, 34f) * ParticleWidth),
                    ParticleCoreColor * 1.15f,
                    Main.rand.Next(ParticleLife - 10, ParticleLife)
                ));
            }
        }
    }
}