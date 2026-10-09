using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;
using SysVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Items.Weapons.Melee.Other
{
    public class MuspelheimMeteor : ModProjectile
    {
        public override string Texture => "Waybound/Content/Items/Weapons/Melee/Other/MuspelheimSpear";

        private const float GripAlong = 0.25f;

        private SpearTrail trail;

        private float SpearLength => MuspelheimFx.SpriteLength(Type, Projectile.scale);

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.timeLeft = 200;
            Projectile.extraUpdates = 1;
            Projectile.scale = 0.65f;

            trail = new SpearTrail(22);
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            Projectile.tileCollide = Projectile.Center.Y > Projectile.ai[0] - 220f;

            Lighting.AddLight(Projectile.Center, 0.9f, 0.4f, 0.15f);

            if (Main.dedServ)
                return;

            float length = SpearLength;
            Vector2 fwd = Projectile.rotation.ToRotationVector2();
            Vector2 tip = Projectile.Center + fwd * (1f - GripAlong) * length;

            if (Projectile.Center.Y > Projectile.ai[0] - 220f && HitTileAlongSpear(fwd, length))
            {
                Projectile.Kill();
                return;
            }

            trail.Step();
            Vector2 prevTip = tip - Projectile.velocity;
            for (int s = 0; s < SpearTrail.Substeps; s++)
            {
                float t = (s + 1f) / SpearTrail.Substeps;
                trail.Add(Vector2.Lerp(prevTip, tip, t), fwd, (SpearTrail.Substeps - 1 - s) / (float)SpearTrail.Substeps);
            }
            trail.Prune();

            if (Main.rand.NextFloat() < 0.5f)
            {
                float u = MathHelper.Lerp(0.15f, 1f, Main.rand.NextFloat());
                Vector2 pos = Projectile.Center + fwd * (u - GripAlong) * length + Main.rand.NextVector2Circular(4f, 4f);
                Vector2 vel = -fwd * Main.rand.NextFloat(0.5f, 2f) + Main.rand.NextVector2Circular(0.8f, 0.8f);
                MuspelheimFx.SpawnFire(pos, vel, Main.rand.NextFloat(14f, 22f), Main.rand.Next(14, 22));
            }

            if (Main.rand.NextBool(7))
            {
                Dust d = Dust.NewDustPerfect(tip, DustID.Torch, -fwd * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1.5f, 1.5f),
                    100, default, Main.rand.NextFloat(0.9f, 1.3f));
                d.noGravity = true;
            }
        }

        private bool HitTileAlongSpear(Vector2 fwd, float length)
        {
            for (float t = 0.1f; t <= 1f; t += 0.1f)
            {
                Vector2 p = Projectile.Center + fwd * (t - GripAlong) * length;
                int tx = (int)(p.X / 16f);
                int ty = (int)(p.Y / 16f);

                if (tx < 0 || tx >= Main.maxTilesX || ty < 0 || ty >= Main.maxTilesY)
                    continue;

                Tile tile = Main.tile[tx, ty];
                if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                    return true;
            }

            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float point = 0f;
            Vector2 fwd = Projectile.rotation.ToRotationVector2();
            float length = SpearLength;

            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                Projectile.Center - fwd * length * GripAlong,
                Projectile.Center + fwd * length * (1f - GripAlong),
                22f,
                ref point);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            MuspelheimFx.Burst(target.Center, 6, 3.5f, 18f, 28f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ)
                return;

            Vector2 fwd = Projectile.rotation.ToRotationVector2();
            Vector2 impactPos = Projectile.Center + fwd * SpearLength * 0.4f;

            MuspelheimFx.Burst(impactPos, 10, 4.5f, 18f, 30f);

            if (ParticleSystem.FlameBoomBuffer != null)
            {
                ParticleSystem.FlameBoomBuffer.Create(new ParticleInfo(
                    position: impactPos.ToNumerics(),
                    velocity: SysVector2.Zero,
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new SysVector2(70f, 70f),
                    color: new Color(255, 140, 40),
                    duration: 18
                ));

                ParticleSystem.FlameBoomBuffer.Create(new ParticleInfo(
                    position: impactPos.ToNumerics(),
                    velocity: SysVector2.Zero,
                    rotation: Main.rand.NextFloat(MathHelper.TwoPi),
                    scale: new SysVector2(130f, 130f),
                    color: new Color(255, 140, 40),
                    duration: 26
                ));

                ParticleSystem.FlameBoomBuffer.Create(new ParticleInfo(
                    position: (impactPos + new Vector2(0f, -6f)).ToNumerics(),
                    velocity: SysVector2.Zero,
                    rotation: 0f,
                    scale: new SysVector2(180f, 50f),
                    color: new Color(255, 140, 40),
                    duration: 22
                ));
            }

            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.4f, Pitch = 0.3f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            trail.Draw(1f, true, SpearLength);

            Main.instance.LoadProjectile(Type);
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            bool flip = Projectile.velocity.X < 0f;
            SpriteEffects effects = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Vector2 origin = new Vector2(
                texture.Width * (flip ? 1f - GripAlong : GripAlong),
                texture.Height * (1f - GripAlong));

            float rot = Projectile.rotation + (flip ? MathHelper.PiOver4 * 3f : MathHelper.PiOver4);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(texture, drawPos, null, Color.White, rot, origin, Projectile.scale, effects, 0);

            Color glowColor = new Color(255, 140, 50, 0) * 0.7f;
            Main.EntitySpriteDraw(texture, drawPos, null, glowColor, rot, origin, Projectile.scale * 1.08f, effects, 0);

            return false;
        }
    }
}