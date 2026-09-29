using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;

namespace Waybound.Content.Items.Weapons.Ranged.LaserGuns.GemLaserGuns
{
    public abstract class LaserGun : ModItem
    {
        public int chargeAdd = 1;
        public int chargeRemove = 4;
        public int chargeMax = 175;

        public override void UpdateInventory(Player player)
        {
            var modPlayer = player.GetModPlayer<PlayerLaserGun>();
            if (player.HeldItem.type == Item.type)
            {
                float scale = modPlayer.laserScale;
                Item.color = Color.Lerp(Color.White, new Color(255, 150, 150), scale);
            }
            base.UpdateInventory(player);
        }
    }

    public class PlayerLaserGun : ModPlayer
    {
        public float laserScale = 0.25f;
        public int charge;

        public override void PostUpdate()
        {
            Item held = Player.HeldItem;

            if (held.ModItem is LaserGun gun)
            {
                int max = gun.chargeMax;
                int add = gun.chargeAdd;
                int remove = gun.chargeRemove;

                if (Main.mouseLeft && Player.itemAnimation > 0)
                    charge = Math.Min(charge + add, max);
                else
                    charge = Math.Max(charge - remove, 0);

                laserScale = max > 0
                    ? (float)charge / max * 0.75f + 0.25f
                    : 0.25f;
            }
            else
            {
                charge = Math.Max(charge - 4, 0);
                laserScale = Math.Max(laserScale - 0.02f, 0.25f);
            }
        }

        public override void ResetEffects()
        {
        }
    }
}