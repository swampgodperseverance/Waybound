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
    /// <summary>Ледяной осколок для атаки «вверх — выстрел, вниз — выстрел».</summary>
    public class KorochunIceShot : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_98";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 320;
        }

        public override void AI()
        {
            if (Projectile.velocity.Length() < 15f)
                Projectile.velocity *= 1.012f;

            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? DustID.PurpleTorch : DustID.IceTorch,
                    -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.8f, 0.8f), 80, Color.White, 1.1f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.3f, 0.6f, 0.9f);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(BuffID.Chilled, 120);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.Server) return;

            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f, Pitch = 0.2f }, Projectile.Center);
            KorochunFx.Flash(Projectile.Center, 70f, new Color(190, 225, 255, 200), 9);
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? DustID.IceTorch : DustID.WhiteTorch,
                    Main.rand.NextVector2Circular(4.5f, 4.5f), 40, Color.White, 1.3f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            float rot = Projectile.rotation + MathHelper.PiOver2;

            KorochunFx.BeginAdditive();

            // шлейф
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float k = 1f - i / (float)Projectile.oldPos.Length;
                Vector2 p = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                KorochunFx.DrawGlow(p, KorochunFx.Ice * (k * 0.5f), 34f * k + 8f);
            }

            Vector2 pos = Projectile.Center - Main.screenPosition;

            // ореол
            KorochunFx.DrawGlow(pos, KorochunFx.IceDeep * 0.8f, 74f);
            KorochunFx.DrawGlow(pos, KorochunFx.Ice, 40f);

            // осколок
            Main.EntitySpriteDraw(tex, pos, null, KorochunFx.Ice * 0.9f, rot, origin, new Vector2(0.55f, 2.0f), SpriteEffects.None, 0);
            Main.EntitySpriteDraw(tex, pos, null, Color.White, rot, origin, new Vector2(0.3f, 1.4f), SpriteEffects.None, 0);

            KorochunFx.EndAdditive();
            return false;
        }
    }
}