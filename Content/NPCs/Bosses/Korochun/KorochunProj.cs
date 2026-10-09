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
    public class KorochunProj : ModProjectile
    {// I was inspired by unowen but decided to change it, nevertheless nova proj is similar
        public override string Texture =>
            "Waybound/Content/NPCs/Bosses/Korochun/KorochunProj";

        private const int TrailLength = 14;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.light = 0.6f;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.985f;
            float wobble = MathF.Sin(Projectile.timeLeft * 0.15f + Projectile.ai[0]) * 0.08f;
            Projectile.velocity = Projectile.velocity.RotatedBy(wobble);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.rand.NextBool(2))
                {
                    Vector2 back = -Projectile.velocity.SafeNormalize(Vector2.UnitX);
                    Dust d = Dust.NewDustPerfect(
                        Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                        DustID.IceTorch,
                        back * Main.rand.NextFloat(0.4f, 1.6f) + Main.rand.NextVector2Circular(0.6f, 0.6f),
                        60, new Color(180, 220, 255), Main.rand.NextFloat(0.9f, 1.3f));
                    d.noGravity = true;

                    if (Main.rand.NextBool(4))
                    {
                        Dust d2 = Dust.NewDustPerfect(
                            Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                            DustID.PurpleTorch,
                            -Projectile.velocity * 0.05f,
                            60, new Color(200, 180, 255), Main.rand.NextFloat(0.7f, 1.1f));
                        d2.noGravity = true;
                    }
                }

                Lighting.AddLight(Projectile.Center, 0.35f, 0.55f, 0.9f);
            }
            if (Projectile.timeLeft < 20)
                Projectile.alpha = (int)MathHelper.Lerp(255f, 0f, Projectile.timeLeft / 20f);
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.Server) return;

            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f, Pitch = 0.2f }, Projectile.Center);

            for (int i = 0; i < 10; i++)
            {
                Vector2 v = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, v,
                    60, new Color(190, 230, 255), Main.rand.NextFloat(1f, 1.4f));
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            float time = Main.GlobalTimeWrappedHourly;

            // === хвост ===
            for (int i = Projectile.oldPos.Length - 1; i >= 1; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;

                float k = i / (float)Projectile.oldPos.Length;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color c = Color.Lerp(new Color(160, 210, 255), new Color(80, 130, 220), k) * ((1f - k) * 0.55f);

                Main.EntitySpriteDraw(tex, trailPos, null, c, Projectile.oldRot[i], origin,
                    Projectile.scale * (1f - k * 0.5f), SpriteEffects.None, 0);
            }

            float pulse = 0.85f + 0.15f * MathF.Sin(time * 14f + Projectile.whoAmI);
            Color glow = new Color(150, 200, 255) * (0.45f * pulse);

            Main.EntitySpriteDraw(tex, pos, null, glow, Projectile.rotation, origin,
                Projectile.scale * 1.35f, SpriteEffects.None, 0);

            Main.EntitySpriteDraw(tex, pos, null, Color.White, Projectile.rotation, origin,
                Projectile.scale, SpriteEffects.None, 0);

            Main.EntitySpriteDraw(tex, pos, null, new Color(220, 240, 255) * 0.7f, Projectile.rotation, origin,
                Projectile.scale * 0.5f, SpriteEffects.None, 0);

            return false;
        }
    }
}