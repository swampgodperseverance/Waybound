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
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;

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
            Item.damage = 15;
            Item.DamageType = DamageClass.Melee;
            Item.width = 68;
            Item.height = 68;
            Item.useTime = 15;
            Item.useAnimation = 15;
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
    }

    public class SlimeSlayerSwing : CustomSwing
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Melee/Swords/SlimeSlayerKatana";

        public override float SwordLength => 70f;
        public override float BaseScale => 0.8f;

        public override int TrailDrawLength => Projectile.oldPos.Length / 3;
        public override Color TrailColor => new Color(120, 200, 255);
        public override Color TrailCoreColor => new Color(180, 230, 255);

        public override Color ParticleColor => new Color(90, 200, 255);
        public override Color ParticleCoreColor => new Color(160, 235, 255);
        public override float ParticleWidth => 1f;
        public override float ParticleSizeMultiplier => 1f;
        public override float ParticleSpeedMultiplier => 1f;
        public override int ParticleLife => 22;
        public override int ParticleCount => 2;

        private bool spawnedProj;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.DamageType = DamageClass.Melee;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void OnSpawn(IEntitySource source)
        {
            spawnedProj = false;
        }

        public override void AI()
        {
            base.AI();

            Lighting.AddLight(Projectile.Center, 0.15f, 0.6f, 0.95f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            if (Main.rand.NextBool(3))
                target.AddBuff(ModContent.BuffType<SlimeSlayerKatanaB>(), 300);

            if (Main.myPlayer == Projectile.owner && !spawnedProj)
            {
                spawnedProj = true;
                Vector2 spawnPos = target.Center;
                Vector2 vel = (target.Center - Player.MountedCenter).SafeNormalize(Vector2.UnitX) * 12f;
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    vel,
                    ModContent.ProjectileType<SlimeSlayerKatanaProj>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
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
    public class SlimeSlayerKatanaProj : ModProjectile
    {
        private Vector2 oldPos = Vector2.Zero;
        private Vector2 startPos = Vector2.Zero;
        private bool returning;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 18;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.extraUpdates = 1;
            Projectile.noEnchantmentVisuals = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            oldPos = Projectile.Center;
            startPos = Projectile.Center;
            returning = false;
        }

        public override void AI()
        {
            if (Projectile.velocity.Length() > 0.1f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
            else if (!Main.gamePaused)
                oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.2f);

            Projectile.ai[0]++;

            float returnTime = 15f;

            if (Projectile.ai[0] < returnTime)
            {
                Projectile.velocity *= 0.985f;
                Projectile.velocity += Projectile.velocity.SafeNormalize(Vector2.Zero) * 0.05f;
            }
            else
            {
                Vector2 toStart = startPos - Projectile.Center;
                float dist = toStart.Length();

                if (dist < 28f)
                {
                    Projectile.Kill();
                    return;
                }

                toStart.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toStart * 18f, 0.08f);
            }

            if (Main.rand.NextBool(2))
            {
                Vector2 vel = Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.6f, 0.6f);
                float size = Main.rand.NextFloat(12f, 22f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.15f)),
                    new Color(90, 200, 255, 200) * Main.rand.NextFloat(0.9f, 1.2f),
                    Main.rand.Next(16, 28)
                ));
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0.55f, 0.75f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(3))
                target.AddBuff(ModContent.BuffType<SlimeSlayerKatanaB>(), 300);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                float size = Main.rand.NextFloat(16f, 28f);
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 1.15f)),
                    new Color(90, 200, 255, 210) * Main.rand.NextFloat(0.9f, 1.2f),
                    Main.rand.Next(18, 32)
                ));
            }

            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.6f, Pitch = 0.1f }, Projectile.Center);
        }

        public override Color? GetAlpha(Color lightColor) => new Color(160, 230, 255, 180);

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            int drawLength = Projectile.oldPos.Length / 2;
            float fadeDenom = MathF.Max(1f, drawLength - 1);

            for (int k = drawLength - 1; k >= 0; k--)
            {
                if (Projectile.oldPos[k] == Vector2.Zero)
                    continue;

                float t = 1f - k / fadeDenom;
                float alpha = t * t * Projectile.Opacity;

                Color trailColor = new Color(120, 200, 255) with { A = 0 };
                trailColor *= alpha;

                float scale = Projectile.scale * MathHelper.Lerp(0.6f, 1f, t);

                Main.EntitySpriteDraw(
                    texture,
                    Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition,
                    null,
                    trailColor,
                    Projectile.oldRot[k],
                    origin,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }

            if (oldPos != Vector2.Zero && oldPos != Projectile.Center)
            {
                Texture2D trailTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;
                Color trailColor = new Color(80, 190, 255, 0) * 0.65f;
                float trailLength = Vector2.Distance(Projectile.Center, oldPos);
                float trailScaleY = trailLength / trailTex.Height * 4.2f;

                Main.EntitySpriteDraw(trailTex, Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor, (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.85f, trailScaleY), SpriteEffects.None, 0f);

                Main.EntitySpriteDraw(trailTex, Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor * 0.4f, (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.4f, trailScaleY * 1.35f), SpriteEffects.None, 0f);
            }

            Color outline = new Color(90, 200, 255) * 0.55f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.6f, 0f).RotatedBy(MathHelper.TwoPi * i / 4f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, outline, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, new Color(180, 235, 255, 200), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}