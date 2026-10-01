using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Tiles.Furniture.Abstract;
using Waybound.Content.Items.Placeable.Furniture.DesertHunterFurniture;
using Waybound.Content.Items.Placeable.Furniture.DeepWoodFurniture;

namespace Waybound.Content.Tiles.Furniture.DeepWoodFurniture
{
    public class DeepChest : Chest
    {
        public override Color MapColor => new(120, 171, 191);
        public override int Dust => ModContent.DustType<DeepTreeDust>();
        public override int ChestID => ItemType<DeepChestItem>();
    }
}