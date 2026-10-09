using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Content.Items.Materials.Misc;
using Waybound.Particles;
using SysVec2 = System.Numerics.Vector2;

namespace Waybound.Content.Items.Weapons.Ranged.Bows
{
    public class DeepFlux : BaseHoldoutBow
    {
        public override int ProjectileType => ModContent.ProjectileType<DeepFluxHoldout>();
        public override int Damage => 26;
        public override int UseTime => 22;
        public override int ShotCooldown => 34;
        public override float HoldoutDistance => 6f;
        public override float BaseOffset => 8f;
        public override int AmmoType => AmmoID.Arrow;
        public override int Rarity => ItemRarityID.Green;
        public override int Value => Item.sellPrice(silver: 32);
        public override float KnockBack => 3f;
        public override SoundStyle UseSound => SoundID.Item5 with { Volume = 0.5f };
        public override SoundStyle ShotSound => SoundID.Item5 with { Volume = 0.9f, Pitch = 0.1f };
        public override int DustType => DustID.PurpleTorch;
        public override bool ItemGlow => true;
        public override Color ItemGlowColor => new Color(200, 190, 255);

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient<DeepGolemCore>(1)
                .AddTile(ModContent.TileType<Tiles.Furniture.DeepStoneAltar>())
                .Register();
        }
    }

    public class DeepFluxHoldout : BaseHoldoutProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Ranged/Bows/DeepFlux";

        public override int ShotCooldown => 34;
        public override float HoldoutDistance => 6f;
        public override float BaseOffset => 8f;
        public override int DustType => DustID.PurpleTorch;
        public override int ProjectileType => ModContent.ProjectileType<deepFluxP>();
        public override SoundStyle ShotSound => SoundID.Item5 with { Volume = 0.9f, Pitch = 0.1f };

        public override float ProjectileSpeed => 16f;
        public override float ChargeSpeedMultMin => 0.9f;
        public override float ChargeSpeedMultMax => 1.3f;
        public override float ChargeDamageMultMin => 0.85f;
        public override float ChargeDamageMultMax => 1.35f;
        public override float RecoilAmount => 0.2f;
        public override float DustScale => 0.9f;
        public override Vector3 LightColor => new Vector3(0.45f, 0.35f, 0.55f);
        public override Color GlowColor => new Color(200, 190, 255);
        public override float GlowIntensity => 1.1f;
        public override bool AimGlow => true;

        public override void AI()
        {
            base.AI();

            if (!Main.dedServ && ParticleSystem.CrystalBuffer != null && ChargeProgress > 0.25f)
            {
                float rate = MathHelper.Lerp(0.4f, 1.8f, ChargeProgress);
                int n = (int)rate;
                if (Main.rand.NextFloat() < rate - n) n++;

                Vector2 lockedDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Vector2 perp = lockedDirection.RotatedBy(MathHelper.PiOver2);

                for (int i = 0; i < n; i++)
                {
                    Vector2 spawnPos = Projectile.Center
                                       + lockedDirection * Main.rand.NextFloat(2f, 12f)
                                       + perp * Main.rand.NextFloat(-5f, 5f);

                    Vector2 vel = -lockedDirection * Main.rand.NextFloat(0.3f, 1.2f)
                                  + perp * Main.rand.NextFloat(-0.6f, 0.6f);

                    float size = Main.rand.NextFloat(6f, 12f) * MathHelper.Lerp(0.7f, 1.1f, ChargeProgress);

                    ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                        spawnPos.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new SysVec2(size, size * Main.rand.NextFloat(0.75f, 1.05f)),
                        new Color(255, 168, 135, 0) * MathHelper.Lerp(0.85f, 1.15f, Main.rand.NextFloat()),
                        Main.rand.Next(18, 30)
                    ));
                }
            }
        }

        protected override void FireShot(Player player)
        {
            base.FireShot(player);

            if (Main.myPlayer != player.whoAmI || !player.HasAmmo(player.HeldItem))
                return;

            Vector2 spawnPos = Projectile.Center + Projectile.velocity * SpawnOffset;
            if (Collision.SolidCollision(spawnPos, 4, 4))
                spawnPos = player.Center + Projectile.velocity * (SpawnOffset + 5f);

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPos,
                Projectile.velocity * ProjectileSpeed,
                ProjectileType,
                Projectile.damage,
                Projectile.knockBack,
                player.whoAmI
            );

            if (!Main.dedServ && ParticleSystem.CrystalBuffer != null)
            {
                Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                for (int i = 0; i < 10; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(4f, 4f) + dir * Main.rand.NextFloat(1f, 4f);
                    float size = Main.rand.NextFloat(8f, 16f);

                    ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                        (spawnPos + Main.rand.NextVector2Circular(8f, 8f)).ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new SysVec2(size, size * Main.rand.NextFloat(0.75f, 1.05f)),
                        new Color(255, 168, 135, 0) * MathHelper.Lerp(0.85f, 1.15f, Main.rand.NextFloat()),
                        Main.rand.Next(20, 34)
                    ));
                }
            }
        }
    }

    public class deepFluxP : ModProjectile
    {
        public override string Texture => "Waybound/Content/Projectiles/Hostile/DeepCrystalProj";

        public int count;
        public bool collide = false;
        public Vector2 newVel;
        private float spawnFade;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.hostile = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.scale = 1.1f;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.alpha = 100;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            collide = true;
            Projectile.tileCollide = false;
            return false;
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            if (Projectile.ai[0] >= 150 || collide)
            {
                Projectile.damage = 0;
                count++;
                if (count >= 5)
                {
                    Projectile.alpha += 30;
                    count = 0;
                }
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                    return;
                }
            }

            spawnFade = MathHelper.Min(spawnFade + 0.1f, 1f);

            Lighting.AddLight(Projectile.position, 0.80f, 0.51f, 0.56f);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);

            if (!collide)
            {
                Movement();
                newVel = Projectile.velocity;
            }
            else
            {
                Projectile.velocity = newVel;
            }

            SpawnMagicTrail();
        }

        private void SpawnMagicTrail()
        {
            if (Main.dedServ)
                return;

            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 lockedDirection = dir;
            float alpha = (1f - Projectile.alpha / 255f) * spawnFade;

            if (alpha <= 0.02f)
                return;

            if (ParticleSystem.CrystalBuffer != null)
            {
                int count = Main.rand.NextBool(3) ? 2 : 1;
                for (int i = 0; i < count; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f) - dir * Main.rand.NextFloat(0.5f, 2.5f);
                    float size = Main.rand.NextFloat(8f, 18f);

                    ParticleSystem.CrystalBuffer.Create(new ParticleInfo(
                        (Projectile.Center + lockedDirection * Main.rand.NextFloat(4f, 24f)).ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new SysVec2(size, size * Main.rand.NextFloat(0.75f, 1.05f)),
                        new Color(255, 168, 135, 0) * MathHelper.Lerp(0.85f, 1.15f, Main.rand.NextFloat()) * alpha,
                        Main.rand.Next(22, 38)
                    ));
                }
            }

            if (Main.rand.NextBool(3))
            {
                ParticleManager.NewParticle<FlameParticleOld>(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -dir * Main.rand.NextFloat(0.4f, 1.5f) + Main.rand.NextVector2Circular(0.6f, 0.6f),
                    new Color(220, 200, 255),
                    Main.rand.NextFloat(0.15f, 0.3f) * alpha,
                    1f);
            }

            if (ParticleSystem.MegasparkBuffer != null && Main.rand.NextBool(4))
            {
                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    Projectile.Center.ToNumerics(),
                    (-dir * Main.rand.NextFloat(0.5f, 2f)).ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVec2(Main.rand.NextFloat(4f, 9f)),
                    new Color(200, 190, 255, 200) * alpha,
                    Main.rand.Next(16, 28)
                ));
            }
        }

        private void Movement()
        {
            if (Projectile.localAI[0] == 0f)
            {
                AdjustMagnitude(ref Projectile.velocity);
            }
            Vector2 move = Vector2.Zero;
            float distance = 250f;
            bool target = false;
            for (int k = 0; k < Main.maxNPCs; k++)
            {
                NPC npc = Main.npc[k];
                if (npc.active && !npc.dontTakeDamage && !npc.friendly && npc.lifeMax > 5 && npc.type != 488 && !npc.townNPC)
                {
                    Vector2 newMove = Main.npc[k].Center - Projectile.Center;
                    float distanceTo = (float)Math.Sqrt(newMove.X * newMove.X + newMove.Y * newMove.Y);
                    if (distanceTo < distance)
                    {
                        move = newMove;
                        distance = distanceTo;
                        target = true;
                    }
                }
            }
            if (target)
            {
                AdjustMagnitude(ref move);
                Projectile.velocity = (9 * Projectile.velocity + move) / 5f;
                AdjustMagnitude(ref Projectile.velocity);
            }
        }

        private void AdjustMagnitude(ref Vector2 vector)
        {
            float magnitude = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            if (magnitude > 9f)
            {
                vector *= 9f / magnitude;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(2))
                target.AddBuff(ModContent.BuffType<DeepFire>(), 90, false);

            collide = true;
            Projectile.tileCollide = false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Color drawColor = lightColor;

            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(new Color(255, 168, 135, 0)) * (1f - Projectile.alpha / 255f) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            return true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 168, 135, 0) * (1f - Projectile.alpha / 255f);
        }
    }
}