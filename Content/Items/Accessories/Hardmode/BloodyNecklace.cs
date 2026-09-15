using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Waybound.Common.WUtils;

namespace Waybound.Content.Items.Accessories.Hardmode;

[AutoloadEquip(EquipType.Neck)]
public class BloodyNecklace : ModItem {
    public override void SetDefaults() {
        Item.width = Item.height = 24;
        Item.rare = ItemRarityID.LightRed;
        Item.accessory = true;
        Item.value = Item.buyPrice(gold: 10);
    }
    public override void UpdateAccessory(Player player, bool hideVisual) => player.GetModPlayer<Common.GlobalPlayer.BloodyNecklacePlayer>().equipped = true;
    public override void ModifyTooltips(List<TooltipLine> tooltips) => tooltips.Add(new(Mod, $"{Waybound.ModName}: Acc Ability", string.Format(Loc.GetTips("Acc.BloodyNecklace"), Loc.GetButtonName(Common.ModSystems.VanillaKeybinds.AccBonusActivation))) { OverrideColor = Color.DarkRed });
};