using Waybound.Content.Tiles.Banners.PreHM;
using Waybound.Content.Tiles.Blocks;
using Waybound.Content.Tiles.BossSummon;
using Waybound.Content.Tiles.Furniture;
using Waybound.Content.Tiles.Furniture.DeepWoodFurniture;
using Waybound.Content.Tiles.Plants;
using Waybound.Content.Tiles.Walls;

namespace Waybound.Common.ModSystems.WorldGens.CrystalDepths;

internal class DepthsTiles
{
    public static ushort Dirt => (ushort)TileType<DeepDirt>();
    public static ushort Stone => (ushort)TileType<DeepStone>();
    public static ushort Crystal => (ushort)TileType<DeepCrystal>();
    public static ushort Crystal2 => (ushort)TileType<DeepCrystal2>();
    public static ushort Brick => (ushort)TileType<DeepBricks>();
    public static ushort Tree => (ushort)TileType<DeepTree>();
    public static ushort Wood => (ushort)TileType<DeepWood>();
    public static ushort Beam => (ushort)TileType<DeepWoodBeam>();
    public static ushort Leaf => (ushort)TileType<DeepbloomLeaf>();
    public static ushort Grass => (ushort)TileType<DeepGrass>();
    public static ushort Vine => (ushort)TileType<DeepbloomVine>();
    public static ushort Herb => (ushort)TileType<DeepHerb>();
    public static ushort Shard => (ushort)TileType<DeepCrystalShard>();
    public static ushort ShardBig1 => (ushort)TileType<DeepCrystalShardBig1>();
    public static ushort ShardBig2 => (ushort)TileType<DeepCrystalShardBig2>();
    public static ushort ShardBig3 => (ushort)TileType<DeepCrystalShardBig3>();

    // Walls
    public static ushort DirtWall => (ushort)WallType<DeepDirtWall>();
    public static ushort GrassWall => (ushort)WallType<DeepGrassWall>();
    public static ushort StoneWall => (ushort)WallType<DeepStoneWall>();
    public static ushort BrickWall => (ushort)WallType<DeepBricksWall>();
    public static ushort WoodWall => (ushort)WallType<DeepWoodWall>();

    // Furniture
    public static ushort Chest => (ushort)TileType<DeepChest>();
    public static ushort Door => (ushort)TileType<DeepWoodDoorClosed>();
    public static ushort Banner => (ushort)TileType<CrystalDepthsBanner>();
    public static ushort Bookcase => (ushort)TileType<DeepWoodBookcase>();
    public static ushort Sofa => (ushort)TileType<DeepWoodSofa>();
    public static ushort Table => (ushort)TileType<DeepWoodTable>();
    public static ushort Chair => (ushort)TileType<DeepWoodChair>();
    public static ushort Workbench => (ushort)TileType<DeepWoodWorkbench>();
    public static ushort Chandelier => (ushort)TileType<DeepWoodChandelier>();
    public static ushort Candle => (ushort)TileType<DeepWoodCandle>();
    public static ushort Lamp => (ushort)TileType<DeepWoodLamp>();
    public static ushort Crate => (ushort)TileType<DeepCrate>();
    public static ushort Platform => (ushort)TileType<DeepWoodPlatform>();

    // Other
    public static ushort KronosSpawn => (ushort)TileType<KronosSummonTile>();
    public static ushort Pillar => (ushort)TileType<DeepSecurityPillar>();
    public static ushort Pot => (ushort)TileType<DeepPot>();
}