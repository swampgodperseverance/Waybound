using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace Waybound.Content.Items.Weapons.Ranged.LaserGuns.GemLaserGuns
{
    public abstract class LaserGun : ModItem
    {
        public int charge, chargeAdd, chargeRemove, chargeMax;
        public float Scale;

        public override void UpdateInventory(Player player)
        {
            charge = Main.mouseLeft && player.HeldItem.type == Item.type
                ? Math.Min(charge + chargeAdd, chargeMax)
                : Math.Max(charge - chargeRemove, 0);
            Scale = (float)charge / chargeMax * 0.75f + 0.25f;
            player.GetModPlayer<PlayerLaserGun>().laserScale = player.HeldItem.type == Item.type
                ? Scale
                : player.GetModPlayer<PlayerLaserGun>().laserScale;
            Item.color = Color.Lerp(Color.White, new Color(255, 150, 150), Scale);
            base.UpdateInventory(player);
        }
    }

    public class PlayerLaserGun : ModPlayer
    {
        public float laserScale;
    }
}