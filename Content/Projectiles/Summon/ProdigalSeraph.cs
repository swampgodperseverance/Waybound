using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Minions;

namespace Waybound.Content.Projectiles.Summon
{
    public class ProdigalSeraph : ModProjectile
    {
        private int shootFrame = 7;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!CheckMinionLimit(player))
            {
                Projectile.Kill();
                return;
            }

            if (player.dead || !player.active || !player.HasBuff(ModContent.BuffType<ProdigyBuff>()))
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            float radius = 80f;
            float angle = (Main.GlobalTimeWrappedHourly * 2f) + Projectile.whoAmI;
            Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
            Vector2 targetPos = player.Center + offset;
            Vector2 move = targetPos - Projectile.Center;
            Projectile.velocity = move * 0.1f;

            NPC target = null;
            float distanceMax = 900f;
            foreach (NPC npc in Main.npc)
            {
                if (npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (dist < distanceMax)
                    {
                        distanceMax = dist;
                        target = npc;
                    }
                }
            }

            bool attacking = target != null;

            if (attacking)
            {
                Projectile.spriteDirection = target.Center.X > Projectile.Center.X ? 1 : -1;
            }
            else
            {
                if (Projectile.velocity.X > 0.1f)
                    Projectile.spriteDirection = 1;
                else if (Projectile.velocity.X < -0.1f)
                    Projectile.spriteDirection = -1;
            }

            Projectile.rotation = Projectile.velocity.X * 0.05f;

            Projectile.frameCounter++;
            if (attacking)
            {
                if (Projectile.frameCounter >= 7)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame++;
                    if (Projectile.frame < 4) Projectile.frame = 4;
                    if (Projectile.frame > shootFrame) Projectile.frame = 4;

                    if (Projectile.frame == shootFrame)
                        Shoot(target);
                }
            }
            else
            {
                if (Projectile.frameCounter >= 8)
                {
                    Projectile.frameCounter = 0;
                    Projectile.frame = (Projectile.frame + 1) % 4;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool CheckMinionLimit(Player player)
        {
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == Projectile.type && p.owner == Projectile.owner)
                {
                    count++;
                    if (count > 5)
                    {
                        if (p.whoAmI < Projectile.whoAmI)
                        {
                            p.Kill();
                        }
                    }
                }
            }
            return count <= 5;
        }

        private void Shoot(NPC target)
        {
            if (target == null || !target.active) return;

            Vector2 direction = target.Center - Projectile.Center;
            direction.Normalize();
            direction *= 10f;

            int projType = GetRandomProjectile();

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                direction,
                projType,
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner
            );
        }

        private int GetRandomProjectile()
        {
            int choice = Main.rand.Next(3);
            return choice switch
            {
                0 => ModContent.ProjectileType<ProdigyGold>(),
                1 => ModContent.ProjectileType<ProdigyAshen>(),
                _ => ModContent.ProjectileType<ProdigyLava>(),
            };
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
            Vector2 origin = frame.Size() / 2f;

            SpriteEffects effects = Projectile.spriteDirection == 1
                ? SpriteEffects.None
                : SpriteEffects.FlipHorizontally;

            Color glowColor = Color.White * 0.6f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 4f).ToRotationVector2() * 2f;
                Main.EntitySpriteDraw(
                    texture,
                    Projectile.Center - Main.screenPosition + offset,
                    frame,
                    glowColor,
                    Projectile.rotation,
                    origin,
                    Projectile.scale,
                    effects,
                    0
                );
            }

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                frame,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                effects,
                0
            );

            return false;
        }
    }
}