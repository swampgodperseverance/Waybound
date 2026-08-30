// Code by SerNik
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace Waybound.Helpers
{
    public class WorldHelper
    {
        public static void Cleaning(int startX, int startY, int endX, int endY, params int[] type) {
            int minX = Math.Min(startX, endX);
            int maxX = Math.Max(startX, endX);
            int minY = Math.Min(startY, endY);
            int maxY = Math.Max(startY, endY);

            HashSet<int> tileTypesToDestroy = [.. type];

            for (int x = minX; x <= maxX; x++) {
                for (int y = minY; y <= maxY; y++) {
                    if (WorldGen.InWorld(x, y, 10) && Main.tile[x, y].HasTile && tileTypesToDestroy.Contains(Main.tile[x, y].TileType)) {
                        Tile tile = Main.tile[x, y];
                        tile.WallType = WallID.None;
                        tile.HasTile = false;
                    }
                }
            }
        }
        public static void CleaningAll(int startX, int startY, int endX, int endY) {
            int minX = Math.Min(startX, endX);
            int maxX = Math.Max(startX, endX);
            int minY = Math.Min(startY, endY);
            int maxY = Math.Max(startY, endY);

            for (int x = minX; x <= maxX; x++) {
                for (int y = minY; y <= maxY; y++) {
                    if (WorldGen.InWorld(x, y, 10)) {
                        Tile tile = Main.tile[x, y];
                        tile.WallType = WallID.None;
                        tile.HasTile = false;
                        tile.LiquidAmount = 0;
                    }
                }
            }
        }
        public static void CleaningLiquid(int startX, int startY, int endX, int endY) {
            int minX = Math.Min(startX, endX);
            int maxX = Math.Max(startX, endX);
            int minY = Math.Min(startY, endY);
            int maxY = Math.Max(startY, endY);

            for (int x = minX; x <= maxX; x++) {
                for (int y = minY; y <= maxY; y++) {
                    if (WorldGen.InWorld(x, y, 10)) {
                        Tile tile = Main.tile[x, y];
                        tile.LiquidAmount = 0;
                    }
                }
            }
        }
        public static bool CheckBiome(Player player, int width, int height, int left, int top, int buff = 1) {
            Point pos = player.Center.ToTileCoordinates();

            int right = left + width;
            int bottom = top + height;

            if (pos.X >= left && pos.X < right && pos.Y >= top && pos.Y < bottom) {
                if (buff != 1) player.AddBuff(buff, 3);
                return true;
            }

            return false;
        }
        public static bool CheckBiomeTile(int x, int y, int width, int height, int left, int top) {
            int right = left + width;
            int bottom = top + height;

            return x >= left && x < right && y >= top && y < bottom;
        }
        public static void AddContainersLoot(int style, int chance, int item, int min = 0, int max = 0) {
            for (int chestIndex = 0; chestIndex < 1000; chestIndex++) {
                Chest chest = Main.chest[chestIndex];
                if (chest != null && Main.tile[chest.x, chest.y].TileType == TileID.Containers && Main.tile[chest.x, chest.y].TileFrameX == style * 36) {
                    if (Main.rand.NextBool(chance)) {
                        for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++) {
                            if (chest.item[inventoryIndex].type == ItemID.None) {
                                chest.item[inventoryIndex].SetDefaults(item);
                                Random(chest.item, ref inventoryIndex, min, max);
                                break;
                            }
                        }
                    }
                }
            }
        }
        public static void AddContainersLoot(int style, int chance, List<int> itemTypeFoEditing, int item, int min = 0, int max = 0) {
            for (int chestIndex = 0; chestIndex < 1000; chestIndex++) {
                Chest chest = Main.chest[chestIndex];
                if (chest != null && Main.tile[chest.x, chest.y].TileType == TileID.Containers && Main.tile[chest.x, chest.y].TileFrameX == style * 36) {
                    if (Main.rand.NextBool(chance)) {
                        for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++) {
                            foreach (int type in itemTypeFoEditing) {
                                if (chest.item[inventoryIndex].type == type) {
                                    chest.item[inventoryIndex].SetDefaults(item);
                                    Random(chest.item, ref inventoryIndex, min, max);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void DestroyerContainersLoot(int style, int itemTypeFoEditing) {
            for (int chestIndex = 0; chestIndex < 1000; chestIndex++) {
                Chest chest = Main.chest[chestIndex];
                if (chest != null && Main.tile[chest.x, chest.y].TileType == TileID.Containers && Main.tile[chest.x, chest.y].TileFrameX == style * 36) {
                    for (int inventoryIndex = 0; inventoryIndex < 40; inventoryIndex++) {
                        if (chest.item[inventoryIndex].type == itemTypeFoEditing) {
                            chest.item[inventoryIndex].SetDefaults();
                            break;
                        }
                    }
                }
            }
        }
        public static void LootInContainers(Item[] ChestInventory, ref int Index, int Item, int Min = 1, int Max = 1) {
            ChestInventory[Index].SetDefaults(Item); Random(ChestInventory, ref Index, Min, Max); Index++;
        }
        public static void RandomLootInCoutainer(Item[] ChestInventory, ref int Index, int Min = 1, int Max = 1, params int[] Items) { 
            ChestInventory[Index].SetDefaults(Utils.SelectRandom(WorldGen.genRand, Items));
            Random(ChestInventory, ref Index, Min, Max); Index++;
        }
        /// <param name="Tire">1:Copper, 2:Iron, 3:Silver, 4:Gold, 5: Copper </param>
        public static void IfOreTireLoot(Item[] ChestInventory, ref int Index, int Tire, int IfTrue, int Else, int Min = 1, int Max = 1) {
            bool ThisWorldHasOre = Tire switch
            {
                1 => WorldGen.SavedOreTiers.Copper == TileID.Copper,
                2 => WorldGen.SavedOreTiers.Iron == TileID.Iron,
                3 => WorldGen.SavedOreTiers.Silver == TileID.Silver,
                4 => WorldGen.SavedOreTiers.Gold == TileID.Gold,
                _ => WorldGen.SavedOreTiers.Copper == TileID.Copper,
            };

            int itemToGive = ThisWorldHasOre ? IfTrue : Else;

            ChestInventory[Index].SetDefaults(itemToGive); Random(ChestInventory, ref Index, Min, Max); Index++;
        }
        static void Random(Item[] ChestInventory, ref int Index, int Min, int Max) {
            int a = Max != 0 ? 1 : 0;
            ChestInventory[Index].stack = Main.rand.Next(Min, Max + a);
        }
    }
}