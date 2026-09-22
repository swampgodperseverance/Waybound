using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;


namespace Waybound.Content.Items.Armor.Ranged.Iceborn;

[AutoloadEquip(EquipType.Body)]
public class IcebornBreastplate : ModItem {
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults() {
        int width = 26; int height = 20;
        Item.Size = new Vector2(width, height);
        Item.rare = ItemRarityID.Blue;
        Item.defense = 4;
        Item.value = Item.sellPrice(0, 2, 1, 50);
    }
    public override void UpdateEquip(Player player) {
        player.GetCritChance(DamageClass.Ranged) += 0.08f;
        player.endurance += 0.06f;
    }
    public override void AddRecipes()
    {
        //CreateRecipe()
        //    .AddIngredient(ModContent.ItemType<ThunderBar>(), 24)
        //    .AddTile(TileID.MythrilAnvil)
        //    .Register();
    }
}