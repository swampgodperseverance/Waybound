using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Projectiles.Armor
{
    public class GutsJew : ModProjectile
    {
        private int passiveAttackCooldown = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 13;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 999999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.netUpdate = true;
            Projectile.scale = 0f;
            Projectile.friendly = false;
            Projectile.hostile = false;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            if (Projectile.ai[2] < 1f)
            {
                Projectile.ai[2] += 0.05f;
                Projectile.scale = MathHelper.Lerp(0f, 1f, Projectile.ai[2]);
            }

            if (Projectile.ai[1] == 0f)
            {
                Projectile.frameCounter++;
                int frameDelay = (Projectile.frame == 6 || Projectile.frame == 12) ? 8 : 5;
                if (Projectile.frameCounter > frameDelay)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame = (Projectile.frame + 1) % 13;
                }
            }
            else
            {
                Projectile.frameCounter += 2;
                int frameDelay = (Projectile.frame == 6 || Projectile.frame == 12) ? 5 : 3;
                if (Projectile.frameCounter > frameDelay)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame = (Projectile.frame + 1) % 13;
                }
            }

            if (Projectile.ai[1] == 0f)
            {
                PositionAI(player);

                passiveAttackCooldown = Math.Max(0, passiveAttackCooldown - 1);

                NPC target = FindNearestEnemy(player, 200f);
                if (target != null)
                {
                    Vector2 toTarget = target.Center - Projectile.Center;  

                    float targetRotation = toTarget.ToRotation();
                    float angleDiff = MathHelper.WrapAngle(targetRotation - Projectile.rotation);
                    Projectile.rotation = MathHelper.WrapAngle(Projectile.rotation + angleDiff * 0.08f);

                    if (Projectile.Hitbox.Intersects(target.Hitbox) && passiveAttackCooldown <= 0)
                    {
                        int baseDamage = 50;

                        NPC.HitInfo hitInfo = target.CalculateHitInfo(
                            baseDamage,
                            hitDirection: Math.Sign(toTarget.X),   // Math.Sign(target.Center.X - Projectile.Center.X)
                            crit: false,
                            knockBack: 4f,
                            damageType: DamageClass.Summon,
                            damageVariation: true,
                            luck: player.luck
                        );

                        target.StrikeNPC(hitInfo, fromNet: false, noPlayerInteraction: false);

                        passiveAttackCooldown = 30;

                        for (int i = 0; i < 5; i++)
                        {
                            Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Blood, 0f, 0f, 100, default, 1.2f);
                            dust.velocity = toTarget.SafeNormalize(Vector2.Zero) * 2f + Main.rand.NextVector2Circular(1f, 1f);
                            dust.noGravity = true;
                        }

                        Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit1, Projectile.Center);
                    }
                }
                else
                {
                    Projectile.rotation = MathHelper.Lerp(Projectile.rotation, 0f, 0.1f);
                }
            }
            else if (Projectile.ai[1] == 1f)
            {
                Vector2 toMouse = Main.MouseWorld - Projectile.Center;
                float targetRotation = toMouse.ToRotation();

                if (toMouse.Length() > 10f)
                {
                    float angleDiff = MathHelper.WrapAngle(targetRotation - Projectile.rotation);
                    float rotationSpeed = MathHelper.Clamp(Math.Abs(angleDiff) * 0.2f, 0.05f, 0.15f);
                    Projectile.rotation = MathHelper.WrapAngle(Projectile.rotation + angleDiff * rotationSpeed);
                }

                if (Math.Abs(MathHelper.WrapAngle(Projectile.rotation - (Main.MouseWorld - Projectile.Center).ToRotation())) < 0.1f)
                {
                    Projectile.ai[1] = 2f;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.netUpdate = true;
                }
            }
            else
            {
                AttackAI(player);
                if (Projectile.velocity.LengthSquared() > 0.1f)
                {
                    Projectile.rotation = Projectile.velocity.ToRotation();
                }
            }

            if (Projectile.owner == Main.myPlayer &&
                Common.ModSystems.VanillaKeybinds.ArmorSetBonusActivation.JustPressed &&
                Projectile.ai[1] == 0f)
            {
                Projectile.ai[1] = 1f;
                Projectile.rotation = (Main.MouseWorld - Projectile.Center).ToRotation();
                Projectile.netUpdate = true;
            }
        }

        private NPC FindNearestEnemy(Player player, float maxDistance)
        {
            NPC nearest = null;
            float minDist = maxDistance;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy() && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = npc;
                    }
                }
            }

            return nearest;
        }

        private void PositionAI(Player player)
        {
            int index = (int)Projectile.ai[0];
            float dir = player.direction;
            float[] baseAngles =
            {
                MathHelper.Pi + 0.2f,
                -MathHelper.PiOver2,
                -0.2f,
                -MathHelper.Pi * 0.85f,
                -MathHelper.Pi * 0.15f
            };

            if (dir < 0)
            {
                for (int i = 0; i < baseAngles.Length; i++)
                    baseAngles[i] = MathHelper.Pi - baseAngles[i];
            }

            float baseRadius = index <= 2 ? 96f : 78f;
            float sway = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.8f + index * 2.1f) * 6f;
            float angle = baseAngles[index];
            Vector2 idealOffset = angle.ToRotationVector2() * (baseRadius + sway);
            Vector2 targetPos = player.Center + idealOffset;

            foreach (Projectile other in Main.ActiveProjectiles)
            {
                if (other.type != Type || other.whoAmI == Projectile.whoAmI) continue;
                if (other.ai[1] != 0f) continue;
                Vector2 diff = Projectile.Center - other.Center;
                float dist = diff.Length();
                if (dist < 64f && dist > 0.1f)
                {
                    Vector2 push = diff.SafeNormalize(Vector2.Zero) * (64f - dist) * 0.7f;
                    targetPos += push;
                }
            }

            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.1f);
        }

        private void AttackAI(Player player)
        {
            Vector2 direction = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 22f, 0.15f);

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood,
                    0f, 0f, 80, default, 1.2f);
                dust.velocity *= 0.8f;
                dust.noGravity = true;
            }

            Projectile.friendly = true;
            Projectile.damage = (int)(200 * player.GetTotalDamage(DamageClass.Summon).Multiplicative);
            Projectile.DamageType = DamageClass.Summon;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 100;

            if (Vector2.Distance(Projectile.Center, Main.MouseWorld) < 40f)
            {
                Projectile.Kill();
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood,
                    0f, 0f, 100, default, 1.8f);
                dust.velocity *= 1.6f;
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.scale <= 0.01f) return false;

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            SpriteEffects effects = Projectile.rotation > MathHelper.PiOver2 && Projectile.rotation < 3 * MathHelper.PiOver2
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                frame,
                lightColor * Projectile.scale,
                Projectile.rotation + (effects == SpriteEffects.FlipHorizontally ? MathHelper.Pi : 0f),
                frame.Size() / 2f,
                Projectile.scale,
                effects,
                0
            );
            return false;
        }
    }

    public class Gut : ModProjectile
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 999999;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.scale = 0f;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            int jewID = (int)Projectile.ai[0];
            if (jewID < 0 || jewID >= Main.maxProjectiles)
            {
                Projectile.Kill();
                return;
            }

            Projectile jew = Main.projectile[jewID];
            if (!jew.active || jew.type != ModContent.ProjectileType<GutsJew>())
            {
                Projectile.scale -= 0.1f;
                if (Projectile.scale <= 0.04f) Projectile.Kill();
                return;
            }

            if (jew.ai[1] != 0f)
            {
                Projectile.scale -= 0.25f;
                if (Projectile.scale <= 0.04f) Projectile.Kill();
                return;
            }

            Projectile.scale = MathHelper.Lerp(Projectile.scale, jew.scale, 0.25f);

            Vector2 start = player.Center + new Vector2(-4f * player.direction, 4f);
            Vector2 end = jew.Center;
            Vector2 dir = end - start;
            float distance = dir.Length();

            Projectile.Center = start + dir * 0.5f;
            Projectile.rotation = dir.ToRotation() + MathHelper.PiOver2;

            Projectile.width = (int)(16 * Projectile.scale);
            Projectile.height = (int)(Math.Max(16, distance * 0.9f) * Projectile.scale);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.scale <= 0.01f) return false;

            int jewID = (int)Projectile.ai[0];
            if (jewID < 0 || jewID >= Main.maxProjectiles) return false;

            Projectile jew = Main.projectile[jewID];
            if (!jew.active || jew.type != ModContent.ProjectileType<GutsJew>()) return false;

            Player player = Main.player[Projectile.owner];
            Vector2 start = player.Center + new Vector2(-6f * player.direction, 4f);
            Vector2 end = jew.Center;
            Vector2 dir = end - start;
            float distance = dir.Length();
            float rotation = dir.ToRotation() - MathHelper.PiOver2;

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 scaleVec = new Vector2(Projectile.scale * 0.9f, distance / texture.Height);

            Main.EntitySpriteDraw(
                texture,
                start - Main.screenPosition,
                null,
                lightColor * Projectile.scale,
                rotation,
                new Vector2(texture.Width / 2f, 0),
                scaleVec,
                SpriteEffects.None,
                0
            );
            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
        }
    }
}