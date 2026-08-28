using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Waybound.Content.Buffs.Minions;

namespace Waybound.Content.Projectiles.Summon
{
    public class OssuaryStaffProj : ModProjectile
    {
        private bool stuckInEnemy = false;
        private NPC stuckTarget;
        private Vector2 stuckOffset;
        private float stuckRotation;
        private int damageTimer;
        private float embedDepth;

        private const float IdleRadius = 100f;
        private const float SearchRange = 400f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.dead || !player.active || !player.HasBuff(ModContent.BuffType<OssuaryBuff>()))
            {
                Projectile.Kill();
                return;
            }

            float distToPlayer = Vector2.Distance(Projectile.Center, player.Center);
            if (distToPlayer > 1200f)
            {
                for (int i = 0; i < 30; i++)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Bone, Main.rand.NextVector2Circular(6f, 6f), 0, default, 2.2f);
                    d.noGravity = true;
                }
                Projectile.Center = player.Center + Main.rand.NextVector2CircularEdge(80f, 80f);
                Projectile.velocity = Vector2.Zero;
                Projectile.ai[0] = 0f;
                Projectile.ai[1] = 0f;
                for (int i = 0; i < 30; i++)
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Bone, Main.rand.NextVector2Circular(6f, 6f), 0, default, 2.2f);
                    d.noGravity = true;
                }
                Projectile.netUpdate = true;
            }

            if (stuckInEnemy && stuckTarget != null && stuckTarget.active)
            {
                Vector2 inward = (stuckTarget.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                embedDepth = MathHelper.Lerp(embedDepth, 6f, 0.08f);
                Projectile.Center = stuckTarget.Center + stuckOffset + inward * embedDepth;
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation = stuckRotation;

                damageTimer++;
                if (damageTimer % 32 == 0)
                {
                    stuckTarget.StrikeNPC(new NPC.HitInfo
                    {
                        Damage = Projectile.damage / 4,
                        Knockback = 0,
                        HitDirection = 0,
                        Crit = false
                    });

                    for (int i = 0; i < 2; i++)
                    {
                        Dust.NewDustPerfect(
                            Projectile.Center + Main.rand.NextVector2Circular(10, 10),
                            DustID.Blood,
                            Main.rand.NextVector2Circular(1.5f, 1.5f),
                            100,
                            default,
                            1.2f
                        ).noGravity = true;
                    }
                }

                Lighting.AddLight(Projectile.Center, 0.6f, 0.2f, 0.2f);
                return;
            }

            if (stuckInEnemy && (stuckTarget == null || !stuckTarget.active))
            {
                stuckInEnemy = false;
                embedDepth = 0f;
            }

            NPC target = FindClosestNPC(SearchRange);
            if (target != null)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                float dist = toTarget.Length();

                if (dist < 34f)
                {
                    stuckInEnemy = true;
                    stuckTarget = target;
                    stuckOffset = Projectile.Center - target.Center;
                    stuckRotation = Projectile.rotation;
                    Projectile.velocity = Vector2.Zero;
                    embedDepth = 0f;
                    Projectile.netUpdate = true;
                    return;
                }

                toTarget.Normalize();
                Projectile.velocity = (Projectile.velocity * 20f + toTarget * 15f) / 21f;
                Projectile.rotation += 0.42f + dist * 0.0012f;
            }
            else
            {
                if (Projectile.ai[0] == 0f || Vector2.Distance(Projectile.Center, new Vector2(Projectile.ai[0], Projectile.ai[1])) < 30f)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 offset = angle.ToRotationVector2() * Main.rand.NextFloat(60f, IdleRadius);
                    Projectile.ai[0] = player.Center.X + offset.X;
                    Projectile.ai[1] = player.Center.Y + offset.Y;
                }

                Vector2 idleTarget = new Vector2(Projectile.ai[0], Projectile.ai[1]);
                Vector2 dir = idleTarget - Projectile.Center;
                dir.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * 5.5f, 0.09f);

                Projectile.rotation += 0.16f + (float)System.Math.Sin(Main.GameUpdateCount * 0.09f) * 0.18f;
            }

            Lighting.AddLight(Projectile.Center, 0.6f, 0.6f, 0.6f);
        }

        private NPC FindClosestNPC(float maxDist)
        {
            NPC closest = null;
            float best = maxDist * maxDist;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.CanBeChasedBy() && n.active)
                {
                    float d = Vector2.DistanceSquared(n.Center, Projectile.Center);
                    if (d < best)
                    {
                        best = d;
                        closest = n;
                    }
                }
            }
            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int p = Projectile.NewProjectile(
                Projectile.GetSource_OnHit(target),
                Projectile.Center,
                Vector2.Zero,
                975,
                1,
                0f,
                Projectile.owner,
                Main.rand.NextFloat(MathHelper.TwoPi),
                1f);

            if (p < Main.maxProjectiles)
                Main.projectile[p].timeLeft = 120;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                Vector2 pos = Projectile.oldPos[i] - Main.screenPosition + origin;
                float alpha = 0.65f * (1f - i / (float)Projectile.oldPos.Length);
                Color col = new Color(230, 230, 230) * alpha;

                float scale = Projectile.scale * (1f - i * 0.09f);

                Main.EntitySpriteDraw(tex, pos, null, col, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0);
            }

            if (stuckInEnemy)
            {
                Player player = Main.player[Projectile.owner];
                int count = player.ownedProjectileCounts[Projectile.type];
                float strength = MathHelper.Clamp(count * 0.35f, 0.4f, 2.5f);

                float pulse = (float)System.Math.Sin(Main.GameUpdateCount * 0.18f) * 0.5f + 0.5f;

                float intensity = (0.35f + pulse * 0.65f) * strength;

                Color glowColor = new Color(220, 40, 40, 0) * intensity;

                Vector2 drawPos = Projectile.Center - Main.screenPosition;

                int layers = 5 + (int)(strength * 3f);

                for (int i = 0; i < layers; i++)
                {
                    float angle = MathHelper.TwoPi * i / layers;

                    float radius = 2.5f + pulse * 3.5f * strength;

                    Vector2 offset = angle.ToRotationVector2() * radius;

                    Main.EntitySpriteDraw(
                        tex,
                        drawPos + offset,
                        null,
                        glowColor,
                        Projectile.rotation,
                        origin,
                        Projectile.scale * (1.1f + pulse * 0.25f * strength),
                        SpriteEffects.None,
                        0
                    );
                }

                Main.EntitySpriteDraw(
                    tex,
                    drawPos,
                    null,
                    new Color(255, 80, 80, 0) * (0.25f * strength),
                    Projectile.rotation,
                    origin,
                    Projectile.scale * (1.2f + pulse * 0.2f),
                    SpriteEffects.None,
                    0
                );
            }

            return true;
        }
    }
}