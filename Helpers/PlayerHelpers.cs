using System.Linq;
using Terraria;

namespace Waybound.Helpers;

public static class PlayerHelpers {
    public static int CheckItem(Player player, int itemType) {
        if (HasItem(player.inventory)) { return 0; }
        else if (HasItem(player.bank.item)) { return 1; }
        else if (HasItem(player.bank2.item)) { return 2; }
        else if (HasItem(player.bank3.item)) { return 3; }
        else if (HasItem(player.bank4.item)) { return 4; }
        else { return -1; };

        bool HasItem(Item[] inv) => inv.Any(x => x.type == itemType);
    }
    public static Item GetLocalItem(Player player) => player.HeldItem;
};