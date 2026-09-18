using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Projectiles.Ranged.Guns.PreHM;

namespace Waybound.Content.Items.Weapons.Ranged.Guns.PreHM
{
    public class IcebornRifle : ModItem
    {
        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 18;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 80);
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 28;
            Item.knockBack = 2.5f;
            Item.crit = 4;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<IcebornRifleProj>();
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Bullet;
            Item.UseSound = SoundID.Item11;
        }

        public override bool CanUseItem(Player player)
        {
            return player.GetModPlayer<IcebornRiflePlayer>().RecoveryTimer <= 0;
        }

        public override void HoldItem(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<IcebornRifleHeld>()] < 1)
            {
                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<IcebornRifleHeld>(),
                    0, 0, player.whoAmI);
            }
            if (player.itemAnimation > 0)
                player.velocity.X *= 0.98f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var global = Item.GetGlobalItem<IcebornRifleGlobal>();
            global.ShotCount++;

            Projectile held = null;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active &&
                    Main.projectile[i].owner == player.whoAmI &&
                    Main.projectile[i].type == ModContent.ProjectileType<IcebornRifleHeld>())
                {
                    held = Main.projectile[i];
                    break;
                }
            }

            if (global.ShotCount >= 4)
            {
                Projectile.NewProjectile(
                    source,
                    position,
                    velocity * 0.6f + new Vector2(0, -4f),
                    ModContent.ProjectileType<IcebornRifleProj2>(),
                    damage * 2,
                    knockback * 1.5f,
                    player.whoAmI
                );

                global.ShotCount = 0;
                player.GetModPlayer<IcebornRiflePlayer>().StartRecovery(90);

                if (held != null)
                {
                    held.ai[1] = 1;
                    held.ai[0] = 0;
                }

                return false;
            }

            Projectile.NewProjectile(
                source,
                position,
                velocity,
                ModContent.ProjectileType<IcebornRifleProj>(),   
                damage,
                knockback,
                player.whoAmI
            );

            if (held != null)
            {
                held.ai[0] = global.ShotCount;
                held.localAI[0] = 18;
            }

            return false;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10, 0);
    }

    public class IcebornRifleGlobal : GlobalItem
    {
        public int ShotCount = 0;
        public override bool InstancePerEntity => true;

        public override GlobalItem Clone(Item item, Item itemClone)
        {
            IcebornRifleGlobal clone = (IcebornRifleGlobal)base.Clone(item, itemClone);
            clone.ShotCount = ShotCount;
            return clone;
        }
    }

    public class IcebornRiflePlayer : ModPlayer
    {
        public int RecoveryTimer = 0;
        public float RecoveryProgress = 0f;

        public void StartRecovery(int duration)
        {
            RecoveryTimer = duration;
            RecoveryProgress = 0f;
        }

        public override void PostUpdate()
        {
            if (RecoveryTimer > 0)
            {
                RecoveryTimer--;
                RecoveryProgress = 1f - (RecoveryTimer / 90f);
                RecoveryProgress = MathHelper.Clamp(RecoveryProgress, 0f, 1f);
            }
            else
            {
                RecoveryProgress = 0f;
            }
        }
    }

    public class IcebornRifleHeld : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Ranged/Guns/PreHM/IcebornRifleHeld";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 18;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.hide = true;
            Projectile.ownerHitCheck = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.dead || !player.active || player.HeldItem.type != ModContent.ItemType<IcebornRifle>())
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;
            player.heldProj = Projectile.whoAmI;

            Vector2 direction = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            player.ChangeDir(direction.X > 0 ? 1 : -1);
            Projectile.direction = player.direction;
            Projectile.spriteDirection = player.direction;

            float holdDistance = 18f;
            Projectile.Center = player.MountedCenter + direction * holdDistance;
            float baseRotation = direction.ToRotation() + (player.direction == -1 ? MathHelper.Pi : 0f);

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, baseRotation - MathHelper.PiOver2 * player.direction);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, baseRotation - MathHelper.PiOver2 * player.direction * 0.85f);

            if (Projectile.ai[1] == 1)
            {
                var modPlayer = player.GetModPlayer<IcebornRiflePlayer>();
                if (modPlayer.RecoveryTimer <= 0)
                {
                    Projectile.ai[1] = 0;
                    Projectile.ai[0] = 0;
                    Projectile.frame = 0;
                    Projectile.alpha = 0;
                }
                else
                {
                    float p = modPlayer.RecoveryProgress;

                    int frameIndex = 3 - (int)(p * 3.999f);
                    Projectile.frame = (int)MathHelper.Clamp(frameIndex, 0, 3);

                    float fadeIn = MathHelper.Clamp(p * 1.5f, 0f, 1f);
                    Projectile.alpha = (int)((1f - fadeIn) * 255f);

                    float wobble = (float)Math.Sin(Main.GameUpdateCount * 0.18f) * 0.05f * (1f - p);
                    Projectile.rotation = baseRotation + wobble * player.direction;

                    if (Main.rand.NextBool(2))
                    {
                        Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(12f, 8f);
                        Dust d = Dust.NewDustPerfect(dustPos, DustID.IceTorch, Vector2.Zero, 100, new Color(120, 200, 255), 1.1f);
                        d.noGravity = true;
                        d.velocity = direction.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.4f, 1.6f) * (1f - p * 0.6f);
                        d.fadeIn = 0.9f;
                    }
                    if (Main.rand.NextBool(4))
                    {
                        Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(10f, 6f);
                        Dust d = Dust.NewDustPerfect(dustPos, DustID.Frost, Vector2.Zero, 80, default, 0.9f);
                        d.noGravity = true;
                        d.velocity *= 0.3f;
                    }

                    player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, baseRotation - MathHelper.PiOver2 * player.direction + wobble * 0.6f);
                    player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, baseRotation - MathHelper.PiOver2 * player.direction * 0.85f + wobble * 0.4f);
                    return;
                }
            }

            Projectile.frame = (int)Projectile.ai[0] % 4;
            Projectile.alpha = 0;

            float kick = 0f;
            if (Projectile.localAI[0] > 0)
            {
                Projectile.localAI[0]--;
                float t = Projectile.localAI[0] / 18f;
                kick = t * t * 0.22f;
            }

            Projectile.rotation = baseRotation - player.direction * kick;
            Vector2 up = direction.RotatedBy(-player.direction * MathHelper.PiOver2);
            Projectile.Center += up * (kick * 5f);

            float armKick = kick * 0.7f;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, baseRotation - MathHelper.PiOver2 * player.direction - armKick * player.direction);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, baseRotation - MathHelper.PiOver2 * player.direction * 0.85f - armKick * player.direction * 0.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, 4, 0, Projectile.frame);
            Vector2 origin = frame.Size() / 2f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Color drawColor = lightColor * ((255 - Projectile.alpha) / 255f);

            Color glowColor = new Color(80, 180, 255, 40) * 0.4f * ((255 - Projectile.alpha) / 255f);
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.6f, 0).RotatedBy(MathHelper.TwoPi * i / 4f);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset,
                    frame, glowColor, Projectile.rotation, origin, Projectile.scale, effects, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
                frame, drawColor, Projectile.rotation, origin, Projectile.scale, effects, 0);
            return false;
        }
    }
}