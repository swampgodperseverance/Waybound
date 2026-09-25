using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Projectiles.Ranged.Guns.HM;

namespace Waybound.Content.Items.Weapons.Ranged.Guns.HM
{
    public class Tesseract : ModItem
    {
        public override void SetStaticDefaults() { }

        public override void SetDefaults()
        {
            Item.width = 102;
            Item.height = 26;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 5);
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 42;
            Item.knockBack = 3.2f;
            Item.crit = 6;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<TesseractProj1>();
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Bullet;
            Item.UseSound = null;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useTime = 68;
                Item.useAnimation = 68;
                Item.channel = false;
                Item.shoot = ModContent.ProjectileType<TesseractLaser>();
                Item.UseSound = SoundID.Item67;
            }
            else
            {
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.channel = true;
                Item.shoot = ModContent.ProjectileType<TesseractProj1>();
                Item.UseSound = null;
            }
            return true;
        }

        public override void HoldItem(Player player)
        {
            bool wantAlt = Main.mouseRight || player.altFunctionUse == 2 || (player.itemAnimation > 0 && player.altFunctionUse == 2);

            int normal = ModContent.ProjectileType<TesseractHeld>();
            int alt = ModContent.ProjectileType<Tesseract2Held>();

            if (wantAlt)
            {
                if (player.ownedProjectileCounts[alt] < 1)
                {
                    // не убиваем старый сразу — даём ему дожить 1 тик
                    Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, alt, 0, 0, player.whoAmI);
                }
                // старый held сам умрёт в своём AI когда увидит, что alt активен
            }
            else
            {
                if (player.ownedProjectileCounts[normal] < 1)
                {
                    Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, normal, 0, 0, player.whoAmI);
                }
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Vector2 dir = velocity.SafeNormalize(Vector2.UnitX);
                Vector2 muzzle = player.MountedCenter + dir * 52f;
                int laser = Projectile.NewProjectile(source, muzzle, dir * 13f, ModContent.ProjectileType<TesseractLaser>(), (int)(damage * 2.8f), knockback * 1.6f, player.whoAmI);
                if (laser >= 0)
                    Main.projectile[laser].scale = 1.5f;

                player.GetModPlayer<TesseractPlayer>().GlowTimer = 55;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI && Main.projectile[i].type == ModContent.ProjectileType<Tesseract2Held>())
                    {
                        Main.projectile[i].localAI[0] = 18;
                        break;
                    }
                }
                return false;
            }

            if (player.ownedProjectileCounts[ModContent.ProjectileType<TesseractProj1>()] < 1)
            {
                int proj = Projectile.NewProjectile(source, player.MountedCenter, Vector2.Zero, ModContent.ProjectileType<TesseractProj1>(), damage, knockback, player.whoAmI);
                if (proj >= 0)
                    Main.projectile[proj].ai[1] = 0;
            }
            return false;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-14, 0);
    }

    public class TesseractPlayer : ModPlayer
    {
        public int GlowTimer = 0;

        public override void PostUpdate()
        {
            if (GlowTimer > 0)
                GlowTimer--;
        }
    }

    public class TesseractHeld : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Ranged/Guns/HM/Tesseract";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 102;
            Projectile.height = 26;
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
            if (player.dead || !player.active || player.HeldItem.type != ModContent.ItemType<Tesseract>())
            {
                Projectile.Kill();
                return;
            }
            if (Main.mouseRight || player.altFunctionUse == 2)
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

            float holdDistance = 26f;
            Projectile.Center = player.MountedCenter + direction * holdDistance;
            float baseRotation = direction.ToRotation() + (player.direction == -1 ? MathHelper.Pi : 0f);

            float charge = 0f;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI &&
                    Main.projectile[i].type == ModContent.ProjectileType<TesseractProj1>() && Main.projectile[i].ai[1] == 0)
                {
                    charge = Main.projectile[i].ai[0];
                    break;
                }
            }

            float shakeStrength = charge * 0.025f;
            float shakeRot = (float)Math.Sin(Main.GameUpdateCount * 0.24f) * shakeStrength;
            Projectile.rotation = baseRotation + shakeRot * player.direction;

            Vector2 shakeOffset = new Vector2(
                (float)Math.Sin(Main.GameUpdateCount * 0.29f) * charge * 0.45f,
                (float)Math.Cos(Main.GameUpdateCount * 0.37f) * charge * 0.35f);
            Projectile.Center += shakeOffset * player.direction;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, baseRotation - MathHelper.PiOver2 * player.direction + shakeRot * 0.4f);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, baseRotation - MathHelper.PiOver2 * player.direction * 0.85f + shakeRot * 0.22f);

            if (Projectile.localAI[0] > 0)
            {
                Projectile.localAI[0]--;
                float t = Projectile.localAI[0] / 12f;
                float kick = t * t * 0.11f;
                Projectile.rotation -= player.direction * kick;
                Vector2 up = direction.RotatedBy(-player.direction * MathHelper.PiOver2);
                Projectile.Center += up * (kick * 2.8f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            var modPlayer = Main.player[Projectile.owner].GetModPlayer<TesseractPlayer>();
            if (modPlayer.GlowTimer > 0)
            {
                Texture2D glowTex = ModContent.Request<Texture2D>("Waybound/Content/Items/Weapons/Ranged/Guns/HM/Tesseract_Glow").Value;
                float t = modPlayer.GlowTimer / 50f;
                float fade = (float)Math.Sin(t * MathHelper.Pi);
                Color glowColor = Color.White * fade * 0.9f;

                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = new Vector2(1.2f, 0).RotatedBy(MathHelper.TwoPi * i / 3f + Main.GlobalTimeWrappedHourly * 1.6f);
                    Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition + offset, null, glowColor * 0.4f, Projectile.rotation, glowTex.Size() / 2f, Projectile.scale * 1.04f, effects, 0);
                }
                Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, glowTex.Size() / 2f, Projectile.scale, effects, 0);
            }
            return false;
        }
    }

    public class Tesseract2Held : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Ranged/Guns/HM/Tesseract2";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 102;
            Projectile.height = 26;
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
            if (player.dead || !player.active || player.HeldItem.type != ModContent.ItemType<Tesseract>())
            {
                Projectile.Kill();
                return;
            }

            if (player.altFunctionUse != 2 && player.itemAnimation <= 0 && !Main.mouseRight)
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

            float holdDistance = 28f;
            Projectile.Center = player.MountedCenter + direction * holdDistance;
            float baseRotation = direction.ToRotation() + (player.direction == -1 ? MathHelper.Pi : 0f);
            Projectile.rotation = baseRotation;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, baseRotation - MathHelper.PiOver2 * player.direction);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, baseRotation - MathHelper.PiOver2 * player.direction * 0.85f);

            if (Projectile.localAI[0] > 0)
            {
                Projectile.localAI[0]--;
                float t = Projectile.localAI[0] / 18f;
                float kick = t * t * 0.16f;
                Projectile.rotation -= player.direction * kick;
                Vector2 up = direction.RotatedBy(-player.direction * MathHelper.PiOver2);
                Projectile.Center += up * (kick * 4f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, effects, 0);

            var modPlayer = Main.player[Projectile.owner].GetModPlayer<TesseractPlayer>();
            if (modPlayer.GlowTimer > 0)
            {
                Texture2D glowTex = ModContent.Request<Texture2D>("Waybound/Content/Items/Weapons/Ranged/Guns/HM/Tesseract_Glow").Value;
                float t = modPlayer.GlowTimer / 55f;
                float fade = (float)Math.Sin(t * MathHelper.Pi);
                Color glowColor = Color.White * fade * 1.0f;

                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new Vector2(1.4f, 0).RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 2f);
                    Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition + offset, null, glowColor * 0.5f, Projectile.rotation, glowTex.Size() / 2f, Projectile.scale * 1.06f, effects, 0);
                }
                Main.EntitySpriteDraw(glowTex, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, glowTex.Size() / 2f, Projectile.scale, effects, 0);
            }
            return false;
        }
    }
}