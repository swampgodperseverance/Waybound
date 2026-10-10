using Terraria;

namespace Waybound.Helpers;

public static class GenHelper
{

    public static void SlopeZero(int x, int y)
    {
        WorldGen.SlopeTile(x, y, 0);
    }
    

    
    public static void RemoveAlone(int x, int y)
    {
        Tile tile = Framing.GetTileSafely(x, y);

        if (tile.HasTile && IsTileAround(x, y, false, false, false, false))
        {
            WorldGen.KillTile(x, y);
        }
    }
    
    public static void SlopeNaturally(int x, int y)
    {
        if (IsTileAround(x, y, false, false, true, true) && Main.rand.NextBool(2))
        {
            WorldGen.SlopeTile(x, y, 1);
        }
        
        
        if (IsTileAround(x, y, false, false, true, true) && Main.rand.NextBool(2))
        {
            WorldGen.SlopeTile(x, y, 1);
        }

        if (IsTileAround(x, y, false, true, true, false) && Main.rand.NextBool(3))
        {
            WorldGen.SlopeTile(x, y, 2);
        }

        if (IsTileAround(x, y, true, false, false, true) && Main.rand.NextBool(3))
        {
            WorldGen.SlopeTile(x, y, 3);
        }

        if (IsTileAround(x, y, true, true, false, false) && Main.rand.NextBool(2))
        {
            WorldGen.SlopeTile(x, y, 4);
        }

    }    

    private static bool IsTileAround(int x, int y, bool above, bool right, bool below, bool left)
    {
        Tile tileAbove = Framing.GetTileSafely(x, y - 1);
        Tile tileRight = Framing.GetTileSafely(x + 1, y);
        Tile tileBelow = Framing.GetTileSafely(x, y + 1);
        Tile tileLeft = Framing.GetTileSafely(x - 1, y);
        return tileAbove.HasTile == above &&
               tileRight.HasTile == right &&
               tileBelow.HasTile == below &&
               tileLeft.HasTile == left;
    }
}