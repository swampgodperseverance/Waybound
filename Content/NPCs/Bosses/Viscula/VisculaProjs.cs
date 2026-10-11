using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Helpers;

namespace Waybound.Content.NPCs.Bosses.Viscula
{
    //so small explanation about viscula's projectiles
    public abstract class HeartP : ModProjectile
    {
        public virtual Color Tint => Heartfx.Purple;

        protected int Age => Projectile.frameCounter;

        public override bool PreAI()
        {
            int idx = NPC.FindFirstNPC(ModContent.NPCType<Viscula>());
            if (idx < 0)
            {
                Projectile.Kill();
                return false;
            }

            NPC boss = Main.npc[idx];
            int state = (int)boss.ai[0];
            bool transition = state == VisculaState.Trans12 || state == VisculaState.Trans23;

            if (state == VisculaState.Dormant || state == VisculaState.Dying || (transition && boss.ai[2] < 8f))
            {
                Projectile.Kill();
                return false;
            }

            Projectile.frameCounter++;
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            Heartfx.Burst(Projectile.Center, 4, 2.5f, 12f, Tint);
        }
    }
    //should I say anything
    public class VisculaBolt : HeartP
    {
        public override string Texture => HeartTextures.Bolt;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 420;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 1f)
            {
                float max = Projectile.ai[2] > 0f ? Projectile.ai[2] : 14f;
                if (Projectile.velocity.Length() < max)
                    Projectile.velocity *= Projectile.ai[1] > 1f ? Projectile.ai[1] : 1.03f;
            }

            Projectile.rotation += 0.15f;
            Lighting.AddLight(Projectile.Center, Tint.ToVector3() * 0.3f);
        }
        //I tried to make it like more visible but still it is what it is
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            float appear = MathHelper.Clamp(Age / 8f, 0.35f, 1f);
            float scale = Projectile.scale * appear;

            Heartfx.DrawTrail(Projectile, tex, Tint, 0.28f, 0f, false);
            Heartfx.DrawSoft(Projectile.Center, Heartfx.Alpha0(Tint) * (0.6f * appear), 44f * scale);
            Heartfx.DrawGlow(tex, Projectile.Center, null, Projectile.rotation, origin, scale, Tint, appear);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, scale, SpriteEffects.None, 0);
            return false;
        }
    }

    public class VisculaOrb : HeartP
    {
        public override string Texture => HeartTextures.Bolt;

        public override Color Tint => Heartfx.Violet;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 480;
            Projectile.aiStyle = -1;
        }

        public override bool CanHitPlayer(Player target) => Age >= 8;

        public override void AI()
        {
            Projectile.rotation += 0.08f;
            Lighting.AddLight(Projectile.Center, Tint.ToVector3() * 0.45f);

            if (Main.rand.NextBool(6))
                Heartfx.Spark(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), Projectile.velocity * -0.1f, 9f, Tint, 16);
        }

        public override void OnKill(int timeLeft)
        {
            Heartfx.Burst(Projectile.Center, 8, 3.5f, 14f, Tint);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            float appear = MathHelper.Clamp(Age / 12f, 0.25f, 1f);
            float pulse = 1f + 0.08f * (float)Math.Sin(Age * 0.25f);
            float scale = 1.7f * appear * pulse;

            Heartfx.DrawTrail(Projectile, tex, Tint, 0.3f, 0f, false);
            Heartfx.DrawSoft(Projectile.Center, Heartfx.Alpha0(Tint) * (0.65f * appear), 40f * scale);
            Heartfx.DrawGlow(tex, Projectile.Center, null, Projectile.rotation, origin, scale, Tint, appear, 0.45f, 0.2f, 2f);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, scale, SpriteEffects.None, 0);
            return false;
        }
    }
    //Diamond because it should've been a projectile which comes out from the ground but i decided to make it boring instead lmfao
    public class VisculaDiamond : HeartP
    {
        public override string Texture => HeartTextures.Diamond;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 480;
            Projectile.aiStyle = -1;
        }

        private int Mode => (int)Projectile.ai[0];
        private bool Hanging => Mode == 1 && Projectile.localAI[0] < Projectile.ai[1];
        private bool Fallen => Mode == 2 && Projectile.localAI[2] == 1f;
        //Only one arguably interesting thing is that it can fall  

        public override bool CanHitPlayer(Player target)
        {
            if (Hanging)
                return false;

            return Mode != 2 || Age >= 14;
        }

        public override void AI()
        {
            if (Mode == 0)
            {
                Accelerate(1.04f, 15f);
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
            else if (Mode == 2)
            {
                if (!Fallen)
                {
                    Accelerate(1.04f, 10f);
                    Projectile.rotation = Projectile.velocity.ToRotation();
                }
                else
                {
                    Projectile.velocity.X *= 0.99f;
                    Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.4f, 14f);
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, MathHelper.PiOver2, 0.15f);
                }
            }
            else
            {
                Projectile.rotation = MathHelper.PiOver2;

                if (Hanging)
                {
                    Projectile.localAI[0]++;
                    Projectile.velocity = Vector2.Zero;

                    if (Main.rand.NextBool(3))
                        Heartfx.Spark(Projectile.Center + new Vector2(Main.rand.NextFloat(-8f, 8f), 6f), new Vector2(0f, Main.rand.NextFloat(0.5f, 2f)), 8f, Tint, 14);
                }
                else
                {
                    Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.45f, 17f);
                }
            }

            Lighting.AddLight(Projectile.Center, Tint.ToVector3() * 0.35f);
        }

        private void Accelerate(float defaultMult, float defaultMax)
        {
            float max = Projectile.ai[2] > 0f ? Projectile.ai[2] : defaultMax;
            if (Projectile.velocity.Length() < max)
                Projectile.velocity *= Projectile.ai[1] > 1f ? Projectile.ai[1] : defaultMult;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Mode == 2 && !Fallen)
            {
                Projectile.localAI[2] = 1f;
                Projectile.velocity = new Vector2(oldVelocity.X * 0.12f, 1.5f);
                Heartfx.Burst(Projectile.Center, 5, 2.5f, 11f, Tint);
                SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.3f, Pitch = -0.1f }, Projectile.Center);
                return false;
            }

            return true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            float offset = HeartTextures.SpriteForward;
            float shake = Hanging ? (float)Math.Sin(Projectile.localAI[0] * 1.9f) * 1.6f : 0f;
            float appear = Mode == 2 ? MathHelper.Clamp(Age / 14f, 0f, 1f) : 1f;
            float scale = Projectile.scale * (0.8f + 0.2f * appear);

            if (!Hanging)
                Heartfx.DrawTrail(Projectile, tex, Tint, 0.28f * appear, offset, true);

            Vector2 world = Projectile.Center + new Vector2(shake, 0f);
            Heartfx.DrawGlow(tex, world, null, Projectile.rotation + offset, origin, scale, Tint, appear);
            Main.EntitySpriteDraw(tex, world - Main.screenPosition, null, Color.White * appear, Projectile.rotation + offset, origin, scale, SpriteEffects.None, 0);
            return false;
        }
    }
    //This one ill skip
    public class VisculaThorn : HeartP
    {
        public override string Texture => HeartTextures.Spike;

        public override Color Tint => new Color(150, 58, 225);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360;
            Projectile.aiStyle = -1;
        }

        private float Hold => Projectile.ai[0];
        private bool Holding => Age < Hold;

        public override bool CanHitPlayer(Player target) => !Holding && Age > 3;

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                Projectile.localAI[1] = Projectile.velocity.ToRotation();
                Projectile.localAI[2] = Math.Max(Projectile.velocity.Length(), 2f);
            }

            if (Holding)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation = Projectile.localAI[1];

                if (Main.rand.NextBool(5))
                    Heartfx.Spark(Projectile.Center + Main.rand.NextVector2Circular(14f, 14f), Vector2.Zero, 8f, Tint, 14);
            }
            else
            {
                if (Hold > 0f && Age == (int)Hold)
                    Projectile.velocity = Projectile.localAI[1].ToRotationVector2() * Projectile.localAI[2];

                float max = Projectile.ai[2] > 0f ? Projectile.ai[2] : 17f;
                if (Projectile.velocity.Length() < max)
                    Projectile.velocity *= Projectile.ai[1] > 1f ? Projectile.ai[1] : 1.05f;

                Projectile.rotation = Projectile.velocity.ToRotation();
            }

            Lighting.AddLight(Projectile.Center, Tint.ToVector3() * 0.35f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            float len = Math.Max(tex.Width, tex.Height) * 0.9f;
            Vector2 dir = Projectile.rotation.ToRotationVector2();
            float point = 0f;

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center - dir * len * 0.5f, Projectile.Center + dir * len * 0.5f, 12f, ref point);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            float offset = HeartTextures.SpriteForward;
            float appear = MathHelper.Clamp(Age / (Hold > 0f ? Hold : 6f), 0f, 1f);
            float scale = Projectile.scale * (0.85f + 0.15f * appear);

            if (Holding)
                HeartMisc.DrawRay(Projectile.Center, Projectile.rotation, 80f * appear, 3f, Heartfx.Alpha0(Tint) * (0.25f * appear));
            else
                Heartfx.DrawTrail(Projectile, tex, Tint, 0.28f, offset, true);

            Heartfx.DrawGlow(tex, Projectile.Center, null, Projectile.rotation + offset, origin, scale, Tint, appear);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.White * appear, Projectile.rotation + offset, origin, scale, SpriteEffects.None, 0);
            return false;
        }
    }
    //This one shouldve been diamond at first, spike that can touch you from the ground (Redemption Thorn reference)
    public class VisculaFloorSpike : HeartP
    {
        public override string Texture => HeartTextures.Spike;

        public override Color Tint => new Color(135, 48, 205);

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 200;
            Projectile.aiStyle = -1;
        }

        private float SpikeLength
        {
            get
            {
                if (Projectile.ai[1] > 0f)
                    return Projectile.ai[1];

                return TextureAssets.Projectile[Type].Value.Height * 0.4f;
            }
        }

        private float Reveal
        {
            get
            {
                float t = Projectile.localAI[0] - Projectile.ai[0];
                if (t < 0f)
                    return 0f;
                if (t < 10f)
                    return EaseFunctions.EaseOutCubic(t / 10f);
                if (t < 28f)
                    return 1f;
                if (t < 52f)
                    return 1f - EaseFunctions.EaseInOutQuad((t - 28f) / 24f);
                return 0f;
            }
        }

        public override bool CanHitPlayer(Player target) => Reveal > 0.3f;

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.timeLeft = (int)Projectile.ai[0] + 56;
                Projectile.velocity = Vector2.Zero;
            }

            Projectile.localAI[0]++;
            float t = Projectile.localAI[0] - Projectile.ai[0];

            if (t < 0f && Main.rand.NextBool(4))
                Heartfx.Spark(Projectile.Center + new Vector2(Main.rand.NextFloat(-10f, 10f), -2f), new Vector2(0f, -Main.rand.NextFloat(0.5f, 1.8f)), 8f, Tint, 16);

            if ((int)t == 1)
                Heartfx.Burst(Projectile.Center, 6, 3f, 12f, Tint);

            Lighting.AddLight(Projectile.Center, Tint.ToVector3() * (0.2f + Reveal * 0.35f));
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float len = SpikeLength * Reveal;
            if (len < 4f)
                return false;

            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center, Projectile.Center + new Vector2(0f, -len), 16f, ref point);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float age = Projectile.localAI[0];
            float reveal = Reveal;
            Vector2 baseWorld = Projectile.Center;

            if (age < Projectile.ai[0])
            {
                float k = age / Math.Max(Projectile.ai[0], 1f);
                float flick = 0.7f + 0.3f * (float)Math.Sin(age * 0.6f);
                HeartMisc.DrawRay(baseWorld, -MathHelper.PiOver2, 34f * k, 5f + 4f * k, Heartfx.Alpha0(Tint) * (0.35f * k * flick));
                HeartMisc.DrawRay(baseWorld + new Vector2(-14f * (0.4f + 0.6f * k), 0f), 0f, 28f * (0.4f + 0.6f * k), 3f, Heartfx.Alpha0(Tint) * (0.5f * k * flick));
            }

            if (reveal <= 0.01f)
                return false;

            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int visibleH = Math.Max((int)(tex.Height * reveal), 1);
            Rectangle src = new Rectangle(0, 0, tex.Width, visibleH);
            Vector2 origin = new Vector2(tex.Width / 2f, visibleH);
            float scale = SpikeLength / tex.Height;

            Heartfx.DrawGlow(tex, baseWorld, src, 0f, origin, scale, Tint, reveal);
            Main.EntitySpriteDraw(tex, baseWorld - Main.screenPosition, src, Color.White, 0f, origin, scale, SpriteEffects.None, 0);
            return false;
        }
    }
    //This one is pretty similar to plantera's wheels, they could be collided btw as i did in ThemisSickle
    public class VisculaWheel : HeartP
    {
        public override string Texture => HeartTextures.Wheel;

        public override Color Tint => new Color(150, 58, 225);

        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 46;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 480;
            Projectile.aiStyle = -1;
        }

        public override bool CanHitPlayer(Player target) => Projectile.localAI[0] >= 14f;

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
                Projectile.localAI[1] = Math.Max(Projectile.velocity.Length(), 4f);

            Projectile.localAI[0]++;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * Projectile.localAI[1];
            Projectile.rotation += 0.22f * (Projectile.velocity.X >= 0f ? 1f : -1f);

            if (Main.netMode != NetmodeID.MultiplayerClient && Projectile.localAI[0] >= 25f)
                CollideWithWheels();

            if (Main.rand.NextBool(6))
                Heartfx.Spark(Projectile.Center + Main.rand.NextVector2CircularEdge(20f, 20f), Vector2.Zero, 9f, Tint, 14);

            Lighting.AddLight(Projectile.Center, Tint.ToVector3() * 0.45f);
        }

        private void CollideWithWheels()
        {
            const float diameter = 44f;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (!other.active || other.type != Projectile.type || other.whoAmI <= Projectile.whoAmI || other.localAI[0] < 25f)
                    continue;

                Vector2 delta = other.Center - Projectile.Center;
                float dist = delta.Length();
                if (dist >= diameter || dist < 0.01f)
                    continue;

                Vector2 n = delta / dist;
                float va = Vector2.Dot(Projectile.velocity, n);
                float vb = Vector2.Dot(other.velocity, n);

                if (va - vb > 0f)
                {
                    Projectile.velocity += n * (vb - va);
                    other.velocity += n * (va - vb);
                    Projectile.netUpdate = true;
                    other.netUpdate = true;

                    Heartfx.Burst(Projectile.Center + delta * 0.5f, 8, 4f, 13f, Tint);
                    SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.45f, Pitch = 0.1f }, Projectile.Center);
                }

                float push = (diameter - dist) * 0.5f;
                Projectile.position -= n * push;
                other.position += n * push;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X;
            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y;

            Heartfx.Burst(Projectile.Center, 5, 3f, 12f, Tint);
            SoundEngine.PlaySound(SoundID.Item70 with { Volume = 0.35f, Pitch = -0.2f }, Projectile.Center);
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            float scale = MathHelper.Clamp(Projectile.localAI[0] / 20f, 0.15f, 1f);
            float fade = MathHelper.Clamp(Projectile.timeLeft / 20f, 0f, 1f);

            Heartfx.DrawGlow(tex, Projectile.Center, null, -Projectile.rotation * 0.6f, origin, scale * 1.08f, Tint, fade, 0.2f, 0.16f);
            Heartfx.DrawGlow(tex, Projectile.Center, null, Projectile.rotation, origin, scale, Tint, fade, 0.4f, 0f, 2f);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.Lerp(lightColor, Color.White, 0.7f) * fade, Projectile.rotation, origin, scale, SpriteEffects.None, 0);
            return false;
        }
    }
    //The hellheart monolith and RoA codes have studied me how to use rays properly, serork is the goat
    public class VisculaRay : HeartP
    {
        public override Color Tint => new Color(220, 40, 50);
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.aiStyle = -1;
        }

        public override void OnKill(int timeLeft)
        {
        }

        private float Warn => Projectile.ai[1];
        private float BeamTime => Projectile.ai[2];
        private bool BeamActive => BeamTime > 0f && Projectile.localAI[0] >= Warn && Projectile.localAI[0] < Warn + BeamTime;
        private float BeamGrow => EaseFunctions.EaseOutCubic(MathHelper.Clamp((Projectile.localAI[0] - Warn) / 8f, 0f, 1f));
        private float BeamWidth => 30f * BeamGrow;

        public override bool CanHitPlayer(Player target) => BeamActive;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (!BeamActive)
                return false;

            Vector2 dir = Projectile.rotation.ToRotationVector2();
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center, Projectile.Center + dir * Projectile.localAI[1], Math.Max(BeamWidth * 0.7f, 4f), ref point);
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
                Projectile.velocity = Vector2.Zero;
                Projectile.timeLeft = (int)(Warn + BeamTime) + 2;
            }

            Projectile.localAI[0]++;
            Projectile.rotation += Projectile.ai[0];

            Vector2 dir = Projectile.rotation.ToRotationVector2();
            Projectile.localAI[1] = HeartMisc.Raycast(Projectile.Center, dir, 2200f, out _);

            if (BeamActive)
            {
                Lighting.AddLight(Projectile.Center + dir * Projectile.localAI[1], Tint.ToVector3() * 0.7f);

                if (Main.rand.NextBool(2))
                {
                    Vector2 end = Projectile.Center + dir * Projectile.localAI[1];
                    Heartfx.Spark(end, dir.RotatedByRandom(2.4f) * -Main.rand.NextFloat(1.5f, 5f), 12f, Tint, 18);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float age = Projectile.localAI[0];
            float len = Projectile.localAI[1];
            float rot = Projectile.rotation;
            Vector2 start = Projectile.Center;

            if (age < Warn)
            {
                float k = age / Math.Max(Warn, 1f);
                float flick = 0.75f + 0.25f * (float)Math.Sin(age * 0.8f);
                HeartMisc.DrawRay(start, rot, len, 6f + 5f * k, Heartfx.Alpha0(Tint) * (0.16f * k * flick));
                HeartMisc.DrawRay(start, rot, len, 2.2f, Heartfx.Alpha0(Heartfx.Violet) * ((0.18f + 0.45f * k) * flick));
            }
            else if (BeamActive)
            {
                float t = age - Warn;
                float fadeOut = MathHelper.Clamp((BeamTime - t) / 12f, 0f, 1f);
                float w = BeamWidth;
                float flick = 1f + 0.08f * (float)Math.Sin(age * 1.7f);

                HeartMisc.DrawRay(start, rot, len, w * 2.0f * flick, Heartfx.Alpha0(Tint) * (0.22f * fadeOut));
                HeartMisc.DrawRay(start, rot, len, w * 1.25f * flick, Heartfx.Alpha0(Tint) * (0.55f * fadeOut));
                HeartMisc.DrawRay(start, rot, len, w * 0.6f, Heartfx.Alpha0(Heartfx.Violet) * (0.9f * fadeOut));
                HeartMisc.DrawRay(start, rot, len, w * 0.22f, new Color(240, 225, 255, 0) * fadeOut);
            }

            return false;
        }
    }
    //RoA again lmfao
    public class VisculaSlash : HeartP
    {
        public override Color Tint => Heartfx.Blood;
        public override string Texture => "Terraria/Images/Projectile_0";

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 200;
            Projectile.aiStyle = -1;
        }

        private float Hold => Projectile.ai[0];
        private bool Holding => Age < Hold;

        private float BladeLength
        {
            get
            {
                float grow = MathHelper.Clamp((Age - Hold) / 10f, 0f, 1f);
                return (50f + Projectile.velocity.Length() * 9f) * grow;
            }
        }

        public override bool CanHitPlayer(Player target) => !Holding;

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                Projectile.localAI[1] = Projectile.velocity.ToRotation();
                Projectile.localAI[2] = Math.Max(Projectile.velocity.Length(), 2f);
                Projectile.timeLeft = (int)Hold + 120;
            }

            if (Holding)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation = Projectile.localAI[1];

                if (Main.rand.NextBool(3))
                {
                    Vector2 dir = Projectile.localAI[1].ToRotationVector2();
                    Heartfx.Spark(Projectile.Center + dir * Main.rand.NextFloat(0f, 260f), Vector2.Zero, 7f, Tint, 12);
                }
            }
            else
            {
                if (Age == (int)Hold)
                {
                    Projectile.velocity = Projectile.localAI[1].ToRotationVector2() * Projectile.localAI[2];
                    SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.35f, Pitch = 0.2f }, Projectile.Center);
                }

                float max = Projectile.ai[2] > 0f ? Projectile.ai[2] : 13f;
                if (Projectile.velocity.Length() < max)
                    Projectile.velocity *= Projectile.ai[1] > 1f ? Projectile.ai[1] : 1.04f;

                Projectile.rotation = Projectile.velocity.ToRotation();

                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Blood, Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.2f, 1.2f), 100, default, 1.2f);
                    d.noGravity = true;
                }
            }

            Lighting.AddLight(Projectile.Center, Tint.ToVector3() * 0.5f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Holding)
                return false;

            Vector2 dir = Projectile.rotation.ToRotationVector2();
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Projectile.Center - dir * BladeLength * 0.3f, Projectile.Center, 18f, ref point);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float rot = Projectile.rotation;
            Vector2 head = Projectile.Center;

            if (Holding)
            {
                float k = MathHelper.Clamp(Age / Math.Max(Hold, 1f), 0f, 1f);
                float flick = 0.8f + 0.2f * (float)Math.Sin(Age * 0.9f);
                float boost = Hold - Age < 10f ? 1.7f : 1f;

                HeartMisc.DrawRay(head, rot, 1100f, 26f * k, Heartfx.Alpha0(Tint) * (0.16f * k * flick * boost));
                HeartMisc.DrawRay(head, rot, 1100f, 3f, Heartfx.Alpha0(Tint) * (0.55f * k * flick * boost));
                HeartMisc.DrawRay(head, rot, 1100f, 1.2f, new Color(255, 215, 230, 0) * (0.6f * k * boost));
                Heartfx.DrawSoft(head, Heartfx.Alpha0(Tint) * (0.8f * k), 40f + 40f * k);
                Heartfx.Star(head, Age * 0.2f, new Vector2(0.3f + 0.5f * k), Heartfx.Alpha0(Tint) * (0.8f * k));
                return false;
            }

            float len = BladeLength;
            float fade = MathHelper.Clamp(Projectile.timeLeft / 15f, 0f, 1f);
            if (len < 2f)
                return false;

            float[] lengths = { 1f, 0.78f, 0.58f, 0.4f, 0.24f };
            float[] widths = { 12f, 17f, 22f, 26f, 28f };

            for (int i = 0; i < lengths.Length; i++)
            {
                float l = len * lengths[i];
                HeartMisc.DrawRay(head - rot.ToRotationVector2() * l, rot, l, widths[i], Heartfx.Alpha0(Tint) * (0.22f * fade));
            }

            HeartMisc.DrawRay(head - rot.ToRotationVector2() * len * 0.6f, rot, len * 0.6f, 5f, new Color(255, 205, 225, 0) * (0.85f * fade));
            Heartfx.DrawSoft(head, Heartfx.Alpha0(Tint) * (0.75f * fade), 52f);
            Heartfx.Star(head, rot, new Vector2(1.1f, 0.3f), new Color(255, 225, 238, 0) * fade);
            return false;
        }
    }
}