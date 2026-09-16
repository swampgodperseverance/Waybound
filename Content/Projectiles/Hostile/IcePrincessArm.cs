using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;
using SystemVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Projectiles.Hostile
{
    public class IcePrincessArm : ModProjectile
    {
        public override void SetDefaults()
        {
            if (ModLoader.TryGetMod("CalamityMod", out Mod calamity))
                calamity.Call("SetDefenseDamageProjectile", Projectile, true);
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = 0;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 180;
            Projectile.penetrate = -1;
            Projectile.hide = true;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, (Color.Aquamarine * 0.3f).ToVector3());

            if (Projectile.ai[0] == 0f)
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    if (Main.rand.NextBool())
                    {
                        float size = Main.rand.NextFloat(16f, 26f);
                        Vector2 vel = Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.8f, 1.8f);
                        ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                            Projectile.Center.ToNumerics(),
                            vel.ToNumerics(),
                            Main.rand.NextFloat(MathHelper.TwoPi),
                            new SystemVector2(size, size * Main.rand.NextFloat(0.5f, 1.15f)),
                            new Color(100, 210, 255, 175) * Main.rand.NextFloat(0.9f, 1.2f),
                            Main.rand.Next(18, 30)
                        ));
                    }

                    if (Main.rand.NextBool(2))
                    {
                        float trailAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 trailOffset = new Vector2(MathF.Cos(trailAngle), MathF.Sin(trailAngle)) * Main.rand.NextFloat(5f, 12f);
                        Vector2 spawnPos = Projectile.Center + trailOffset;
                        Vector2 vel = -trailOffset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.5f, 1.4f);

                        ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                            spawnPos.ToNumerics(),
                            vel.ToNumerics(),
                            trailAngle,
                            new SystemVector2(Main.rand.NextFloat(13f, 21f)),
                            new Color(70, 190, 255, 150) * Main.rand.NextFloat(0.85f, 1.15f),
                            Main.rand.Next(14, 24)
                        ));
                    }
                }

                if (!Main.expertMode)
                    Projectile.tileCollide = true;

                Projectile.rotation = Projectile.velocity.ToRotation();
                return;
            }

            NPC npc = Main.npc[(int)Projectile.ai[0] - 1];
            if (!npc.active)
            {
                Projectile.Kill();
                return;
            }

            if (npc.ai[0] >= -180f && npc.ai[0] <= -120f)
                Projectile.localAI[0] = npc.ai[0] + 180f;

            if (Projectile.ai[2] != 0f)
            {
                Projectile.timeLeft++;
                Vector2 hoverPos = npc.Center + npc.rotation.ToRotationVector2() * npc.width / 4 * npc.direction * Projectile.ai[2] + (npc.rotation + Projectile.ai[1] * npc.direction * Projectile.ai[2]).ToRotationVector2() * npc.width / 2 * npc.direction * Projectile.ai[2];
                Projectile.Center = Vector2.Lerp(Projectile.Center, hoverPos, 0.5f);
                Projectile.rotation = (npc.Center + npc.velocity - Projectile.Center).ToRotation();
            }
            else if (Projectile.timeLeft > 150)
            {
                Projectile.rotation = (Vector2.Lerp(Main.player[npc.target].Center - (Main.player[npc.target].Center - Projectile.Center) * 2f, npc.Center + npc.velocity, (Projectile.timeLeft - 150) / 30f) - Projectile.Center).ToRotation();
            }
            else
            {
                SoundEngine.PlaySound(SoundID.Item28, Projectile.position);
                Projectile.velocity = Vector2.Normalize(Main.player[npc.target].Center - Projectile.Center) * 6f;
                Projectile.rotation = (-Projectile.velocity).ToRotation();
                Projectile.ai[0] = Projectile.ai[1] = 0f;
            }

            Projectile.spriteDirection = Projectile.rotation < MathHelper.PiOver2 && Projectile.rotation >= -MathHelper.PiOver2 ? -1 : 1;
            Projectile.rotation += MathHelper.Pi * Projectile.spriteDirection;

            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                if (Projectile.whoAmI != projectile.whoAmI && projectile.type == Type && Projectile.ai[0] == projectile.ai[0] && Projectile.ai[1] == projectile.ai[1] && projectile.ai[2] != 0f && Projectile.ai[2] == projectile.ai[2])
                {
                    projectile.Kill();
                    break;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = new Vector2(texture.Height) * 0.5f;
            SpriteEffects spriteEffects = Projectile.spriteDirection < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            if (Projectile.localAI[0] > 0f && Projectile.localAI[0] < 60f)
            {
                for (int i = 0; i < 4; i++)
                    Main.EntitySpriteDraw(texture, Projectile.Center + Vector2.UnitX.RotatedBy(MathHelper.PiOver2 * i + Projectile.rotation) * (float)Math.Sin(MathHelper.Pi * Projectile.localAI[0] / 60f) * 4f - Main.screenPosition, null, Color.Aquamarine with { A = 0 } * 0.3f * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            }
            else if (Projectile.ai[2] == 0f)
            {
                for (int i = 0; i < 4; i++)
                    Main.EntitySpriteDraw(texture, Projectile.Center + Vector2.UnitX.RotatedBy(MathHelper.PiOver2 * i + Projectile.rotation) * 2f - Main.screenPosition, null, Color.Aquamarine with { A = 0 } * 0.3f * Projectile.Opacity * (Projectile.ai[0] > 0f ? MathHelper.Clamp(1f - (Projectile.timeLeft - 150) / 30f, 0f, 1f) : 1f), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            }

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            if (Projectile.localAI[0] > 0f)
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.Aquamarine with { A = 0 } * 0.3f * (Projectile.localAI[0] / 60f) * Projectile.Opacity, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

            return false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            if (Projectile.ai[2] == 1f)
                behindNPCs.Add(index);
            else
                behindProjectiles.Add(index);
        }

        public override void OnKill(int timeLeft)
        {
            if (timeLeft > 150)
                return;

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 16; i++)
                {
                    float size = Main.rand.NextFloat(18f, 30f);
                    Vector2 vel = Main.rand.NextVector2Circular(3.8f, 3.8f);
                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        Projectile.Center.ToNumerics(),
                        vel.ToNumerics(),
                        Main.rand.NextFloat(MathHelper.TwoPi),
                        new SystemVector2(size, size * Main.rand.NextFloat(0.5f, 1.15f)),
                        new Color(110, 220, 255, 195) * Main.rand.NextFloat(0.95f, 1.25f),
                        Main.rand.Next(22, 40)
                    ));
                }

                for (int i = 0; i < 12; i++)
                {
                    float trailAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 trailOffset = new Vector2(MathF.Cos(trailAngle), MathF.Sin(trailAngle)) * Main.rand.NextFloat(7f, 16f);
                    Vector2 spawnPos = Projectile.Center + trailOffset;
                    Vector2 vel = -trailOffset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(0.7f, 1.8f);

                    ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                        spawnPos.ToNumerics(),
                        vel.ToNumerics(),
                        trailAngle,
                        new SystemVector2(Main.rand.NextFloat(15f, 24f)),
                        new Color(80, 200, 255, 160) * Main.rand.NextFloat(0.9f, 1.2f),
                        Main.rand.Next(16, 28)
                    ));
                }
            }

            SoundEngine.PlaySound(SoundID.Item50, Projectile.position);
        }

        public override bool ShouldUpdatePosition() => !Main.expertMode || Projectile.ai[0] != 0f || !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height);
    }
}