using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Helpers;

namespace Waybound.Content.Items.Weapons.Melee.Other
{
    public class MuspelheimSpear : ModProjectile
    {
        private const float GripAlong = 0.25f;
        private const int TrailLife = 34;

        private const float ChargePerSwingHit = 6f;
        private const float ChargePerThrustHit = 9f;

        private SpearTrail trail;

        private float progress;
        private int maxTime;
        private float arc;
        private float extension;
        private bool isThrust;
        private int dir = 1;

        private bool hasPrev;
        private Vector2 prevHand;
        private float prevRot;

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
            Projectile.timeLeft = 70;
            Projectile.extraUpdates = 1;
            Projectile.scale = 1.2f;

            trail = new SpearTrail(TrailLife);
        }

        public override void Unload()
        {
            SpearTrail.Unload();
        }

        private float SpearLength => MuspelheimFx.SpriteLength(Type, Projectile.scale);

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.dead || !player.active)
            {
                Projectile.Kill();
                return;
            }

            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;

            if (maxTime == 0)
            {
                maxTime = (int)(player.itemAnimationMax * 1.35f) * (Projectile.extraUpdates + 1);
                Projectile.timeLeft = maxTime;

                dir = Projectile.velocity.X >= 0f ? 1 : -1;

                var mp = player.GetModPlayer<MuspelheimPlayer>();
                int step = mp.Combo % 4;
                isThrust = step % 2 == 1;
                mp.Combo++;
                mp.ComboTimer = 70;

                if (isThrust)
                {
                    arc = 0f;
                    SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -0.15f }, player.Center);
                }
                else if (step == 0)
                {
                    arc = 5.6f * dir;
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.35f, Volume = 1.1f }, player.Center);
                }
                else
                {
                    arc = -5.6f * dir;
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.05f, Volume = 1.1f }, player.Center);
                }
            }

            progress = 1f - (float)Projectile.timeLeft / maxTime;

            if (isThrust)
            {
                extension = progress < 0.4f
                    ? EaseFunctions.EaseOutCubic(progress / 0.4f)
                    : 1f - EaseFunctions.EaseInCubic((progress - 0.4f) / 0.6f);
            }
            else
            {
                if (progress < 0.38f)
                    extension = EaseFunctions.EaseOutBack(progress / 0.38f);
                else if (progress < 0.72f)
                    extension = 1f;
                else
                    extension = 1f - EaseFunctions.EaseInCubic((progress - 0.72f) / 0.28f);
            }

            CalculateMovement(arc, out Vector2 center, out float rotation);
            Projectile.Center = center;
            Projectile.rotation = rotation;

            if (!hasPrev)
            {
                prevHand = center;
                prevRot = rotation;
                hasPrev = true;
            }

            player.ChangeDir(dir);
            player.itemRotation = MathF.Atan2(Projectile.rotation.ToRotationVector2().Y * dir, Projectile.rotation.ToRotationVector2().X * dir);
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

            RecordTrail();
            SpawnFireEffects();
        }

        private void RecordTrail()
        {
            if (Main.dedServ)
                return;

            trail.Step();

            float length = SpearLength;
            float rotDiff = MathHelper.WrapAngle(Projectile.rotation - prevRot);

            for (int s = 0; s < SpearTrail.Substeps; s++)
            {
                float t = (s + 1f) / SpearTrail.Substeps;
                Vector2 hand = Vector2.Lerp(prevHand, Projectile.Center, t);
                float rot = prevRot + rotDiff * t;
                Vector2 fwd = rot.ToRotationVector2();

                trail.Add(hand + fwd * (1f - GripAlong) * length, fwd, (SpearTrail.Substeps - 1 - s) / (float)SpearTrail.Substeps);
            }

            trail.Prune();
        }

        private void SpawnFireEffects()
        {
            if (Main.dedServ)
                return;

            float edge = MathHelper.Clamp(MathF.Min(progress, 1f - progress) / 0.1f, 0f, 1f);

            float length = SpearLength;
            float rotDiff = MathHelper.WrapAngle(Projectile.rotation - prevRot);

            int substeps = isThrust ? 2 : 3;
            int perStep = isThrust ? 1 : 3;

            for (int s = 0; s < substeps; s++)
            {
                float t = (s + 1f) / substeps;
                Vector2 hand = Vector2.Lerp(prevHand, Projectile.Center, t);
                float rot = prevRot + rotDiff * t;
                Vector2 forward = rot.ToRotationVector2();
                Vector2 tangent = (rot + MathHelper.PiOver2 * (isThrust ? 0 : Math.Sign(arc))).ToRotationVector2();

                for (int i = 0; i < perStep; i++)
                {
                    if (Main.rand.NextFloat() > edge)
                        continue;

                    float u = MathHelper.Lerp(0.15f, 1f, MathF.Pow(Main.rand.NextFloat(), 0.8f));
                    Vector2 pos = hand + forward * (u - GripAlong) * length + Main.rand.NextVector2Circular(4f, 4f);

                    Vector2 vel;
                    if (isThrust)
                        vel = -forward * Main.rand.NextFloat(0.4f, 1.8f) + Main.rand.NextVector2Circular(0.8f, 0.8f);
                    else
                        vel = tangent * Main.rand.NextFloat(0.4f, 1.8f) + Main.rand.NextVector2Circular(0.6f, 0.6f);

                    float sizeMul = isThrust ? 0.7f : 1f;
                    float size = Main.rand.NextFloat(16f, 26f) * MathHelper.Lerp(0.7f, 1.25f, u) * sizeMul;
                    MuspelheimFx.SpawnFire(pos, vel, size, Main.rand.Next(18, 30));
                }
            }

            if (edge > 0.3f && Main.rand.NextBool(isThrust ? 5 : 3))
            {
                Vector2 forward = Projectile.rotation.ToRotationVector2();
                Vector2 tangent = (Projectile.rotation + MathHelper.PiOver2 * (isThrust ? 0 : Math.Sign(arc))).ToRotationVector2();
                Vector2 pos = Projectile.Center + forward * length * Main.rand.NextFloat(0.5f, 1f - GripAlong);
                Vector2 vel = (isThrust ? -forward : tangent) * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustPerfect(pos, DustID.Torch, vel, 100, default, Main.rand.NextFloat(1f, 1.5f));
                d.noGravity = true;
            }

            prevHand = Projectile.Center;
            prevRot = Projectile.rotation;
        }

        private void CalculateMovement(float arcScale, out Vector2 position, out float rotation)
        {
            Player player = Main.player[Projectile.owner];

            float t = EaseFunctions.EaseOutQuint(progress);
            float rotate = arcScale == 0f ? 0f : -arcScale * 0.55f + t * arcScale;

            rotation = Projectile.velocity.ToRotation() + rotate;

            float hand = isThrust ? (14f + extension * 30f) : (14f + extension * 8f);

            position = player.MountedCenter +
                       new Vector2(0f, player.gfxOffY) +
                       rotation.ToRotationVector2() * hand;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            Vector2 dir2 = Projectile.rotation.ToRotationVector2();
            float length = SpearLength;

            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                Projectile.Center - dir2 * length * GripAlong,
                Projectile.Center + dir2 * length * (1f - GripAlong),
                34f,
                ref point);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer && target.lifeMax > 5 && !target.friendly)
            {
                Main.player[Projectile.owner].GetModPlayer<MuspelheimPlayer>()
                    .AddCharge(isThrust ? ChargePerThrustHit : ChargePerSwingHit);
            }

            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust.NewDustPerfect(target.Center, DustID.Torch, vel, 100, default, 1.6f).noGravity = true;
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                MuspelheimFx.SpawnFire(target.Center + Main.rand.NextVector2Circular(8f, 8f), vel,
                    Main.rand.NextFloat(22f, 34f), Main.rand.Next(24, 40));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float global = MathHelper.Clamp(progress / 0.06f, 0f, 1f) * MathHelper.Clamp((1f - progress) / 0.12f, 0f, 1f);
            trail.Draw(global, isThrust, SpearLength);

            Main.instance.LoadProjectile(Type);
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            bool flip = dir == -1;
            SpriteEffects effects = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Vector2 origin = new Vector2(
                texture.Width * (flip ? 1f - GripAlong : GripAlong),
                texture.Height * (1f - GripAlong));

            float rot = Projectile.rotation + (flip ? MathHelper.PiOver4 * 3f : MathHelper.PiOver4);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, rot, origin, Projectile.scale, effects, 0);

            Color glowColor = new Color(255, 140, 50, 0) * 0.55f;
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor, rot, origin, Projectile.scale * 1.05f, effects, 0);

            return false;
        }
    }
}