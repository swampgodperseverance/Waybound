using Terraria;
using Terraria.ID;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.GloblaItems;
using Waybound.Common.WUtils;

namespace Waybound.Content.Items.Armor.Magic.Water;

[AutoloadEquip(EquipType.Head)]
public class OceanicHelmet : ModItem {
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults() {
        Item.width = Item.height = 24;
        Item.rare = ItemRarityID.Red;
        Item.value = Item.buyPrice(gold: 20);
        Item.defense = 18;
        Item.GetGlobalItem<RaceItemBonus>().info = RaceItemInfo.SetArmor(Common.WUtils.Race.ID.Lihzard, Loc.GetTips("Armor.OceanicRaceSetBonus"), items: [ItemType<OceanicBreastplate>(), ItemType<OceanLeggings>()]);
    }
    public override bool IsArmorSet(Item head, Item body, Item legs) => head.type == Type && body.type == ItemType<OceanicBreastplate>() && legs.type == ItemType<OceanLeggings>();
    public override void UpdateArmorSet(Player player) {
        player.GetModPlayer<OceanicArmor>().isFullSet = true;
        if (Common.WUtils.Race.CheckRace(player, Common.WUtils.Race.ID.Lihzard)) {
            player.statManaMax2 += 20;
        } else {
            player.statManaMax2 += 10;
        }
        player.setBonus = Loc.GetTips("Armor.OceanicSetBonus");
    }
    public override void UpdateEquip(Player player) {
        player.GetDamage(DamageClass.Magic) += .15f;
        player.statManaMax2 += 60;
    }
    public override void AddRecipes() {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.Coral, 20);
        recipe.AddIngredient(ItemID.GoldBar, 12);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}