using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Minions;
using Waybound.Particles;

namespace Waybound.Content.Projectiles.Summon
{
    public class FriendlyStar : ModProjectile
    {
        private const int Cooldown = 42;
        private const int ChargeTime = 14;
        private const float AttackRange = 440f;
        private const float OrbitRange = 190f;

        internal static readonly Color Gold = new Color(255, 214, 90);
        internal static readonly Color Amber = new Color(255, 150, 50);
        internal static readonly Color Pale = new Color(255, 245, 200);

        private static Texture2D softGlow;

        private int attackTimer;
        private float charge;

        internal static Texture2D SoftGlow
        {
            get
            {
                if (softGlow == null || softGlow.IsDisposed)
                {
                    const int size = 128;
                    softGlow = new Texture2D(Main.instance.GraphicsDevice, size, size);
                    Color[] data = new Color[size * size];
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dx = (x - 63.5f) / 63.5f;
                            float dy = (y - 63.5f) / 63.5f;
                            float d = MathF.Sqrt(dx * dx + dy * dy);
                            float a = MathF.Pow(MathHelper.Clamp(1f - d, 0f, 1f), 2.2f);
                            data[y * size + x] = new Color(a, a, a, a);
                        }
                    }
                    softGlow.SetData(data);
                }
                return softGlow;
            }
        }

        public override void Unload()
        {
            Texture2D tex = softGlow;
            softGlow = null;
            if (tex != null)
                Main.QueueMainThreadAction(() => tex.Dispose());
        }

        internal static void Spark(Vector2 pos, Vector2 vel, Color color, float size, int life)
        {
            if (Main.dedServ)
                return;

            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new System.Numerics.Vector2(size),
                color,
                life));
        }

        internal static void Flash(Vector2 pos, float size, Color color, int duration)
        {
            if (Main.dedServ)
                return;

            ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                position: pos.ToNumerics(),
                velocity: System.Numerics.Vector2.Zero,
                rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                scale: new System.Numerics.Vector2(size),
                color: color,
                duration: duration));
        }

        internal static void Ring(Vector2 pos, int count, float speed, Color color, float size, int life)
        {
            float off = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < count; i++)
                Spark(pos, (off + MathHelper.TwoPi * i / count).ToRotationVector2() * speed, color, size, life);
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
        }

        public override bool? CanCutTiles() => false;

        public override bool MinionContactDamage() => false;

        private NPC FindTarget(Player player)
        {
            NPC chosen = player.MinionAttackTargetNPC >= 0 ? Main.npc[player.MinionAttackTargetNPC] : null;
            if (chosen != null && chosen.CanBeChasedBy(Projectile) && Vector2.Distance(chosen.Center, Projectile.Center) < 1400f
                && Collision.CanHitLine(Projectile.Center, 1, 1, chosen.Center, 1, 1))
                return chosen;

            NPC best = null;
            float bestDist = 900f * 900f;
            foreach (NPC n in Main.ActiveNPCs)
            {
                if (!n.CanBeChasedBy(Projectile))
                    continue;

                float d = Vector2.DistanceSquared(n.Center, player.Center);
                if (d >= bestDist || !Collision.CanHitLine(Projectile.Center, 1, 1, n.Center, 1, 1))
                    continue;

                bestDist = d;
                best = n;
            }
            return best;
        }

        private void Steer(Vector2 destination, float maxSpeed, float inertia)
        {
            Vector2 to = destination - Projectile.Center;
            float speed = Math.Min(maxSpeed, to.Length() * 0.14f);
            Vector2 desired = to.SafeNormalize(Vector2.Zero) * speed;
            Projectile.velocity = (Projectile.velocity * (inertia - 1f) + desired) / inertia;
        }

        private void Separate()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile o = Main.projectile[i];
                if (!o.active || o.type != Type || o.owner != Projectile.owner || o.whoAmI == Projectile.whoAmI)
                    continue;

                Vector2 diff = Projectile.Center - o.Center;
                float d = diff.Length();
                if (d < 44f)
                    Projectile.velocity += diff.SafeNormalize(Vector2.UnitX) * (44f - d) * 0.02f;
            }
        }

        private int MinionCount(Player player) => Math.Max(player.ownedProjectileCounts[Type], 1);

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (player.dead || !player.active || !player.HasBuff(ModContent.BuffType<Starlord>()))
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

            float far = Vector2.Distance(Projectile.Center, player.Center);
            if (far > 1600f)
            {
                Vector2 from = Projectile.Center;
                Projectile.Center = player.Center + Main.rand.NextVector2CircularEdge(80f, 80f);
                Projectile.velocity = Vector2.Zero;
                Projectile.netUpdate = true;

                if (!Main.dedServ)
                {
                    Flash(from, 90f, Gold, 12);
                    Flash(Projectile.Center, 110f, Pale, 14);
                    Ring(Projectile.Center, 14, 5f, Gold, 11f, 20);
                }
            }

            NPC target = FindTarget(player);
            float t = Main.GameUpdateCount * 0.03f;
            int count = MinionCount(player);
            float slot = MathHelper.TwoPi * Projectile.minionPos / count;

            if (target == null)
            {
                attackTimer = Math.Min(attackTimer, Cooldown - ChargeTime - 1);
                charge = Math.Max(0f, charge - 0.1f);

                Vector2 idle = player.Center + new Vector2(MathF.Cos(t + slot) * 75f, MathF.Sin(t * 0.8f + slot) * 50f - 20f);
                Steer(idle, 8f + far / 50f, 14f);
                Projectile.frame = 0;
            }
            else
            {
                float dir = Projectile.minionPos % 2 == 0 ? 1f : -1f;
                Vector2 orbit = target.Center + (t * 1.4f * dir + slot).ToRotationVector2() * OrbitRange;
                Steer(orbit, 11f, 16f);
                Animate();
                Attack(target);
            }

            Separate();

            if (Projectile.velocity.Length() > 14f)
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 14f;

            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, MathHelper.Clamp(Projectile.velocity.X * 0.05f, -0.4f, 0.4f), 0.15f);

            Visuals();
            Lighting.AddLight(Projectile.Center, Gold.ToVector3() * (0.5f + 0.5f * charge));
        }

        private void Animate()
        {
            if (++Projectile.frameCounter < 6)
                return;

            Projectile.frameCounter = 0;
            Projectile.frame = Projectile.frame % 2 + 1;
        }

        private void Attack(NPC target)
        {
            float dist = Vector2.Distance(Projectile.Center, target.Center);
            bool ready = dist <= AttackRange && Collision.CanHitLine(Projectile.Center, 1, 1, target.Center, 1, 1);

            if (!ready)
            {
                charge = Math.Max(0f, charge - 0.08f);
                attackTimer = Math.Min(attackTimer, Cooldown - ChargeTime);
                return;
            }

            attackTimer++;
            charge = attackTimer >= Cooldown - ChargeTime ? (attackTimer - (Cooldown - ChargeTime)) / (float)ChargeTime : 0f;

            if (!Main.dedServ && charge > 0f)
            {
                Vector2 off = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(24f, 44f);
                Spark(Projectile.Center + off, -off * 0.1f, Main.rand.NextBool(3) ? Pale : Gold, Main.rand.NextFloat(6f, 9f), 12);
            }

            if (attackTimer < Cooldown)
                return;

            attackTimer = 0;
            charge = 0f;

            Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            Vector2 muzzle = Projectile.Center + dir * 16f;

            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), muzzle, dir * 12f, ModContent.ProjectileType<StarBolt>(),
                    Projectile.damage, Projectile.knockBack, Projectile.owner);
            }

            Projectile.velocity -= dir * 2.5f;
            SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.45f, Pitch = 0.3f }, Projectile.Center);

            if (Main.dedServ)
                return;

            Flash(muzzle, 80f, Pale, 10);
            for (int i = 0; i < 10; i++)
                Spark(muzzle, dir.RotatedByRandom(0.5f) * Main.rand.NextFloat(2f, 7f), Main.rand.NextBool(3) ? Amber : Gold, Main.rand.NextFloat(7f, 12f), Main.rand.Next(14, 22));
        }

        private void Visuals()
        {
            if (Main.dedServ)
                return;

            float speed = Projectile.velocity.Length();

            if (speed > 3f && Main.rand.NextBool(3))
                Spark(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), -Projectile.velocity * 0.12f, Main.rand.NextBool(3) ? Amber : Gold, Main.rand.NextFloat(5f, 9f), 16);

            if (Main.rand.NextBool(22))
                Spark(Projectile.Center + Main.rand.NextVector2Circular(14f, 14f), new Vector2(0f, -Main.rand.NextFloat(0.3f, 0.9f)), Pale, Main.rand.NextFloat(4f, 7f), 26);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ)
                return;

            Flash(Projectile.Center, 120f, Gold, 14);
            Ring(Projectile.Center, 16, 5f, Gold, 11f, 22);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Texture2D star = TextureAssets.Extra[98].Value;
            Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Vector2 origin = frame.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;
            float breath = 0.9f + 0.1f * MathF.Sin(time * 3f + Projectile.whoAmI);
            Color add = new Color(Gold.R, Gold.G, Gold.B, 0);
            Color addA = new Color(Amber.R, Amber.G, Amber.B, 0);

            int len = Projectile.oldPos.Length;
            float trailK = MathHelper.Clamp(Projectile.velocity.Length() / 8f, 0f, 1f);
            for (int k = 1; k < len; k++)
            {
                if (Projectile.oldPos[k] == Vector2.Zero)
                    continue;

                float f = 1f - k / (float)len;
                Main.EntitySpriteDraw(tex, Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition, frame, addA * (f * 0.35f * trailK),
                    Projectile.rotation, origin, Projectile.scale * (0.6f + 0.4f * f), SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(SoftGlow, pos, null, add * (0.5f + 0.4f * charge), 0f, new Vector2(64f), (58f + 40f * charge) * breath / 128f, SpriteEffects.None, 0);

            for (int i = 0; i < 4; i++)
            {
                Vector2 off = new Vector2(1.6f + 1.5f * charge, 0f).RotatedBy(MathHelper.PiOver2 * i + time);
                Main.EntitySpriteDraw(tex, pos + off, frame, add * 0.4f, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, pos, frame, Color.Lerp(lightColor, Color.White, 0.6f), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            Main.EntitySpriteDraw(star, pos, null, add * (0.35f + 0.5f * charge), time * 0.5f, star.Size() / 2f, new Vector2(0.22f + 0.28f * charge), SpriteEffects.None, 0);

            return false;
        }
    }

    public class StarBolt : ModProjectile
    {
        private static readonly Color Gold = new Color(255, 214, 90);
        private static readonly Color Amber = new Color(255, 150, 50);
        private static readonly Color Pale = new Color(255, 245, 200);

        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 150;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.localAI[0]++;
            float age = Projectile.localAI[0];

            if (age > 10f)
            {
                NPC best = null;
                float bd = 320f * 320f;
                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (!n.CanBeChasedBy(Projectile))
                        continue;

                    float d = Vector2.DistanceSquared(n.Center, Projectile.Center);
                    if (d < bd)
                    {
                        bd = d;
                        best = n;
                    }
                }

                if (best != null)
                {
                    float speed = Projectile.velocity.Length();
                    Vector2 want = (best.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    Vector2 cur = Projectile.velocity.SafeNormalize(want);
                    Projectile.velocity = Vector2.Lerp(cur, want, 0.09f).SafeNormalize(want) * speed;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            if (!Main.dedServ)
            {
                if (Main.rand.NextBool(2))
                    FriendlyStar.Spark(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.6f, 0.6f),
                        Main.rand.NextBool(3) ? Amber : Gold, Main.rand.NextFloat(5f, 9f), 14);

                if (Main.rand.NextBool(8))
                    FriendlyStar.Spark(Projectile.Center, Main.rand.NextVector2Circular(1.2f, 1.2f), Pale, Main.rand.NextFloat(4f, 6f), 22);
            }

            Lighting.AddLight(Projectile.Center, Gold.ToVector3() * 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.4f, Pitch = 0.4f }, Projectile.Center);

            if (Main.dedServ)
                return;

            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            FriendlyStar.Flash(Projectile.Center, 70f, Pale, 8);
            for (int i = 0; i < 8; i++)
                FriendlyStar.Spark(Projectile.Center, dir.RotatedByRandom(1f) * Main.rand.NextFloat(2f, 6f), Main.rand.NextBool(3) ? Amber : Gold, Main.rand.NextFloat(7f, 11f), Main.rand.Next(14, 22));
        }

        public override void OnKill(int timeLeft)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.45f, Pitch = 0.2f }, Projectile.Center);

            if (Main.dedServ)
                return;

            FriendlyStar.Flash(Projectile.Center, 100f, Gold, 12);
            FriendlyStar.Ring(Projectile.Center, 12, 4.5f, Gold, 10f, 20);
            for (int i = 0; i < 6; i++)
                FriendlyStar.Spark(Projectile.Center, Main.rand.NextVector2Circular(5f, 5f), Main.rand.NextBool() ? Amber : Pale, Main.rand.NextFloat(6f, 10f), Main.rand.Next(14, 24));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D star = TextureAssets.Extra[98].Value;
            Texture2D soft = FriendlyStar.SoftGlow;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Color add = new Color(Gold.R, Gold.G, Gold.B, 0);
            Color addA = new Color(Amber.R, Amber.G, Amber.B, 0);
            float fade = MathHelper.Clamp(Projectile.timeLeft / 14f, 0f, 1f);
            float appear = MathHelper.Clamp(Projectile.localAI[0] / 5f, 0.3f, 1f);

            int len = Projectile.oldPos.Length;
            for (int k = 1; k < len; k++)
            {
                if (Projectile.oldPos[k] == Vector2.Zero)
                    continue;

                float f = 1f - k / (float)len;
                Vector2 p = Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(soft, p, null, addA * (f * 0.5f * fade), 0f, new Vector2(64f), 26f * f / 128f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(star, p, null, add * (f * 0.4f * fade), Projectile.oldRot[k], star.Size() / 2f, new Vector2(0.22f * f, 0.09f * f), SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(soft, pos, null, add * (0.8f * fade), 0f, new Vector2(64f), 44f * appear / 128f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(star, pos, null, add * fade, Projectile.rotation, star.Size() / 2f, new Vector2(0.5f, 0.2f) * appear, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(star, pos, null, addA * (0.7f * fade), Projectile.rotation + MathHelper.PiOver2, star.Size() / 2f, new Vector2(0.28f, 0.14f) * appear, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(star, pos, null, new Color(Pale.R, Pale.G, Pale.B, 0) * fade, 0f, star.Size() / 2f, new Vector2(0.14f) * appear, SpriteEffects.None, 0);

            return false;
        }
    }
}