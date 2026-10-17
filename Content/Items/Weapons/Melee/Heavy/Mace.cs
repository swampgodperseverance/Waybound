using System;
using System.Collections.Generic;
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
using Waybound.Content.Projectiles.Base;
using Waybound.Helpers;
using Waybound.Particles;
using Waybound.Resources;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Waybound.Content.Items.Weapons.Melee.Heavy
{
    public class Mace : ModItem
    {
        //3d mace
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 46;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.holdStyle = 0;
            Item.noUseGraphic = true;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.UseSound = Audio.Get("HeavyWeapon_Swing");
            Item.autoReuse = true;
            Item.DamageType = DamageClass.Melee;
            Item.damage = 50;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(0, 8);
            Item.rare = ItemRarityID.LightRed;
            Item.noMelee = true;
            Item.useTurn = false;
            Item.shoot = ModContent.ProjectileType<MaceProjectile>();
            Item.shootSpeed = 0f;
        }
    }

    public class MaceSpinPlayer : ModPlayer
    {
        private int _buffer;

        public bool Spinning => _buffer > 0;

        public void Refresh()
        {
            _buffer = 3;
        }

        public override void PostUpdate()
        {
            if (_buffer > 0)
                _buffer--;
        }

        public override void PostUpdateRunSpeeds()
        {
            if (!Spinning)
                return;

            Player.maxRunSpeed *= 0.65f;
            Player.accRunSpeed *= 0.65f;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (!Spinning)
                return;

            modifiers.FinalDamage *= 0.35f;
            modifiers.Knockback *= 0f;
        }
    }

    public class MaceProjectile : BaseHeavySword
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Melee/Heavy/Mace";

        protected override int MaxCombo => 4;

        protected override float SwingMaxAngle => 170f;
        protected override float SwordLength => 40f;
        protected override float SwordAngleOffset => 50f;
        protected override float SwordHiltOffset => 6f;
        protected override float HitboxSize => 14f;
        protected override int BaseSwingTime => 36;
        protected override int ComboContinueTime => 16;

        private static readonly Color WindColor = new Color(120, 210, 255, 0);
        private static readonly Color SteelColor = new Color(170, 190, 205);
        private static readonly Color SteelCore = new Color(225, 238, 248);
        private static readonly Color ThreadOutline = new Color(58, 36, 20, 255);
        private static readonly Color ThreadColor = new Color(122, 82, 46, 255);
        private static readonly float[] Offsets = { 0f, MathHelper.Pi };

        private const float SPRITE_TIP_ANGLE = MathHelper.PiOver4;

        private const int SPIN_UP = 16;
        private const int SPIN_HOLD = 70;
        private const int SPIN_DOWN = 14;
        private const int RECOVER_TIME = 26;
        private const int SPECIAL_TOTAL = SPIN_UP + SPIN_HOLD + SPIN_DOWN + RECOVER_TIME;

        private const float MAX_RADIUS = 72f;
        private const float MAX_SPEED = 0.55f;
        private const float PULL_RANGE = 200f;

        private const int MAX_STREAKS = 90;

        private struct Streak
        {
            public Vector2 Offset;   
            public float Length;
            public float Width;
            public float Speed;
            public float Tint;
            public int Life;
            public int MaxLife;
        }

        private readonly List<Streak> _streaks = new List<Streak>();

        private ref float SpecialTimer => ref Projectile.ai[1];

        private float _phase;
        private float _prevPhase;
        private float _speed;
        private float _radius;
        private float _tilt;
        private int _sign = 1;
        private bool _slamDone;
        private float _fade = 1f;
        private float _settle;

        protected override bool CanUseSpecialAttack() => Combo == 4;

        protected override bool CanHit_OnSpecial(NPC target) => SpecialTimer > SPIN_UP * 0.5f && SpecialTimer <= SPIN_UP + SPIN_HOLD + SPIN_DOWN;

        protected override bool Colliding_OnSpecial(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 center = Owner.Center;
            float point = 0f;

            for (int i = 0; i < Offsets.Length; i++)
            {
                Vector2 head = HeadPosition(_phase + Offsets[i]);
                Vector2 prev = HeadPosition(_prevPhase + Offsets[i]);

                if (targetHitbox.Intersects(new Rectangle((int)head.X - 44, (int)head.Y - 44, 88, 88)))
                    return true;

                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), center, head, 76f, ref point))
                    return true;

                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), prev, head, 76f, ref point))
                    return true;
            }

            return false;
        }

        protected override void InitializeTrail()
        {
            Trail = new DrawHelper.RibbonTrail(8);
            Trail.Initialize();

            Color outer = SteelColor;
            Color inner = SteelCore;

            Trail.SetPasses(
                x => (
                    -5f,
                    Color.Lerp(outer * 0.35f, Color.Transparent, x * x),
                    -5f * (x * x),
                    Color.Lerp(inner * 0.55f, Color.Transparent, x * x),
                    null
                ),
                x => (
                    -5f - 30f * (1f - x),
                    Color.Transparent,
                    -5f,
                    Color.Lerp(outer * 0.25f, Color.Transparent, x * x),
                    null
                )
            );
        }

        protected override void OnHitNPC_Extra(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            Vector2 pos = target.Hitbox.ClosestPointInRect(Projectile.Center);
            SpawnFlash(pos, 60f);

            int count = 4 + (hit.Crit ? 3 : 0);
            for (int i = 0; i < count; i++)
            {
                SpawnWind(
                    pos,
                    Main.rand.NextVector2Circular(5f, 5f),
                    Main.rand.NextFloat(6f, 11f),
                    Main.rand.Next(12, 20)
                );
            }
        }

        protected override void CastSpecialAttack()
        {
            Combo = 0;
            SpecialTimer = 0;
            _sign = Owner.direction == 0 ? 1 : Owner.direction;
            _phase = _sign > 0 ? -MathHelper.PiOver2 : MathHelper.Pi + MathHelper.PiOver2;
            _prevPhase = _phase;
            _speed = 0.1f;
            _radius = 26f;
            _tilt = 0.42f;
            _slamDone = false;
            _fade = 1f;
            _settle = 0f;
            _streaks.Clear();
            Projectile.ResetLocalNPCHitImmunity();
            SoundEngine.PlaySound(SoundID.Item7 with { Pitch = -0.3f, Volume = 0.8f }, Projectile.Center);
            CurrentState = AttackState.Special;
            ClearTrail();
        }

        private Vector2 HeadPosition(float phase)
        {
            Vector2 dir = new Vector2((float)Math.Cos(phase), (float)Math.Sin(phase) * _tilt);
            return Owner.Center + dir * _radius;
        }

        private static float Smooth(float x)
        {
            x = MathHelper.Clamp(x, 0f, 1f);
            return x * x * (3f - 2f * x);
        }

        protected override void AI_OnSpecial()
        {
            SpecialTimer++;
            float t = SpecialTimer;

            Owner.GetModPlayer<MaceSpinPlayer>().Refresh();
            _fade = 1f;
            _settle = 0f;

            if (t <= SPIN_UP)
            {
                float p = EaseFunctions.EaseOutCubic(t / SPIN_UP);
                _speed = MathHelper.Lerp(0.1f, MAX_SPEED, p);
                _radius = MathHelper.Lerp(26f, MAX_RADIUS, p);
            }
            else if (t <= SPIN_UP + SPIN_HOLD)
            {
                _speed = MAX_SPEED;
                _radius = MAX_RADIUS + 4f * (float)Math.Sin(t * 0.3f);
            }
            else if (t <= SPIN_UP + SPIN_HOLD + SPIN_DOWN)
            {
                float p = (t - SPIN_UP - SPIN_HOLD) / SPIN_DOWN;
                _speed = MathHelper.Lerp(MAX_SPEED, 0.2f, p);
                _radius = MathHelper.Lerp(MAX_RADIUS, 46f, p);

                if (!_slamDone && p >= 0.9f)
                {
                    _slamDone = true;
                    DoSlam();
                }
            }
            else if (t <= SPECIAL_TOTAL)
            {
                float p = (t - SPIN_UP - SPIN_HOLD - SPIN_DOWN) / RECOVER_TIME;

                _speed = MathHelper.Lerp(0.2f, 0.05f, p);
                _radius = MathHelper.Lerp(46f, 30f, EaseFunctions.EaseOutCubic(p));
                _fade = 1f - Smooth(p / 0.8f);
                _settle = Smooth((p - 0.45f) / 0.55f);

                UpdateAngle(MaxAngle, MinAngle, _settle);
                UpdateTrail();
            }
            else
            {
                End();
                return;
            }

            _tilt = 0.42f + 0.1f * (float)Math.Sin(t * 0.22f);
            _prevPhase = _phase;
            _phase += _sign * _speed;

            if ((int)t % 12 == 0)
                Projectile.ResetLocalNPCHitImmunity();

            if (t <= SPIN_UP + SPIN_HOLD + SPIN_DOWN)
                Projectile.rotation = (HeadPosition(_phase) - Owner.Center).ToRotation();

            if ((int)t % 6 == 0 && t < SPIN_UP + SPIN_HOLD + SPIN_DOWN)
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -0.5f + _speed, Volume = 0.55f, MaxInstances = 4 }, Owner.Center);

            for (int i = 0; i < Offsets.Length; i++)
                Lighting.AddLight(HeadPosition(_phase + Offsets[i]), 0.25f * _fade, 0.55f * _fade, 0.85f * _fade);

            SpinVisuals();
            SpawnTrail();

            UpdateStreaks();
            if (t <= SPIN_UP + SPIN_HOLD + SPIN_DOWN)
                SpawnStreaks();

            if (t > SPIN_UP * 0.5f && t <= SPIN_UP + SPIN_HOLD + SPIN_DOWN)
                PullEnemies();

            Owner.SetCompositeArmFront(
                true,
                Player.CompositeArmStretchAmount.Full,
                Projectile.rotation * -Projectile.spriteDirection
            );
        }

        private void SpawnStreaks()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            float strength = MathHelper.Clamp(_speed / MAX_SPEED, 0f, 1f);
            if (strength < 0.25f || _streaks.Count >= MAX_STREAKS)
                return;

            int count = strength > 0.7f ? 2 : 1;
            for (int i = 0; i < count; i++)
            {
                if (Main.rand.NextFloat() > 0.35f + 0.65f * strength)
                    continue;

                int life = Main.rand.Next(9, 14);
                _streaks.Add(new Streak
                {
                    Offset = new Vector2(
                        Main.rand.NextFloat(-1f, 1f) * (_radius + 16f),
                        Main.rand.NextFloat(-4f, 26f)),
                    Length = Main.rand.NextFloat(26f, 64f) * (0.7f + 0.3f * strength),
                    Width = Main.rand.NextFloat(1.2f, 2.4f),
                    Speed = Main.rand.NextFloat(3.5f, 7.5f),
                    Tint = Main.rand.NextFloat(),
                    Life = life,
                    MaxLife = life
                });
            }
        }

        private void UpdateStreaks()
        {
            for (int i = _streaks.Count - 1; i >= 0; i--)
            {
                Streak s = _streaks[i];
                s.Offset.Y -= s.Speed;
                s.Speed *= 0.985f;
                s.Life--;

                if (s.Life <= 0)
                    _streaks.RemoveAt(i);
                else
                    _streaks[i] = s;
            }
        }

        private void DrawStreaks()
        {
            if (_streaks.Count == 0 || _fade <= 0.01f)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            const int segments = 6;

            for (int i = 0; i < _streaks.Count; i++)
            {
                Streak s = _streaks[i];

                float lifeProgress = 1f - s.Life / (float)s.MaxLife;
                float envelope = (float)Math.Sin(lifeProgress * MathHelper.Pi);
                float baseAlpha = 0.42f * envelope * _fade;
                if (baseAlpha <= 0.01f)
                    continue;

                Color color = Color.Lerp(new Color(70, 140, 255, 0), new Color(210, 245, 255, 0), s.Tint);
                Vector2 pos = Owner.Center + s.Offset - Main.screenPosition;
                float seg = s.Length / segments;

                for (int k = 0; k < segments; k++)
                {
                    float a = (float)Math.Sin((k + 0.5f) / segments * MathHelper.Pi);
                    Vector2 p = new Vector2(pos.X - s.Width * 0.5f, pos.Y - s.Length + k * seg);

                    Main.EntitySpriteDraw(
                        pixel,
                        p,
                        new Rectangle(0, 0, 1, 1),
                        color * (baseAlpha * a),
                        0f,
                        Vector2.Zero,
                        new Vector2(s.Width, seg + 1f),
                        SpriteEffects.None
                    );
                }
            }
        }
        private void SpinVisuals()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            float strength = _speed / MAX_SPEED;

            for (int i = 0; i < Offsets.Length; i++)
            {
                float ph = _phase + Offsets[i];
                float sin = (float)Math.Sin(ph);
                float cos = (float)Math.Cos(ph);
                Vector2 head = HeadPosition(ph);
                Vector2 tangent = new Vector2(-sin, cos * _tilt) * _sign;
                tangent = tangent.SafeNormalize(Vector2.UnitX);

                if (Main.rand.NextFloat() < 0.7f * strength * _fade)
                {
                    SpawnWind(
                        head + Main.rand.NextVector2Circular(6f, 6f),
                        tangent * Main.rand.NextFloat(2f, 5f) * strength,
                        Main.rand.NextFloat(6f, 10f) * (0.8f + 0.3f * sin),
                        Main.rand.Next(10, 16)
                    );
                }

                if ((int)SpecialTimer % 2 == i % 2 && strength > 0.3f)
                {
                    float halo = 40f * (0.8f + 0.3f * sin);
                    Color haloColor = Color.Lerp(new Color(60, 120, 255, 0), new Color(200, 245, 255, 0), (sin + 1f) * 0.5f) * 0.8f * _fade;
                    SpawnTrailRing(head, Owner.velocity * 0.9f, 0f, new Vector2(halo, halo * 0.9f), haloColor, 9);
                }

                if ((int)SpecialTimer % 3 == 0 && strength > 0.3f)
                    SpawnFlash(head, 26f * (0.8f + 0.3f * sin));
            }

            if (_fade > 0.5f && Main.rand.NextBool(3))
            {
                float a = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 pos = Owner.Center + new Vector2((float)Math.Cos(a) * _radius, (float)Math.Sin(a) * _radius * _tilt);
                SpawnWind(pos, Vector2.Zero, Main.rand.NextFloat(4f, 7f), Main.rand.Next(10, 14));
            }

            if (_fade > 0.5f && Main.rand.NextBool(4))
            {
                Vector2 foot = new Vector2(Owner.Center.X + Main.rand.NextFloat(-30f, 30f), Owner.Bottom.Y - 2f);
                SpawnWind(foot, new Vector2(Main.rand.NextFloat(-2f, 2f), -Main.rand.NextFloat(0.5f, 1.5f)), Main.rand.NextFloat(6f, 10f), Main.rand.Next(14, 20));
            }
        }

        private void SpawnTrail()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            float strength = MathHelper.Clamp(_speed / MAX_SPEED, 0f, 1f);
            if (strength < 0.2f)
                return;

            const int steps = 3;
            for (int k = 0; k < Offsets.Length; k++)
            {
                for (int s = 1; s <= steps; s++)
                {
                    float ph = MathHelper.Lerp(_prevPhase, _phase, s / (float)steps) + Offsets[k];
                    float sin = (float)Math.Sin(ph);
                    float cos = (float)Math.Cos(ph);

                    Vector2 pos = Owner.Center + new Vector2(cos, sin * _tilt) * _radius;
                    Vector2 tangent = new Vector2(-sin, cos * _tilt) * _sign;

                    float size = 30f * (0.75f + 0.35f * sin) * (0.6f + 0.4f * strength);
                    Color color = Color.Lerp(new Color(50, 100, 255, 0), new Color(190, 240, 255, 0), (sin + 1f) * 0.5f) * (0.55f + 0.45f * strength) * _fade;

                    SpawnTrailRing(pos, Owner.velocity * 0.9f, tangent.ToRotation(), new Vector2(size * 0.4f, size), color, 18);
                }
            }
        }

        private void PullEnemies()
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            Vector2 center = Owner.Center;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal || npc.boss)
                    continue;

                if (npc.knockBackResist <= 0f)
                    continue;

                float dist = Vector2.Distance(npc.Center, center);
                if (dist > PULL_RANGE || dist < 46f)
                    continue;

                Vector2 pull = (center - npc.Center).SafeNormalize(Vector2.Zero) * 0.9f * npc.knockBackResist;
                npc.velocity += pull;

                if (npc.velocity.Length() > 9f)
                    npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * 9f;
            }
        }

        private void DoSlam()
        {
            Vector2 pos = Owner.Center;

            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.2f, Volume = 0.9f }, pos);
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.1f, Volume = 0.6f }, pos);

            if (Main.myPlayer == Projectile.owner)
            {
                Main.instance.CameraModifiers.Add(new Terraria.Graphics.CameraModifiers.PunchCameraModifier(
                    pos,
                    new Vector2(0f, -1f),
                    6f,
                    8f,
                    14,
                    800f,
                    "MaceSlam"
                ));

                const float radius = 150f;
                int dmg = (int)(Projectile.damage * 1.4f);

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                        continue;

                    if (Vector2.Distance(npc.Center, pos) > radius)
                        continue;

                    int hitDir = npc.Center.X >= pos.X ? 1 : -1;
                    NPC.HitInfo hit = npc.CalculateHitInfo(dmg, hitDir, false, 9f, DamageClass.Melee, true, Owner.luck);
                    npc.StrikeNPC(hit);
                    if (Main.netMode != NetmodeID.SinglePlayer)
                        NetMessage.SendStrikeNPC(npc, hit);

                    if (!npc.boss && npc.knockBackResist > 0f)
                    {
                        Vector2 push = (npc.Center - pos).SafeNormalize(Vector2.UnitX * hitDir) * 9f;
                        npc.velocity += push * npc.knockBackResist;
                    }
                }
            }

            if (Main.netMode == NetmodeID.Server)
                return;

            SpawnFlash(pos, 220f);

            const int ring = 28;
            for (int i = 0; i < ring; i++)
            {
                Vector2 dir = (MathHelper.TwoPi * i / ring).ToRotationVector2();
                SpawnWind(
                    pos + dir * 16f,
                    dir * Main.rand.NextFloat(8f, 12f) * new Vector2(1f, 0.5f),
                    Main.rand.NextFloat(9f, 15f),
                    Main.rand.Next(16, 24)
                );
            }

            for (int i = 0; i < 16; i++)
            {
                float angle = -MathHelper.PiOver2 + Main.rand.NextFloat(-1.3f, 1.3f);
                SpawnWind(
                    pos,
                    angle.ToRotationVector2() * Main.rand.NextFloat(3f, 9f),
                    Main.rand.NextFloat(6f, 11f),
                    Main.rand.Next(14, 24)
                );
            }
        }

        private void End()
        {
            SpecialTimer = 0;
            _slamDone = false;
            _streaks.Clear();
            TimeLeft = ComboContinueTime;
            CurrentState = AttackState.Default;
        }

        protected override void Draw_OnSpecial(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            float strength = MathHelper.Clamp(_speed / MAX_SPEED, 0f, 1f);

            DrawStreaks();

            float a = _phase;
            float b = _phase + Offsets[1];
            if (Math.Sin(a) > Math.Sin(b))
            {
                float tmp = a;
                a = b;
                b = tmp;
            }

            if (_fade > 0.01f)
            {
                DrawOrbiter(texture, a, strength, lightColor);
                DrawOrbiter(texture, b, strength, lightColor);
            }

            if (_settle > 0f)
            {
                DrawTrail(lightColor);
                DrawSword(texture, Projectile.rotation, lightColor * _settle);
            }
        }

        private void DrawOrbiter(Texture2D texture, float phase, float strength, Color lightColor)
        {
            DrawThread(phase);

            const int ghosts = 6;
            for (int k = ghosts; k >= 1; k--)
            {
                float ghostPhase = phase - _sign * _speed * k * 0.85f;
                float alpha = 0.45f * strength * _fade * (1f - k / (float)(ghosts + 1));
                Color tint = Color.Lerp(lightColor, new Color(150, 225, 255), 0.65f) * alpha;
                DrawMace(texture, ghostPhase, tint, 0.95f);
            }

            DrawMace(texture, phase, lightColor * _fade, 1f);
        }

        private void DrawThread(float phase)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            float sin = (float)Math.Sin(phase);
            float cos = (float)Math.Cos(phase);
            Vector2 dir = new Vector2(cos, sin * _tilt);
            Vector2 head = Owner.Center + dir * _radius;

            Vector2 start = Owner.Center + new Vector2(0f, -4f);
            Vector2 end = head - dir.SafeNormalize(Vector2.UnitX) * 12f;

            Vector2 delta = end - start;
            float length = delta.Length();
            if (length < 1f)
                return;

            float rotation = delta.ToRotation();
            float thickness = 1.8f + 0.9f * (sin + 1f) * 0.5f;
            Vector2 from = start - Main.screenPosition;

            Main.EntitySpriteDraw(
                pixel,
                from,
                new Rectangle(0, 0, 1, 1),
                ThreadOutline * _fade,
                rotation,
                new Vector2(0f, 0.5f),
                new Vector2(length, thickness + 1.6f),
                SpriteEffects.None
            );

            Main.EntitySpriteDraw(
                pixel,
                from,
                new Rectangle(0, 0, 1, 1),
                ThreadColor * _fade,
                rotation,
                new Vector2(0f, 0.5f),
                new Vector2(length, thickness),
                SpriteEffects.None
            );
        }

        private void DrawMace(Texture2D texture, float phase, Color color, float scaleMul)
        {
            Vector2 dir = new Vector2((float)Math.Cos(phase), (float)Math.Sin(phase) * _tilt);
            float depth = (float)Math.Sin(phase);

            Vector2 pos = Owner.Center + dir * _radius - Main.screenPosition;
            float scale = Projectile.scale * scaleMul * (0.88f + 0.32f * depth) * (0.8f + 0.2f * _fade);
            float brightness = MathHelper.Lerp(0.55f, 1.1f, (depth + 1f) * 0.5f);
            float rotation = (float)Math.Atan2(dir.Y, dir.X) + SPRITE_TIP_ANGLE;

            Main.EntitySpriteDraw(
                texture,
                pos,
                null,
                color * brightness,
                rotation,
                texture.Size() / 2f,
                scale,
                SpriteEffects.None
            );
        }

        internal static void SpawnTrailRing(Vector2 position, Vector2 velocity, float rotation, Vector2 size, Color color, int duration)
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.TrailBuffer == null)
                return;

            ParticleSystem.TrailBuffer.Create(new ParticleInfo(
                position.ToNumerics(),
                velocity.ToNumerics(),
                rotation,
                size.ToNumerics(),
                color,
                duration
            ));
        }

        internal static void SpawnWind(Vector2 position, Vector2 velocity, float size, int duration)
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.MegasparkBuffer == null)
                return;

            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                position.ToNumerics(),
                velocity.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new System.Numerics.Vector2(size, size * Main.rand.NextFloat(0.5f, 0.9f)),
                WindColor * Main.rand.NextFloat(0.7f, 1f),
                duration
            ));
        }

        internal static void SpawnFlash(Vector2 position, float size)
        {
            if (Main.netMode == NetmodeID.Server || ParticleSystem.MegasparkBuffer == null)
                return;

            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                position.ToNumerics(),
                System.Numerics.Vector2.Zero,
                Main.rand.NextFloat(MathHelper.TwoPi),
                new System.Numerics.Vector2(size, size),
                new Color(140, 215, 255, 0),
                7
            ));

            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                position.ToNumerics(),
                System.Numerics.Vector2.Zero,
                Main.rand.NextFloat(MathHelper.TwoPi),
                new System.Numerics.Vector2(size * 0.5f, size * 0.5f),
                new Color(235, 250, 255, 0),
                5
            ));
        }
    }
}