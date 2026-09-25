using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Dusts;
using Waybound.Content.Items.Ammo.Bullets;
using Waybound.Content.Items.Materials.Bars;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Ranged.Guns.PreHM
{
    public class ColdShotgun : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 10;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 54;
            Item.height = 40;
            Item.useTime = 29;
            Item.useAnimation = 29;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 1f;
            Item.value = Item.sellPrice(0, 2, 90, 0);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item36;
            Item.autoReuse = false;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 9f;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient<HielitiumBar>(7)
                .AddIngredient(664, 35)
                .AddIngredient(2503, 28)
                .AddIngredient(964, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override Vector2? HoldoutOffset() => new Vector2(-6, -8);

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            var mp = player.GetModPlayer<ColdShotgunPlayer>();
            if (mp.CrystalCooldown > 0)
                return false;

            if (player.altFunctionUse == 2)
            {
                Item.useTime = 36;
                Item.useAnimation = 36;
                Item.UseSound = SoundID.Item28;
            }
            else
            {
                Item.useTime = 29;
                Item.useAnimation = 29;
                Item.UseSound = SoundID.Item36;
            }
            return true;
        }

        public override void HoldItem(Player player)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ColdShotgunHeld>()] < 1)
            {
                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<ColdShotgunHeld>(),
                    0, 0, player.whoAmI);
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var mp = player.GetModPlayer<ColdShotgunPlayer>();

            if (player.altFunctionUse == 2)
            {
                mp.CrystalCooldown = 180;
                mp.OverheatTimer = 50;

                Vector2 dir = velocity.SafeNormalize(Vector2.UnitX);
                Vector2 muzzle = player.MountedCenter + dir * 30f;

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile h = Main.projectile[i];
                    if (h.active && h.owner == player.whoAmI && h.type == ModContent.ProjectileType<ColdShotgunHeld>())
                    {
                        muzzle = h.Center + dir * 24f;
                        h.localAI[0] = 20;
                        break;
                    }
                }

                for (int i = 0; i < 4; i++)
                {
                    float spread = MathHelper.Lerp(-0.22f, 0.22f, i / 3f);
                    Vector2 crystalVel = dir.RotatedBy(spread) * Main.rand.NextFloat(9f, 11.5f);
                    int proj = Projectile.NewProjectile(source, muzzle, crystalVel, ModContent.ProjectileType<ColdShotgunP>(), (int)(damage * 1.6f), knockback * 1.4f, player.whoAmI);
                    if (proj >= 0)
                        Main.projectile[proj].scale = 0.95f;
                }

                int fogCount = Main.rand.Next(2, 4);
                for (int i = 0; i < fogCount; i++)
                {
                    Vector2 fogPos = muzzle + Main.rand.NextVector2Circular(6f, 4f);
                    int d = Dust.NewDust(fogPos, 2, 2, ModContent.DustType<Fog>(), 0f, 0f, 0, default, 0.4f);
                    Main.dust[d].velocity = dir * Main.rand.NextFloat(0.4f, 1.2f) + Main.rand.NextVector2Circular(0.2f, 0.2f);
                    Main.dust[d].scale = 0.35f + Main.rand.NextFloat(0f, 0.1f);
                }

                SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.85f, Pitch = -0.2f }, player.Center);
                SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.6f, Pitch = 0.1f }, player.Center);
                return false;
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 newVelocity = velocity.RotatedByRandom(MathHelper.ToRadians(25));
                newVelocity *= Main.rand.NextFloat(0.8f, 1f);
                Projectile.NewProjectile(source, position, newVelocity, ModContent.ProjectileType<FrozenBulletP>(), damage, knockback, player.whoAmI);
            }

            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<ColdShotgunP>(), damage, 2f, player.whoAmI);

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active &&
                    Main.projectile[i].owner == player.whoAmI &&
                    Main.projectile[i].type == ModContent.ProjectileType<ColdShotgunHeld>())
                {
                    Main.projectile[i].localAI[0] = 16;
                    break;
                }
            }
            return false;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            if (player.altFunctionUse == 2)
                return false;
            return Main.rand.NextFloat() >= 0.10f;
        }
    }

    public class ColdShotgunPlayer : ModPlayer
    {
        public int CrystalCooldown;
        public int OverheatTimer;

        public override void PostUpdate()
        {
            if (CrystalCooldown > 0)
                CrystalCooldown--;
            if (OverheatTimer > 0)
                OverheatTimer--;
        }
    }

    public class ColdShotgunHeld : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Ranged/Guns/PreHM/ColdShotgun";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 54;
            Projectile.height = 40;
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
            if (player.dead || !player.active || player.HeldItem.type != ModContent.ItemType<ColdShotgun>())
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;
            player.heldProj = Projectile.whoAmI;

            var mp = player.GetModPlayer<ColdShotgunPlayer>();
            float heat = 0f;
            if (mp.CrystalCooldown > 0)
                heat = MathHelper.Clamp(mp.CrystalCooldown / 180f, 0f, 1f);
            if (mp.OverheatTimer > 0)
                heat = Math.Max(heat, MathHelper.Clamp(mp.OverheatTimer / 50f, 0f, 1f));

            Vector2 mouseDir = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX);
            player.ChangeDir(mouseDir.X > 0 ? 1 : -1);

            float downAngle = heat * 0.55f;
            Vector2 direction = mouseDir.RotatedBy(player.direction * downAngle);

            Projectile.direction = player.direction;
            Projectile.spriteDirection = player.direction;

            float holdDistance = MathHelper.Lerp(22f, 18f, heat);
            Projectile.Center = player.MountedCenter + direction * holdDistance;
            float baseRotation = direction.ToRotation() + (player.direction == -1 ? MathHelper.Pi : 0f);

            float kick = 0f;
            if (Projectile.localAI[0] > 0)
            {
                Projectile.localAI[0]--;
                float t = Projectile.localAI[0] / 16f;
                kick = t * t * 0.2f;
            }

            Projectile.rotation = baseRotation - player.direction * kick;
            Vector2 up = direction.RotatedBy(-player.direction * MathHelper.PiOver2);
            Projectile.Center += up * (kick * 4.5f);

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, baseRotation - MathHelper.PiOver2 * player.direction - kick * 0.65f * player.direction);
            player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, baseRotation - MathHelper.PiOver2 * player.direction * 0.85f - kick * 0.35f * player.direction);

            if (heat > 0.05f && Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 6f), DustID.Torch, Vector2.Zero, 100, new Color(255, 80, 60), 0.75f + heat * 0.35f);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(0.5f, 0.5f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            var mp = Main.player[Projectile.owner].GetModPlayer<ColdShotgunPlayer>();
            float heat = 0f;
            if (mp.CrystalCooldown > 0)
                heat = MathHelper.Clamp(mp.CrystalCooldown / 180f, 0f, 1f);
            if (mp.OverheatTimer > 0)
                heat = Math.Max(heat, MathHelper.Clamp(mp.OverheatTimer / 50f, 0f, 1f));

            Color drawColor = Color.Lerp(lightColor, new Color(255, 70, 55), heat * 0.85f);
            Color glowColor = Color.Lerp(new Color(80, 180, 255, 40) * 0.35f, new Color(255, 60, 40, 60) * 0.55f, heat);

            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.5f + heat * 0.8f, 0).RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * heat * 2f);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset,
                    null, glowColor, Projectile.rotation, origin, Projectile.scale, effects, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
                null, drawColor, Projectile.rotation, origin, Projectile.scale, effects, 0);
            return false;
        }
    }

    public class ColdShotgunP : ModProjectile
    {
        private Vector2 oldPos = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 14;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 45;
            Projectile.extraUpdates = 1;
            Projectile.scale = 0.85f;
            Projectile.tileCollide = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            oldPos = Projectile.Center;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 120, false);
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
            else if (!Main.gamePaused)
                oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.28f);

            if (Main.rand.NextBool(3))
            {
                Vector2 vel = Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.8f, 0.8f);
                float size = Main.rand.NextFloat(14f, 22f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.15f)),
                    new Color(110, 220, 255, 195) * Main.rand.NextFloat(0.95f, 1.25f),
                    Main.rand.Next(16, 28)
                ));
            }

            Lighting.AddLight(Projectile.Center, 0.25f, 0.5f, 0.75f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 perturbedSpeed = Main.rand.NextVector2Circular(3.5f, 3.5f);
                if (perturbedSpeed.LengthSquared() < 1f)
                    perturbedSpeed = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 3.2f;
                Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center, perturbedSpeed, ModContent.ProjectileType<ColdShotgunP1>(), Projectile.damage / 2, 1f, Projectile.owner);
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                float size = Main.rand.NextFloat(25f, 36f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.15f)),
                    new Color(110, 220, 255, 195) * Main.rand.NextFloat(0.95f, 1.25f),
                    Main.rand.Next(18, 32)
                ));
            }

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = -0.25f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f, Pitch = 0.15f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (Projectile.oldPos[k] == Vector2.Zero)
                    continue;
                float fade = 1f - k / (float)Projectile.oldPos.Length;
                Main.EntitySpriteDraw(texture, Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition, null,
                    new Color(100, 200, 255, 0) * fade * 0.45f, Projectile.oldRot[k], origin, Projectile.scale * (1f - k * 0.04f), SpriteEffects.None, 0);
            }

            if (oldPos != Vector2.Zero && oldPos != Projectile.Center)
            {
                Texture2D trailTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;
                Color trailColor = new Color(70, 170, 255, 0) * 0.6f;
                float trailLength = Vector2.Distance(Projectile.Center, oldPos);
                float trailScaleY = trailLength / trailTex.Height * 3.0f;

                Main.EntitySpriteDraw(trailTex, Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor, (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.65f, trailScaleY), SpriteEffects.None, 0f);

                Main.EntitySpriteDraw(trailTex, Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor * 0.4f, (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.3f, trailScaleY * 1.2f), SpriteEffects.None, 0f);
            }

            Color outline = new Color(90, 180, 255) * 0.5f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.4f, 0f).RotatedBy(MathHelper.TwoPi * i / 4f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, outline, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }

    public class ColdShotgunP1 : ModProjectile
    {
        private Vector2 oldPos = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 3;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = true;
            Projectile.scale = 0.9f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            oldPos = Projectile.Center;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 60, false);
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
            else if (!Main.gamePaused)
                oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.4f);

            if (Main.rand.NextBool(3))
            {
                Vector2 vel = Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                float size = Main.rand.NextFloat(12f, 18f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.15f)),
                    new Color(110, 220, 255, 195) * Main.rand.NextFloat(0.95f, 1.25f),
                    Main.rand.Next(14, 24)
                ));
            }

            Lighting.AddLight(Projectile.Center, 0.2f, 0.4f, 0.65f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2.2f, 2.2f);
                float size = Main.rand.NextFloat(12f, 20f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.15f)),
                    new Color(110, 220, 255, 195) * Main.rand.NextFloat(0.95f, 1.25f),
                    Main.rand.Next(14, 24)
                ));
            }
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.4f, Pitch = 0.3f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (Projectile.oldPos[k] == Vector2.Zero)
                    continue;
                float fade = 1f - k / (float)Projectile.oldPos.Length;
                Main.EntitySpriteDraw(texture, Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition, null,
                    new Color(100, 200, 255, 0) * fade * 0.4f, Projectile.oldRot[k], origin, Projectile.scale * (1f - k * 0.05f), SpriteEffects.None, 0);
            }

            if (oldPos != Vector2.Zero && oldPos != Projectile.Center)
            {
                Texture2D trailTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;
                Color trailColor = new Color(70, 170, 255, 0) * 0.5f;
                float trailLength = Vector2.Distance(Projectile.Center, oldPos);
                float trailScaleY = trailLength / trailTex.Height * 2.2f;

                Main.EntitySpriteDraw(trailTex, Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor, (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.45f, trailScaleY), SpriteEffects.None, 0f);
            }

            Color outline = new Color(90, 180, 255) * 0.45f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.2f, 0f).RotatedBy(MathHelper.TwoPi * i / 4f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, outline, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}