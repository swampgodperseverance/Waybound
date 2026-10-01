using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
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

        private bool spawnedEffect;
        private bool spawnedProj;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.DamageType = DamageClass.Melee;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void OnSpawn(IEntitySource source)
        {
            spawnedEffect = false;
            spawnedProj = false;
        }

        public override void AI()
        {
            base.AI();

            float progress = 1f - (float)Player.itemAnimation / Player.itemAnimationMax;

            Lighting.AddLight(Projectile.Center, 0.15f, 0.6f, 0.95f);

            if (Main.myPlayer == Projectile.owner && !spawnedEffect && progress >= 0.35f)
            {
                spawnedEffect = true;
                float bladeRot = Projectile.rotation - MathHelper.PiOver4;
                Vector2 tip = Projectile.Center + bladeRot.ToRotationVector2() * (SwordLength * 0.65f * Projectile.scale);
            }
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

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = new(Projectile.spriteDirection == -1 ? texture.Width : 0f, texture.Height);
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float extraRotation = Projectile.spriteDirection == -1 ? MathHelper.PiOver2 : 0f;

            Color trailColor = new Color(120, 200, 255) with { A = 0 } * Projectile.Opacity;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                trailColor *= 0.78f;
                Main.spriteBatch.Draw(texture, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, trailColor, Projectile.oldRot[i] + extraRotation, origin, Projectile.scale, spriteEffects, 0f);
            }

            Color glow = new Color(100, 180, 255) * Projectile.Opacity * 0.45f;
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(1.5f, 0f).RotatedBy(MathHelper.TwoPi * i / 6f);
                Main.spriteBatch.Draw(texture, Projectile.Center + offset - Main.screenPosition, null, glow, Projectile.rotation + extraRotation, origin, Projectile.scale, spriteEffects, 0f);
            }

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, new Color(180, 230, 255) * Projectile.Opacity, Projectile.rotation + extraRotation, origin, Projectile.scale, spriteEffects, 0f);
            return false;
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
        private NPC targetNPC;
        private bool returning;
        private readonly VertexStrip vertexStrip = new VertexStrip();

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
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
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
            targetNPC = null;
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

            if (!returning)
            {
                Projectile.velocity *= 0.94f;

                if (Projectile.ai[0] >= 16f)
                {
                    returning = true;

                    if (targetNPC == null || !targetNPC.active)
                    {
                        float closest = 500f;
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC npc = Main.npc[i];
                            if (npc.active && !npc.friendly && npc.CanBeChasedBy(Projectile))
                            {
                                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                                if (dist < closest)
                                {
                                    closest = dist;
                                    targetNPC = npc;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Vector2 targetPos = targetNPC != null && targetNPC.active ? targetNPC.Center : startPos;
                Vector2 toTarget = targetPos - Projectile.Center;
                float dist = toTarget.Length();

                if (dist < 24f)
                {
                    Projectile.Kill();
                    return;
                }

                toTarget.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 16f, 0.12f);
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

            if (!returning)
            {
                targetNPC = target;
                returning = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = new(texture.Width / 2f, texture.Height / 2f);
            Color trailColor = new Color(120, 200, 255) with { A = 0 } * Projectile.Opacity;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                trailColor *= 0.78f;
                Main.spriteBatch.Draw(texture, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, trailColor, Projectile.oldRot[i], origin, Projectile.scale, SpriteEffects.None, 0f);
            }

            Color glow = new Color(100, 180, 255) * Projectile.Opacity * 0.45f;
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(1.5f, 0f).RotatedBy(MathHelper.TwoPi * i / 6f);
                Main.spriteBatch.Draw(texture, Projectile.Center + offset - Main.screenPosition, null, glow, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, new Color(180, 230, 255) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}