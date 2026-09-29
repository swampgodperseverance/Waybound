using Waybound.Content.Items.Placeable.Furniture.DesertHunterFurniture;

namespace Waybound.Content.Tiles.Furniture.DesertHunterFurniture {
    public class DesertHunterChest : Abstract.Chest {
        public override Color MapColor => new(120, 171, 191);
        public override int Dust => ModContent.DustType<Dusts.DesertHunterDust>();
        public override int ChestID => ItemType<DesertHunterChestI>();
    }
};