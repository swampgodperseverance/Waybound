using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.ModSystems;
using Waybound.Content.Buffs.Minions;
using Waybound.Content.Items.Ammo.Rockets;
using Waybound.Content.Items.Armor.Generic.DesertHunter;
using Waybound.Particles;

namespace Waybound.Content.Projectiles.Armor
{
    public class DesertHunterArmorDrone : ModProjectile
    {
        private float gunPulse;
        private float outlinePulse;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 22;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.minion = false;
            Projectile.scale = 1.1f;
            Main.projFrames[Projectile.type] = 4;
            Projectile.alpha = 0;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Projectile.localAI[0] = 1;
            base.OnSpawn(source);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!CheckActive(player))
                return;

            if (VanillaKeybinds.ArmorSetBonusActivation.JustPressed && player.GetModPlayer<ArmorOfDesertHunter>().armorOfDesertConqueror == 1)
            {
                Projectile.tileCollide = !Projectile.tileCollide;
                Projectile.localAI[0] *= -1;
            }

            Lighting.AddLight(Projectile.Center, 1f, 0.65f, 0.5f);
            Movement(player);
            Visual();

            if (Projectile.localAI[0] == -1)
            {
                FightMode(player);
                AttackTrail();
                outlinePulse = MathHelper.Lerp(outlinePulse, 0f, 0.12f);
            }
            else
            {
                ScoutMode();
                outlinePulse = MathHelper.Lerp(outlinePulse, 0.55f + 0.45f * (0.5f + 0.5f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3.2f)), 0.1f);
                gunPulse = MathHelper.Lerp(gunPulse, 0f, 0.12f);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<DesertHunterArmorDroneB>());
                return false;
            }
            if (owner.HasBuff(ModContent.BuffType<DesertHunterArmorDroneB>()))
                Projectile.timeLeft = 2;
            return true;
        }

        private void Movement(Player player)
        {
            float speed = 8f;
            float inertia = 20f;
            Vector2 mousePos = Main.MouseWorld;
            Vector2 playerToMouse = (mousePos - player.MountedCenter).SafeNormalize(Vector2.UnitX) * 5f;

            while (Vector2.Distance(player.MountedCenter, mousePos) > 300)
                mousePos -= playerToMouse;

            Vector2 targetPos = Projectile.localAI[0] == 1 ? mousePos : player.MountedCenter;
            Vector2 toTarget = targetPos - Projectile.Center;
            Vector2 toPlayer = player.MountedCenter - Projectile.Center;
            float distance = toTarget.Length();

            if (toPlayer.Length() > 1000f)
            {
                Projectile.Center = player.position;
            }
            else if (toPlayer.Length() > 500f)
            {
                if (speed < 80f)
                    speed += 0.5f;
                else
                    speed = 80f;
                inertia = 40f;
                if (Projectile.localAI[0] == 1)
                    Projectile.tileCollide = false;
            }
            else
            {
                if (speed > 5)
                    speed -= 0.1f;
                else
                    speed = 8;
                inertia = 60f;
                if (Projectile.localAI[0] == 1)
                    Projectile.tileCollide = true;
            }

            if (distance > 20f)
            {
                toTarget.Normalize();
                toTarget *= speed;
                Projectile.velocity = (Projectile.velocity * (inertia - 1) + toTarget) / inertia;
            }
            else if (Projectile.velocity == Vector2.Zero)
            {
                Projectile.velocity.X = -0.15f;
                Projectile.velocity.Y = -0.15f;
            }
        }

        private void FightMode(Player player)
        {
            Projectile.ai[1]++;
            if (Main.mouseLeft && Projectile.ai[1] > 100)
            {
                Vector2 baseDir = (Main.MouseWorld - Projectile.Center).SafeNormalize(Vector2.UnitX);
                for (int i = 0; i < 2; i++)
                {
                    float spread = i == 0 ? -8f : 8f;
                    Vector2 vel = baseDir.RotatedBy(MathHelper.ToRadians(spread) + Main.rand.NextFloat(-0.04f, 0.04f)) * Main.rand.NextFloat(5.5f, 7.2f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel, ModContent.ProjectileType<FlamingDuneRocketP>(), 20, 3, Main.myPlayer);
                }
                Projectile.ai[1] = 0;
                gunPulse = 1f;
            }

            if (gunPulse > 0f)
                gunPulse = MathHelper.Lerp(gunPulse, 0f, 0.1f);
        }

        private void AttackTrail()
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            if (Projectile.velocity.Length() < 1.5f)
                return;
            if (!Main.rand.NextBool(2))
                return;

            Vector2 pos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(4f, 8f);
            Vector2 vel = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.35f, 0.35f);

            ParticleManager.NewParticle<FlameParticleOld>(
                pos,
                vel,
                Color.White,
                0.28f,
                1f
            );
        }

        private void ScoutMode()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            int startX = (int)(Main.screenPosition.X / 16f) - 2;
            int endX = (int)((Main.screenPosition.X + Main.screenWidth) / 16f) + 2;
            int startY = (int)(Main.screenPosition.Y / 16f) - 2;
            int endY = (int)((Main.screenPosition.Y + Main.screenHeight) / 16f) + 2;

            for (int n = 0; n < 28; n++)
            {
                int x = Main.rand.Next(startX, endX + 1);
                int y = Main.rand.Next(startY, endY + 1);
                if (!WorldGen.InWorld(x, y))
                    continue;

                Tile tile = Framing.GetTileSafely(x, y);
                if (!tile.HasTile)
                    continue;

                bool isOre = Main.tileSpelunker[tile.TileType] || TileID.Sets.Ore[tile.TileType];
                bool isChest = TileID.Sets.BasicChest[tile.TileType] || TileID.Sets.BasicChestFake[tile.TileType];
                if (!isOre && !isChest)
                    continue;

                Vector2 pos = new Vector2(x * 16 + 8, y * 16 + 8) + Main.rand.NextVector2Circular(4f, 4f);
                Vector2 vel = Main.rand.NextVector2Circular(0.25f, 0.25f);
                Color col = isChest ? new Color(255, 230, 90) : new Color(255, 190, 70);

                ParticleManager.NewParticle<FlameParticleOld>(
                    pos,
                    vel,
                    col,
                    isChest ? 0.7f : 0.5f,
                    1f
                );

                if (Main.rand.NextBool(3))
                {
                    ParticleManager.NewParticle<FlameParticleOld>(
                        pos + Main.rand.NextVector2Circular(6f, 6f),
                        vel * 0.5f,
                        col,
                        isChest ? 0.45f : 0.32f,
                        1f
                    );
                }
            }
        }

        private void Visual()
        {
            if (Projectile.velocity.X != 0)
                Projectile.spriteDirection = Projectile.velocity.X < 0 ? -1 : 1;

            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.frame = 0;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            var bodyEffects = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            int height = texture.Height / Main.projFrames[Projectile.type];
            int y = height * Projectile.frame;
            Rectangle bodyRect = new(0, y, 46, height);
            Vector2 bodyOrigin = new Vector2(23, 13);
            Vector2 pos = Projectile.Center - Main.screenPosition;

            Color bodyColor = Projectile.GetAlpha(lightColor);

            if (outlinePulse > 0.05f && Projectile.localAI[0] == 1)
            {
                Color glow = new Color(255, 200, 60, 0) * outlinePulse * 0.7f;
                float scaleAdd = 1f + outlinePulse * 0.12f;
                for (int i = 0; i < 6; i++)
                {
                    Vector2 offset = new Vector2(3f * outlinePulse, 0f).RotatedBy(MathHelper.TwoPi * i / 6f + Main.GlobalTimeWrappedHourly * 2f);
                    Main.EntitySpriteDraw(texture, pos + offset, bodyRect, glow, Projectile.velocity.X * 0.1f, bodyOrigin, Projectile.scale * scaleAdd, bodyEffects, 0);
                }
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new Vector2(5.5f * outlinePulse, 0f).RotatedBy(MathHelper.TwoPi * i / 4f);
                    Main.EntitySpriteDraw(texture, pos + offset, bodyRect, glow * 0.4f, Projectile.velocity.X * 0.1f, bodyOrigin, Projectile.scale * (scaleAdd + 0.05f), bodyEffects, 0);
                }
            }

            Main.EntitySpriteDraw(texture, pos, bodyRect, bodyColor, Projectile.velocity.X * 0.1f, bodyOrigin, Projectile.scale, bodyEffects, 0);

            Rectangle gunRect = new(48, 0, 24, 18);
            Vector2 gunOrigin = new Vector2(12, 9);

            float needfulRot = (Main.MouseWorld - Projectile.Center).ToRotation();
            if (needfulRot < 0)
                needfulRot += MathHelper.TwoPi;
            if (Projectile.rotation < MathHelper.Pi / 2 && needfulRot > MathHelper.TwoPi / 3)
                Projectile.rotation += MathHelper.TwoPi;
            else if (needfulRot < MathHelper.Pi / 2 && Projectile.rotation > MathHelper.TwoPi / 3)
                Projectile.rotation -= MathHelper.TwoPi;
            Projectile.rotation = MathHelper.Lerp(Projectile.rotation, needfulRot, 0.1f);

            Color gunColor = bodyColor;
            if (gunPulse > 0.05f)
            {
                gunColor = Color.Lerp(bodyColor, new Color(255, 50, 30), gunPulse * 0.9f);
                Color gunGlow = new Color(255, 40, 25, 0) * gunPulse * 0.75f;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new Vector2(2.5f * gunPulse, 0f).RotatedBy(MathHelper.TwoPi * i / 4f);
                    Main.EntitySpriteDraw(texture, pos + offset, gunRect, gunGlow, Projectile.rotation, gunOrigin, Projectile.scale * (1f + gunPulse * 0.08f), SpriteEffects.None, 0);
                }
            }

            Main.EntitySpriteDraw(texture, pos, gunRect, gunColor, Projectile.rotation, gunOrigin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            for (int i = 0; i < 75; i++)
            {
                int dust = Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y), Projectile.width / 2, Projectile.height / 2, 6, Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f), 120, default, Main.rand.NextFloat(1f, 2f));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 2f;
            }
            for (int i = 0; i < 15; i++)
            {
                int dust = Dust.NewDust(new Vector2(Projectile.Center.X, Projectile.Center.Y), Projectile.width / 2, Projectile.height / 2, 31, Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f), 120, default, Main.rand.NextFloat(1f, 2f));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1f;
            }
            for (int i = 0; i < Main.rand.Next(1, 3); i++)
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(1.5f, 1.5f), 61);
            for (int i = 0; i < Main.rand.Next(1, 3); i++)
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(1.5f, 1.5f), 62);
            for (int i = 0; i < Main.rand.Next(1, 3); i++)
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(1.5f, 1.5f), 63);
        }

        public override bool? CanDamage() => false;
    }
}