using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Misc;
using Waybound.Helpers;
using Waybound.Particles;

namespace Waybound.Content.Items.Weapons.Melee.Swords
{

    //TODO
    public class BattleaxeOfDesertHunter : ModItem
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.damage = 90;
            Item.DamageType = DamageClass.Melee;
            Item.width = 64;
            Item.height = 64;
            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 8f;
            Item.value = Item.sellPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<BattleaxeOfDesertHunterP>();
            Item.shootSpeed = 1f;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient<DesertCore>(1)
                .AddIngredient<DesertWreckage>(12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class BattleaxeOfDesertHunterP : ModProjectile
    {
        public Player Owner => Main.player[Projectile.owner];
        public float SwingDirection;
        public float AngleToMouse;
        public float InitialTimeLeft;

        private const float MaxRotation = MathHelper.Pi;
        private const float BaseScale = 1.25f;
        private const float SwingRadius = 25f;
        private const float SpriteAngleOffset = 0f;
        private static readonly Vector2 GripOffset = new Vector2(0f, 6f);

        private bool firstFrame = true;
        private bool hasExploded = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 30;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.hide = true;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.scale = BaseScale;
        }

        public override void AI()
        {
            if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
            {
                Projectile.Kill();
                return;
            }

            Owner.heldProj = Projectile.whoAmI;

            if (firstFrame)
            {
                Projectile.scale = BaseScale;
                Projectile.Size *= Projectile.scale;
                Projectile.timeLeft = Owner.itemAnimationMax;
                InitialTimeLeft = Projectile.timeLeft;
                SwingDirection = Main.MouseWorld.X >= Owner.MountedCenter.X ? 1f : -1f;
                AngleToMouse = (Main.MouseWorld - Owner.MountedCenter).ToRotation();
                firstFrame = false;
            }

            float curRotProgress = 1f - Projectile.timeLeft / Math.Max(1f, InitialTimeLeft);
            float easedRotProgress = EaseFunctions.EaseOutCubic(curRotProgress);

            float scaleMult = 1f;
            if (curRotProgress < 0.35f)
            {
                float t = curRotProgress / 0.35f;
                scaleMult = MathHelper.Lerp(0.5f, 1f, t);
            }
            else if (curRotProgress > 0.6f)
            {
                float t = (curRotProgress - 0.6f) / 0.4f;
                scaleMult = MathHelper.Lerp(1f, 0.7f, t);
            }
            Projectile.scale = BaseScale * scaleMult;

            Vector2 handPosition = Owner.RotatedRelativePoint(Owner.MountedCenter.Floor())
                + new Vector2(Owner.direction * 4f, -6f);

            float initialRotOffset = MaxRotation * 0.5f;
            float swingAngle = (MaxRotation * easedRotProgress - initialRotOffset) * SwingDirection * Owner.gravDir;

            Vector2 swingDir = AngleToMouse.ToRotationVector2().RotatedBy(swingAngle);
            Vector2 gripWorldPos = handPosition + swingDir * SwingRadius;

            Projectile.spriteDirection = 1;
            Vector2 grip = GripOffset;
            Projectile.Center = gripWorldPos - grip.RotatedBy(swingDir.ToRotation()) * Projectile.scale;
            Projectile.rotation = Projectile.AngleFrom(handPosition) + SpriteAngleOffset;

            float armAngle = swingDir.ToRotation();
            Owner.SetCompositeArmFront(
                true,
                Player.CompositeArmStretchAmount.Full,
                armAngle - MathHelper.PiOver2
            );
            Owner.ChangeDir(SwingDirection >= 0 ? 1 : -1);
            Owner.itemRotation = armAngle * Owner.direction;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;

            EmitParticles(handPosition, SwingRadius, curRotProgress, easedRotProgress);
        }

        private void EmitParticles(Vector2 handPosition, float swingRadius, float rotationProgress, float easedRotationProgress)
        {
            int count = (int)MathHelper.Lerp(1f, 4f, easedRotationProgress);
            for (int i = 0; i < count; i++)
            {
                Vector2 spawnPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);

                ParticleManager.NewParticle<FlameParticleOld>(
                    spawnPos,
                    vel,
                    new Color(255, 200, 120),
                    Main.rand.NextFloat(0.15f, 0.3f) * Projectile.scale,
                    1f
                );

                if (Main.rand.NextBool(4))
                {
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        spawnPos.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(4f, 8f)),
                        new Color(255, 160, 50, 200),
                        Main.rand.Next(16, 28)
                    ));
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (targetHitbox.Intersects(projHitbox))
                return true;

            float point = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                Owner.MountedCenter,
                Projectile.Center,
                24f * Projectile.scale,
                ref point
            );
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float diff = target.Center.X - Owner.Center.X;
            modifiers.HitDirectionOverride = diff > 0 ? 1 : -1;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (hasExploded)
                return;
            hasExploded = true;
            Explode(target.Center);
        }

        private void Explode(Vector2 center)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.1f, Volume = 0.9f }, center);

            for (int i = 0; i < 40; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 11f);
                Vector2 pos = center + Main.rand.NextVector2Circular(10f, 10f);

                ParticleManager.NewParticle<FlameParticleOld>(
                    pos,
                    vel,
                    new Color(255, 210, 130),
                    Main.rand.NextFloat(0.2f, 0.5f),
                    1f
                );

                if (Main.rand.NextBool(2))
                {
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        pos.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new System.Numerics.Vector2(Main.rand.NextFloat(6f, 16f)),
                        new Color(255, 160, 50, 200),
                        Main.rand.Next(16, 28)
                    ));
                }
            }

            for (int i = 0; i < 10; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                Vector2 pos = center + Main.rand.NextVector2Circular(16f, 16f);

                ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                    pos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new System.Numerics.Vector2(Main.rand.NextFloat(12f, 22f)),
                    new Color(255, 120, 40, 220),
                    Main.rand.Next(20, 34)
                ));
            }

            for (int i = 0; i < 8; i++)
            {
                int dust = Dust.NewDust(center, 20, 20, DustID.Torch, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 100, default, Main.rand.NextFloat(1.1f, 1.8f));
                Main.dust[dust].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Rectangle frame = tex.Frame();
            Vector2 origin = frame.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            SpriteEffects flip = SpriteEffects.None;

            float progress = 1f - Projectile.timeLeft / Math.Max(1f, InitialTimeLeft);
            float pulse = (float)Math.Sin(progress * MathHelper.Pi) * 0.6f;

            Texture2D trailTex = ModContent.Request<Texture2D>(Texture + "_Trail").Value;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;

                Vector2 oldDrawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float alpha = 1f - i / (float)Projectile.oldPos.Length;

                Main.EntitySpriteDraw(
                    trailTex,
                    oldDrawPos,
                    frame,
                    new Color(255, 190, 100) * 0.5f * alpha,
                    Projectile.oldRot[i],
                    origin,
                    Projectile.scale,
                    flip,
                    0f
                );
            }

            int layers = 4;
            for (int i = layers; i > 0; i--)
            {
                float offset = i * 2.5f * pulse;
                float alpha = (1f - i / (float)layers) * pulse * 0.5f;
                Color outlineColor = new Color(255, 60, 40) * alpha;

                for (int d = 0; d < 8; d++)
                {
                    float angle = MathHelper.TwoPi * d / 8f;
                    Vector2 dir = angle.ToRotationVector2() * offset;
                    Main.EntitySpriteDraw(
                        tex,
                        drawPos + dir,
                        frame,
                        outlineColor,
                        Projectile.rotation,
                        origin,
                        Projectile.scale,
                        flip,
                        0f
                    );
                }
            }

            Main.EntitySpriteDraw(
                tex,
                drawPos,
                frame,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                flip,
                0f
            );

            return false;
        }

        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = Terraria.Enums.TileCuttingContext.AttackProjectile;
            Utils.TileActionAttempt cut = new(DelegateMethods.CutTiles);
            Utils.PlotTileLine(Projectile.Center, Owner.MountedCenter, Projectile.width, cut);
        }
    }
}