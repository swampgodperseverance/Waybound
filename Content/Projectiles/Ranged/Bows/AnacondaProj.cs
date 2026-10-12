using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.GlobalPlayer;

namespace Waybound.Content.Projectiles.Ranged.Bows
{
    public class AnacondaProj : ModProjectile
    {
        /// <summary>
        /// Finaly decided to finish the Titanoboa
        /// </summary>
        private const int SegmentCount = 28; // i decided to go with static amount
        private const float SegmentSpacing = 11f;
        private const int MaxTrailPoints = 420;

        //declare this as start
        private const float SlitherSpeed = 9.5f;
        private const float StrikeSpeed = 18f; 
        private const float WaveAmplitude = 38f;
        private const float WavePhaseStep = 0.24f;
        private const float StrikeRange = 260f;
        private const float SeekRange = 900f;

        private const int WindupTime = 12;
        private const int StrikeTime = 36;
        private const int CoilTime = 150;
        private const int ReleaseTime = 28;
        private const int SlitherTime = 340;

        private const float StrikeDamageMul = 2.2f;
        private const float CoilDamageMul = 0.4f;
        private const float BodyDamageMul = 0.55f;
        private const float HeadScale = 1.55f;

        private enum Mode
        {
            Slither = 0,
            Strike = 1,
            Coil = 2,
            Release = 3
        }

        private static Asset<Texture2D> headTexture;
        private static Asset<Texture2D> bodyTexture;
        private static Asset<Texture2D> tailTexture;

        private float heading;
        private float phase;
        private float timer;
        private float age;
        private float coilAngle;
        private float coilRadiusStart;
        private float coilRadiusEnd;
        private bool headContact;

        private List<Vector2> trail;
        private List<Vector2> segments;

        private Mode Current
        {
            get => (Mode)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        private int Side => Projectile.identity % 2 == 0 ? 1 : -1;

        public override void SetStaticDefaults()
        {
            if (Main.dedServ)
                return;

            headTexture = ModContent.Request<Texture2D>(Texture + "_Head");
            bodyTexture = ModContent.Request<Texture2D>(Texture + "_Body");
            tailTexture = ModContent.Request<Texture2D>(Texture + "_Tail");
        }

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(26f, 26f);
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.netImportant = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 900;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 16;

            trail = new List<Vector2>(MaxTrailPoints + 1);
            segments = new List<Vector2>(SegmentCount + 1);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(heading);
            writer.Write(phase);
            writer.Write(timer);
            writer.Write(age);
            writer.Write(coilAngle);
            writer.Write(coilRadiusStart);
            writer.Write(coilRadiusEnd);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            heading = reader.ReadSingle();
            phase = reader.ReadSingle();
            timer = reader.ReadSingle();
            age = reader.ReadSingle();
            coilAngle = reader.ReadSingle();
            coilRadiusStart = reader.ReadSingle();
            coilRadiusEnd = reader.ReadSingle();
        }

        public override bool? CanDamage() => Current == Mode.Release ? false : null;

        private void Sync()
        {
            if (Projectile.owner == Main.myPlayer)
                Projectile.netUpdate = true;
        }

        private static float Thickness(float t)
        {
            if (t < 0.3f)
                return MathHelper.Lerp(1.15f, 1.75f, t / 0.3f);

            return MathHelper.Lerp(1.75f, 0.55f, (t - 0.3f) / 0.7f);
        }

        private void Burst(Vector2 center, int count, float speed)
        {
            if (Main.dedServ)
                return;

            for (int i = 0; i < count; i++)
            {
                Dust d = Dust.NewDustPerfect(center, DustID.GemEmerald,
                    Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(0.4f, 1f) * speed,
                    100, default, Main.rand.NextFloat(1f, 1.5f));
                d.noGravity = true;
            }
        }

        private NPC TrackedTarget()
        {
            int index = (int)Projectile.ai[1] - 1;
            if (index < 0 || index >= Main.maxNPCs)
                return null;

            NPC npc = Main.npc[index];
            return npc.active && npc.CanBeChasedBy(Projectile) ? npc : null;
        }

        private NPC AcquireTarget()
        {
            NPC tracked = TrackedTarget();
            if (tracked != null)
                return tracked;

            if (Projectile.owner != Main.myPlayer || (int)age % 8 != 0)
                return null;

            int best = -1;
            float bestScore = float.MaxValue;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || !npc.CanBeChasedBy(Projectile))
                    continue;

                float toSelf = Vector2.Distance(npc.Center, Projectile.Center);
                if (toSelf > SeekRange)
                    continue;

                float score = toSelf + Vector2.Distance(npc.Center, Main.MouseWorld) * 1.2f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = i;
                }
            }

            if (best < 0)
                return null;

            Projectile.ai[1] = best + 1;
            Sync();
            return Main.npc[best];
        }

        public override void AI()
        {
            if (age == 0f)
            {
                heading = Projectile.velocity.SafeNormalize(Vector2.UnitX).ToRotation();
                phase = Projectile.identity * 1.37f;
                SoundEngine.PlaySound(SoundID.Item20 with { PitchVariance = 0.1f, Pitch = -0.1f, Volume = 0.9f }, Projectile.Center);
            }

            age += 1f;

            if (Projectile.owner == Main.myPlayer && age % 24f == 0f)
                Projectile.netUpdate = true;

            switch (Current)
            {
                case Mode.Slither:
                    UpdateSlither();
                    break;
                case Mode.Strike:
                    UpdateStrike();
                    break;
                case Mode.Coil:
                    UpdateCoil();
                    break;
                case Mode.Release:
                    UpdateRelease();
                    break;
            }

            if (Projectile.velocity.LengthSquared() > 0.04f)
                Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.ToRotation() + MathHelper.PiOver2, 0.45f);

            float fadeIn = MathHelper.Clamp(age / 8f, 0f, 1f);
            float fadeOut = Current == Mode.Release ? MathHelper.Clamp(1f - timer / ReleaseTime, 0f, 1f) : 1f;
            Projectile.Opacity = fadeIn * fadeOut;

            RecordTrail();
            BuildSegments();
            SpawnEffects();
        }

        private void UpdateSlither()
        {
            NPC target = AcquireTarget();
            float forwardSpeed = SlitherSpeed * (0.9f + 0.2f * MathF.Sin(phase * 2f + 1.3f));
            float proximity = 1f;

            if (target != null)
            {
                Vector2 to = target.Center - Projectile.Center;
                float dist = to.Length();
                float closeness = 1f - MathHelper.Clamp(dist / 600f, 0f, 1f);
                heading = heading.AngleTowards(to.ToRotation(), MathHelper.Lerp(0.035f, 0.1f, closeness));
                proximity = MathHelper.Clamp(dist / 320f, 0.3f, 1f);

                if (dist < StrikeRange && Projectile.owner == Main.myPlayer && age > 14f)
                {
                    BeginStrike();
                    return;
                }
            }
            else if (age > SlitherTime)
            {
                BeginRelease();
                return;
            }

            phase += WavePhaseStep;
            float ramp = MathHelper.Clamp(age / 22f, 0.3f, 1f);
            float amplitude = WaveAmplitude * ramp * proximity;
            Vector2 forward = heading.ToRotationVector2();
            Vector2 side = new Vector2(-forward.Y, forward.X);
            float lateral = MathF.Cos(phase) * amplitude * WavePhaseStep * Side;

            Projectile.velocity = forward * forwardSpeed + side * lateral;
        }

        private void BeginStrike()
        {
            Current = Mode.Strike;
            timer = 0f;
            SoundEngine.PlaySound(SoundID.Item20 with { Pitch = 0.6f, Volume = 0.8f }, Projectile.Center);
            Sync();
        }

        private void UpdateStrike()
        {
            NPC target = TrackedTarget();
            if (target == null)
            {
                Current = Mode.Slither;
                timer = 0f;
                Sync();
                return;
            }

            timer++;

            Vector2 to = target.Center - Projectile.Center;
            heading = heading.AngleTowards(to.ToRotation(), timer < WindupTime ? 0.18f : 0.28f);
            Vector2 forward = heading.ToRotationVector2();
            Vector2 side = new Vector2(-forward.Y, forward.X);

            if (timer < WindupTime)
            {
                Vector2 rear = -forward * 2.2f + side * MathF.Sin(timer * 0.8f) * 2.5f * Side;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, rear, 0.35f);
                return;
            }

            if (timer == WindupTime)
            {
                Burst(Projectile.Center, 8, 3f);
                SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.8f, Pitch = -0.5f }, Projectile.Center);
            }

            float t = MathHelper.Clamp((timer - WindupTime) / 7f, 0f, 1f);
            float speed = MathHelper.Lerp(SlitherSpeed, StrikeSpeed, t);
            Projectile.velocity = forward * speed + side * MathF.Sin(timer * 0.7f) * 1.6f * (1f - t);

            if (timer > WindupTime + StrikeTime)
            {
                Current = Mode.Slither;
                timer = 0f;
                Sync();
            }
        }

        private void BeginCoil(NPC target)
        {
            Current = Mode.Coil;
            timer = 0f;
            Projectile.ai[1] = target.whoAmI + 1;

            Vector2 offset = Projectile.Center - target.Center;
            if (offset.LengthSquared() < 1f)
                offset = Main.rand.NextVector2CircularEdge(1f, 1f);

            coilAngle = offset.ToRotation();
            coilRadiusStart = Math.Max(offset.Length(), 14f);
            coilRadiusEnd = MathHelper.Clamp((target.width + target.height) * 0.25f * 0.9f + 18f, 30f, 140f);

            Burst(Projectile.Center, 12, 4f);
            SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.6f, Pitch = -0.4f }, Projectile.Center);

            if (Projectile.owner == Main.myPlayer)
                Main.player[Projectile.owner].GetModPlayer<ScreenShakePlayer>().TriggerShake(6, 0.5f);

            Sync();
        }

        private void UpdateCoil()
        {
            NPC target = TrackedTarget();
            if (target == null)
            {
                BeginRelease();
                return;
            }

            timer++;

            float open = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(timer / 18f, 0f, 1f));
            float settle = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((timer - 18f) / 60f, 0f, 1f));
            float bulge = coilRadiusEnd + 30f;
            float radius = timer < 18f
                ? MathHelper.Lerp(coilRadiusStart, bulge, open)
                : MathHelper.Lerp(bulge, coilRadiusEnd, settle);
            radius += MathF.Sin(timer * 0.35f) * 2.5f * settle;

            coilAngle += (0.14f + 0.06f * settle) * Side;
            heading = coilAngle + MathHelper.PiOver2 * Side;

            Vector2 desired = target.Center + coilAngle.ToRotationVector2() * radius;
            Vector2 move = desired - Projectile.Center;
            float length = move.Length();
            if (length > 34f)
                move *= 34f / length;

            Projectile.velocity = move;

            if (timer >= CoilTime)
                BeginRelease();
        }

        private void BeginRelease()
        {
            Vector2 dir = Projectile.velocity.SafeNormalize(heading.ToRotationVector2());
            Projectile.velocity = dir * 11f;
            Current = Mode.Release;
            timer = 0f;
            Burst(Projectile.Center, 10, 3.5f);
            Sync();
        }

        private void UpdateRelease()
        {
            timer++;
            Projectile.velocity *= 0.965f;

            if (timer >= ReleaseTime)
                Projectile.Kill();
        }

        private void RecordTrail()
        {
            if (trail.Count > 0 && Vector2.DistanceSquared(trail[0], Projectile.Center) <= 2.25f)
                return;

            trail.Insert(0, Projectile.Center);
            if (trail.Count > MaxTrailPoints)
                trail.RemoveAt(trail.Count - 1);
        }

        private void BuildSegments()
        {
            segments.Clear();

            Vector2 prev = Projectile.Center;
            float carried = 0f;

            for (int i = 0; i < trail.Count && segments.Count < SegmentCount; i++)
            {
                Vector2 cur = trail[i];
                Vector2 delta = cur - prev;
                float length = delta.Length();
                if (length < 0.001f)
                    continue;

                Vector2 dir = delta / length;
                float walked = 0f;

                while (segments.Count < SegmentCount && length - walked >= SegmentSpacing - carried)
                {
                    walked += SegmentSpacing - carried;
                    carried = 0f;
                    segments.Add(prev + dir * walked);
                }

                carried += length - walked;
                prev = cur;
            }
        }

        private void SpawnEffects()
        {
            if (Main.dedServ)
                return;

            Lighting.AddLight(Projectile.Center, 0.12f, 0.5f * Projectile.Opacity, 0.22f);

            if (Projectile.Opacity > 0.4f && Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), DustID.GemEmerald,
                    -Projectile.velocity * 0.1f, 100, default, 1.1f);
                d.noGravity = true;
            }

            if (segments.Count > 2 && Projectile.Opacity > 0.4f && Main.rand.NextBool(Current == Mode.Coil ? 2 : 6))
            {
                Vector2 p = segments[Main.rand.Next(segments.Count)];
                Dust d = Dust.NewDustPerfect(p + Main.rand.NextVector2Circular(6f, 6f), DustID.GemEmerald,
                    Main.rand.NextVector2Circular(1f, 1f), 120, default, 0.9f);
                d.noGravity = true;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            headContact = projHitbox.Intersects(targetHitbox);
            if (headContact)
                return true;

            for (int i = 0; i < segments.Count; i++)
            {
                int r = (int)(7f * Thickness(i / (float)(SegmentCount - 1)));
                Vector2 p = segments[i];
                Rectangle box = new Rectangle((int)p.X - r, (int)p.Y - r, r * 2, r * 2);
                if (box.Intersects(targetHitbox))
                    return true;
            }

            return false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Current == Mode.Coil)
            {
                modifiers.SourceDamage *= CoilDamageMul;
                modifiers.Knockback *= 0.1f;
                return;
            }

            if (headContact)
            {
                modifiers.SourceDamage *= StrikeDamageMul;
                modifiers.Knockback *= 1.4f;
            }
            else
            {
                modifiers.SourceDamage *= BodyDamageMul;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!headContact || (Current != Mode.Slither && Current != Mode.Strike))
                return;

            target.AddBuff(BuffID.Poisoned, 240);

            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.GemEmerald, 0f, 0f, 120, default, 1.2f);
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
                d.noGravity = true;
            }

            BeginCoil(target);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (headTexture == null || bodyTexture == null || tailTexture == null)
                return false;

            BuildSegments();

            Texture2D head = headTexture.Value;
            Texture2D body = bodyTexture.Value;
            Texture2D tail = tailTexture.Value;

            float opacity = Projectile.Opacity;
            float time = Main.GlobalTimeWrappedHourly;

            for (int i = segments.Count - 1; i >= 0; i--)
            {
                Vector2 ahead = i == 0 ? Projectile.Center : segments[i - 1];
                Vector2 behind = i + 1 < segments.Count ? segments[i + 1] : segments[i];
                Vector2 tangent = ahead - behind;
                if (tangent.LengthSquared() < 0.01f)
                    tangent = Projectile.velocity.SafeNormalize(Vector2.UnitY);

                float rotation = tangent.ToRotation() + MathHelper.PiOver2;
                Texture2D tex = i == segments.Count - 1 ? tail : body;
                float scale = Thickness(i / (float)(SegmentCount - 1)) * Projectile.scale;

                Color color = Lighting.GetColor(segments[i].ToTileCoordinates());
                float lum = (color.R + color.G + color.B) / 765f;
                float shimmer = 0.5f + 0.5f * MathF.Sin(time * 7f - i * 0.55f);
                color = Color.Lerp(color, new Color(140, 255, 170), shimmer * 0.2f * lum);

                SpriteEffects effects = tangent.X < 0f ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Main.EntitySpriteDraw(tex, segments[i] - Main.screenPosition, null, color * opacity, rotation, tex.Size() * 0.5f, scale, effects, 0);
            }

            Vector2 headPos = Projectile.Center - Main.screenPosition;
            Color headColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates());
            float facing = MathF.Cos(Projectile.rotation - MathHelper.PiOver2);
            SpriteEffects headEffects = facing < 0f ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float glowPulse = 0.5f + 0.5f * MathF.Sin(time * 9f);
            float headScale = HeadScale * Projectile.scale;

            Main.EntitySpriteDraw(head, headPos, null, new Color(70, 220, 120, 0) * (0.35f + 0.25f * glowPulse) * opacity,
                Projectile.rotation, head.Size() * 0.5f, headScale * 1.25f, headEffects, 0);
            Main.EntitySpriteDraw(head, headPos, null, headColor * opacity,
                Projectile.rotation, head.Size() * 0.5f, headScale, headEffects, 0);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ)
                return;

            for (int i = 0; i < segments.Count; i++)
            {
                if (!Main.rand.NextBool(2))
                    continue;

                Dust d = Dust.NewDustPerfect(segments[i], DustID.GemEmerald, Main.rand.NextVector2Circular(2.5f, 2.5f), 100, default, 1.2f);
                d.noGravity = true;
            }

            SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.15f, Pitch = -0.3f }, Projectile.Center);
        }
    }
}