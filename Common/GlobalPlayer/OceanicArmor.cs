using Terraria;
using Waybound.Common.WUtils;
using Waybound.Content.Projectiles.Armor;

namespace Waybound.Common.GlobalPlayer;

public class OceanicArmor : ModPlayer {
    public byte activeProjCount = 0;
    public byte createTime = 0;

    public bool isFullSet = false;

    public override void ResetEffects() {
        isFullSet = false;
    }
    public override void PostUpdate() {
        if (!isFullSet) { return; }
        activeProjCount = (byte)Player.ownedProjectileCounts[ProjectileType<Bubble>()];
        if (activeProjCount <= 20) {
            if (createTime == 30) { 
                Projectile.NewProjectile(Player.GetSource_Misc("Create Bubbles"), Player.Center.X(Main.rand.Next(-50, 51)).Y(Main.rand.Next(-50, 51)), Vector2.Zero, ProjectileType<Bubble>(), 160, 1f, Player.whoAmI);
                createTime = 0;
            }
            createTime++;
        }
    }
}