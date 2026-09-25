using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Waybound.Common.GlobalPlayer
{
    public class ScreenShakePlayer : ModPlayer
    {
        public int shakeTimer = 0;
        private float shakeIntensity = 1f;
        private Vector2 shakeOffset;

        public override void ModifyScreenPosition()
        {
            if (shakeTimer > 0)
            {
                shakeTimer--;
                shakeOffset = new Vector2(
                    Main.rand.NextFloat(-5f, 5f) * shakeIntensity, 
                    Main.rand.NextFloat(-5f, 5f) * shakeIntensity);
                Main.screenPosition += shakeOffset;
            }
        }

        public void TriggerShake(int duration, float intensity = 1f)
        {
            shakeTimer = duration;
            shakeIntensity = intensity;
        }
    }
}