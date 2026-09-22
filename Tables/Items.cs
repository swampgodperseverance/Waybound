using System.Collections.Generic;
using Terraria.ID;

namespace Waybound.Tables;

public static class Items {
    public static Dictionary<int, int> IceShime { get; } = new() {
        [ItemID.StoneBlock] = ItemID.IceBlock,
    };
};
