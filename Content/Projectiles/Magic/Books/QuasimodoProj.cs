using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Helpers;

namespace Waybound.Content.Projectiles.Magic.Books
{
    public class QuasimodoProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(93);
            Projectile.aiStyle = -1;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            if (Projectile.ai[0] <= 35f)
            {
                float fadeProgress = Projectile.ai[0] / 35f;
                Projectile.alpha = (int)(255 * (1f - EaseFunctions.EaseOutCubic(fadeProgress)));
            }
            else
            {
                Projectile.alpha = 0;
            }
            Vector2 toMouse = Vector2.Normalize(Main.MouseWorld - Projectile.Center);

            if (Projectile.ai[0] > 60f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                return;
            }

            if (Projectile.ai[0] == 60f)
            {
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.velocity = toMouse * 16f;
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
                }
                SoundEngine.PlaySound(SoundID.Item46, Projectile.position);
                return;
            }


            Projectile.rotation += Projectile.velocity.X / 8f;

            Projectile.velocity *= 0.97f;

            Projectile.position += Main.player[Projectile.owner].velocity / Projectile.MaxUpdates;


            if (Projectile.ai[0] > 38f)
            {
                float windUpProgress = (Projectile.ai[0] - 38f) / 22f; // 0  1
                windUpProgress = EaseFunctions.EaseInCubic(windUpProgress);

                float pullStrength = 1.15f * windUpProgress;
                Projectile.velocity -= toMouse * pullStrength;

                if (Projectile.ai[0] > 52f)
                {
                    Projectile.velocity *= 0.94f;
                }
            }

            oldPos = Projectile.Center;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (oldPos.HasNaNs() || oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
            else if (!Main.gamePaused)
                oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.12f);

            Texture2D glow = ModContent.Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;
            Texture2D texture = ModContent.Request<Texture2D>(Texture, AssetRequestMode.ImmediateLoad).Value;

            float overallAlpha = 1f - (Projectile.alpha / 255f);

            float flyProgress = MathHelper.Clamp((Projectile.ai[0] - 60f) / 25f, 0f, 1f);
            float glowAlpha = EaseFunctions.EaseOutCubic(flyProgress) * overallAlpha;

            if (glowAlpha > 0.01f)
            {
                Color glowColor = new Color(255, 140, 180, 0) * (0.38f * glowAlpha); 
                Vector2 glowOrigin = texture.Size() * 0.5f;

                for (int i = 0; i < 4; i++)
                {
                    float scale = Projectile.scale * (1.29f + i * 0.1f); 
                    float layerAlpha = glowAlpha * (1f - i * 0.20f);

                    Main.EntitySpriteDraw(
                        texture,
                        Projectile.Center - Main.screenPosition,
                        null,
                        glowColor * layerAlpha,
                        Projectile.rotation,
                        glowOrigin,
                        scale,
                        Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                        0f
                    );
                }
            }

            if (oldPos != Projectile.Center && Projectile.ai[0] >= 60f)
            {
                float trailProgress = MathHelper.Clamp((Projectile.ai[0] - 60f) / 20f, 0f, 1f);
                float trailAlpha = EaseFunctions.EaseOutCubic(trailProgress) * 0.32f * overallAlpha; 

                float lifeFade = MathHelper.Clamp(Projectile.timeLeft / 30f, 0f, 1f);
                trailAlpha *= lifeFade;

                Color trailColor = new Color(255, 160, 200, 0) * trailAlpha;

                float trailLength = Vector2.Distance(Projectile.Center, oldPos);
                float trailScaleY = trailLength / glow.Height * 3f; 

                Main.EntitySpriteDraw(
                    glow,
                    Projectile.Center - Main.screenPosition,
                    new Rectangle(0, glow.Height / 2, glow.Width, glow.Height / 2),
                    trailColor,
                    (Projectile.Center - oldPos).ToRotation() + MathHelper.PiOver2,
                    new Vector2(glow.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.52f, trailScaleY), 
                    SpriteEffects.None,
                    0f
                );
            }

            Color drawColor = lightColor * overallAlpha;
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                drawColor,
                Projectile.rotation,
                texture.Size() * 0.5f,
                Projectile.scale,
                Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                0f
            );

            return false;
        }

        private Vector2 oldPos = Vector2.Zero;
    }
}