using System;
using System.Collections.Generic;
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


    public static List<PointCluster> ClusterPoints(
        List<Vector2> points,
        float threshold)
    {
        var clusters = new List<PointCluster>();
        var visited = new bool[points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            if (visited[i])
                continue;

            var cluster = new PointCluster();
            var queue = new Queue<int>();

            queue.Enqueue(i);
            visited[i] = true;

            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                Vector2 point = points[current];

                cluster.Points.Add(point);

                for (int j = 0; j < points.Count; j++)
                {
                    if (visited[j])
                        continue;

                    if ((points[j] - point).LengthSquared() <= threshold * threshold)
                    {
                        visited[j] = true;
                        queue.Enqueue(j);
                    }
                }
            }

            CalculateBounds(cluster);
            clusters.Add(cluster);
        }

        return clusters;
    }


    public class PointCluster
    {
        public List<Vector2> Points { get; } = [];
        public Vector2 Center { get; set; }

        public Vector2 Min { get; set; }
        public Vector2 Max { get; set; }
    }

    
    private static void CalculateBounds(PointCluster cluster)
    {
        Vector2 sum = Vector2.Zero;

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (var p in cluster.Points)
        {
            sum += p;

            minX = Math.Min(minX, p.X);
            minY = Math.Min(minY, p.Y);
            maxX = Math.Max(maxX, p.X);
            maxY = Math.Max(maxY, p.Y);
        }

        cluster.Center = sum / cluster.Points.Count;
        cluster.Min = new Vector2(minX, minY);
        cluster.Max = new Vector2(maxX, maxY);
    }
}