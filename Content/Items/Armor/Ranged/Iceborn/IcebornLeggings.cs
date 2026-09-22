using Terraria;
using Terraria.ID;

namespace Waybound.Content.Items.Armor.Ranged.Iceborn;

[AutoloadEquip(EquipType.Legs)]
public class IcebornLeggings : ModItem {
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults() {
        Item.Size = new Vector2(26, 20);
        Item.rare = ItemRarityID.Blue;
        Item.defense = 2;
        Item.value = Item.sellPrice(0, 1, 8, 50);
    }
    public override void UpdateEquip(Player player) {
        player.GetCritChance(DamageClass.Ranged) += 0.08f;
        player.moveSpeed += 0.07f;
    }
    public override void AddRecipes()
    {
        //CreateRecipe()
        //    .AddIngredient(ItemType<ThunderBar>(), 18)
        //    .AddTile(TileID.MythrilAnvil)
        //    .Register();
    }
}