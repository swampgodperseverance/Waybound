using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;
using SysVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Items.Weapons.Melee.Rapiers
{
    // whats better =? Prince or La Veuve du Prince
    public class Prince : BaseRapierItem
    {
        protected override int HoldoutType => ModContent.ProjectileType<PrinceHoldout>();

        protected override void SetRapierDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 5);
            Item.damage = 55;
            Item.knockBack = 5f;
            Item.crit = 10;
        }

        public override bool CanUseItem(Player player)
        {
            return !player.GetModPlayer<PrinceDashPlayer>().Active && base.CanUseItem(player);
        }
    }

    public static class PrinceFx
    {
        public static readonly Color Red = new Color(235, 40, 60, 255);
        public static readonly Color Hot = new Color(255, 120, 70, 255);
        public static readonly Color Gold = new Color(255, 212, 90, 230);
        public static readonly Color White = new Color(255, 242, 205, 255);

        public static void Spark(Vector2 pos, Vector2 vel, Color color, float scale, int life)
        {
            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new SysVector2(scale),
                color,
                life));
        }

        public static void Boom(Vector2 pos, Vector2 vel, float scale, int life)
        {
            ParticleSystem.FlameBoomBuffer.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new SysVector2(scale),
                Color.White,
                life));
        }

        public static void Ring(Vector2 center, Color color, int count, float speed, float scale)
        {
            float offset = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < count; i++)
            {
                float a = offset + MathHelper.TwoPi * i / count;
                Spark(center, a.ToRotationVector2() * speed * Main.rand.NextFloat(0.85f, 1.15f), color,
                    scale * Main.rand.NextFloat(0.8f, 1.2f), Main.rand.Next(18, 30));
            }
        }

        public static void Line(Vector2 from, Vector2 to, Color a, Color b, float s0, float s1, int life, float step = 8f)
        {
            float len = Vector2.Distance(from, to);
            if (len < 1f)
                return;
            Vector2 dir = (to - from) / len;
            for (float d = 0f; d < len; d += step)
            {
                float f = d / len;
                Spark(from + dir * d + Main.rand.NextVector2Circular(2f, 2f), Vector2.Zero,
                    Color.Lerp(a, b, f), MathHelper.Lerp(s0, s1, f), life + Main.rand.Next(-3, 4));
            }
        }

        public static void Explosion(Vector2 c, float radius, int stacks)
        {
            Boom(c, Vector2.Zero, radius * 2.4f, 36);
            int puffs = 4 + stacks * 2;
            for (int i = 0; i < puffs; i++)
            {
                Boom(c + Main.rand.NextVector2Circular(radius * 0.45f, radius * 0.45f),
                    Main.rand.NextVector2Circular(2.5f, 2.5f),
                    radius * Main.rand.NextFloat(0.9f, 1.5f), Main.rand.Next(24, 40));
            }
            Ring(c, Gold, 18 + stacks * 4, 9f + stacks, 12f);
            Ring(c, White, 10, 4.5f, 10f);
            for (int i = 0; i < 8 + stacks * 2; i++)
            {
                Vector2 d = Main.rand.NextVector2CircularEdge(1f, 1f);
                Line(c + d * radius * 0.25f, c + d * radius * Main.rand.NextFloat(0.9f, 1.4f), Gold, Red, 10f, 4f, 14, 9f);
            }
            if (stacks >= PrinceBrandNPC.MaxMarks)
            {
                for (int i = -3; i <= 3; i++)
                    Spark(c, (-Vector2.UnitY).RotatedBy(i * 0.3f) * Main.rand.NextFloat(6f, 11f), Gold, 14f, Main.rand.Next(26, 40));
            }
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = Main.rand.NextFloat(-0.1f, 0.2f), Volume = 0.8f }, c);
            Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                c, Main.rand.NextVector2CircularEdge(1f, 1f), 3f + stacks * 2f, 10f, 10, 900f, "PrinceBoom"));
        }
    }

    public class PrinceBrandNPC : GlobalNPC
    {
        public const int MaxMarks = 3;
        public const int MarkDuration = 900;
        public const int MaxDepth = 4;

        public override bool InstancePerEntity => true;

        public int Marks;
        public int Timer;
        public int Lock;
        public int FuseTimer;
        public int FuseDepth;
        public int FuseOwner = -1;
        public int FuseDamage;
        public float FuseKnock;

        public static bool IsValidTarget(NPC npc)
        {
            if (!npc.active || npc.friendly || npc.townNPC || npc.lifeMax <= 5)
                return false;
            if (npc.type == NPCID.TargetDummy)
                return true;
            return !npc.dontTakeDamage;
        }

        public void AddMark(NPC npc)
        {
            if (Lock > 0)
                return;
            bool wasCrowned = Marks >= MaxMarks;
            Marks = Math.Min(MaxMarks, Marks + 1);
            Timer = MarkDuration;
            if (Main.dedServ)
                return;
            bool crowned = Marks >= MaxMarks;
            PrinceFx.Ring(npc.Center, crowned ? PrinceFx.Gold : PrinceFx.Hot, 8, 3f, 9f);
            if (crowned && !wasCrowned)
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.6f, Volume = 0.7f }, npc.Center);
        }

        public void Schedule(int delay, int owner, int damage, float knockback, int depth)
        {
            if (FuseTimer > 0)
                return;
            FuseTimer = Math.Max(1, delay);
            FuseOwner = owner;
            FuseDamage = damage;
            FuseKnock = knockback;
            FuseDepth = depth;
        }

        public override void PostAI(NPC npc)
        {
            if (Lock > 0)
                Lock--;

            if (FuseTimer > 0)
            {
                if (!Main.dedServ)
                    PrinceFx.Spark(npc.Center + Main.rand.NextVector2Circular(npc.width * 0.4f, npc.height * 0.4f),
                        Vector2.Zero, PrinceFx.White, 12f, 4);
                if (--FuseTimer == 0 && Main.myPlayer == FuseOwner && npc.active)
                    Detonate(npc);
            }

            if (Marks <= 0)
                return;
            if (--Timer <= 0)
            {
                Marks = 0;
                return;
            }
            if (Main.dedServ)
                return;
            if (Timer < 90 && Timer % 6 < 3)
                return;

            bool crowned = Marks >= MaxMarks;
            float spin = Main.GameUpdateCount * (crowned ? 0.16f : 0.1f);
            Vector2 halo = npc.Top + new Vector2(0f, -10f);
            float rx = 10f + npc.width * 0.25f;
            for (int i = 0; i < Marks; i++)
            {
                float a = spin + MathHelper.TwoPi * i / Marks;
                Vector2 p = halo + new Vector2((float)Math.Cos(a) * rx, (float)Math.Sin(a) * rx * 0.35f);
                PrinceFx.Spark(p, Vector2.Zero, crowned ? PrinceFx.Gold : PrinceFx.Hot, crowned ? 9f : 7f, 5);
            }
        }

        private void Detonate(NPC npc)
        {
            if (FuseOwner < 0 || FuseOwner >= Main.maxPlayers)
                return;
            Player owner = Main.player[FuseOwner];
            int stacks = Math.Max(1, Marks);
            bool crowned = stacks >= MaxMarks;
            Vector2 c = npc.Center;
            Marks = 0;
            Timer = 0;
            Lock = 45;
            float radius = 100f + 22f * stacks;
            PrinceFx.Explosion(c, radius, stacks);

            int selfDamage = (int)(FuseDamage * (1.5f + 0.8f * stacks));
            npc.SimpleStrikeNPC(selfDamage, owner.Center.X < c.X ? 1 : -1, crowned, FuseKnock, DamageClass.Melee, true, owner.luck);

            int neighborDamage = (int)(FuseDamage * (0.8f + 0.25f * stacks));
            foreach (NPC other in Main.npc)
            {
                if (other.whoAmI == npc.whoAmI || !other.active || !IsValidTarget(other))
                    continue;
                float dist = Vector2.Distance(other.Center, c);
                if (dist > radius + Math.Max(other.width, other.height) * 0.5f)
                    continue;
                other.SimpleStrikeNPC(neighborDamage, other.Center.X > c.X ? 1 : -1, false, FuseKnock * 0.6f, DamageClass.Melee, true, owner.luck);
                if (!other.active)
                    continue;
                var brand = other.GetGlobalNPC<PrinceBrandNPC>();
                if (brand.Marks > 0 && FuseDepth < MaxDepth)
                    brand.Schedule(5 + (int)(dist / 30f), FuseOwner, FuseDamage, FuseKnock, FuseDepth + 1);
                else
                    brand.AddMark(other);
            }
        }
    }

    public class PrinceDashPlayer : ModPlayer
    {
        private const float Speed = 48f;
        private const float DashLength = 420f;
        private const int MaxDuration = 30;

        public bool Active;
        public Vector2 Direction = Vector2.UnitX;

        private float traveled;
        private int timer;
        private int damage;
        private float knockback;
        private Vector2 lastCenter;
        private readonly HashSet<int> swept = new();

        public override void PreUpdate()
        {
            if (!Active)
                return;
            Player.SetImmuneTimeForAllTypes(8);
            Player.immuneNoBlink = true;
        }

        public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot) => !Active;
        public override bool CanBeHitByProjectile(Projectile proj) => !Active;

        public override void PostUpdateEquips()
        {
            if (Active)
                Player.armorEffectDrawShadow = true;
        }

        public override void UpdateDead() => Active = false;

        public override void PostUpdate()
        {
            if (!Active)
                return;
            if (Player.dead)
            {
                Active = false;
                return;
            }
            StepDash();
        }

        public void StartDash(int dmg, float kb)
        {
            if (Active)
                return;
            Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), Player.Center, Vector2.Zero,
    ModContent.ProjectileType<PrinceRift>(), (int)(dmg * 0.4f), 0f, Player.whoAmI,
    Player.Center.X, Player.Center.Y);
            Vector2 aim = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.UnitX * Player.direction);
            Direction = aim;
            damage = dmg;
            knockback = kb;
            traveled = 0f;
            timer = 0;
            lastCenter = Player.Center;
            swept.Clear();
            Active = true;

            DetonateAllMarked();

            PrinceFx.Ring(Player.Center, PrinceFx.Gold, 18, 6f, 11f);
            for (int i = 0; i < 14; i++)
                PrinceFx.Spark(Player.Center, (-Direction).RotatedBy(Main.rand.NextFloat(-0.5f, 0.5f)) * Main.rand.NextFloat(4f, 11f),
                    PrinceFx.Gold, Main.rand.NextFloat(10f, 18f), Main.rand.Next(16, 28));
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.5f, Volume = 0.9f }, Player.Center);

            Projectile.NewProjectile(Player.GetSource_ItemUse(Player.HeldItem), Player.Center, Direction,
                ModContent.ProjectileType<PrinceDashBlade>(), 0, 0f, Player.whoAmI);
        }

        private void DetonateAllMarked()
        {
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || !PrinceBrandNPC.IsValidTarget(npc))
                    continue;
                var brand = npc.GetGlobalNPC<PrinceBrandNPC>();
                if (brand.Marks <= 0)
                    continue;
                brand.Schedule(1 + Main.rand.Next(0, 4), Player.whoAmI, damage, knockback, 0);
            }
        }

        private void StepDash()
        {
            timer++;
            float remaining = Speed;
            bool blocked = false;

            while (remaining > 0.01f && traveled < DashLength)
            {
                float step = Math.Min(Math.Min(remaining, 8f), DashLength - traveled);
                Vector2 next = Player.Center + Direction * step;
                if (Collision.SolidCollision(next - Player.Size / 2f, Player.width, Player.height))
                {
                    blocked = true;
                    break;
                }
                Player.Center = next;
                traveled += step;
                remaining -= step;
            }

            Player.velocity = Vector2.Zero;
            Player.fallStart = (int)(Player.position.Y / 16f);
            Player.ChangeDir(Direction.X >= 0f ? 1 : -1);

            Vector2 now = Player.Center;
            SweepMarked(lastCenter, now);
            SpawnTrail(lastCenter, now);
            lastCenter = now;

            if (blocked || traveled >= DashLength || timer > MaxDuration)
                End();
        }

        private void SweepMarked(Vector2 a, Vector2 b)
        {
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || !PrinceBrandNPC.IsValidTarget(npc) || swept.Contains(npc.whoAmI))
                    continue;
                var brand = npc.GetGlobalNPC<PrinceBrandNPC>();
                if (brand.Marks <= 0)
                    continue;
                float point = 0f;
                if (!Collision.CheckAABBvLineCollision(npc.position, npc.Size, a, b, 48f, ref point))
                    continue;
                swept.Add(npc.whoAmI);
                if (brand.FuseTimer <= 0)
                    brand.Schedule(2, Player.whoAmI, damage, knockback, 0);
                Vector2 d = Direction;
                PrinceFx.Line(npc.Center - d * 110f, npc.Center + d * 110f, PrinceFx.Gold, PrinceFx.White, 7f, 13f, 14);
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = Math.Min(0.9f, -0.1f + 0.15f * swept.Count), Volume = 0.9f }, npc.Center);
            }
        }

        private void SpawnTrail(Vector2 a, Vector2 b)
        {
            float len = Vector2.Distance(a, b);
            if (len < 1f)
                return;
            Vector2 dir = (b - a) / len;
            for (float d = 0f; d < len; d += 7f)
            {
                Vector2 p = a + dir * d;
                PrinceFx.Spark(p + Main.rand.NextVector2Circular(8f, 8f), Main.rand.NextVector2Circular(0.7f, 0.7f),
                    PrinceFx.Red, Main.rand.NextFloat(14f, 22f), Main.rand.Next(14, 24));
            }
            for (int i = 0; i < 3; i++)
            {
                float side = Main.rand.NextBool() ? 1f : -1f;
                Vector2 v = dir.RotatedBy(MathHelper.PiOver2 * side) * Main.rand.NextFloat(1.5f, 4f) - dir * Main.rand.NextFloat(0.5f, 2f);
                PrinceFx.Spark(b - dir * Main.rand.NextFloat(0f, 20f), v, PrinceFx.Gold, Main.rand.NextFloat(8f, 13f), Main.rand.Next(14, 22));
            }
        }

        private void End()
        {
            Active = false;
            Player.velocity = Direction * 8f;
            Player.fallStart = (int)(Player.position.Y / 16f);
            Player.SetImmuneTimeForAllTypes(20);
            PrinceFx.Ring(Player.Center, PrinceFx.Gold, 22, 7f, 12f);
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.1f, Volume = 0.8f }, Player.Center);
        }
    }

    public class PrinceDashBlade : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Melee/Rapiers/Prince";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 42;
            Projectile.height = 42;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 40;
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
            var dash = player.GetModPlayer<PrinceDashPlayer>();
            if (!dash.Active)
            {
                Projectile.Kill();
                return;
            }
            if (Projectile.owner == Main.myPlayer && Vector2.DistanceSquared(Projectile.velocity, dash.Direction) > 0.0004f)
            {
                Projectile.velocity = dash.Direction;
                Projectile.netUpdate = true;
            }

            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX * player.direction);
            int d = dir.X > 0f ? 1 : -1;
            Projectile.Center = player.Center + dir * 38f;
            Projectile.spriteDirection = d;
            Projectile.direction = d;
            Projectile.rotation = dir.ToRotation() + MathHelper.PiOver4;
            if (d == -1)
                Projectile.rotation += MathHelper.PiOver2;
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(2);
            player.itemRotation = (dir * d).ToRotation();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            int len = Projectile.oldPos.Length;
            for (int k = 1; k < len; k++)
            {
                if (Projectile.oldPos[k] == Vector2.Zero)
                    continue;
                float f = 1f - k / (float)len;
                Main.EntitySpriteDraw(texture, Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition, null,
                    PrinceFx.Gold * (f * 0.5f), Projectile.oldRot[k], origin, Projectile.scale, effects, 0);
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 offset = new Vector2(3f, 0f).RotatedBy(MathHelper.TwoPi * i / 8f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, PrinceFx.Gold * 0.4f,
                    Projectile.rotation, origin, Projectile.scale * 1.04f, effects, 0);
            }
            Main.EntitySpriteDraw(texture, drawPos, null, Color.White, Projectile.rotation, origin, Projectile.scale, effects, 0);
            return false;
        }
    }

    public class PrinceHoldout : BaseRapierHoldout
    {
        public override string RapierTexture => "Waybound/Content/Items/Weapons/Melee/Rapiers/PrinceRapier";
        public override bool UseSpecial => true;
        public override float BladeLength => 70f;
        public override float EmpoweredDistance => 200f;

        protected override float GetThrustDistance(int i) => i switch { 0 => 118f, 1 => 134f, _ => 150f };
        protected override int WindupFrames(int i) => i == 2 ? 12 : 3;
        protected override int RecoverFrames(int i) => i == 2 ? 12 : 5;
        protected override int EndFrames => 6;
        protected override bool IsCorrectItem(Player player) => player.HeldItem.type == ModContent.ItemType<Prince>();

        protected override Color GetLungeParticleColor() => new Color(255, 215, 100, 255);
        protected override Color GetTipParticleColor() => new Color(255, 230, 140, 255);
        protected override Color GetTrailParticleColor() => new Color(255, 205, 80, 200);
        protected override Color GetGlowColor() => new Color(255, 170, 70, 190);
        protected override Color GetGreenPulseColor() => new Color(255, 212, 90, 230);

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitNPC(target, ref modifiers);
            int marks = target.GetGlobalNPC<PrinceBrandNPC>().Marks;
            if (marks > 0)
                modifiers.FinalDamage *= 1f + 0.1f * marks;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            target.GetGlobalNPC<PrinceBrandNPC>().AddMark(target);
        }

        protected override void OnEmpoweredStrike(Player player)
        {
            int dmg = player.GetWeaponDamage(player.HeldItem);
            float kb = player.GetWeaponKnockback(player.HeldItem, player.HeldItem.knockBack);
            player.GetModPlayer<PrinceDashPlayer>().StartDash(dmg, kb);
        }
    }
    public class PrinceRift : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.None;

        private const int LingerTime = 100;
        private const float PointSpacing = 14f;
        private const float HitWidth = 44f;
        private const int MaxPoints = 48;

        private static readonly Color RedGlow = new Color(150, 10, 30, 0);
        private static readonly Color RedEdge = new Color(255, 45, 60, 0);

        private readonly Vector2[] pts = new Vector2[MaxPoints];
        private readonly float[] wid = new float[MaxPoints];

        private int age;
        private int linger;
        private int seed;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 800;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.timeLeft = 5;
        }

        private Vector2 Start => new Vector2(Projectile.ai[0], Projectile.ai[1]);
        private Vector2 End => new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
        private bool Frozen => Projectile.ai[2] == 1f;
        private float Open => Smooth(MathHelper.Clamp(age / 10f, 0f, 1f));
        private float Fade => linger <= 0 ? 1f : 1f - Smooth(MathHelper.Clamp((linger - (LingerTime - 30)) / 30f, 0f, 1f));

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Projectile.timeLeft = 5;
            age++;

            if (age == 1)
                seed = (int)(Projectile.ai[0] * 0.37f + Projectile.ai[1] * 0.71f);

            if (!Frozen)
            {
                if (owner.active && !owner.dead)
                {
                    Projectile.localAI[0] = owner.Center.X;
                    Projectile.localAI[1] = owner.Center.Y;
                }

                if (Projectile.owner == Main.myPlayer &&
                    (!owner.active || owner.dead || !owner.GetModPlayer<PrinceDashPlayer>().Active))
                {
                    Projectile.ai[2] = 1f;
                    Projectile.netUpdate = true;
                }

                if (Frozen && !Main.dedServ)
                    SoundEngine.PlaySound(SoundID.Item104 with { Pitch = -0.6f, Volume = 0.8f }, Projectile.Center);
            }
            else
            {
                if (++linger >= LingerTime)
                {
                    Projectile.Kill();
                    return;
                }
            }

            Vector2 a = Start;
            Vector2 b = End;
            Projectile.Center = (a + b) * 0.5f;

            if (Main.dedServ)
                return;

            float len = Vector2.Distance(a, b);
            if (len < 4f)
                return;

            float fade = Fade * Open;

            for (int i = 0; i < 2; i++)
            {
                if (Main.rand.NextFloat() > fade)
                    continue;

                Vector2 p = Vector2.Lerp(a, b, Main.rand.NextFloat());
                Dust d = Dust.NewDustPerfect(p + Main.rand.NextVector2Circular(8f, 8f), DustID.RedTorch,
                    Main.rand.NextVector2Circular(1.2f, 1.2f), 100, default, Main.rand.NextFloat(1f, 1.6f));
                d.noGravity = true;
            }

            if (Main.rand.NextBool(6))
            {
                Vector2 p = Vector2.Lerp(a, b, Main.rand.NextFloat());
                PrinceFx.Spark(p, Main.rand.NextVector2Circular(1.5f, 1.5f), PrinceFx.Red, Main.rand.NextFloat(8f, 14f), Main.rand.Next(12, 20));
            }

            Lighting.AddLight(Projectile.Center, 0.8f * fade, 0.1f * fade, 0.15f * fade);
            Lighting.AddLight(a, 0.5f * fade, 0.05f * fade, 0.1f * fade);
            Lighting.AddLight(b, 0.5f * fade, 0.05f * fade, 0.1f * fade);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Open < 0.4f || Fade < 0.1f)
                return false;

            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
                Start, End, HitWidth * Open, ref point);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.dedServ)
                return;

            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.RedTorch,
                    Main.rand.NextVector2Circular(4f, 4f), 100, default, 1.5f);
                d.noGravity = true;
            }
            PrinceFx.Ring(target.Center, PrinceFx.Red, 6, 3f, 9f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Vector2 a = Start;
            Vector2 b = End;
            float len = Vector2.Distance(a, b);
            if (len < 4f)
                return false;

            Vector2 dir = (b - a) / len;
            Vector2 perp = new Vector2(-dir.Y, dir.X);
            float open = Open;
            float alpha = Fade;
            float time = Main.GlobalTimeWrappedHourly;

            int n = Math.Min((int)(len / PointSpacing) + 2, MaxPoints);
            for (int i = 0; i < n; i++)
            {
                float d = i == n - 1 ? len : i * PointSpacing;
                float t = d / len;
                float shape = MathF.Pow(MathF.Max(0.05f, MathF.Sin(MathHelper.Pi * t)), 0.5f);
                float jag = Hash(i + seed) * 12f * open * shape + MathF.Sin(time * 7f + i * 1.7f) * 1.5f;
                pts[i] = a + dir * d + perp * jag;
                wid[i] = (4f + 15f * (0.5f + 0.5f * MathF.Abs(Hash(i + seed + 99)))) * MathHelper.Clamp(shape, 0.15f, 1f) * open;
            }

            for (int i = 0; i < n - 1; i++)
                Seg(pts[i], pts[i + 1], (wid[i] + wid[i + 1]) * 1.6f + 6f, RedGlow * (0.4f * alpha));

            for (int i = 3; i < n - 1; i += 3)
                DrawCrack(i, dir, perp, open, true, alpha);

            for (int i = 0; i < n - 1; i++)
                Seg(pts[i], pts[i + 1], (wid[i] + wid[i + 1]) * 0.5f + 1f, Color.Black * alpha);

            for (int i = 0; i < n - 1; i++)
            {
                Vector2 sd = pts[i + 1] - pts[i];
                if (sd.LengthSquared() < 0.01f)
                    continue;
                sd.Normalize();
                Vector2 sp = new Vector2(-sd.Y, sd.X);
                float w = (wid[i] + wid[i + 1]) * 0.25f;
                Seg(pts[i] + sp * w, pts[i + 1] + sp * w, 2f, RedEdge * alpha);
                Seg(pts[i] - sp * w, pts[i + 1] - sp * w, 2f, RedEdge * (0.8f * alpha));
            }

            for (int i = 3; i < n - 1; i += 3)
                DrawCrack(i, dir, perp, open, false, alpha);

            return false;
        }

        private void DrawCrack(int i, Vector2 dir, Vector2 perp, float open, bool black, float alpha)
        {
            float side = Hash(i * 7 + seed) > 0f ? 1f : -1f;
            float length = (14f + 24f * MathF.Abs(Hash(i * 13 + seed))) * open;
            Vector2 mid = pts[i] + perp * side * length * 0.5f + dir * Hash(i * 3 + seed) * 6f;
            Vector2 tip = pts[i] + perp * side * length + dir * Hash(i * 5 + seed) * 12f;

            if (black)
            {
                Seg(pts[i], mid, 5f, Color.Black * alpha);
                Seg(mid, tip, 3f, Color.Black * alpha);
            }
            else
            {
                Seg(pts[i], mid, 1.6f, RedEdge * (0.9f * alpha));
                Seg(mid, tip, 1.2f, RedEdge * (0.7f * alpha));
            }
        }

        private static void Seg(Vector2 from, Vector2 to, float thickness, Color color)
        {
            Vector2 d = to - from;
            float len = d.Length();
            if (len < 0.01f)
                return;

            Texture2D px = TextureAssets.MagicPixel.Value;
            Main.EntitySpriteDraw(px, from - Main.screenPosition, new Rectangle(0, 0, 1, 1), color,
                d.ToRotation(), new Vector2(0f, 0.5f), new Vector2(len + 1.5f, thickness), SpriteEffects.None, 0);
        }

        private static float Hash(int n)
        {
            float s = MathF.Sin(n * 12.9898f) * 43758.5453f;
            return (s - MathF.Floor(s)) * 2f - 1f;
        }

        private static float Smooth(float t) => t * t * (3f - 2f * t);
    }
}