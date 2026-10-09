using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace Waybound.Common.Systems
{
    public class DeepOceanSystem : ModSystem
    {
        public const float DepthMultiplier = 2f;
        public const float WidthMultiplier = 1.6f;
        public const float MaxWidthFraction = 0.22f;
        public const int SandDepth = 10;
        public const int UnderworldMargin = 350;

        public static bool OceanOnLeft;
        public static int OceanWidth;
        public static int OceanFloorY;
        public static int SeaTopY;

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int index = tasks.FindIndex(t => t.Name.Equals("Create Ocean Caves"));
            if (index == -1)
                index = tasks.FindIndex(t => t.Name.Equals("Beaches"));
            if (index == -1)
                return;

            tasks.Insert(index + 1, new PassLegacy("Waybound Deep Ocean", GenerateDeepOcean));
        }

        public override void ClearWorld()
        {
            OceanOnLeft = false;
            OceanWidth = 0;
            OceanFloorY = 0;
            SeaTopY = 0;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["oceanLeft"] = OceanOnLeft;
            tag["oceanWidth"] = OceanWidth;
            tag["oceanFloor"] = OceanFloorY;
            tag["oceanSeaTop"] = SeaTopY;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            OceanOnLeft = tag.GetBool("oceanLeft");
            OceanWidth = tag.GetInt("oceanWidth");
            OceanFloorY = tag.GetInt("oceanFloor");
            SeaTopY = tag.GetInt("oceanSeaTop");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(OceanOnLeft);
            writer.Write(OceanWidth);
            writer.Write(OceanFloorY);
            writer.Write(SeaTopY);
        }

        public override void NetReceive(BinaryReader reader)
        {
            OceanOnLeft = reader.ReadBoolean();
            OceanWidth = reader.ReadInt32();
            OceanFloorY = reader.ReadInt32();
            SeaTopY = reader.ReadInt32();
        }

        private static int ToX(int u)
        {
            return OceanOnLeft ? u : Main.maxTilesX - 1 - u;
        }

        private static int FindWaterTop(int x)
        {
            int limit = Main.maxTilesY - 200;
            for (int y = 0; y < limit; y++)
            {
                Tile tile = Main.tile[x, y];

                if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                    return -1;

                if (tile.LiquidAmount > 0 && tile.LiquidType == LiquidID.Water)
                    return y;
            }
            return -1;
        }

        private static int FindSolidFrom(int x, int startY)
        {
            int limit = Main.maxTilesY - 200;
            for (int y = Math.Max(10, startY); y < limit; y++)
            {
                Tile tile = Main.tile[x, y];
                if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                    return y;
            }
            return -1;
        }

        private void GenerateDeepOcean(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Digging the deep ocean";

            OceanOnLeft = Main.dungeonX > Main.maxTilesX / 2;

            int maxScan = Main.maxTilesX / 3;
            int seaTop = int.MaxValue;
            int deepest = 0;
            int width = 0;

            for (int u = 0; u < maxScan; u++)
            {
                int x = ToX(u);
                int water = FindWaterTop(x);

                if (water < 0)
                {
                    if (u > 40) break;
                    continue;
                }

                int floor = FindSolidFrom(x, water);
                if (floor < 0 || floor - water < 3)
                {
                    if (u > 40) break;
                    continue;
                }

                seaTop = Math.Min(seaTop, water);
                deepest = Math.Max(deepest, floor);
                width = u + 1;
            }

            if (width < 30 || seaTop == int.MaxValue)
            {
                Mod.Logger.Warn("Deep ocean: vanilla ocean was not detected, skipping.");
                OceanWidth = 0;
                return;
            }

            int baseDepth = deepest - seaTop;
            int maxFloor = Main.maxTilesY - UnderworldMargin;
            int newWidth = Math.Min((int)(width * WidthMultiplier), (int)(Main.maxTilesX * MaxWidthFraction));
            int newDepth = Math.Min((int)(baseDepth * DepthMultiplier), maxFloor - seaTop);

            if (newWidth <= 0 || newDepth <= 3)
                return;

            float phase = (float)(WorldGen.genRand.NextDouble() * MathHelper.TwoPi);
            int carvedFloor = 0;

            for (int u = 0; u < newWidth; u++)
            {
                progress.Set(u / (float)newWidth);

                int x = ToX(u);
                float norm = u / (float)newWidth;
                float t = MathHelper.Clamp((norm - 0.3f) / 0.7f, 0f, 1f);
                float shape = 1f - t * t * (3f - 2f * t);
                float noise = (MathF.Sin(u * 0.045f + phase) * 2f + MathF.Sin(u * 0.013f + phase * 1.7f) * 4f)
                    * MathHelper.Clamp(shape * 4f, 0f, 1f);

                int target = seaTop + (int)(newDepth * shape + noise);
                target = Math.Min(target, maxFloor);

                if (target < seaTop + 3) continue;

                int surface = FindSolidFrom(x, seaTop - 120);
                if (surface < 0 || target <= surface) continue;

                for (int y = surface; y < target; y++)
                {
                    Tile tile = Main.tile[x, y];

                    if (tile.HasTile && Main.tileDungeon[tile.TileType])
                        continue;

                    tile.ClearTile();
                    tile.WallType = 0;

                    if (y >= seaTop)
                    {
                        tile.LiquidType = LiquidID.Water;
                        tile.LiquidAmount = 255;
                    }
                    else
                    {
                        tile.LiquidAmount = 0;
                    }
                }

                for (int y = target; y < target + SandDepth && y < Main.maxTilesY - 200; y++)
                {
                    Tile tile = Main.tile[x, y];

                    if (tile.HasTile && Main.tileDungeon[tile.TileType])
                        continue;

                    tile.ResetToType(y < target + SandDepth - 3 ? TileID.Sand : TileID.HardenedSand);
                    tile.LiquidAmount = 0;
                }

                carvedFloor = Math.Max(carvedFloor, target);
            }

            OceanWidth = newWidth;
            OceanFloorY = carvedFloor;
            SeaTopY = seaTop;
        }
    }

    public class DeepOceanBiome : ModBiome
    {
        public override bool IsBiomeActive(Player player)
        {
            if (DeepOceanSystem.OceanWidth <= 0) return false;

            int tileX = (int)(player.Center.X / 16f);
            int tileY = (int)(player.Center.Y / 16f);
            int dist = DeepOceanSystem.OceanOnLeft ? tileX : Main.maxTilesX - 1 - tileX;

            if (dist < 0 || dist >= DeepOceanSystem.OceanWidth) return false;
            if (tileY < DeepOceanSystem.SeaTopY - 40 || tileY > DeepOceanSystem.OceanFloorY) return false;

            return true;
        }

        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/DeepOcean");
    }
}