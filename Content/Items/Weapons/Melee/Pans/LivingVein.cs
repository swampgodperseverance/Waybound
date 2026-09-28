using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Trails;

namespace Waybound.Content.Items.Weapons.Melee.Pans
{
    public class LivingVein : SpinnablePanItem
    {
        public override int SpinProjectileType => ModContent.ProjectileType<LivingVeinProjectile>();
        public override int ThrowProjectileType => ModContent.ProjectileType<LivingVeinThrownProjectile>();

        public override int BaseDamage => 30;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 3);
        }
        public override int SpinUseTime => 15;
        public override int ThrowUseTime => 24;
        public override int ThrowDebuffType => BuffID.Ichor;
        public override int ThrowDebuffDuration => 480;
        public override float ThrowSpeed => 17f;
        public override float Knockback => 8f;
    }

    public class LivingVeinProjectile : SpinnablePanProjectile
    {
        public override int ProjWidth => 32;
        public override int ProjHeight => 26;
        public override float OrbitRadius => 8f;
        public override float SpriteLength => 26f;
        public override float MaxSpinTime => 320f;
        public override float ParticleStart => 40f;
        public override float OutlineStart => 60f;
        public override float MinRotationSpeed => 0.025f;
        public override float MaxRotationSpeed => 0.36f;

        public override float ScaleStart => 1f;
        public override float ScalePeak => 1.4f;
        public override float ScaleAtFade => 0.9f;

        public override int BaseBuffType => BuffID.WellFed;
        public override int BaseBuffDuration => 540;
        public override int BuffInterval => 30;
        public override int BuffStartTime => 65;

        public override Color ParticleColor => new Color(220, 20, 40, 230);
        public override float ParticleSizeMin => 16f;
        public override float ParticleSizeMax => 28f;
        public override float ParticleSizeBonus => 10f;
        public override int ParticleLifeMin => 18;
        public override int ParticleLifeMax => 30;

        public override bool CanShoot => false;

        protected override void UpdateLight(float eased)
        {
            if (Timer <= OutlineStart) return;
            float outlineStrength = MathHelper.Clamp((Timer - OutlineStart) / 60f, 0f, 1f);
            Vector2 lightPos = Projectile.Center + Projectile.rotation.ToRotationVector2() * (OrbitRadius + SpriteLength * 0.5f);
            Lighting.AddLight(lightPos, 1.1f * outlineStrength * Projectile.Opacity, 0.08f * outlineStrength * Projectile.Opacity, 0.1f * outlineStrength * Projectile.Opacity);
        }

        protected override void DrawGlow(Texture2D texture, Vector2 origin, SpriteEffects effects, Vector2 drawPos, float drawRotation)
        {
            if (Timer <= OutlineStart) return;
            float strength = MathHelper.Clamp((Timer - OutlineStart) / 50f, 0f, 1f) * Projectile.Opacity;
            Color glow = new Color(220, 20, 40, 0) * strength * 0.65f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2.6f * strength, 0f).RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 3.5f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, glow, drawRotation, origin, Projectile.scale * 1.06f, effects, 0);
            }
        }

        protected override void DrawTrail(Texture2D texture, Vector2 origin, SpriteEffects effects)
        {
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float fade = 1f - i / (float)Projectile.oldPos.Length;
                Color trail = new Color(220, 20, 40, 0) * 0.35f * fade * Projectile.Opacity;
                Vector2 oldOrbitDir = Projectile.oldRot[i].ToRotationVector2();
                Vector2 oldDrawCenter = Projectile.oldPos[i] + Projectile.Size / 2f + oldOrbitDir * OrbitRadius;

                float oldDrawRotation;
                if (Projectile.spriteDirection == 1)
                    oldDrawRotation = Projectile.oldRot[i] - MathHelper.PiOver2 + MathHelper.Pi;
                else
                    oldDrawRotation = Projectile.oldRot[i];

                Main.EntitySpriteDraw(texture, oldDrawCenter - Main.screenPosition, null, trail, oldDrawRotation, origin, Projectile.scale * (1f - i * 0.03f), effects, 0);
            }
        }
    }

    public class LivingVeinThrownProjectile : ThrownPanProjectile
    {
        public override int ProjWidth => 32;
        public override int ProjHeight => 26;

        public override float MaxOutTime => 50f;
        public override float MaxRange => 480f;
        public override float ReturnAccel => 1.1f;
        public override float ReturnMaxSpeed => 24f;
        public override float SpinSpeed => 0.4f;
        public override int AirTime => 600;

        public override Color ParticleColor => new Color(220, 20, 40, 230);
        public override float ParticleSizeMin => 12f;
        public override float ParticleSizeMax => 22f;
        public override int ParticleLifeMin => 16;
        public override int ParticleLifeMax => 28;

        public override string[] HitWords => new[] { "OMG", "BOOM", "DAMN" };
        public override Color HitTextColor => new Color(220, 20, 40);

        private int spawnTimer;

        public override void AI()
        {
            base.AI();

            if (Returning != 0f) return;
            if (Main.myPlayer != Projectile.owner) return;

            spawnTimer++;
            if (spawnTimer >= 20)
            {
                spawnTimer = 0;
                SpawnVeinProj(1);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);

            if (Main.myPlayer != Projectile.owner) return;
            if (!target.active || target.CountsAsACritter || target.immortal || target.dontTakeDamage) return;

            SpawnVeinProj(3);
        }

        private void SpawnVeinProj(int count)
        {
            int veinType = ModContent.ProjectileType<LivingVeinProj>();

            for (int i = 0; i < count; i++)
            {
                Vector2 spawn = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f).SafeNormalize(Vector2.UnitX);
                float speed = Main.rand.NextFloat(8f, 13f);

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawn,
                    dir * speed,
                    veinType,
                    Projectile.damage / 2,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 origin = new(Projectile.spriteDirection == 1 ? texture.Width : 0f, texture.Height);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color color = Color.White * Projectile.Opacity;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float fade = 1f - i / (float)Projectile.oldPos.Length;
                Color trail = new Color(220, 20, 40, 0) * 0.45f * fade * Projectile.Opacity;
                Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, trail, Projectile.oldRot[i], origin, Projectile.scale, effects, 0);
            }

            Color glow = new Color(220, 20, 40, 0) * 0.8f * Projectile.Opacity;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2.4f, 0f).RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 3.5f);
                Main.EntitySpriteDraw(texture, drawPos + offset, null, glow, Projectile.rotation, origin, Projectile.scale * 1.06f, effects, 0);
            }

            Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, origin, Projectile.scale, effects, 0);
            return false;
        }
    }

    public class LivingVeinProj : ModProjectile
    {
        private int tickCounter;
        private Vector2 baseVelocity;
        private PrimDrawer visualTrail;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.hostile = false;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = false;
            Projectile.aiStyle = -1;
            Projectile.idStaticNPCHitCooldown = 15;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.netUpdate = true;
            Projectile.GetGlobalProjectile<TarHelper>().Cangoback = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            float randomOffset = Main.rand.NextFloat(-200f, 200f);
            Vector2 spawn = new Vector2(Main.MouseWorld.X + randomOffset, Main.screenPosition.Y + Main.screenHeight + 50f);
            Projectile.position = spawn - Projectile.Size / 2f;

            Vector2 targetDir = Main.MouseWorld - Projectile.Center;
            targetDir = targetDir.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-10f, 10f)));
            baseVelocity = Vector2.Normalize(targetDir) * 16f;
            Projectile.velocity = baseVelocity;

            MiscShaderData shader = null;
            string[] keys = { "FlameLashTrailColorGradient", "FlameLashTrailShape", "FlameLashTrailErosion" };
            foreach (var k in keys)
                if (GameShaders.Misc.TryGetValue(k, out shader)) break;

            if (shader != null)
            {
                shader.UseImage1("Images/Misc/noise");
                shader.UseOpacity(0.8f);
                shader.UseColor(new Color(160, 10, 20));
                shader.UseSecondaryColor(new Color(220, 30, 50));
            }

            visualTrail = new PrimDrawer(
                widthFunc: t => MathHelper.Lerp(3f, 0.3f, t),
                colorFunc: t =>
                {
                    Color start = new Color(160, 10, 20);
                    Color end = new Color(220, 30, 50);
                    Color c = Color.Lerp(start, end, t);
                    c *= (1f - t);
                    return c;
                },
                shader: shader
            );
        }

        public override void AI()
        {
            tickCounter++;
            Projectile.rotation -= 0.1f;

            if (tickCounter == 1 && Main.myPlayer == Projectile.owner)
                Projectile.velocity = baseVelocity;

            if (tickCounter is >= 50 and <= 68)
                Projectile.velocity *= 0.92f;

            if (tickCounter == 68)
                Projectile.velocity = Vector2.Zero;

            if (tickCounter > 68)
            {
                Projectile.velocity.Y += 1f;
                Projectile.velocity.X *= 0.5f;
                Projectile.tileCollide = true;
            }

            Lighting.AddLight(Projectile.Center, 0.9f, 0.1f, 0.15f);

            if (Main.rand.NextBool(3))
            {
                int d = Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Blood,
                    -Projectile.velocity.X * 0.2f,
                    -Projectile.velocity.Y * 0.2f,
                    100,
                    new Color(220, 20, 40),
                    1.2f
                );
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.3f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            Projectile.NewProjectile(
                Projectile.GetSource_Death(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<TarSpikeFlash>(),
                0,
                0,
                Projectile.owner
            );

            for (int i = 0; i < 20; i++)
            {
                int d = Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Blood,
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-2f, 2f),
                    150,
                    new Color(220, 20, 40),
                    1.3f
                );
                Main.dust[d].noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.NewProjectile(
                Projectile.GetSource_OnHit(target),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<TarSpikeFlash>(),
                0,
                0,
                Projectile.owner
            );
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (visualTrail != null)
            {
                List<Vector2> points = Projectile.oldPos
                    .Where(v => v != Vector2.Zero)
                    .Select(v => v + Projectile.Size / 2f)
                    .ToList();

                if (points.Count > 1)
                {
                    Vector2 offset = -Main.screenPosition;
                    visualTrail.DrawPrims(points, offset, totalTrailPoints: 20);
                }
            }

            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            Color glow = new Color(220, 20, 40, 0) * 0.6f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2f, 0f).RotatedBy(MathHelper.TwoPi * i / 4f + Main.GlobalTimeWrappedHourly * 3.5f);
                Main.EntitySpriteDraw(tex, pos + offset, null, glow, Projectile.rotation, tex.Size() / 2f, Projectile.scale * 1.06f, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, pos, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public class TarSpikeFlash : ModProjectile
        {
            public override string Texture => "Waybound/Assets/Textures/Star";

            public override void SetDefaults()
            {
                Projectile.width = 120;
                Projectile.height = 120;
                Projectile.timeLeft = 8;
                Projectile.penetrate = -1;
                Projectile.tileCollide = false;
                Projectile.hostile = false;
                Projectile.friendly = false;
            }

            public override void AI()
            {
                Projectile.scale += 0.15f;
                Projectile.alpha += 30;
            }

            public override bool PreDraw(ref Color lightColor)
            {
                Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

                Color c = new Color(255, 40, 60, 0) * (1f - Projectile.alpha / 255f);

                Main.EntitySpriteDraw(
                    tex,
                    Projectile.Center - Main.screenPosition,
                    null,
                    c,
                    0f,
                    tex.Size() / 2f,
                    Projectile.scale,
                    SpriteEffects.None
                );

                return false;
            }
        }

        public class TarHelper : GlobalProjectile
        {
            public override bool InstancePerEntity => true;
            public bool Cangoback = false;
        }
    }
}