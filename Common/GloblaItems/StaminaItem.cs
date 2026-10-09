using Terraria;
using Waybound.Common.GlobalPlayer;

namespace Waybound.Common.GloblaItems;

public class StaminaItem : GlobalItem {
    public override bool? UseItem(Item item, Player player) {
        if (item.damage <= 0) { return base.UseItem(item, player); };
        if (!player.GetModPlayer<StaminaPlayer>().Active2) {
            if (player.GetModPlayer<StaminaPlayer>().IsInPulseWindow) { return true; };
            return false;
        };
        if (player.itemAnimation == player.itemAnimationMax) { player.GetModPlayer<StaminaPlayer>().UseItem(1, true); };
        return player.GetModPlayer<StaminaPlayer>().Active2;
    }
    public override bool CanUseItem(Item item, Player player) {
        if (!player.GetModPlayer<StaminaPlayer>().Active2) { return player.GetModPlayer<StaminaPlayer>().IsInPulseWindow; };
        return base.CanUseItem(item, player);
    }
    public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        StaminaPlayer staminaPlayer = player.GetModPlayer<StaminaPlayer>();
        if (staminaPlayer.StaminaPercent <= 0.5f && !staminaPlayer.IsOverheating) { velocity *= 1.35f; };
        if (staminaPlayer.IsInPulseWindow) {
            int shotCount = staminaPlayer.GetEmpoweredShotCount();
            velocity *= 1.55f;
            damage = (int)(damage * 1.65f);
            if (shotCount > 1) { for (int i = 1; i < shotCount; i++) { Projectile.NewProjectile(player.GetSource_ItemUse(item), position, velocity.RotatedByRandom(MathHelper.ToRadians(8 + i * 4)) * (0.92f + i * 0.04f), type, (int)(damage * 0.85f), knockback * 0.9f, player.whoAmI); }; };
        };
    }
}