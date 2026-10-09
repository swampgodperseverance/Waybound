using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

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
        public override void SetDefaults()
        {
            Item.useTime = 3;
            Item.useAnimation = 3;
        }
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<RangedLaser>()] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
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