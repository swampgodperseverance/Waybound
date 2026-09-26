using System;
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
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Ranged.Bows
{
    public class BowOfDesertHunter : BaseHoldoutBow
    {
        public override int ProjectileType => ModContent.ProjectileType<BowOfDesertHunterHoldout>();
        public override int Damage => 28;
        public override int UseTime => 18;
        public override int ShotCooldown => 24;
        public override float HoldoutDistance => 16f;
        public override float BaseOffset => 2f;
        public override int AmmoType => AmmoID.Arrow;
        public override int Rarity => ItemRarityID.Orange;
        public override int Value => Item.sellPrice(gold: 3);
        public override float KnockBack => 2.8f;
        public override SoundStyle UseSound => SoundID.Item5;
        public override SoundStyle ShotSound => SoundID.Item5;
        public override int DustType => DustID.Torch;

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useTime = 42;
                Item.useAnimation = 42;
                Item.channel = false;
                Item.shoot = ModContent.ProjectileType<BowOfDesertHunterBomb>();
                Item.shootSpeed = 9f;
                Item.UseSound = SoundID.Item1;
            }
            else
            {
                Item.useTime = UseTime;
                Item.useAnimation = UseTime;
                Item.channel = true;
                Item.shoot = ProjectileType;
                Item.shootSpeed = 1f;
                Item.UseSound = UseSound;
            }
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Vector2 dir = velocity.SafeNormalize(Vector2.UnitX);
                Vector2 spawn = player.MountedCenter + dir * 28f;
                int id = Projectile.NewProjectile(source, spawn, dir * 9f, ModContent.ProjectileType<BowOfDesertHunterBomb>(), (int)(damage * 1.35f), knockback, player.whoAmI);
                return false;
            }

            Projectile.NewProjectile(source, position, velocity, ProjectileType, damage, knockback, player.whoAmI);
            return false;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            if (player.altFunctionUse == 2)
                return false;
            return false;
        }
    }

    public class BowOfDesertHunterHoldout : BaseHoldoutProjectile
    {
        public override int ShotCooldown => 24;
        public override string Texture => "Waybound/Content/Items/Weapons/Ranged/Bows/BowOfDesertHunter";
        public override float HoldoutDistance => 16f;
        public override float BaseOffset => 2f;
        public override float RecoilAmount => 0.06f;
        public override float SpawnOffset => 26f;
        public override int DustType => DustID.Torch;
        public override int ProjectileType => ModContent.ProjectileType<BowOfDesertHunterP>();
        public override SoundStyle ShotSound => SoundID.Item5;
        public override float ProjectileSpeed => 10f;
        public override int DustCount => 8;
        public override float DustScale => 1.2f;
        public override Vector3 LightColor => new Vector3(0.55f, 0.3f, 0.15f);
    }

    public class BowOfDesertHunterP : ModProjectile
    {
        private Vector2 oldPos;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 280;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            oldPos = Projectile.Center;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * Math.Max(Projectile.velocity.Length(), 8f);
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            float speed = Projectile.velocity.Length();
            speed *= 1.018f;
            speed = MathHelper.Clamp(speed, 8f, 22f);
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * speed;

            if (oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
            else if (!Main.gamePaused)
                oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.3f);

            if (Main.netMode != NetmodeID.Server)
            {
                Vector2 pos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(2f, 6f);
                Vector2 vel = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(0.5f, 0.5f);

                ParticleManager.NewParticle<FlameParticleOld>(
                    pos,
                    vel,
                    Color.White,
                    0.5f,
                    1f
                );

                ParticleManager.NewParticle<FlameParticleOld>(
                    pos,
                    vel,
                    Color.White,
                    0.5f,
                    1f
                );
            }

            Lighting.AddLight(Projectile.Center, 0.7f, 0.35f, 0.15f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                    ParticleSystem.FlameBuffer.Create(new ParticleInfo(
                        Projectile.Center.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(2.5f, 4.2f)),
                        new Color(255, 130, 30, 0),
                        Main.rand.Next(16, 28)
                    ));
                }
            }
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f, Pitch = 0.2f }, Projectile.Center);
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
                    new Color(255, 90, 30, 0) * fade * 0.35f, Projectile.oldRot[k], origin, Projectile.scale * (1f - k * 0.04f), SpriteEffects.None, 0);
            }

            Color outline = new Color(255, 70, 30, 0) * 0.65f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.8f, 0f).RotatedBy(MathHelper.TwoPi * i / 4f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, outline, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }

    public class BowOfDesertHunterBomb : ModProjectile
    {
        public override string Texture => "Waybound/Content/NPCs/Bosses/Themis/ThemisBomb";

        private float fadeIn;
        private float outlinePulse;
        private int pulseTimer;

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 255;
        }

        public override void OnSpawn(IEntitySource source)
        {
            fadeIn = 0f;
            Projectile.velocity *= 0.65f;
        }

        public override void AI()
        {
            fadeIn = MathHelper.Clamp(fadeIn + 0.06f, 0f, 1f);
            Projectile.alpha = (int)((1f - fadeIn) * 255f);

            if (Projectile.timeLeft > 40)
            {
                Projectile.velocity *= 0.94f;
                if (Projectile.velocity.Length() < 0.4f)
                    Projectile.velocity *= 0.85f;
            }
            else
            {
                fadeIn = MathHelper.Clamp(Projectile.timeLeft / 40f, 0f, 1f);
                Projectile.alpha = (int)((1f - fadeIn) * 255f);
            }

            pulseTimer++;
            outlinePulse = 0.45f + 0.55f * (0.5f + 0.5f * (float)Math.Sin(pulseTimer * 0.18f));
            Projectile.rotation += 0.06f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && !npc.dontTakeDamage && npc.Hitbox.Intersects(Projectile.Hitbox))
                {
                    Projectile.Kill();
                    return;
                }
            }

            int arrowType = ModContent.ProjectileType<BowOfDesertHunterP>();
            int bombType = Type;
            int blastType = ModContent.ProjectileType<BowOfDesertHunterBombBlast>();

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.whoAmI == Projectile.whoAmI)
                    continue;
                if (p.owner != Projectile.owner)
                    continue;

                if (p.type == blastType && p.Hitbox.Intersects(Projectile.Hitbox))
                {
                    Projectile.Kill();
                    return;
                }

                if ((p.type == arrowType || p.arrow) && p.friendly && p.Hitbox.Intersects(Projectile.Hitbox))
                {
                    Projectile.Kill();
                    return;
                }

                if (p.type == bombType && p.Hitbox.Intersects(Projectile.Hitbox) && p.timeLeft < Projectile.timeLeft)
                {
                    Projectile.Kill();
                    return;
                }
            }

            Lighting.AddLight(Projectile.Center, 0.8f * outlinePulse, 0.25f * outlinePulse, 0.1f);
        }

        public override bool? CanDamage() => false;

        public override void OnKill(int timeLeft)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.InheritSource(Projectile),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<BowOfDesertHunterBombBlast>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 10; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                    ParticleSystem.FlameBuffer.Create(new ParticleInfo(
                        Projectile.Center.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(18f, 30f)),
                        new Color(255, 140, 40, 220),
                        Main.rand.Next(24, 40)
                    ));
                }
            }

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f, Pitch = -0.15f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float fade = fadeIn * ((255f - Projectile.alpha) / 255f);
            if (fade < 0.01f)
                return false;

            Color glow = new Color(255, 40, 25, 0) * outlinePulse * 0.75f * fade;
            float scaleAdd = 1f + outlinePulse * 0.2f;

            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(3.5f * outlinePulse, 0f).RotatedBy(MathHelper.TwoPi * i / 6f + Main.GlobalTimeWrappedHourly * 2f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, glow, Projectile.rotation, origin, Projectile.scale * scaleAdd, SpriteEffects.None, 0);
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(6.5f * outlinePulse, 0f).RotatedBy(MathHelper.TwoPi * i / 4f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, glow * 0.4f, Projectile.rotation, origin, Projectile.scale * (scaleAdd + 0.08f), SpriteEffects.None, 0);
            }

            Color body = Color.Lerp(lightColor, new Color(255, 70, 40), outlinePulse * 0.35f) * fade;
            Main.EntitySpriteDraw(texture, drawPos, null, body, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }

    public class BowOfDesertHunterBombBlast : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.width = 140;
            Projectile.height = 140;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 14;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;

                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(5.5f, 5.5f);
                        ParticleSystem.FlameBuffer.Create(new ParticleInfo(
                            Projectile.Center.ToNumerics(),
                            vel.ToNumerics(),
                            Main.rand.NextFloat(MathHelper.TwoPi),
                            new System.Numerics.Vector2(Main.rand.NextFloat(20f, 36f)),
                            new Color(255, 140, 40, 220),
                            Main.rand.Next(26, 44)
                        ));
                    }

                    for (int i = 0; i < 14; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                        ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                            Projectile.Center.ToNumerics(),
                            vel.ToNumerics(),
                            Main.rand.NextFloat(MathHelper.TwoPi),
                            new System.Numerics.Vector2(Main.rand.NextFloat(16f, 28f)),
                            new Color(255, 160, 50, 200),
                            Main.rand.Next(18, 32)
                        ));
                    }
                }
            }
        }
    }
}