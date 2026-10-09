using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.NPCs.Bosses.Korochun
{
    public class KorochunHeadProj : ModProjectile
    {
        private const int HeadFrames = 1;
        private const bool HeadFacesRight = false;
        // mog mog mog
        private int ModeId => (int)Projectile.ai[0];
        private ref float Delay => ref Projectile.ai[1];
        private ref float Param => ref Projectile.ai[2];
        private ref float Clock => ref Projectile.localAI[0];
        private ref float Aux => ref Projectile.localAI[1]; 

        private bool Launched => Clock >= Delay;
        private bool IsWave => ModeId == 1;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
        }

        public override bool? CanDamage()
        {
            if (!Launched) return false; 
            return null;
        }

        public override void AI()
        {
            int b = NPC.FindFirstNPC(ModContent.NPCType<Korochun>());
            if (b < 0)
            {
                Projectile.Kill();
                return;
            }

            if (Clock == 0f)
            {
                Aux = IsWave ? Projectile.Center.Y : 0f;
                Projectile.timeLeft = (int)Delay + (IsWave ? 340 : 160);

                if (Main.netMode != NetmodeID.Server)
                {
                    KorochunFx.Flash(Projectile.Center, 90f, new Color(190, 170, 255, 200), 10);
                    KorochunFx.Spray(Projectile.Center, Main.rand.NextVector2CircularEdge(1f, 1f), 5, 1.5f, 4f, 0.8f, 1f, KorochunFx.Violet);
                }
            }

            Clock++;

            if (IsWave) WaveAI();
            else OrbitAI(Main.npc[b]);

            Lighting.AddLight(Projectile.Center, 0.35f, 0.3f, 0.7f);
        }

        private void OrbitAI(NPC boss)
        {
            Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];

            if (!Launched)
            {
                float appear = MathHelper.Clamp(Clock / 20f, 0f, 1f);
                float spin = Clock * 0.055f;
                float radius = (150f + 30f * MathF.Sin(Clock * 0.08f)) * appear;
                Projectile.Center = boss.Center + Vector2.UnitX.RotatedBy(Param + spin) * radius;
                Projectile.velocity = Vector2.Zero;

                if (Clock < Delay - 6f)
                    Aux = (target.Center - Projectile.Center).ToRotation();

                if (Clock > Delay - 26f && Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
                    KorochunFx.Converge(Projectile.Center, 40f, 90f, 1, 1f, 10, KorochunFx.Violet);

                Projectile.rotation = Aux;
                return;
            }
            if (Clock == Delay)
            {
                Projectile.timeLeft = 100;
                if (Authority)
                {
                    Projectile.velocity = Aux.ToRotationVector2() * 10f;
                    Projectile.netUpdate = true;
                }
                if (Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.6f, Pitch = 0.2f }, Projectile.Center);
                    KorochunFx.Flash(Projectile.Center, 110f, new Color(210, 235, 255, 230), 10);
                    KorochunFx.Spray(Projectile.Center, -Aux.ToRotationVector2(), 8, 2f, 6f, 0.5f, 1.1f);
                }
            }

            if (Projectile.velocity.Length() < 21f)
                Projectile.velocity *= 1.07f;

            Projectile.rotation = Projectile.velocity.ToRotation();
            TrailParticles();
        }

        private void WaveAI()
        {
            if (!Launched)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.Center = new Vector2(Projectile.Center.X, Aux);
                return;
            }

            float tt = Clock - Delay;
            float ramp = Math.Min(tt / 30f, 1f);
            Projectile.velocity = new Vector2(Param * ramp, 0f);
            Projectile.Center = new Vector2(Projectile.Center.X, Aux + MathF.Sin(tt * 0.07f + Projectile.identity) * 28f);
            Projectile.rotation = Param >= 0f ? 0f : MathHelper.Pi;

            TrailParticles();
        }

        private static bool Authority => Main.netMode != NetmodeID.MultiplayerClient;

        private void TrailParticles()
        {
            if (Main.netMode == NetmodeID.Server || !Main.rand.NextBool(3)) return;
            Color c = Main.rand.NextBool() ? KorochunFx.RandIce() : KorochunFx.Tint(KorochunFx.Violet);
            KorochunFx.Snow(Projectile.Center + Main.rand.NextVector2Circular(14f, 14f),
                -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.6f, 0.6f),
                Main.rand.NextFloat(12f, 20f), c, 24);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Chilled, 180);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.Server) return;

            KorochunFx.Flash(Projectile.Center, 120f, new Color(190, 170, 255, 210), 12);
            KorochunFx.Ring(Projectile.Center, 14, 4f, 1.2f, KorochunFx.Violet);
            KorochunFx.Ring(Projectile.Center, 10, 2f, 1f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int fh = tex.Height / HeadFrames;
            Rectangle frame = new Rectangle(0, 0, tex.Width, fh);
            Vector2 origin = frame.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            bool launched = Launched;
            float time = Main.GlobalTimeWrappedHourly;

            float appear = IsWave
                ? MathHelper.Clamp(Clock / Math.Max(Delay, 1f), 0f, 1f)
                : MathHelper.Clamp(Clock / 20f, 0f, 1f);
            float fadeOut = MathHelper.Clamp(Projectile.timeLeft / 20f, 0f, 1f);
            float flick = 1f + 0.08f * MathF.Sin(time * 11f + Projectile.identity);
            float a = (launched ? 0.8f : 0.45f) * appear * fadeOut * flick;
            Vector2 face;
            if (IsWave) face = new Vector2(Param >= 0f ? 1f : -1f, 0f);
            else if (launched) face = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            else face = Aux.ToRotationVector2();

            float ang = face.ToRotation();
            bool right = face.X >= 0f;
            float rot = right ? ang : ang + MathHelper.Pi;
            SpriteEffects fx = right != HeadFacesRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Texture2D px = TextureAssets.MagicPixel.Value;

            KorochunFx.BeginAdditive();
            if (!launched)
            {
                if (IsWave)
                {
                    float k = Clock / Math.Max(Delay, 1f);
                    float al = (0.07f + 0.2f * k) * fadeOut;
                    if (Clock > Delay - 14f) al *= 0.6f + 0.4f * (((int)(Clock / 2f)) % 2);
                    float dirX = Param >= 0f ? 1f : -1f;
                    for (float d = 40f; d < 1900f; d += 70f)
                    {
                        Main.EntitySpriteDraw(px, pos + new Vector2(dirX * d, 0f), new Rectangle(0, 0, 1, 1),
                            KorochunFx.Violet * al, 0f, new Vector2(0.5f), new Vector2(36f, 3f), SpriteEffects.None, 0);
                    }
                }
                else if (Clock > Delay - 26f)
                {
                    float k = (Clock - (Delay - 26f)) / 26f;
                    float al = 0.15f + 0.45f * k;
                    if (Clock > Delay - 8f) al *= 0.6f + 0.4f * (((int)(Clock / 2f)) % 2);
                    Vector2 dir = Aux.ToRotationVector2();
                    for (float d = 30f; d < 1300f; d += 56f)
                    {
                        Main.EntitySpriteDraw(px, pos + dir * d, new Rectangle(0, 0, 1, 1),
                            KorochunFx.Ice * al, Aux, new Vector2(0.5f), new Vector2(28f, 2f + 2f * k), SpriteEffects.None, 0);
                    }
                }
            }
            if (launched)
            {
                for (int i = 1; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    float k = 1f - i / (float)Projectile.oldPos.Length;
                    Color c = Color.Lerp(KorochunFx.Violet, KorochunFx.Ice, k) * (k * 0.35f * a);
                    Main.EntitySpriteDraw(tex, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, frame, c,
                        rot, origin, Projectile.scale * (0.6f + 0.4f * k), fx, 0);
                }
            }
            Color halo = Color.Lerp(KorochunFx.Violet, KorochunFx.Ice, 0.5f + 0.5f * MathF.Sin(time * 3f + Projectile.identity));
            KorochunFx.DrawGlow(pos, halo * (0.5f * a), 120f);
            KorochunFx.DrawGlow(pos, KorochunFx.Ice * (0.45f * a), 62f);
            KorochunFx.EndAdditive();
            Main.EntitySpriteDraw(tex, pos, frame, Color.White * (a * 0.85f), rot, origin, Projectile.scale, fx, 0);
            KorochunFx.BeginAdditive();
            Main.EntitySpriteDraw(tex, pos, frame, KorochunFx.Ice * (a * 0.5f), rot, origin, Projectile.scale * 1.06f, fx, 0);
            KorochunFx.EndAdditive();

            return false;
        }
    }
}