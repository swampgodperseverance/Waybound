using Terraria;

namespace Waybound.Common.WUtils;

public static class PL {
    public static bool CheckHelmet(Player player, int itemType) => player.armor[0].type == itemType || player.armor[10].type == itemType;
};
