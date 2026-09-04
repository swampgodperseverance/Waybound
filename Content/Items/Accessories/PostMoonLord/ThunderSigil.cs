using Terraria;
using Terraria.ID;

namespace Waybound.Content.Items.Accessories.PostMoonLord;

public class ThunderSigil : ModItem {
    public override void SetDefaults() {
        Item.width = 26;
        Item.height = 26;
        Item.accessory = true;
        Item.rare = ItemRarityID.LightRed;
        Item.value = Item.sellPrice(gold: 2);
    }
    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetModPlayer<Common.GlobalPlayer.ThunderSigilPlayer>().equipped = true;
};