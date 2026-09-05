using Waybound.Helpers;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.WorldBuilding;
using Microsoft.Xna.Framework;

namespace Waybound.Common.ModSystems.WorldGens
{
    public class GenerationDesertPillar1 : BaseWorldGens
    {
        //x = 5, y = 25
        static readonly byte[,] DesertPillar1Tiles =
        {
            // 0 - empty / 1 - Sandstone Slab  / 2 - Sandstone  Brick
            {1,2,2,1,2 }, // 1
            {1,2,2,1,2 }, // 2
            {1,1,2,1,1 }, // 3
            {1,1,2,1,1 }, // 4
            {1,1,2,1,1 },    // 5
            {1,2,2,2,0 }, // 6
            {1,2,0,0,0 }, // 7
            {2,1,2,2,0 }, // 8
            {2,1,2,2,0 },    // 9
            {0,1,2,1,0 }, // 10
            {0,2,2,1,0 }, // 11
            {0,2,2,1,0 }, // 12
            {0,0,2,2,0 }, // 13
            {0,0,1,1,0 }, // 14
            {0,0,2,1,0 }, // 15
            
        };
        static readonly byte[,] DesertPillar1Slopes =
        {
            // 0 - empty / 1 - hamer / 2 - /| / 3 - |/ / 4 - \| / 5 - |\
            {0,0,0,0,0 }, // 1
            {0,0,0,0,0 }, // 2
            {0,0,0,0,0 }, // 3
            {0,0,0,0,0 }, // 4
            {0,0,0,0,5 }, // 5
            {0,0,0,5,0 }, // 6
            {0,0,0,0,0 }, // 7
            {0,0,0,3,0 }, // 8
            {2,0,0,0,0 }, // 9
            {0,0,0,0,0 }, // 10
            {0,0,0,0,0 }, // 11
            {0,2,0,0,0 }, // 12
            {0,0,0,0,0 }, // 13
            {0,0,0,0,0 }, // 14
            {0,0,2,0,0 }, // 15
        };
        static readonly byte[,] DesertPillar1Walls =
        {
            // 0 - empty / 1 - BackWoods Root Wall / 2 - Stone Slab Wall / 3 - Palm Wood Fence + Brown / 4 - Shadewood Fence + Brown / 5 - Gray brick Wall / 6 - Living wood wall / 7 - Rich Mahogany Fence + Brown / 8 - Wood Wall / 9 - Planked Wall / 10:a - Glass Wall / 11:b - ice Brick Wall / 12:c - Resistant Wood Fence / 13:d - Everwood Wall / 14:e - Cloud Wail / 15:f - Tin Brick Wall
            {0,0,0,0,0 }, // 1
            {0,0,0,0,0 }, // 2
            {0,0,0,0,0 }, // 3
            {0,0,0,0,0 }, // 4
            {0,0,0,0,0 }, // 5
            {0,0,1,0,0 }, // 6
            {0,0,1,0,0 }, // 7
            {0,0,0,0,0 }, // 8
            {0,1,0,0,0 }, // 9
            {0,1,1,1,0 }, // 10
            {0,0,1,1,1 }, // 11
            {0,0,0,0,0 }, // 12
            {0,0,0,0,0 }, // 13
            {0,0,0,0,0 }, // 14
            {0,0,0,0,0 }, // 15
        };

        static readonly int[] DesertPillar1GenTiles = [TileID.Sand, TileID.HardenedSand, TileID.Sandstone];
        bool GenerateDesertPillar1 = false;
        public override bool GensBool { get => GenerateDesertPillar1; set => GenerateDesertPillar1 = value; }
        public override string NameGen => "[Waybound] Desert Pillar";

        public override string VanillaIndexName => "Full Desert";
        public override int Index => 1;

        public override bool Do_MakeGen(GenerationProgress progress)
        {
            if (progress != null)
            {
                progress.Message = Language.GetTextValue("Mods.Waybound.WorldGenString.Pillar1");
                progress.Set(0.33f);
            }

            int width = DesertPillar1Tiles.GetLength(1);
            int height = DesertPillar1Tiles.GetLength(0);

            int pillarsSpawned = 0;
            int targetPillars = 3;
            int attempts = 0;

            while (pillarsSpawned < targetPillars && attempts < 1500)
            {
                attempts++;

                int i = WorldGen.genRand.Next(Main.maxTilesX / 5, Main.maxTilesX / 5 * 4);
                int y = (int)Main.worldSurface;

                while (y < Main.maxTilesY - 300 && !WorldGen.SolidOrSlopedTile(i, y + 1))
                {
                    y++;
                }

                bool rightTile = false;
                int tileTypeUnder = Main.tile[i, y + 1].TileType;
                foreach (int t in DesertPillar1GenTiles)
                {
                    if (tileTypeUnder == t)
                    {
                        rightTile = true;
                        break;
                    }
                }
                if (!rightTile) continue;

                bool canBeGenerated = true;
                for (int j = 0; j < width; j++)
                {
                    if (!WorldGen.InWorld(i + j, y + 1) || !WorldGen.SolidOrSlopedTile(i + j, y + 1))
                    {
                        canBeGenerated = false;
                        break;
                    }
                }

                if (canBeGenerated)
                {
                    int basePosX = i;
                    int basePosY = y;

                    for (int X = 0; X < width; X++)
                    {
                        for (int Y = 0; Y < height; Y++)
                        {
                            int worldX = basePosX + X;
                            int worldY = basePosY - Y;

                            if (!WorldGen.InWorld(worldX, worldY, 10))
                                continue;

                            Tile tile = Framing.GetTileSafely(worldX, worldY);
                            tile.ClearEverything();

                            switch (DesertPillar1Tiles[Y, X])
                            {
                                case 0: break;
                                case 1: tile.TileType = TileID.SandStoneSlab; tile.HasTile = true; break;
                                case 2: tile.TileType = TileID.SandstoneBrick; tile.HasTile = true; break;
                            }
                            switch (DesertPillar1Walls[Y, X])
                            {
                                case 0: WorldGen.KillWall(worldX, worldY); break;
                                case 1: tile.WallType = WallID.SandstoneBrick; break;
                            }
                            switch (DesertPillar1Slopes[Y, X])
                            {
                                case 0: break;
                                case 1: tile.IsHalfBlock = true; break;
                                case 2: tile.Slope = SlopeType.SlopeDownRight; break;
                                case 3: tile.Slope = SlopeType.SlopeUpLeft; break;
                                case 4: tile.Slope = SlopeType.SlopeUpRight; break;
                                case 5: tile.Slope = SlopeType.SlopeDownLeft; break;
                            }
                            if (DesertPillar1Tiles[Y, X] != 0)
                            {
                                WayboundGenVars.VillageTiles.Add(new Vector2(worldX, worldY));
                            }
                            if (DesertPillar1Walls[Y, X] != 0)
                            {
                                WayboundGenVars.VillageWalles.Add(new Vector2(worldX, worldY));
                            }
                        }
                    }

                    WorldGen.PlaceObject(basePosX + 2, basePosY - 6, TileID.Torches, mute: false, style: 16);

                    pillarsSpawned++;
                }
            }

            if (pillarsSpawned > 0)
            {
                WayboundGenVars.DesertPillar1Gen = true;
                return true;
            }

            return false;
        }
    }
}