using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.NPCs.Bosses.Viscula;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{    //gonna balance later
    public class IschemicStiletto : BaseRapierItem
    {
        protected override int HoldoutType => ModContent.ProjectileType<IschemicStilettoHoldout>();

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        protected override void SetRapierDefaults()
        {
            Item.damage = 24;
            Item.knockBack = 3f;
            Item.value = Item.buyPrice(0, 1, 50, 0);
            Item.rare = ItemRarityID.Green;
        }

        public override void PostUpdate()
        {
            float pulse = 0.75f + 0.25f * MathF.Sin(Main.GlobalTimeWrappedHourly * 4f + Item.whoAmI);
            Lighting.AddLight(Item.Center, new Vector3(0.9f, 0.18f, 0.35f) * 0.35f * pulse);
        }
    }

    public class IschemicStilettoHoldout : BaseRapierHoldout
    {
        private static readonly Color Blood = new Color(235, 45, 90, 0);

        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/IschemicStiletto";

        protected override Color GetLungeParticleColor() => new Color(210, 55, 95, 255);
        protected override Color GetTipParticleColor() => new Color(175, 40, 80, 255);
        protected override Color GetTrailParticleColor() => new Color(190, 50, 105, 200);
        protected override Color GetGlowColor() => new Color(225, 70, 120, 180);
        protected override Color GetGreenPulseColor() => new Color(175, 105, 245, 230);

        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<IschemicStiletto>();

        protected override void OnEmpoweredStrike(Player player)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f, Pitch = -0.3f }, Tip);

            for (int i = 0; i < 24; i++)
            {
                Vector2 vel = Projectile.velocity.RotatedByRandom(0.7f) * Main.rand.NextFloat(4f, 13f);
                Spark(Tip, vel, Main.rand.NextBool(3) ? GetGreenPulseColor() : new Color(235, 45, 90, 255), Main.rand.NextFloat(10f, 18f), Main.rand.Next(18, 32));
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);

            target.AddBuff(BuffID.Bleeding, empowered ? 420 : 180);

            for (int i = 0; i < (empowered ? 10 : 4); i++)
            {
                Vector2 pos = target.Center + Main.rand.NextVector2Circular(target.width * 0.3f, target.height * 0.3f);
                Vector2 vel = Projectile.velocity.RotatedByRandom(0.8f) * Main.rand.NextFloat(2f, 6f);
                Spark(pos, vel, Main.rand.NextBool(3) ? GetGreenPulseColor() : new Color(235, 45, 90, 255), Main.rand.NextFloat(7f, 13f), Main.rand.Next(14, 24));
            }

            if (!empowered)
                return;

            Player player = Main.player[Projectile.owner];

            if (target.boss)
            {
                int amount = Math.Max(8, Projectile.damage / 4);
                if (player.statLife < player.statLifeMax2)
                    player.Heal(amount);

                target.AddBuff(BuffID.Bleeding, 300);
                SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.6f, Pitch = -0.3f }, target.Center);

                for (int i = 0; i < 14; i++)
                    Spark(target.Center + Main.rand.NextVector2Circular(target.width * 0.4f, target.height * 0.4f),
                        Main.rand.NextVector2Circular(5f, 5f), GetGreenPulseColor(), Main.rand.NextFloat(8f, 14f), 20);
                return;
            }

            int type = ModContent.ProjectileType<IschemicShackle>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.type == type && p.owner == Projectile.owner && (int)p.ai[0] == target.whoAmI)
                    return;
            }

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, type,
                (int)(Projectile.damage * 1.8f), 0f, Projectile.owner, target.whoAmI, 0f);
        }

        public override void AI()
        {
            base.AI();

            if (!Projectile.active)
                return;

            Vector3 light = new Vector3(0.9f, 0.18f, 0.35f) * (Attacking ? 0.6f : 0.25f);
            Lighting.AddLight(Projectile.Center, light);
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D star = TextureAssets.Extra[98].Value;
            Vector2 hilt = Projectile.Center - Projectile.velocity * BladeLength * 0.5f - Main.screenPosition;
            float time = Main.GlobalTimeWrappedHourly;
            float beat = MathF.Pow(MathF.Max(0f, MathF.Sin(time * 7f)), 6f);
            float boost = Attacking ? 1.6f : 1f;
            float a = (0.25f + 0.55f * beat) * boost * fadeIn;

            Main.EntitySpriteDraw(star, hilt, null, Blood * a, 0f, star.Size() / 2f, new Vector2(0.22f + 0.2f * beat), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(star, hilt, null, Blood * (a * 0.5f), MathHelper.PiOver2, star.Size() / 2f, new Vector2(0.14f + 0.12f * beat), SpriteEffects.None, 0);
        }
    }

    public class IschemicShackle : ModProjectile
    {
        private const int ChainCount = 8;
        private const int BindTime = 46;
        private const int DamageFrames = 6;
        private const int BlastLife = 28;
        private const float BlastRadius = 105f;
        private bool locked;
        private bool exploded;
        private Vector2 lockPos;
        private static readonly Color Blood = new Color(235, 45, 90);
        private static readonly Color Violet = new Color(175, 105, 245);
        private static Texture2D softCircle;
        private static Texture2D linkTex;

        private float lastRadius = 30f;

        public override string Texture => "Terraria/Images/Projectile_0";

        private bool Blasting => Projectile.ai[1] >= BindTime;

        private static Texture2D Link
        {
            get
            {
                if (linkTex == null || linkTex.IsDisposed)
                {
                    const int w = 12;
                    const int h = 6;
                    linkTex = new Texture2D(Main.instance.GraphicsDevice, w, h);
                    Color[] data = new Color[w * h];
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            float nx = (x - (w - 1) / 2f) / (w / 2f);
                            float ny = (y - (h - 1) / 2f) / (h / 2f);
                            float d = MathF.Sqrt(nx * nx + ny * ny);
                            data[y * w + x] = d <= 1f && d >= 0.55f ? new Color(205, 195, 215, 255) : Color.Transparent;
                        }
                    }
                    linkTex.SetData(data);
                }
                return linkTex;
            }
        }

        private static Texture2D SoftCircle
        {
            get
            {
                if (softCircle == null || softCircle.IsDisposed)
                {
                    const int size = 128;
                    softCircle = new Texture2D(Main.instance.GraphicsDevice, size, size);
                    Color[] data = new Color[size * size];
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dx = (x - 63.5f) / 63.5f;
                            float dy = (y - 63.5f) / 63.5f;
                            float d = MathF.Sqrt(dx * dx + dy * dy);
                            float a = MathF.Pow(MathHelper.Clamp(1f - d, 0f, 1f), 1.4f);
                            data[y * size + x] = new Color(a, a, a, a);
                        }
                    }
                    softCircle.SetData(data);
                }
                return softCircle;
            }
        }

        public override void Unload()
        {
            Texture2D soft = softCircle;
            Texture2D link = linkTex;
            softCircle = null;
            linkTex = null;
            Main.QueueMainThreadAction(() =>
            {
                soft?.Dispose();
                link?.Dispose();
            });
        }

        private static void Spark(Vector2 pos, Vector2 vel, Color color, float size, int life)
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

        private void SpawnShortFlash()
        {
            if (Main.dedServ)
                return;

            ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                position: Projectile.Center.ToNumerics(),
                velocity: System.Numerics.Vector2.Zero,
                rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                scale: new System.Numerics.Vector2(Main.rand.NextFloat(110f, 135f)),
                color: new Color(0, 0, 0, 255),
                duration: 13
            ));

            ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                position: Projectile.Center.ToNumerics(),
                velocity: System.Numerics.Vector2.Zero,
                rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                scale: new System.Numerics.Vector2(Main.rand.NextFloat(85f, 105f)),
                color: new Color(0, 0, 0, 235),
                duration: 14
            ));

            ParticleSystem.FlashBuffer.Create(new ParticleInfo(
                position: Projectile.Center.ToNumerics(),
                velocity: System.Numerics.Vector2.Zero,
                rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                scale: new System.Numerics.Vector2(Main.rand.NextFloat(60f, 75f)),
                color: new Color(0, 0, 0, 210),
                duration: 12
            ));
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 400;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override bool? CanDamage() => Blasting && Projectile.ai[1] < BindTime + DamageFrames ? (bool?)null : false;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (!Blasting || Projectile.ai[1] >= BindTime + DamageFrames)
                return false;

            Vector2 c = Projectile.Center;
            Vector2 closest = Vector2.Clamp(c, targetHitbox.TopLeft(), targetHitbox.BottomRight());
            return Vector2.DistanceSquared(c, closest) <= BlastRadius * BlastRadius;
        }

        private static int BreakStart(int i) => 12 + (i * 3 % ChainCount) * 3;

        private static float BreakProgress(int i, float t) => MathHelper.Clamp((t - BreakStart(i)) / 7f, 0f, 1f);

        private static float PreProgress(int i, float t) => MathHelper.Clamp((t - (BreakStart(i) - 7f)) / 7f, 0f, 1f);

        private void ChainGeometry(int i, float t, out Vector2 a, out Vector2 c, out Vector2 e)
        {
            Vector2 center = Projectile.Center;
            float k = MathHelper.Clamp(t / BindTime, 0f, 1f);
            float grow = 1f - MathF.Pow(1f - MathHelper.Clamp(t / 8f, 0f, 1f), 3f);
            float rb = Math.Max(lastRadius, 14f);
            float time = Main.GlobalTimeWrappedHourly;
            float ang = Projectile.whoAmI * 1.7f + i * MathHelper.TwoPi / ChainCount;
            float r = rb * 1.6f + 70f - 25f * k;

            a = center + ang.ToRotationVector2() * r;
            float angB = ang + 0.9f;
            Vector2 body = center + new Vector2(MathF.Cos(angB) * rb, MathF.Sin(angB) * rb * 0.8f);
            e = Vector2.Lerp(a, body, grow);

            Vector2 perp = (e - a).SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);
            float pre = PreProgress(i, t);
            Vector2 shake = new Vector2(MathF.Sin(t * 2.3f + i), MathF.Cos(t * 2.9f + i * 1.7f)) * pre * 3f;
            c = (a + e) / 2f + perp * (MathF.Sin(time * 3f + i * 1.3f) * 6f + 12f * (1f - k)) + shake;
        }

        private static Vector2 Bezier(Vector2 a, Vector2 c, Vector2 b, float u)
        {
            float v = 1f - u;
            return v * v * a + 2f * v * u * c + u * u * b;
        }

        private static Vector2 BezierTangent(Vector2 a, Vector2 c, Vector2 b, float u)
        {
            return 2f * (1f - u) * (c - a) + 2f * u * (b - c);
        }

        public override void AI()
        {
            Projectile.ai[1]++;
            float t = Projectile.ai[1];
            int idx = (int)Projectile.ai[0];
            NPC target = idx >= 0 && idx < Main.maxNPCs ? Main.npc[idx] : null;
            bool valid = target != null && target.active && target.life > 0;

            if (!Blasting)
            {
                if (!valid)
                    Projectile.ai[1] = BindTime;
                else
                {
                    lastRadius = Math.Max(target.width, target.height) * 0.5f;

                    if (!locked)
                    {
                        locked = true;
                        lockPos = target.position;
                    }

                    if (!target.boss)
                    {
                        target.velocity = Vector2.Zero;
                        target.position = lockPos;
                    }

                    Projectile.Center = target.Center;
                    BindFx(t);
                }
            }

            if (Blasting && !exploded)
                Explode();
            else if (Blasting)
                BlastFx();

            float glow = Blasting ? 1.6f : 0.3f + 0.5f * (t / BindTime);
            Lighting.AddLight(Projectile.Center, new Vector3(0.55f, 0.15f, 0.8f) * glow);
        }

        private void BindFx(float t)
        {
            if (Main.dedServ)
                return;

            Vector2 c = Projectile.Center;

            if (t == 1f)
            {
                SoundEngine.PlaySound(SoundID.Item17 with { Volume = 0.7f, Pitch = -0.4f }, c);
                for (int i = 0; i < 12; i++)
                    Spark(c, Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3f, 7f), Violet, Main.rand.NextFloat(8f, 13f), 18);
            }

            for (int i = 0; i < ChainCount; i++)
            {
                int start = BreakStart(i);
                float br = BreakProgress(i, t);

                if ((int)t == start)
                    SoundEngine.PlaySound(SoundID.Item37 with { Volume = 0.45f, Pitch = 0.2f, MaxInstances = 4 }, c);

                if (br <= 0f)
                    continue;

                ChainGeometry(i, t, out Vector2 a, out Vector2 cc, out Vector2 e);

                if (br < 1f)
                {
                    float edge = br * 0.5f;
                    Vector2 p1 = Bezier(a, cc, e, edge);
                    Vector2 p2 = Bezier(a, cc, e, 1f - edge);
                    Vector2 tan = BezierTangent(a, cc, e, 0.5f).SafeNormalize(Vector2.UnitX);
                    Vector2 side = tan.RotatedBy(MathHelper.PiOver2);

                    Spark(p1, -tan * Main.rand.NextFloat(1f, 4f) + side * Main.rand.NextFloat(-2f, 2f), Main.rand.NextBool(3) ? Violet : Blood, Main.rand.NextFloat(5f, 9f), 14);
                    Spark(p2, tan * Main.rand.NextFloat(1f, 4f) + side * Main.rand.NextFloat(-2f, 2f), Main.rand.NextBool(3) ? Violet : Blood, Main.rand.NextFloat(5f, 9f), 14);
                }
                else if ((int)t == start + 7)
                {
                    Vector2 mid = Bezier(a, cc, e, 0.5f);
                    for (int s = 0; s < 6; s++)
                        Spark(mid, Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 6f), Main.rand.NextBool() ? Blood : Violet, Main.rand.NextFloat(7f, 12f), 16);
                }
            }
        }

        private void BlastFx()
        {
            if (Main.dedServ)
                return;

            float bt = (Projectile.ai[1] - BindTime) / BlastLife;
            if (bt > 0.35f)
                return;

            Vector2 c = Projectile.Center;
            for (int i = 0; i < 3; i++)
            {
                Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f);
                Spark(c + dir * Main.rand.NextFloat(20f, 90f), dir * Main.rand.NextFloat(2f, 6f), Main.rand.NextBool(3) ? Blood : Violet, Main.rand.NextFloat(8f, 14f), 18);
            }
        }

        private void Explode()
        {
            exploded = true;
            Projectile.localAI[2] = 1f;
            Projectile.Resize(210, 210);
            Projectile.timeLeft = BlastLife;

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.9f, Pitch = -0.6f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.5f, Pitch = -0.4f }, Projectile.Center);

            if (Main.dedServ)
                return;

            Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Vector2.UnitY, 6f, 8f, 14, 1200f, "ShackleBlast"));

            SpawnShortFlash();

            float off = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < 36; i++)
            {
                Vector2 dir = (off + MathHelper.TwoPi * i / 36f).ToRotationVector2();
                Spark(Projectile.Center, dir * Main.rand.NextFloat(5f, 11f), i % 3 == 0 ? Blood : Violet, Main.rand.NextFloat(10f, 18f), 24);
            }

            for (int i = 0; i < 22; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
                Color col = i % 2 == 0 ? new Color(80, 20, 120, 220) : new Color(30, 30, 30, 200);
                Spark(pos, vel, col, Main.rand.NextFloat(8f, 15f), Main.rand.Next(16, 26));
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Bleeding, 300);

            for (int i = 0; i < 6; i++)
                Spark(target.Center + Main.rand.NextVector2Circular(10f, 10f), Main.rand.NextVector2Circular(4f, 4f), Main.rand.NextBool() ? Blood : Violet, Main.rand.NextFloat(8f, 13f), 18);
        }

        private void DrawChain(Texture2D tex, int i, float t)
        {
            ChainGeometry(i, t, out Vector2 a, out Vector2 c, out Vector2 e);

            float br = BreakProgress(i, t);
            float pre = PreProgress(i, t);
            if (br >= 1f)
                return;

            Vector2 half = tex.Size() / 2f;
            float spacing = Math.Max(tex.Width * 0.8f, 4f);
            int steps = Math.Max((int)(Vector2.Distance(a, e) / spacing), 2);
            float edge = br * 0.5f;
            float time = Main.GlobalTimeWrappedHourly;
            Color tint = Color.Lerp(Violet, Blood, Math.Max(pre * 0.6f, br));
            float pulse = (t * 0.05f + i * 0.17f) % 1f;
            float scale = 1f - br * 0.25f;

            float sizeVar = 0.85f + 0.3f * (float)Math.Sin(i * 12.9898f + Projectile.whoAmI * 78.233f);

            for (int s = 0; s <= steps; s++)
            {
                float u = s / (float)steps;
                if (u < edge || u > 1f - edge)
                    continue;

                if (br > 0.25f && (s + (int)t) % 3 == 0 && Main.rand.NextFloat() < br)
                    continue;

                Vector2 p = Bezier(a, c, e, u);
                float rot = BezierTangent(a, c, e, u).ToRotation();

                if (br > 0f)
                {
                    float fromEdge = Math.Min(u - edge, 1f - edge - u);
                    float loose = MathHelper.Clamp(1f - fromEdge * 6f, 0f, 1f) * br;
                    Vector2 perp = rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2);
                    p += perp * MathF.Sin(time * 40f + s * 1.9f) * loose * 3f;
                    rot += MathF.Sin(time * 30f + s) * loose * 0.5f;
                }

                float localScale = scale * sizeVar * (0.95f + 0.1f * (s % 3));

                Color light = Color.Lerp(Lighting.GetColor(p.ToTileCoordinates()), Color.White, 0.35f);
                Main.EntitySpriteDraw(tex, p - Main.screenPosition, null, light, rot, half, localScale, SpriteEffects.None, 0);

                float g = 0.7f * Math.Max(0f, 1f - Math.Abs(u - pulse) * 5f);
                g += pre * 0.35f + br * 0.5f;
                if (g > 0.01f)
                    Main.EntitySpriteDraw(tex, p - Main.screenPosition, null, new Color(tint.R, tint.G, tint.B, 0) * Math.Min(g, 1f), rot, half, localScale * 1.15f, SpriteEffects.None, 0);
            }

            if (br > 0f)
            {
                float fl = (float)Math.Sin(br * MathHelper.Pi);
                Vector2 p1 = Bezier(a, c, e, edge);
                Vector2 p2 = Bezier(a, c, e, 1f - edge);
                Color add = new Color(Blood.R, Blood.G, Blood.B, 255);

                for (int s = 0; s < 3; s++)
                {
                    Vector2 off = Main.rand.NextVector2Circular(6f, 6f);
                    Spark(p1 + off, Main.rand.NextVector2Circular(3f, 3f), add, Main.rand.NextFloat(6f, 10f) * fl, 12);
                    Spark(p2 + off, Main.rand.NextVector2Circular(3f, 3f), add, Main.rand.NextFloat(6f, 10f) * fl, 12);
                }
            }
        }

        private void DrawBind(float t)
        {
            Vector2 c = Projectile.Center;
            float rb = Math.Max(lastRadius, 14f);

            Texture2D tex = HeartTextures.WireTex ?? Link;
            for (int i = 0; i < ChainCount; i++)
                DrawChain(tex, i, t);

            int broken = 0;
            for (int i = 0; i < ChainCount; i++)
                if (BreakProgress(i, t) >= 1f)
                    broken++;

            float coreK = MathHelper.Clamp(t / BindTime, 0f, 1f);
            float crackK = broken / (float)ChainCount;
            Texture2D soft = SoftCircle;
            float d = MathHelper.Lerp(16f, 30f + rb * 1.1f, coreK);
            float pulseK = 1f + 0.08f * MathF.Sin(t * 0.8f) + crackK * 0.15f;
            Main.EntitySpriteDraw(soft, c - Main.screenPosition, null, new Color(Violet.R, Violet.G, Violet.B, 0) * (0.3f + 0.4f * crackK), 0f, new Vector2(64f), d * 1.6f * pulseK / 128f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(soft, c - Main.screenPosition, null, Color.Black * (0.7f * coreK), 0f, new Vector2(64f), d * pulseK / 128f, SpriteEffects.None, 0);
        }

        private void DrawBlast()
        {
            float bt = MathHelper.Clamp((Projectile.ai[1] - BindTime) / BlastLife, 0f, 1f);
            float grow = 1f - MathF.Pow(1f - MathHelper.Clamp(bt / 0.35f, 0f, 1f), 3f);
            float fade = bt < 0.35f ? 1f : 1f - (bt - 0.35f) / 0.65f;
            float d = MathHelper.Lerp(40f, BlastRadius * 2f * 1.15f, grow);

            Texture2D soft = SoftCircle;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(soft, pos, null, new Color(Violet.R, Violet.G, Violet.B, 0) * (0.55f * fade), 0f, new Vector2(64f), d * 1.4f / 128f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(soft, pos, null, Color.Black * (0.95f * fade), 0f, new Vector2(64f), d / 128f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(soft, pos, null, Color.Black * (0.9f * fade), 0f, new Vector2(64f), d * 0.7f / 128f, SpriteEffects.None, 0);

            if (bt < 0.25f)
            {
                float pulse = (0.25f - bt) / 0.25f;
                if (Main.rand.NextFloat() < pulse * 0.5f)
                    Spark(Projectile.Center + Main.rand.NextVector2Circular(40f, 40f), Main.rand.NextVector2Circular(2f, 4f), new Color(80, 20, 120, 255), Main.rand.NextFloat(10f, 18f), 14);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Blasting)
                DrawBlast();
            else
                DrawBind(Projectile.ai[1]);

            return false;
        }
    }
}