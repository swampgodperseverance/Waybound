using System.Collections.Generic;
using Terraria.ID;
using Waybound.Content.Items.Weapons.Melee.Chakrams;

namespace Waybound.Tables;

public static class Items {
    public static Dictionary<int, int> IceShime { get; } = new() {
        [ItemID.StoneBlock] = ItemID.IceBlock,
        [ItemID.Torch] = ItemID.IceTorch,
        [ItemID.HermesBoots] = ItemID.IceSkates,
        [ItemID.WoodenBoomerang] = ItemID.IceBoomerang,
        [ItemID.ThornChakram] = ModContent.ItemType<IceChakram>(),
    };
};
