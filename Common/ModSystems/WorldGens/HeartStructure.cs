using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;
using Waybound.Common.WUtils;
using Waybound.Content.Tiles.Blocks;
using Waybound.Content.Tiles.Walls;
using Waybound.Helpers;

namespace Waybound.Common.ModSystems.WorldGens;

internal static class HeartStructureTiles
{
    public static ushort Solid => (ushort)TileType<ResinBlock>();
    public static ushort Wall => (ushort)WallType<ResinWall>();
}



public class HeartStructure : BaseWorldGens
{
    // Gen variables
    public readonly (int min, int max) VeinLength = (50, 70);
    public readonly (int min, int max) VeinsCount = (10, 13);
    public readonly float VeinRotationChanceIncrement = 0.005f;
    public readonly float VeinRotationLimit = MathHelper.Pi / 6;
    public readonly int StartVeinWidth = 8;
    public readonly int VeinOffset = 30;

    public readonly float BranchChance = 0.2f;

    public readonly int Radius = 35;
    public readonly int Thickness = 9;
    public readonly int Harmonics = 12;

    public float AmplitudeInnerFac => 0.14f;
    public float AmplitudeOuterFac => 0.10f;


    public readonly int EvilBiomeSearchStep = 50;
    public readonly int EvilBiomeClusterThreshold = 200;

        

    public (int x, int y) Center { get; private set; }
    
    
    public (int x, int y) ApproximatePosMin => (Center.x - VeinOffset - VeinLength.max, Center.y - VeinOffset - VeinLength.max);
    public (int x, int y) ApproximatePosMax => (Center.x + VeinOffset + VeinLength.max, Center.y + VeinOffset + VeinLength.max);

    


    public override string NameGen => "Heart Structure????";
    public override bool GensBool { get; set; }

    public override bool Do_MakeGen(GenerationProgress progress)
    {
        progress.Message = Loc.GetChat("WorldGen.HeartStructure");

        var biomes = FindEvilBiomes(WorldGen.crimson);
        var inWhichBiome = WorldGen.genRand.Next(biomes.Count);

    
        Center = GetSpawnPosition(biomes[inWhichBiome]);
        VeinsGen(Center.x, Center.y);
        RoomGen(Center.x, Center.y);
        SlopeTiles();
        return true;
    }


    private (int x, int y) GetSpawnPosition(GenHelper.PointCluster biome)
    {
        var x = (int)biome.Center.X;
        var y = (int)biome.Max.Y + 50;
        return (x, y);
    }

    public List<GenHelper.PointCluster> FindEvilBiomes(bool crimson)
    {
        // TODO: mb fix long generating issue at seed 2.4.1.1434884539 

        List<Vector2> biomePoints = [];
        var ids = crimson
            ? new HashSet<ushort>  { TileID.CrimsonGrass, TileID.Crimstone, TileID.Crimsand }
            : new HashSet<ushort> { TileID.CorruptGrass, TileID.Ebonstone, TileID.Ebonsand };

        
        for (int x = 0; x < Main.maxTilesX; x += EvilBiomeSearchStep)
        for (int y = (int)Main.worldSurface; y < Main.rockLayer; y += EvilBiomeSearchStep)
        {
            Tile tile = Framing.GetTileSafely(x, y);
            if (ids.Contains(tile.TileType))
            {
                biomePoints.Add(new Vector2(x, y));
            }
        }

        var evilBiomes = GenHelper.ClusterPoints(biomePoints, EvilBiomeClusterThreshold);
        return evilBiomes;
    }


    private void RoomGen(int cX, int cY)
    {
        var ampInner = Radius * AmplitudeInnerFac;
        var ampOuter = Radius * AmplitudeOuterFac;

        var size = (Radius + ampInner + Thickness + ampOuter) * 2.25f;


        var a = new float[Harmonics];
        var a2 = new float[Harmonics];

        var ph = new float[Harmonics];
        for (int k = 0; k < Harmonics; k++)
        {
            a[k] = ampInner / (k + 1); // low fr
            a2[k] = ampOuter / (k + 1);
            ph[k] = (float)(WorldGen.genRand.NextDouble() * Math.PI * 2);
        }

        float c = size / 2f, halfThickness = Thickness / 2f;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x - c, dy = y - c;
            float dist = MathF.Sqrt(dx * dx + dy * dy);
            float ang = MathF.Atan2(dy, dx);

            float r = Radius;
            float outerR = Radius;
            for (int k = 0; k < Harmonics; k++)
            {
                r += a[k] * MathF.Sin((k + 2) * ang + ph[k]);
                outerR += a2[k] * MathF.Sin((k + 2) * ang + ph[k]);
            }

            var halfSize = (int)(size / 2);
            var globalX = cX + x - halfSize;
            var globalY = cY + y - halfSize;

            var tile = Framing.GetTileSafely(globalX, globalY);
            if (dist < r - halfThickness)
            {
                // inside
                tile.LiquidAmount = 0;
                WorldGen.KillTile(globalX, globalY);
                WorldGen.KillWall(globalX, globalY);
                WorldGen.PlaceWall(globalX, globalY, HeartStructureTiles.Wall);
            }
            else if (dist <= outerR - halfThickness + 2)
            {
                // inner border edge
                WorldGen.KillWall(globalX, globalY);
                WorldGen.PlaceWall(globalX, globalY, HeartStructureTiles.Wall);
                tile.TileType = HeartStructureTiles.Solid;
                tile.HasTile = true;
                tile.Slope = 0;
            }
            else if (dist <= outerR + halfThickness)
            {
                // border
                tile.TileType = HeartStructureTiles.Solid;
                tile.HasTile = true;
                tile.Slope = 0;
            }
        }
    }


    private void SlopeTiles()
    {
        for (int x = ApproximatePosMin.x; x < ApproximatePosMax.x; x++)
        for (int y = ApproximatePosMin.y; y < ApproximatePosMax.y; y++)
        {
            Tile tile = Framing.GetTileSafely(x, y);
            if (tile.HasTile && tile.TileType == HeartStructureTiles.Solid)
            {
                GenHelper.SlopeZero(x, y);
                GenHelper.SlopeNaturally(x, y);
            }
        }
    }

    private void VeinsGen(int x, int y)
    {
        var veins = WorldGen.genRand.Next(VeinsCount.min, VeinsCount.max);

        for (int i = 0; i < veins; i++)
        {
            var sector = MathHelper.TwoPi / veins;
            var directionOffset = WorldGen.genRand.NextFloat(-sector / 3, sector / 3);
            var rotation = sector * i + directionOffset;


            var stepsToLive = WorldGen.genRand.Next(VeinLength.min, VeinLength.max);

            Vector2 directionVector = Rotate(Vector2.UnitY, rotation);

            var gX = (int)(x + directionVector.X * VeinOffset);
            var gY = (int)(y + directionVector.Y * VeinOffset);

            VeinWalker(gX, gY, directionVector, stepsToLive, StartVeinWidth);
        }
    }


    private void VeinWalker(int startX, int startY, Vector2 targetDirVec, int stepsToLive, int startVeinWidth,
        bool canHaveBranches = true)
    {
        float x = startX;
        float y = startY;

        float rotChance = 0;
        var dirVec = targetDirVec;

        var isFirstRotation = true;

        for (int i = 0; i < stepsToLive; i++)
        {
            var veinWidth = startVeinWidth - (int)((float)i / stepsToLive * startVeinWidth);
            veinWidth = Math.Max(veinWidth, 1);

            PlaceVeinBlocks((int)x, (int)y, veinWidth);


            x += dirVec.X;
            y += dirVec.Y;
            rotChance += VeinRotationChanceIncrement;

            if (WorldGen.genRand.NextFloat(0, 1) < rotChance)
            {
                float rotationOffset;
                if (isFirstRotation)
                {
                    rotationOffset = WorldGen.genRand.NextFloat(-VeinRotationLimit, VeinRotationLimit);
                    isFirstRotation = false;
                }
                else
                {
                    float cross = targetDirVec.X * dirVec.Y - targetDirVec.Y * dirVec.X;
                    rotationOffset = WorldGen.genRand.NextFloat(VeinRotationLimit / 2, VeinRotationLimit * 2);
                    if (cross > 0)
                    {
                        rotationOffset = -rotationOffset;
                    }
                }

                dirVec = Rotate(dirVec, rotationOffset);
                rotChance = 0;

                if (canHaveBranches)
                {
                    GenerateBranchWalker(x, y, i, stepsToLive, rotationOffset, dirVec, veinWidth);
                }
            }
        }
    }

    private void GenerateBranchWalker(float x, float y, int step, int stepsToLive, float rotationOffset, Vector2 dirVec,
        int veinWidth)
    {
        if (WorldGen.genRand.NextFloat(0, 1) < BranchChance)
        {
            rotationOffset = -rotationOffset * WorldGen.genRand.NextFloat(1.25f, 2);
            dirVec = Rotate(dirVec, rotationOffset);

            var maxStepsToLive = stepsToLive - step;
            stepsToLive = WorldGen.genRand.Next(maxStepsToLive / 3, maxStepsToLive);


            VeinWalker((int)x, (int)y, dirVec, stepsToLive, veinWidth - 1, false);
        }
    }


    private static void PlaceVeinBlocks(int cX, int cY, int width)
    {
        var halfWidth = width / 2;
        for (int x = 0; x < width; x++)
        for (int y = 0; y < width; y++)
        {
            var gX = cX + x - halfWidth;
            var gY = cY + y - halfWidth;
            var tile = Framing.GetTileSafely(gX, gY);

            tile.TileType = HeartStructureTiles.Solid;
            tile.HasTile = true;
            tile.Slope = 0;
        }
    }


    static Vector2 Rotate(Vector2 v, float radians)
    {
        // TODO: put in som static helper class??
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);

        return new Vector2(
            v.X * cos - v.Y * sin,
            v.X * sin + v.Y * cos
        );
    }
}