using Terraria.ID;
using Waybound.Content.Items.Placeable.Furniture.ScorchedWoodFurniture;

namespace Waybound.Content.Tiles.Furniture.ScorchedWoodFurniture
{
    public class ScorchedWoodChest : Abstract.Chest {
        public override Color MapColor => new(115, 115, 120);
        public override int Dust => DustID.Ash;
        public override int ChestID => ItemType<ScorchedWoodChestI>();
    }
};