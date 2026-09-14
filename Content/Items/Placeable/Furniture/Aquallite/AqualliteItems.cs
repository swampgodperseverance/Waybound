using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Tiles.Furniture.Aquallite;
using static Waybound.Content.Tiles.Furniture.Aquallite.AqualliteChest;

namespace Waybound.Content.Items.Placeable.Furniture.Aquallite;

public class AqualliteChestItem : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AqualliteChest>());
        Item.width = 26;
        Item.height = 22;
        Item.value = Item.sellPrice(silver: 1);
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Wood, 8)
            .AddIngredient(ItemID.IronBar, 2)
            .AddTile(TileID.WorkBenches)
            .Register();
    }

}
public class AquallitePlatform : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 200;
    }
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.StoneBlock);
        Item.rare = ItemRarityID.Blue;
        Item.width = 26;
        Item.height = 14;
        Item.createTile = ModContent.TileType<AquallitePlatformTile>();
    }
    public override void AddRecipes()
    {
        CreateRecipe(2).AddIngredient(ModContent.ItemType<AqualliteBlock>()).Register();
        Recipe.Create(ModContent.ItemType<AqualliteBlock>()).AddIngredient(this, 2).Register();
    }
}
public class AqualliteLamp : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 10;
    }
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.StoneBlock);
        Item.rare = ItemRarityID.Blue;
        Item.width = 26;
        Item.height = 14;
        Item.createTile = ModContent.TileType<AqualliteLampTile>();
    }
    public override void AddRecipes()
    {
        CreateRecipe(2).AddIngredient(ModContent.ItemType<AqualliteBlock>()).Register();
        Recipe.Create(ModContent.ItemType<AqualliteBlock>()).AddIngredient(this, 2).Register();
    }
}