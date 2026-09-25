using Terraria;
using Terraria.DataStructures;

namespace Waybound.Helpers
{
	public static class BaseHelper
	{
		public static PlayerScreen PlayerScreen(this Player player) => player.GetModPlayer<PlayerScreen>();
		//public static GlobalItemOther ItemOther(this Item item) => item.GetGlobalItem<GlobalItemOther>();
  //      public static GlobalItemThrowing ItemThrowing(this Item item) => item.GetGlobalItem<GlobalItemThrowing>();
  //      public static GlobalProjOther ProjOther(this Projectile projectile) => projectile.GetGlobalProjectile<GlobalProjOther>();
	}
    public class PlayerScreen : ModPlayer
    {
        public int rumbleDuration;
        public int rumbleStrength;
        public int interpolantTimer;
        public bool lockScreen = false;
        public bool cutscene = false;
        public bool notHurt = false;

        public override void PostUpdate()
        {
            if (rumbleDuration > 0)
            {
                rumbleDuration--;
            }
            if (lockScreen)
            {
                if (interpolantTimer < 100) interpolantTimer += 2;
            }
            else
            {
                if (interpolantTimer > 0) interpolantTimer -= 2;
            }
            ScreenFocusInterpolant = Utils.GetLerpValue(15f, 80f, interpolantTimer, true);
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (cutscene && notHurt)
                base.ModifyHurt(ref modifiers);
            // 	return false;
            // else
            //              return base.ModifyHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
        }

        public override bool CanUseItem(Item item)
        {
            return base.CanUseItem(item) && !cutscene;
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (cutscene && notHurt)
            {
                Player.statLife = 1;
                return false;
            }
            return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);
        }

        public override void UpdateDead()
        {
            lockScreen = false;
            rumbleDuration = 0;
        }

        public void Rumble(int duration, int intensity = 10)
        {
            rumbleDuration = duration;
            rumbleStrength = intensity;
        }

        public float ScreenShakeIntensity;
        public float fastScreenShake;
        public Vector2 ScreenFocusPosition;
        public float ScreenFocusInterpolant;

        public override void ModifyScreenPosition()
        {
            if (ScreenFocusInterpolant > 0f)
            {
                Vector2 idealScreenPosition = ScreenFocusPosition - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
                Main.screenPosition = Vector2.Lerp(Main.screenPosition, idealScreenPosition, ScreenFocusInterpolant);
            }
            Waybound.Instance.CameraOffset *= 0.9f;
            Main.screenPosition += Waybound.Instance.CameraOffset;
            if (rumbleDuration > 0)
            {
                int r = rumbleStrength;
                Main.screenPosition.X += Main.rand.Next(-r, r + 1);
                Main.screenPosition.Y += Main.rand.Next(-r, r + 1);
            }
            if (ScreenShakeIntensity > 0.1f)
            {
                Main.screenPosition += new Vector2(Main.rand.NextFloat(ScreenShakeIntensity),
                    Main.rand.NextFloat(ScreenShakeIntensity));

                ScreenShakeIntensity *= 0.9f;
            }

            if (fastScreenShake > 0.1f)
            {
                Main.screenPosition += new Vector2(
                    Main.rand.NextFloat(fastScreenShake * 0.75f, fastScreenShake) * (Main.rand.NextBool(2) ? -1 : 1),
                    Main.rand.NextFloat(fastScreenShake * 0.75f, fastScreenShake) * (Main.rand.NextBool(2) ? -1 : 1));

                fastScreenShake *= 0.15f;
            }
        }
    }
}