using Waybound.Common.GlobalPlayer;
using Waybound.Common.Utils;
using Terraria;
using Terraria.ID;

namespace Waybound.Content.Items.Armor.Melee.Thunder;

[AutoloadEquip(EquipType.Head)]
public sealed class ThunderHead : ModItem {
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults() {
        Item.width = 26;
        Item.height = 20;
        Item.rare = ItemRarityID.Yellow;
        Item.defense = 17;
        Item.value = Item.sellPrice(0, 3, 4, 50);
    }
    public override void UpdateEquip(Player player) {
        player.GetAttackSpeed(DamageClass.Melee) += 0.08f;
        player.GetCritChance(DamageClass.Melee) += 10f;
    }
    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ItemType<ThunderBody>() && legs.type == ItemType<ThunderLegs>() && head.type == Type;
    public override void UpdateArmorSet(Player player) {
        player.setBonus = Loc.GetTips("Armor.ThunderSetBonus");
        player.GetDamage(DamageClass.Melee) += 0.10f;
        player.GetModPlayer<ArmorPlayers>().thunderSet = true;
    }
    public override void AddRecipes()
    {
        //CreateRecipe()
        //    .AddIngredient(ModContent.ItemType<ThunderBar>(), 12)
        //    .AddTile(TileID.MythrilAnvil)
        //    .Register();
    }
}