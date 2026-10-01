using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Weapons.Melee.Swords
{
    public class SlimeSlayerKatana : ModItem
    {
        public int attackPattern;

        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 80;
            Item.DamageType = DamageClass.Melee;
            Item.width = 68;
            Item.height = 68;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = Item.sellPrice(0, 8, 40, 0);
            Item.rare = ItemRarityID.Master;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<SlimeSlayerSwing>();
            Item.shootSpeed = 1f;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<SlimeSlayerSwing>()] == 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(
                source,
                player.MountedCenter,
                Vector2.Zero,
                ModContent.ProjectileType<SlimeSlayerSwing>(),
                damage,
                knockback,
                player.whoAmI,
                ai0: attackPattern
            );
            attackPattern = attackPattern == 0 ? 1 : 0;
            return false;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(3))
                target.AddBuff(ModContent.BuffType<SlimeSlayerKatanaB>(), 300);
        }
    }

    public class SlimeSlayerSwing : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Melee/Swords/SlimeSlayerKatana";

        private const int TimeMax = 10;
        private bool swingDown;
        private bool initialized;

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 74;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TimeMax;
            Projectile.alpha = 0;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.noEnchantmentVisuals = true;
            Projectile.ownerHitCheck = false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            swingDown = Projectile.ai[0] == 0f;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.dead || !player.active)
            {
                Projectile.Kill();
                return;
            }

            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            Projectile.Center = player.MountedCenter;

            if (Main.myPlayer == Projectile.owner && !initialized)
            {
                Vector2 aim = Main.MouseWorld - player.MountedCenter;
                Projectile.velocity = aim.SafeNormalize(Vector2.UnitX);
                initialized = true;
                Projectile.netUpdate = true;
            }

            Vector2 aimDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float aimAngle = aimDir.ToRotation();

            if (aimDir.X > 0.05f) { Projectile.spriteDirection = 1; player.ChangeDir(1); }
            else if (aimDir.X < -0.05f) { Projectile.spriteDirection = -1; player.ChangeDir(-1); }

            float progress = 1f - (Projectile.timeLeft / (float)TimeMax);
            float eased = 1f - MathF.Pow(1f - progress, 2.1f);

            float swingRange = MathHelper.Pi * 0.42f;
            float baseAngle = swingDown
                ? MathHelper.Lerp(-swingRange, swingRange, eased)
                : MathHelper.Lerp(swingRange, -swingRange, eased);

            Projectile.rotation = aimAngle + baseAngle;

            float armRotation = Projectile.rotation;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);

            float scaleProgress = MathF.Sin(progress * MathHelper.Pi);
            Projectile.scale = 1f + scaleProgress * 0.12f;

            Projectile.Opacity = MathHelper.Clamp(Projectile.timeLeft / (float)TimeMax * 1.5f, 0f, 1f);

            Lighting.AddLight(Projectile.Center, 0.15f, 0.6f, 0.95f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 180);

            if (Main.myPlayer == Projectile.owner && Main.rand.NextBool(3))
            {
                Vector2 vel = Main.rand.NextVector2Circular(5.5f, 5.5f) + new Vector2(0f, -2.5f);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    vel,
                    ModContent.ProjectileType<SlimeSlayerKatanaP>(),
                    (int)(Projectile.damage * 0.5f),
                    Projectile.knockBack * 0.5f,
                    Projectile.owner
                );
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Vector2 origin = Projectile.spriteDirection == 1
                ? new Vector2(0f, texture.Height)
                : new Vector2(texture.Width, texture.Height);

            float drawRotation = Projectile.rotation + MathHelper.PiOver4 * Projectile.spriteDirection;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color color = new Color(180, 230, 255) * Projectile.Opacity;

            for (int k = 1; k <= 4; k++)
            {
                float trailProgress = k / 4.5f;
                float trailRotOffset = (swingDown ? -1f : 1f) * k * 0.08f * Projectile.spriteDirection;
                Vector2 trailOffset = new Vector2(0f, texture.Height).RotatedBy(drawRotation + trailRotOffset) * 0.08f * k;

                Color trailColor = new Color(120, 200, 255) * Projectile.Opacity * (1f - trailProgress) * 0.5f;

                Main.EntitySpriteDraw(
                    texture,
                    drawPos - trailOffset,
                    null,
                    trailColor,
                    drawRotation + trailRotOffset,
                    origin,
                    Projectile.scale * (1f - k * 0.05f),
                    effects,
                    0
                );
            }

            Color outline = new Color(100, 180, 255) * Projectile.Opacity * 0.5f;
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(1.6f, 0f).RotatedBy(MathHelper.TwoPi * i / 6f);
                Main.EntitySpriteDraw(
                    texture,
                    drawPos + offset,
                    null,
                    outline,
                    drawRotation,
                    origin,
                    Projectile.scale,
                    effects,
                    0
                );
            }

            Main.EntitySpriteDraw(
                texture,
                drawPos,
                null,
                color,
                drawRotation,
                origin,
                Projectile.scale,
                effects,
                0
            );

            return false;
        }
    }

    public class SlimeSlayerKatanaP : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }


           public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.light = 0.5f;
            Projectile.aiStyle = -1;
            Projectile.scale = 0.9f;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 60;
            Projectile.alpha = 0;
        }
        

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity *= 1.025f;
            Projectile.alpha += 5;

            if (Main.rand.NextBool(6))
            {
                Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(0.5f, 2f);
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Frost, vel.X, vel.Y, Projectile.alpha, new Color(120, 200, 255), 1.1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.2f;
            }

            Lighting.AddLight(Projectile.Center, 0.15f, 0.6f, 0.95f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(9))
                target.AddBuff(ModContent.BuffType<SlimeSlayerKatanaB>(), 300);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(180, 230, 255) * (1f - Projectile.alpha / 255f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * (1f - Projectile.alpha / 255f) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin,
                    Projectile.scale * (1f - k / 10f), SpriteEffects.None, 0);
            }
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(0.5f, 2f);
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Frost, vel.X, vel.Y, Projectile.alpha, new Color(120, 200, 255), 1.1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 3f;
            }
        }
    }

    public class SlimeSlayerKatanaB : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.defense -= 10;
            if (npc.defense < 0) npc.defense = 0;
        }
    }
}