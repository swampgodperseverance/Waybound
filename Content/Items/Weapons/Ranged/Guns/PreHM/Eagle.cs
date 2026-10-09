using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.GlobalPlayer;

namespace Waybound.Content.Items.Weapons.Ranged.Guns.PreHM
{
    public class Eagle : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 24;
            Item.damage = 21;
            Item.DamageType = DamageClass.Ranged;
            Item.crit = 6;
            Item.knockBack = 4.5f;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 14f;
            Item.useAmmo = AmmoID.Bullet;
            Item.value = Item.buyPrice(gold: 2);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item41 with { Volume = 0.85f, Pitch = -0.2f };
        }

        public override void HoldItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return;

            int type = ModContent.ProjectileType<EagleHeld>();
            if (player.ownedProjectileCounts[type] > 0) return;

            Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, type, 0, 0f, player.whoAmI);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX * player.direction);
            Vector2 center = player.RotatedRelativePoint(player.MountedCenter, true);
            Vector2 muzzle = EagleHeld.MuzzlePosition(player, dir);

            if (!Collision.CanHit(center, 1, 1, muzzle, 1, 1))
                muzzle = center;

            if (type == ProjectileID.Bullet)
                type = ModContent.ProjectileType<EagleBullet>();

            Vector2 shotVelocity = dir.RotatedByRandom(0.012f) * velocity.Length();
            Projectile.NewProjectile(source, muzzle, shotVelocity, type, damage, knockback, player.whoAmI);

            EagleHeld.Kick(player);
            return false;
        }

        public override void PostUpdate()
        {
            float pulse = 0.75f + 0.25f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f + Item.whoAmI);
            Lighting.AddLight(Item.Center, new Vector3(0.5f, 0.38f, 0.1f) * 0.35f * pulse);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = TextureAssets.Item[Type].Value;
            Vector2 position = Item.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 0.7f + 0.3f * MathF.Sin(time * 3f + whoAmI);
            Color glow = new Color(255, 205, 100, 0);

            for (int i = 0; i < 4; i++)
            {
                Vector2 off = new Vector2(2f, 0f).RotatedBy(time * 1.2f + MathHelper.PiOver2 * i);
                spriteBatch.Draw(texture, position + off, null, glow * (0.22f * pulse), rotation, origin, scale * 1.03f, SpriteEffects.None, 0f);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup(RecipeGroupID.IronBar, 6)
                .AddIngredient(ItemID.AntlionMandible, 5)
                .AddIngredient(ItemID.DesertFossil, 15)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class EagleHeld : ModProjectile
    {
        public const float HoldDistance = 12f;
        public const float MuzzleDistance = 24f;
        private const float GripX = 0.28f;
        private const float GripY = 0.62f;

        public override string Texture => "Waybound/Content/Items/Weapons/Ranged/Guns/PreHM/Eagle";

        public static Vector2 MuzzlePosition(Player player, Vector2 dir)
            => player.RotatedRelativePoint(player.MountedCenter, true) + dir * (HoldDistance + MuzzleDistance);

        public static void Kick(Player player)
        {
            int type = ModContent.ProjectileType<EagleHeld>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.owner != player.whoAmI || p.type != type) continue;

                p.ai[0] = (p.ai[0] + 1f) % 1000f;
                p.ai[1] = 1f;
                p.netUpdate = true;
                return;
            }
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.ownerHitCheck = true;
            Projectile.aiStyle = -1;
            Projectile.hide = true;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead || player.HeldItem.ModItem is not Eagle)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            Vector2 center = player.RotatedRelativePoint(player.MountedCenter, true);

            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 target = (Main.MouseWorld - center).SafeNormalize(Vector2.UnitX * player.direction);
                Vector2 old = Projectile.velocity;
                Vector2 current = target;

                if (old.LengthSquared() > 0.01f)
                    current = Utils.AngleLerp(old.ToRotation(), target.ToRotation(), 0.5f).ToRotationVector2();

                if (Vector2.DistanceSquared(old, current) > 0.0004f)
                    Projectile.netUpdate = true;

                Projectile.velocity = current;
                player.ChangeDir(current.X >= 0f ? 1 : -1);
            }

            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);

            if (Projectile.localAI[2] == 0f)
            {
                Projectile.localAI[2] = 1f;
                Projectile.localAI[0] = Projectile.ai[0];
            }
            else if (Projectile.localAI[0] != Projectile.ai[0])
            {
                Projectile.localAI[0] = Projectile.ai[0];
                ShotEffects(player, dir);
            }

            Projectile.ai[1] = Math.Max(0f, Projectile.ai[1] * 0.82f - 0.01f);
            float kick = Projectile.ai[1];

            Vector2 grip = center + dir * (HoldDistance - kick * 5f);
            grip.Y += 0.6f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2.2f);
            Projectile.Center = grip;

            float lift = -0.3f * kick * player.direction;
            float aimAngle = dir.ToRotation();
            float rot = aimAngle + lift;
            if (player.direction == -1)
                rot += MathHelper.Pi;
            Projectile.rotation = rot;
            Projectile.spriteDirection = player.direction;

            player.heldProj = Projectile.whoAmI;
            player.itemRotation = MathF.Atan2(dir.Y * player.direction, dir.X * player.direction);

            Player.CompositeArmStretchAmount stretch = kick > 0.35f ? Player.CompositeArmStretchAmount.ThreeQuarters : Player.CompositeArmStretchAmount.Full;
            player.SetCompositeArmFront(true, stretch, aimAngle + lift - MathHelper.PiOver2);

            Vector2 muzzle = grip + dir * MuzzleDistance;

            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(120))
            {
                Dust glint = Dust.NewDustPerfect(grip + dir * Main.rand.NextFloat(4f, MuzzleDistance) + Main.rand.NextVector2Circular(3f, 3f),
                    DustID.GoldFlame, Vector2.Zero, 100, default, 0.7f);
                glint.noGravity = true;
            }

            Lighting.AddLight(muzzle, new Vector3(0.5f, 0.38f, 0.1f) * (0.15f + kick * 1.2f));
        }

        private void ShotEffects(Player player, Vector2 dir)
        {
            if (Main.netMode == NetmodeID.Server) return;

            Vector2 muzzle = MuzzlePosition(player, dir);

            for (int i = 0; i < 9; i++)
            {
                Dust spark = Dust.NewDustPerfect(muzzle, DustID.GoldFlame,
                    dir.RotatedByRandom(0.35f) * Main.rand.NextFloat(3f, 9f), 100, default, Main.rand.NextFloat(1f, 1.5f));
                spark.noGravity = true;
            }

            for (int i = 0; i < 6; i++)
            {
                Dust sand = Dust.NewDustPerfect(muzzle, DustID.Sand,
                    dir.RotatedByRandom(0.6f) * Main.rand.NextFloat(1.5f, 5f), 80, default, Main.rand.NextFloat(0.9f, 1.3f));
                sand.noGravity = true;
            }

            for (int i = 0; i < 3; i++)
            {
                Dust smoke = Dust.NewDustPerfect(muzzle, DustID.Smoke,
                    dir.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.8f, 2.5f) + new Vector2(0f, -0.5f), 140, default, Main.rand.NextFloat(0.9f, 1.3f));
                smoke.noGravity = true;
            }

            Vector2 eject = new Vector2(-player.direction * Main.rand.NextFloat(1f, 2.2f), Main.rand.NextFloat(-3.5f, -1.5f));
            Dust shell = Dust.NewDustPerfect(Projectile.Center + dir * 4f, DustID.GoldFlame, eject, 0, default, 0.9f);
            shell.noGravity = false;

            if (Projectile.owner == Main.myPlayer)
                player.GetModPlayer<ScreenShakePlayer>().TriggerShake(5, 0.6f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 position = Projectile.Center - Main.screenPosition;
            Color color = Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16);

            bool flipped = player.direction == -1;
            SpriteEffects effects = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float gx = flipped ? 1f - GripX : GripX;
            Vector2 origin = new Vector2(texture.Width * gx, texture.Height * GripY);

            float kick = Projectile.ai[1];
            float time = Main.GlobalTimeWrappedHourly;

            Color glow = new Color(255, 205, 100, 0);
            float glowStrength = 0.1f + kick * 0.45f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 off = new Vector2(1.3f + kick * 2f, 0f).RotatedBy(MathHelper.PiOver2 * i + time * 1.5f);
                Main.EntitySpriteDraw(texture, position + off, null, glow * glowStrength, Projectile.rotation, origin, 1f, effects, 0);
            }

            Main.EntitySpriteDraw(texture, position, null, color, Projectile.rotation, origin, 1f, effects, 0);

            float flash = MathHelper.Clamp((kick - 0.55f) / 0.45f, 0f, 1f);
            if (flash > 0.01f)
            {
                Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
                Vector2 muzzle = Projectile.Center + dir * MuzzleDistance - Main.screenPosition;
                Texture2D ray = TextureAssets.Extra[98].Value;
                Vector2 rayOrigin = new Vector2(ray.Width / 2f, ray.Height);
                float baseRot = dir.ToRotation() + MathHelper.PiOver2;

                for (int i = -1; i <= 1; i++)
                {
                    float len = i == 0 ? 0.8f : 0.4f;
                    Main.EntitySpriteDraw(ray, muzzle, null, glow * (flash * 0.85f), baseRot + i * 0.5f, rayOrigin,
                        new Vector2(i == 0 ? 0.22f : 0.14f, len * flash + 0.1f), SpriteEffects.None, 0);
                }

                Main.EntitySpriteDraw(ray, muzzle, null, Color.White with { A = 0 } * (flash * 0.6f), baseRot, rayOrigin,
                    new Vector2(0.1f, 0.45f * flash), SpriteEffects.None, 0);
            }

            return false;
        }
    }

    public class EagleBullet : ModProjectile
    {
        private static readonly Color Gold = new Color(255, 205, 100);
        private static readonly Color Sand = new Color(240, 215, 150);

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bullet;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Bullet);
            AIType = ProjectileID.Bullet;
        }

        public override void PostAI()
        {
            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    Main.rand.NextBool() ? DustID.GoldFlame : DustID.Sand,
                    -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.3f, 0.3f),
                    100, default, Main.rand.NextFloat(0.7f, 1.1f));
                dust.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, new Vector3(0.45f, 0.34f, 0.1f));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode == NetmodeID.Server) return;

            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame,
                    Main.rand.NextVector2Circular(3.5f, 3.5f), 80, default, Main.rand.NextFloat(0.9f, 1.3f));
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = 0.2f }, Projectile.position);

            if (Main.netMode == NetmodeID.Server) return;

            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center,
                    i % 2 == 0 ? DustID.GoldFlame : DustID.Sand,
                    Main.rand.NextVector2Circular(3.5f, 3.5f), 80, default, Main.rand.NextFloat(0.9f, 1.4f));
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            Color tone = Gold with { A = 0 };

            for (int i = Projectile.oldPos.Length - 1; i >= 1; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                float k = i / (float)Projectile.oldPos.Length;
                Main.EntitySpriteDraw(tex, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null,
                    tone * ((1f - k) * 0.5f), Projectile.oldRot[i], origin, Projectile.scale * (1.1f - k * 0.5f), SpriteEffects.None, 0);
            }

            Vector2 pos = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(tex, pos, null, (Sand with { A = 0 }) * 0.55f, Projectile.rotation, origin, Projectile.scale * 1.35f, SpriteEffects.None, 0);

            return true;
        }
    }
}