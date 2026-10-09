using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;


namespace Waybound.Content.Items.Armor.Melee.DespairArmor;

[AutoloadEquip(EquipType.Body)]
public class DespairBreastplate : ModItem {
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults() {
        int width = 26; int height = 20;
        Item.Size = new Vector2(width, height);
        Item.rare = ItemRarityID.Pink;
        Item.defense = 23;
        Item.value = Item.sellPrice(0, 4, 8, 50);
    }
    public override void UpdateEquip(Player player) {
        player.GetDamage(DamageClass.Melee) += 0.20f;
        player.GetAttackSpeed(DamageClass.Melee) += 0.08f;
    }
    public override void AddRecipes()
    {
        //CreateRecipe()
        //    .AddIngredient(ModContent.ItemType<ThunderBar>(), 24)
        //    .AddTile(TileID.MythrilAnvil)
        //    .Register();
    }
}