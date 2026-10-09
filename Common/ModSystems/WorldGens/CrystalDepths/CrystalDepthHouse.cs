using Terraria;
using Waybound.Content.Tiles.Furniture.DeepWoodFurniture;

namespace Waybound.Common.ModSystems.WorldGens.CrystalDepths;

public class CrystalDepthHouse
{
    private static readonly byte[,] _variant0 =
{
    { 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0 },
    { 0, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0 },
    { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
    { 0, 0, 1, 3, 3, 3, 3, 3, 5, 5, 3, 3, 3, 0, 0 },
    { 0, 0, 8, 4, 4, 4, 4, 4, 6, 6, 4, 4, 4, 0, 0 },
    { 0, 0, 8, 4, 4, 4, 4, 4, 6, 6, 4, 4, 4, 0, 0 },
    { 0, 0, 2, 4, 4, 4, 4, 4, 6, 6, 4, 4, 2, 2, 0 },
    { 0, 0, 2, 4, 4, 4, 4, 4, 6, 6, 4, 4, 2, 0, 0 },
    { 1, 1, 2, 2, 2, 2, 2, 2, 6, 6, 2, 2, 2, 1, 1 },
    { 0, 1, 1, 4, 4, 4, 4, 4, 4, 4, 4, 4, 1, 1, 0 },
    { 0, 0, 1, 1, 4, 4, 4, 4, 4, 4, 4, 1, 1, 0, 0 },
    { 0, 0, 0, 1, 1, 4, 4, 4, 4, 4, 1, 1, 0, 0, 0 },
    { 0, 0, 0, 0, 1, 1, 4, 4, 4, 1, 1, 0, 0, 0, 0 },
    { 0, 0, 0, 0, 0, 1, 1, 4, 1, 1, 0, 0, 0, 0, 0 },
    { 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
    { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 },
};

    private static readonly byte[,] _variant1 =
    {
        { 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 0, 0, 0 },
        { 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 },
        { 8, 3, 3, 3, 5, 5, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 0, 0, 0 },
        { 8, 8, 8, 8, 5, 5, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 0, 0, 0, 0 },
        { 0, 8, 8, 8, 5, 5, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 0, 0, 0, 0, 0 },
        { 0, 8, 7, 8, 5, 5, 8, 8, 8, 8, 8, 8, 8, 8, 8, 7, 0, 0, 0, 0, 0 },
        { 1, 1, 1, 1, 5, 5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 },
        { 0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 1, 0, 0 },
        { 0, 0, 0, 8, 8, 8, 8, 8, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 8, 0, 0 },
        { 0, 0, 0, 0, 0, 8, 8, 8, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 8, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 8, 2, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 2, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 2, 4, 4, 4, 4, 4, 4, 4, 4, 4, 1, 1, 1, 1 },
        { 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 0, 0, 0, 0, 0, 0, 0, 7, 4, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
    };

    private static readonly byte[,] _variant2 =
    {
        { 0, 0, 0, 0, 7, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 1, 1, 1, 1, 1, 5, 5, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 1, 3, 3, 3, 3, 5, 5, 1, 0, 0, 0, 7, 0, 0, 0, 0 },
        { 0, 0, 0, 4, 4, 4, 4, 5, 5, 1, 0, 0, 1, 1, 1, 0, 0, 0 },
        { 0, 0, 0, 4, 4, 4, 4, 5, 5, 1, 1, 1, 1, 1, 1, 1, 0, 0 },
        { 0, 0, 2, 4, 4, 4, 4, 5, 5, 3, 3, 3, 3, 3, 3, 1, 0, 0 },
        { 1, 1, 2, 2, 2, 2, 2, 5, 5, 5, 5, 4, 4, 4, 4, 0, 0, 0 },
        { 0, 1, 1, 4, 4, 4, 2, 4, 4, 5, 5, 4, 4, 4, 4, 0, 0, 0 },
        { 0, 0, 1, 1, 4, 4, 2, 4, 4, 5, 5, 4, 4, 4, 4, 2, 0, 0 },
        { 0, 0, 0, 1, 1, 4, 2, 4, 4, 5, 5, 4, 4, 4, 4, 2, 0, 0 },
        { 0, 0, 0, 0, 1, 1, 2, 2, 2, 5, 5, 2, 2, 2, 2, 2, 1, 1 },
        { 0, 0, 0, 0, 0, 1, 1, 4, 4, 4, 4, 4, 4, 4, 4, 1, 1, 0 },
        { 0, 0, 0, 0, 0, 0, 1, 1, 4, 4, 4, 4, 4, 4, 1, 1, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 1, 1, 4, 4, 4, 4, 1, 1, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 4, 4, 1, 1, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0 },
    };

    private static readonly byte[,] _variant3 =
    {
        { 0, 0, 0, 0, 7, 0, 0, 0, 0, 0, 7, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0 },
        { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 },
        { 0, 0, 0, 7, 3, 3, 3, 3, 3, 3, 3, 3, 1, 0, 0, 0 },
        { 0, 0, 0, 7, 0, 8, 4, 4, 4, 4, 4, 4, 8, 0, 0, 0 },
        { 0, 0, 0, 7, 0, 8, 4, 4, 4, 6, 6, 4, 8, 0, 0, 0 },
        { 0, 0, 0, 7, 0, 2, 4, 4, 4, 6, 6, 4, 2, 0, 0, 0 },
        { 0, 1, 1, 1, 1, 1, 1, 1, 1, 6, 6, 1, 1, 1, 1, 0 },
        { 0, 1, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 1, 0 },
        { 0, 0, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 2, 0 },
        { 0, 0, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 2, 0 },
        { 0, 2, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 2, 0 },
        { 1, 1, 1, 1, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 2, 0 },
        { 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 0 },
        { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 7, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 7, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1 },
    };


    public int Variant { get; private set; }

    public int Height => _targetTiles.GetLength(0);
    public int Width => _targetTiles.GetLength(1);

    private byte[,] _targetTiles;
    

    public CrystalDepthHouse(int variant)
    {
        Variant = variant;
        switch (Variant)
        {
            case 0:
                _targetTiles = _variant0;
                break;
            case 1:
                _targetTiles = _variant1;
                break;
            case 2:
                _targetTiles = _variant2;
                break;
            case 3:
                _targetTiles = _variant3;
                break;
        }
    }
    
    public void Place(int startX, int startY)
    {
        var houseChestX = 0;
        var houseChestY = 0;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tile tile = Framing.GetTileSafely(startX + x, startY - y);
                var globalX = startX + x;
                var globalY = startY - y;

                PlaceBlock(_targetTiles[y, x], globalX, globalY);
            }
        }

        GenerateFurnitureAndChest(startX, startY, houseChestX, houseChestY);
    }

    private void PlaceBlock(byte block, int globalX, int globalY)
    {
        Tile tile = Framing.GetTileSafely(globalX, globalY);
        switch (block)
        {
            case 0:
                break;
            case 1:
                tile.TileType = DepthsTiles.Brick;
                tile.HasTile = true;
                WorldGen.SlopeTile(globalX, globalY, 0);
                break;
            case 2:
                tile.TileType = DepthsTiles.Wood;
                tile.HasTile = true;
                WorldGen.SlopeTile(globalX, globalY, 0);
                break;
            case 3:
                WorldGen.KillTile(globalX, globalY);
                WorldGen.KillWall(globalX, globalY);
                WorldGen.PlaceWall(globalX, globalY, DepthsTiles.BrickWall);
                break;
            case 4:
                WorldGen.KillTile(globalX, globalY);
                WorldGen.KillWall(globalX, globalY);
                WorldGen.PlaceWall(globalX, globalY, DepthsTiles.WoodWall);
                break;
            case 5:
                WorldGen.KillTile(globalX, globalY);
                tile.TileType = DepthsTiles.Platform;
                tile.HasTile = true;
                WorldGen.KillWall(globalX, globalY);
                WorldGen.PlaceWall(globalX, globalY, DepthsTiles.BrickWall);
                break;
            case 6:
                WorldGen.KillTile(globalX, globalY);
                tile.TileType = DepthsTiles.Platform;
                tile.HasTile = true;
                WorldGen.KillWall(globalX, globalY);
                WorldGen.PlaceWall(globalX, globalY, DepthsTiles.WoodWall);
                break;
            case 7:
                if (!tile.HasTile)
                {
                    tile.TileType = DepthsTiles.Beam;
                    tile.HasTile = true;
                }
                WorldGen.SlopeTile(globalX, globalY, 0);
                break;
            case 8:
                WorldGen.KillTile(globalX, globalY);
                break;
        }
    }

    private void GenerateFurnitureAndChest(int startX, int startY, int houseChestX, int houseChestY)
    {
        (int x, int y, ushort type)[] furniture = [];

        switch (Variant)
        {
            case 0:
                furniture =
                [
                    (5, 3, DepthsTiles.Table),
                    (5, 5, DepthsTiles.Candle),
                    (7, 3, DepthsTiles.Chair),
                    (12, 3, DepthsTiles.Door),
                ];
                houseChestX = 5;
                houseChestY = 9;
                break;
            case 1:
                furniture =
                [
                    (7, 8, DepthsTiles.Door),
                    (10, 8, DepthsTiles.Table),
                    (10, 12, DepthsTiles.Chandelier),
                    (12, 8, DepthsTiles.Chair),
                    (15, 8, DepthsTiles.Bookcase),
                    (12, 3, DepthsTiles.Sofa),
                    (14, 3, DepthsTiles.Lamp),
                ];
                houseChestX = 6;
                houseChestY = 3;
                break;
            case 2:
                furniture =
                [
                    (12, 6, DepthsTiles.Table),
                    (12, 8, DepthsTiles.Candle),
                    (14, 6, DepthsTiles.Chair),
                    (11, 12, DepthsTiles.Crate),
                ];
                houseChestX = 5;
                houseChestY = 3;
                break;
            case 3:
                furniture =
                [
                    (8, 3, DepthsTiles.Workbench),
                    (9, 4, DepthsTiles.Candle),
                    (7, 3, DepthsTiles.Chair),
                    (5, 8, DepthsTiles.Crate),
                    (3, 8, DepthsTiles.Crate),
                    (6, 10, DepthsTiles.Candle),
                    (4, 10, DepthsTiles.Crate),
                    (12, 10, DepthsTiles.Bookcase),
                    (5, 3, DepthsTiles.Door),
                ];
                break;
        }

        foreach (var item in furniture)
        {
            WorldGen.PlaceObject(startX + item.x, startY - item.y, item.type, true, 0, -1, -1);
        }

        if (houseChestX != 0 || houseChestY != 0)
        {
            WorldGen.PlaceChest(startX + houseChestX, startY - houseChestY, DepthsTiles.Chest, false, 0);
        }
    }
}