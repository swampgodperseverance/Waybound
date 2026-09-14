using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Consumable.Food;

public class FrozenApple : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.FoodParticleColors[Item.type] = new Microsoft.Xna.Framework.Color[] {
            new Microsoft.Xna.Framework.Color(135, 206, 250),
            new Microsoft.Xna.Framework.Color(240, 248, 255) 
        };

        ItemID.Sets.IsFood[Item.type] = true;
    }

    public override void SetDefaults()
    {
        Item.DefaultToFood(24, 28   , BuffID.WellFed, 10800);

        Item.value = Item.buyPrice(0, 0, 26, 0);
        Item.rare = ItemRarityID.Blue;         
    }

    public override void OnConsumeItem(Player player)
    {
        player.AddBuff(BuffID.Chilled, 900);

    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Apple, 1)
            .AddIngredient(ItemID.IceBlock, 50)
            .AddTile(TileID.CookingPots)
            .Register();
    }
}