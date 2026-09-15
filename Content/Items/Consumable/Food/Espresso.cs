using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Consumable.Food;

public class Espresso : ModItem
{
    public override void SetStaticDefaults()
    {
        ItemID.Sets.FoodParticleColors[Item.type] = new Microsoft.Xna.Framework.Color[] {
            new Microsoft.Xna.Framework.Color(101, 67, 33), 
            new Microsoft.Xna.Framework.Color(160, 82, 45)  
        };

        ItemID.Sets.IsFood[Item.type] = true;
    }

    public override void SetDefaults()
    {
        //24sec.
        Item.DefaultToFood(24, 24, BuffID.Swiftness, 1440);

        Item.UseSound = SoundID.Item3;

        Item.value = Item.buyPrice(0, 0, 30, 0);
        Item.rare = ItemRarityID.Green;        
    }

}