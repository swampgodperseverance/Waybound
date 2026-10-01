using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Waybound.Content.Tiles.BossSummon;
using Waybound.Content.Tiles.Banners;
using Waybound.Content.Tiles.Blocks;
using Waybound.Content.Tiles.Furniture;
using Waybound.Content.Tiles.Plants;
using Waybound.Content.Tiles.Walls;
using Waybound.Content.Tiles.Furniture.DeepWoodFurniture;
using Waybound.Content.Tiles.Banners.PreHM;

namespace Waybound.Common.ModSystems.WorldGens
{
	public class CrystalDepthGenSystem : ModSystem
	{
		public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
		{
			int crystalDepthIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
			if (crystalDepthIndex != -1)
			{
				tasks.Insert(crystalDepthIndex + 5, new crystalDepthGen("Infecting the bowels of the Earth...", 237.4298f));
			}
		}
	}
	public class crystalDepthGen : GenPass
	{
		public float size;
		public int XPos;
		public int YPos;
		public int width;
		public int height;
		public int smoothCycles = 4;
		private int[,] cavePoints;
		public int randFillPercent = 49;

		public int dirtTile = ModContent.TileType<DeepDirt>();
		public int stoneTile = ModContent.TileType<Content.Tiles.Blocks.DeepStone>();
		public int crystalTile = ModContent.TileType<DeepCrystal>();
		public int crystalTile2 = ModContent.TileType<DeepCrystal2>();
		public int brickTile = ModContent.TileType<DeepBricks>();
		public int treeTile = ModContent.TileType<DeepTree>();
		public int woodTile = ModContent.TileType<DeepWood>();
		public int beamTile = ModContent.TileType<DeepWoodBeam>();
		public int leafTile = ModContent.TileType<DeepbloomLeaf>();
		public int grassTile = ModContent.TileType<DeepGrass>();
		public int vine = ModContent.TileType<DeepbloomVine>();
		public int shard = ModContent.TileType<DeepCrystalShard>();
		public int shardBig1 = ModContent.TileType<DeepCrystalShardBig1>();
		public int shardBig2 = ModContent.TileType<DeepCrystalShardBig2>();
		public int shardBig3 = ModContent.TileType<DeepCrystalShardBig3>();
		public int herb = ModContent.TileType<DeepHerb>();

		public int dirtWall = ModContent.WallType<DeepDirtWall>();
		public int grassWall = ModContent.WallType<DeepGrassWall>();
		public int stoneWall = ModContent.WallType<DeepStoneWall>();
		public int brickWall = ModContent.WallType<DeepBricksWall>();
		public int woodWall = ModContent.WallType<DeepWoodWall>();

		public int chest = ModContent.TileType<DeepChest>();
		public int door = ModContent.TileType<DeepWoodDoorClosed>();
		public int banner = ModContent.TileType<CrystalDepthsBanner>();
		public int bookcase = ModContent.TileType<DeepWoodBookcase>();
		public int sofa = ModContent.TileType<DeepWoodSofa>();
		public int table = ModContent.TileType<DeepWoodTable>();
		public int chair = ModContent.TileType<DeepWoodChair>();
		public int workbench = ModContent.TileType<DeepWoodWorkbench>();
        public int chandelier = ModContent.TileType <DeepWoodChandelier>();
        public int candle = ModContent.TileType<DeepWoodCandle>();
        public int lamp = ModContent.TileType<DeepWoodLamp>();
        public int crate = ModContent.TileType<DeepCrate>();
		public int plat = ModContent.TileType<DeepWoodPlatform>();

		public int kronosSpawn = ModContent.TileType<KronosSummonTile>();
		public int pillar = ModContent.TileType<DeepSecurityPillar>();
		public int pot = ModContent.TileType<DeepPot>();

		public int threshold = 4;
		public int treeRandom;
		public int treasureRandom;
		public int houseRandom;
		public int houseChestX = 0;
		public int houseChestY = 0;
		public int waterRand;
		public int treeChestX;
		public int treeChestY;
		public int seed;
		public int maxTreasure;
		public int maxHouse;
		public System.Random randChoice;

		public crystalDepthGen(string name, float loadWeight) : base(name, loadWeight)
		{
		}
		protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
		{
			progress.Message = "Infecting caves with mysterious crystals...";

			if (Main.maxTilesX == 8400)
			{
				width = WorldGen.genRand.Next(600, 800);
				maxTreasure = Main.rand.Next(5, 8);
				maxHouse = Main.rand.Next(10, 14);
			}
			else if (Main.maxTilesX == 6400)
			{
				width = WorldGen.genRand.Next(400, 600);
				maxTreasure = Main.rand.Next(4, 6);
				maxHouse = Main.rand.Next(8, 12);
			}
			else
			{
				width = WorldGen.genRand.Next(300, 400);
				maxTreasure = Main.rand.Next(2, 4);
				maxHouse = Main.rand.Next(4, 8);
			}
			height = width - 75;

			if (Main.dungeonX > Main.maxTilesX / 2)
				XPos = Main.maxTilesX / 2 + width / 2 + Main.rand.Next(50, 150);
			else
				XPos = Main.maxTilesX / 2 - width / 2 - Main.rand.Next(50, 150);
			YPos = Main.maxTilesY - (int)(width * 1.25f);

			int startX = XPos - width / 2;
			int startY = YPos - height / 2;

			WorldGen.TileRunner(XPos, YPos, width, (int)(height / 2.3f), dirtTile, true, 0f, 0f, false, true);

			StoneGen();
			GenerateCave();
			CrystalGen();
			PlaceGrid(startX, startY);
			OtherGen(startX, startY);
            GrassGen(startX, startY);
            ObjectTilesGen(startX, startY);
		}
		private void StoneGen()
		{
			for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 10E-04); k++)
			{
				int x = WorldGen.genRand.Next(0, Main.maxTilesX);

				int y = WorldGen.genRand.Next((int)GenVars.worldSurface, Main.maxTilesY);

				Tile tile = Framing.GetTileSafely(x, y);
				if (tile.TileType == dirtTile)
				{
					WorldGen.TileRunner(x, y, WorldGen.genRand.Next(8, 18), WorldGen.genRand.Next(8, 18), stoneTile, true, 0f, 0f, false, true);
				}
			}
		}
		private void GenerateCave()
		{
			cavePoints = new int[width, height];
			seed = WorldGen.genRand.Next(0, 1000000);
			randChoice = new System.Random(seed.GetHashCode());

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
						cavePoints[x, y] = 1;
					else if (randChoice.Next(0, 100) < randFillPercent)
						cavePoints[x, y] = 1;
					else
						cavePoints[x, y] = 0;
				}
			}
			for (int i = 0; i < smoothCycles; i++)
			{
				for (int x = 0; x < width; x++)
				{
					for (int y = 0; y < height; y++)
					{
						int neighboringWalls = GetNeighbors(x, y);
						if (neighboringWalls > threshold)
							cavePoints[x, y] = 1;
						else if (neighboringWalls < threshold)
							cavePoints[x, y] = 0;
					}
				}
			}
		}
		private int GetNeighbors(int pointX, int pointY)
		{
			int wallNeighbors = 0;
			for (int x = pointX - 1; x <= pointX + 1; x++)
			{
				for (int y = pointY - 1; y <= pointY + 1; y++)
				{
					if (x >= 0 && x < width && y >= 0 && y < height)
					{
						if (x != pointX || y != pointY)
						{
							if (cavePoints[x, y] == 1)
								wallNeighbors++;
						}
					}
					else
					{
						wallNeighbors++;
					}
				}
			}

			return wallNeighbors;
		}

		private void CrystalGen()
		{
			for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 9E-04); k++)
			{
				int x = WorldGen.genRand.Next(0, Main.maxTilesX);

				int y = WorldGen.genRand.Next((int)GenVars.worldSurface, Main.maxTilesY);

				Tile tile = Framing.GetTileSafely(x, y);
				if (tile.TileType == dirtTile || tile.TileType == stoneTile)
					WorldGen.TileRunner(x, y, WorldGen.genRand.Next(6, 8), WorldGen.genRand.Next(6, 8), crystalTile, true, 0f, 0f, false, true);
			}
		}

		private void PlaceGrid(float startX, float startY)
		{
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (Main.tile[(int)(startX + x), (int)(startY + y)].TileType == dirtTile
						|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == crystalTile)
					{
						WorldGen.PlaceWall((int)(startX + x), (int)(startY + y), grassWall);
					}
					if (Main.tile[(int)(startX + x), (int)(startY + y)].TileType == stoneTile)
					{
						WorldGen.PlaceWall((int)(startX + x), (int)(startY + y), stoneWall);
					}
					if ((Main.tile[(int)(startX + x), (int)(startY + y)].TileType == dirtTile
						|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == stoneTile
						|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == crystalTile)
						&& Main.tile[(int)(startX + x), (int)(startY + y)].WallType != grassWall
						&& Main.tile[(int)(startX + x), (int)(startY + y)].WallType != stoneWall)
					{
						WorldGen.KillWall((int)(startX + x), (int)(startY + y));
					}

					if (cavePoints[x, y] == 0
						&& (Main.tile[(int)(startX + x), (int)(startY + y)].TileType == dirtTile
						|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == stoneTile
						|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == crystalTile
						|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == 147
						|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == 368
						|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == 367))
					{
						WorldGen.KillTile((int)(startX + x), (int)(startY + y));
					}
					if (cavePoints[x, y] == 1)
					{
					}
				}
			}

			for (int treasureCount = 0; treasureCount < maxTreasure; treasureCount++)
			{
				treasureRandom = Main.rand.Next(0, 2);
				var TreasureX = WorldGen.genRand.Next(width / 5, width - width / 5) + startX;
				var TreasureY = WorldGen.genRand.Next(width / 5, height - width / 5) + startY;
				TreasureGen((int)TreasureX, (int)TreasureY);
			}

			for (int houseCount = 0; houseCount < maxHouse; houseCount++)
			{
				houseRandom = Main.rand.Next(0, 4);
				var HouseX = WorldGen.genRand.Next(width / 5, width - width / 5) + startX;
				var HouseY = WorldGen.genRand.Next(width / 5, height - width / 5) + startY;
				HouseGen((int)HouseX, (int)HouseY);
			}

			treeRandom = Main.rand.Next(0, 2);
			var TreeX = WorldGen.genRand.Next(width / 5, width - width / 5) + startX;
			var TreeY = WorldGen.genRand.Next(width / 5, height - width / 5) + startY;
			TreeGen((int)TreeX, (int)TreeY);

			var BossSpawnX = WorldGen.genRand.Next(width / 5, width - width / 5) + startX;
			var BossSpawnY = WorldGen.genRand.Next(width / 5, height - width / 5) + startY;
			BossSpawnGen((int)BossSpawnX, (int)BossSpawnY);
		}

		private void GrassGen(float startX, float startY)
		{
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if (Main.tile[(int)(startX + x), (int)(startY + y)].TileType == dirtTile &&
						(Main.tile[(int)(startX + x + 1), (int)(startY + y)].HasTile == false ||
						Main.tile[(int)(startX + x - 1), (int)(startY + y)].HasTile == false ||
						Main.tile[(int)(startX + x), (int)(startY + y + 1)].HasTile == false ||
						Main.tile[(int)(startX + x), (int)(startY + y - 1)].HasTile == false))
					{
						Main.tile[(int)(startX + x), (int)(startY + y)].TileType = (ushort)grassTile;

                    }
				}
			}
		}

		static readonly byte[,] TreasureTiles0 =
		{
			{3,3,3,3,3,3,3,1,1,1,1,1,1,1,3,3},
			{3,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
			{1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
			{1,1,1,1,2,1,1,1,1,1,1,1,1,1,1,1},
			{3,1,1,2,2,2,1,4,4,1,2,2,2,2,1,3},
			{0,2,2,2,2,2,2,0,0,2,2,2,2,2,0,0},
			{0,0,2,2,2,2,0,0,0,0,0,2,2,0,0,0},
			{0,0,2,2,2,0,0,0,0,0,0,0,2,0,0,0},
			{0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3},
			{3,0,0,0,0,0,0,0,0,3,3,3,0,0,3,3},
			{3,3,3,0,0,0,3,3,3,3,3,3,3,3,3,3},
		};
		static readonly byte[,] TreasureTiles1 =
		{
			{3,3,1,1,1,3,3,3,1,1,1,1,1,3,3,3},
			{3,1,1,1,1,1,1,1,1,1,1,1,1,1,1,3},
			{1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
			{1,1,1,2,2,1,1,1,1,2,2,1,1,1,1,1},
			{3,2,2,2,2,2,1,4,4,2,2,2,2,2,1,3},
			{0,2,2,2,2,2,2,0,0,2,2,2,2,2,0,0},
			{0,0,2,2,2,2,2,0,0,0,0,2,2,0,0,0},
			{0,0,0,2,2,2,0,0,0,0,0,0,2,0,0,0},
			{0,0,0,0,2,2,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,3},
			{3,0,0,0,0,0,0,0,0,3,3,3,0,0,3,3},
			{3,3,3,0,0,0,3,3,3,3,3,3,3,3,3,3},
		};
		private void TreasureSwitch(byte treasure, int pointX, int pointY, int x, int y, int X, int Y)
		{
			Tile tile = Framing.GetTileSafely(X, Y);
			switch (treasure)
			{
				case 0:
					WorldGen.KillTile(X, Y);
					WorldGen.KillTile(X, Y);
					break;
				case 1:
					tile.TileType = (ushort)dirtTile;
					tile.HasTile = true;
					WorldGen.SlopeTile(X, Y, 0);
					break;
				case 2:
					tile.TileType = (ushort)crystalTile;
					tile.HasTile = true;
					WorldGen.SlopeTile(X, Y, 0);
					break;
				case 3:
					break;
				case 4:
					tile.TileType = (ushort)brickTile;
					tile.HasTile = true;
					WorldGen.SlopeTile(X, Y, 0);
					break;
				default:
					break;
			}
		}
		private void TreasureGen(int pointX, int pointY)
		{
			for (int x = 0; x < 16; x++)
			{
				for (int y = 0; y < 12; y++)
				{
					if (x == 16 && y == 12)
					{
						break;
					}
					Tile tile = Framing.GetTileSafely(pointX + x, pointY - y);
					var X = pointX + x;
					var Y = pointY - y;

					switch (treasureRandom)
					{
						case 0:
							TreasureSwitch(TreasureTiles0[y, x], pointX, pointY, x, y, X, Y);
							break;
						case 1:
							TreasureSwitch(TreasureTiles1[y, x], pointX, pointY, x, y, X, Y);
							break;
					}
					WorldGen.PlaceChest(pointX + 7, pointY - 4, (ushort)chest, false, 0);
				}
			}
		}
		#region houses
		static readonly byte[,] HouseTiles0 =
		{
			{0,0,0,0,0,0,0,0,7,0,0,0,0,0,0,0,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,1,3,3,3,3,3,5,5,3,3,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,8,4,4,4,4,4,6,6,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,8,4,4,4,4,4,6,6,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,2,4,4,4,4,4,6,6,4,4,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,2,4,4,4,4,4,6,6,4,4,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,1,1,2,2,2,2,2,2,6,6,2,2,2,1,1,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,1,1,4,4,4,4,4,4,4,4,4,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,1,1,4,4,4,4,4,4,4,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,1,1,4,4,4,4,4,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,1,1,4,4,4,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,1,1,4,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
		};
		static readonly byte[,] HouseTiles1 =
		{
			{0,0,0,0,0,0,7,0,0,0,0,0,0,0,0,0,0,0,7,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0},
			{0,0,0,8,3,3,3,5,5,3,3,3,3,3,3,3,3,3,3,3,3,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,8,8,8,8,5,5,8,8,8,8,8,8,8,8,8,8,8,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,8,8,8,5,5,8,8,8,8,8,8,8,8,8,8,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,8,7,8,5,5,8,8,8,8,8,8,8,8,8,7,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,1,1,1,1,5,5,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,8,8,8,8,8,4,4,4,4,4,4,4,4,4,4,8,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,8,8,8,4,4,4,4,4,4,4,4,4,4,8,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,8,2,4,4,4,4,4,4,4,4,4,4,2,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,2,4,4,4,4,4,4,4,4,4,1,1,1,1,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,7,4,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
		};
		static readonly byte[,] HouseTiles2 =
		{
			{0,0,0,0,0,0,0,0,0,0,0,0,0,7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,5,5,1,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,1,3,3,3,3,5,5,1,0,0,0,7,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,4,4,4,4,5,5,1,0,0,1,1,1,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,4,4,4,4,5,5,1,1,1,1,1,1,1,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,2,4,4,4,4,5,5,3,3,3,3,3,3,1,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,1,1,2,2,2,2,2,5,5,5,5,4,4,4,4,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,1,1,4,4,4,2,4,4,5,5,4,4,4,4,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,1,1,4,4,2,4,4,5,5,4,4,4,4,2,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,1,1,4,2,4,4,5,5,4,4,4,4,2,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,2,2,2,5,5,2,2,2,2,2,1,1,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,4,4,4,4,4,4,4,4,1,1,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,4,4,4,4,4,4,1,1,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,4,4,4,4,1,1,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,4,4,1,1,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0},
		};
		static readonly byte[,] HouseTiles3 =
		{
			{0,0,0,0,0,0,0,0,0,0,0,0,7,0,0,0,0,0,7,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,7,3,3,3,3,3,3,3,3,1,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,7,0,8,4,4,4,4,4,4,8,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,7,0,8,4,4,4,6,6,4,8,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,7,0,2,4,4,4,6,6,4,2,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,6,6,1,1,1,1,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,1,3,3,3,3,3,3,3,3,3,3,3,3,1,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,4,4,4,4,4,4,2,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,4,4,4,4,4,4,4,4,4,4,4,4,2,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,2,4,4,4,4,4,4,4,4,4,4,4,4,2,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,1,1,1,1,4,4,4,4,4,4,4,4,4,4,2,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,0,0,0,7,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,7,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0},
		};
		private void HouseSwitch(byte house, int pointX, int pointY, int x, int y, int X, int Y)
		{
			Tile tile = Framing.GetTileSafely(X, Y);
			switch (house)
			{
				case 0:
					break;
				case 1:
					tile.TileType = (ushort)brickTile;
					tile.HasTile = true;
					WorldGen.SlopeTile(X, Y, 0);
					break;
				case 2:
					tile.TileType = (ushort)woodTile;
					tile.HasTile = true;
					WorldGen.SlopeTile(X, Y, 0);
					break;
				case 3:
					WorldGen.KillTile(X, Y);
					WorldGen.KillTile(X, Y);
					WorldGen.KillWall(pointX + x, pointY - y);
					WorldGen.PlaceWall(pointX + x, pointY - y, (ushort)brickWall);
					break;
				case 4:
					WorldGen.KillTile(X, Y);
					WorldGen.KillTile(X, Y);
					WorldGen.KillWall(pointX + x, pointY - y);
					WorldGen.PlaceWall(pointX + x, pointY - y, (ushort)woodWall);
					break;
				case 5:
					WorldGen.KillTile(X, Y);
					WorldGen.KillTile(X, Y);
					tile.TileType = (ushort)plat;
					tile.HasTile = true;
					WorldGen.KillWall(pointX + x, pointY - y);
					WorldGen.PlaceWall(pointX + x, pointY - y, (ushort)brickWall);
					break;
				case 6:
					WorldGen.KillTile(X, Y);
					WorldGen.KillTile(X, Y);
					tile.TileType = (ushort)plat;
					tile.HasTile = true;
					WorldGen.KillWall(pointX + x, pointY - y);
					WorldGen.PlaceWall(pointX + x, pointY - y, (ushort)woodWall);
					break;
				case 7:
					if (Main.tile[X, Y].HasTile == false)
					{
						tile.TileType = (ushort)beamTile;
						tile.HasTile = true;
					}
					WorldGen.SlopeTile(X, Y, 0);
					break;
				case 8:
					WorldGen.KillTile(X, Y);
					WorldGen.KillTile(X, Y);
					break;
				default:
					break;
			}
		}
		private void HouseGen(int pointX, int pointY)
		{
			for (int x = 0; x < 31; x++)
			{
				for (int y = 0; y < 18; y++)
				{
					if (x == 31 && y == 18)
					{
						break;
					}
					Tile tile = Framing.GetTileSafely(pointX + x, pointY - y);
					var X = pointX + x;
					var Y = pointY - y;
					switch (houseRandom)
					{
						case 0:
							HouseSwitch(HouseTiles0[y, x], pointX, pointY, x, y, X, Y);
							WorldGen.PlaceObject(pointX + 9, pointY - 3, (ushort)table, true, 0, -1, -1);
                            WorldGen.PlaceObject(pointX + 9, pointY - 5, (ushort)candle, true, 0, -1, -1);
                            WorldGen.PlaceObject(pointX + 11, pointY - 3, (ushort)chair, true, 0, -1, -1);
							WorldGen.PlaceObject(pointX + 16, pointY - 3, (ushort)door, true, 0, -1, -1);
							houseChestX = 9;
							houseChestY = 9;
							break;
						case 1:
							HouseSwitch(HouseTiles1[y, x], pointX, pointY, x, y, X, Y);
							WorldGen.PlaceObject(pointX + 10, pointY - 8, (ushort)door, true, 0, -1, -1);
							WorldGen.PlaceObject(pointX + 13, pointY - 8, (ushort)table, true, 0, -1, -1);
                            WorldGen.PlaceObject(pointX + 13, pointY - 12, (ushort)chandelier, true, 0, -1, -1);
                            WorldGen.PlaceObject(pointX + 15, pointY - 8, (ushort)chair, true, 0, -1, -1);
							WorldGen.PlaceObject(pointX + 18, pointY - 8, (ushort)bookcase, true, 0, -1, -1);
							WorldGen.PlaceObject(pointX + 15, pointY - 3, (ushort)sofa, true, 0, -1, -1);
                            WorldGen.PlaceObject(pointX + 17, pointY - 3, (ushort)lamp, true, 0, -1, -1);
                            houseChestX = 9;
							houseChestY = 3;
							break;
						case 2:
							HouseSwitch(HouseTiles2[y, x], pointX, pointY, x, y, X, Y);
							WorldGen.PlaceObject(pointX + 21, pointY - 6, (ushort)table, true, 0, -1, -1);
                            WorldGen.PlaceObject(pointX + 21, pointY - 8, (ushort)candle, true, 0, -1, -1);
                            WorldGen.PlaceObject(pointX + 23, pointY - 6, (ushort)chair, true, 0, -1, -1);
							WorldGen.PlaceObject(pointX + 20, pointY - 12, (ushort)crate, true, 0, -1, -1);
							houseChestX = 14;
							houseChestY = 3;
							break;
						case 3:
							HouseSwitch(HouseTiles3[y, x], pointX, pointY, x, y, X, Y);
							WorldGen.PlaceObject(pointX + 16, pointY - 3, (ushort)workbench, true, 0, -1, -1);
                            WorldGen.PlaceObject(pointX + 17, pointY - 4, (ushort)candle, true, 0, -1, -1);
                            WorldGen.PlaceObject(pointX + 15, pointY - 3, (ushort)chair, true, 0, -1, -1);
							WorldGen.PlaceObject(pointX + 13, pointY - 8, (ushort)crate, true, 0, -1, -1);
							WorldGen.PlaceObject(pointX + 11, pointY - 8, (ushort)crate, true, 0, -1, -1);
                            WorldGen.PlaceObject(pointX + 14, pointY - 10, (ushort)candle, true, 0, -1, -1);
                            WorldGen.PlaceObject(pointX + 12, pointY - 10, (ushort)crate, true, 0, -1, -1);
							WorldGen.PlaceObject(pointX + 20, pointY - 10, (ushort)bookcase, true, 0, -1, -1);
							WorldGen.PlaceObject(pointX + 13, pointY - 3, (ushort)door, true, 0, -1, -1);
							break;
					}
					if (houseChestX != 0 || houseChestY != 0)
					{
						WorldGen.PlaceChest(pointX + houseChestX, pointY - houseChestY, (ushort)ModContent.TileType<DeepChest>(), false, 0);
					}
				}
			}
		}
		#endregion
		#region trees
		static readonly byte[,] TreeTiles0 =
		{
			{0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,1,0,0,0,0,0},
			{0,0,0,0,0,1,1,0,0,0,0,0,0,1,1,0,0,0,0,0,0,1,1,0,0,0,0,0},
			{0,0,0,0,0,1,0,0,1,1,0,0,1,1,0,0,0,1,1,1,1,1,0,0,0,0,0,0},
			{0,1,0,0,0,1,1,1,1,1,1,1,1,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0},
			{0,1,1,1,0,0,0,0,0,1,1,1,1,0,1,1,1,1,1,0,0,0,0,0,1,0,0,0},
			{0,0,0,1,1,1,1,1,0,0,1,1,1,1,1,1,1,1,0,1,1,1,1,0,0,0,0,0},
			{1,1,1,1,1,1,1,1,1,1,1,1,3,3,1,1,1,1,1,1,1,1,1,1,1,1,1,0},
			{1,0,1,1,1,0,1,1,1,1,1,1,3,3,1,1,1,1,1,1,1,1,1,1,1,1,0,0},
			{0,0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,1,1,1,0},
			{0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,0,1,1,1,1,1,0,1,0,0,0,1,0},
			{0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,1,1,1,1,0,0,1,1,0},
			{0,0,0,0,0,0,2,0,0,0,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,2,0,0,0,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,2,0,2,2,0,0,0,0,1,1,1,1,1,0,0,0,0,0,0,0,0,2,0,0,0},
			{0,0,0,2,0,2,2,0,0,0,0,1,1,1,1,1,0,0,0,0,0,0,0,0,2,0,0,0},
			{0,0,0,2,0,2,2,0,0,0,2,1,1,1,1,1,0,2,0,0,0,0,0,0,2,0,2,0},
			{0,2,0,2,2,2,2,0,0,2,2,1,1,1,1,0,0,2,2,0,0,0,2,0,2,0,2,0},
			{0,2,0,2,2,2,2,2,0,2,1,1,1,1,1,1,2,2,2,0,2,0,2,0,2,2,2,0},
			{0,2,2,2,2,2,2,2,2,1,1,2,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,0},
			{2,2,2,2,2,2,1,1,2,2,2,2,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,0},
			{0,2,2,2,2,1,2,1,1,2,2,1,1,1,1,1,1,1,2,2,2,2,2,1,2,2,2,0},
			{2,2,1,2,2,2,2,2,1,1,1,1,1,1,1,1,1,2,2,1,1,2,2,2,2,2,2,2},
			{2,2,2,1,1,2,2,2,2,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2},
			{2,2,2,2,1,1,1,1,2,2,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,0},
			{0,2,2,1,2,2,1,1,1,1,1,1,1,1,2,1,1,1,2,1,1,1,1,1,2,2,0,0},
			{0,0,2,2,2,2,2,2,1,1,1,1,1,2,2,2,1,1,1,2,2,2,2,2,1,2,2,0},
			{0,0,0,2,2,2,2,2,2,2,1,1,2,1,2,2,2,2,1,2,2,2,2,2,2,2,0,0},
			{0,0,2,2,2,2,2,2,1,1,1,2,2,2,2,2,2,2,2,1,2,2,2,2,2,0,0,2},
			{0,0,0,0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0},
			{0,0,0,2,0,0,0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0,0,0,0},
			{0,0,0,0,0,0,0,0,2,0,2,2,2,2,2,2,0,0,0,2,2,2,0,0,0,0,0,0},
		};
		static readonly byte[,] TreeTiles1 =
		{
			{0,0,0,0,1,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,1,0,0,0,0},
			{0,0,0,0,1,1,1,1,1,1,1,0,0,1,1,0,0,0,0,0,1,1,1,1,0,0,0,0},
			{0,1,1,0,0,1,1,1,1,1,1,1,1,1,0,0,1,1,1,1,1,1,0,0,0,0,0,0},
			{0,0,1,1,1,1,0,0,0,0,1,1,1,1,0,1,1,1,1,1,1,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0},
			{0,0,0,0,0,1,1,1,1,1,1,1,3,3,1,1,1,1,0,0,1,1,1,0,0,1,0,0},
			{0,0,1,0,1,1,0,0,0,1,1,1,3,3,1,1,1,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,1,1,1,0,0,0,1,1,1,1,1,1,1,1,1,1,1,0,1,1,1,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,0,1,1,1,1,1,1,1,1,1,0,0,0},
			{0,0,0,0,0,1,0,1,1,1,0,1,1,1,1,0,0,0,1,1,1,0,0,0,1,0,0,0},
			{0,0,0,0,0,1,1,1,0,0,0,1,1,1,0,0,2,0,0,0,1,1,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,2,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,2,0,0,0,0,0,0,1,1,1,1,0,2,0,0,0,0,2,0,0,0,0,0,0},
			{0,0,0,0,2,0,0,0,0,0,0,2,1,1,1,2,2,0,0,0,0,2,0,0,0,0,0,0},
			{0,0,0,0,2,0,0,0,0,0,0,2,1,1,1,2,2,0,0,0,0,2,0,0,0,0,0,0},
			{0,0,0,0,2,2,0,0,0,0,0,2,1,1,1,2,2,2,0,0,0,2,0,0,0,0,0,0},
			{0,0,0,0,2,2,0,0,0,0,0,2,1,1,1,2,2,2,0,0,0,2,2,0,0,0,0,0},
			{0,0,0,0,2,2,0,2,0,0,2,2,1,1,1,1,2,2,0,0,2,2,2,0,0,0,0,0},
			{0,0,0,2,2,2,0,2,0,2,2,1,1,1,1,1,2,2,2,0,2,2,2,0,0,0,0,2},
			{0,2,0,2,2,2,2,2,2,2,2,1,1,1,1,1,1,1,1,2,2,2,2,0,2,0,0,2},
			{0,2,0,2,2,1,1,1,2,2,1,1,1,1,1,1,1,1,1,1,1,2,2,0,2,2,0,2},
			{0,2,2,2,2,2,2,1,1,1,1,1,1,1,1,1,1,2,2,2,1,1,1,2,2,2,0,2},
			{0,2,1,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2},
			{2,2,1,1,1,1,1,1,2,2,1,1,1,1,1,2,2,1,1,1,2,2,1,1,1,2,2,2},
			{2,2,2,2,2,2,2,2,2,1,1,1,2,1,1,1,2,2,1,1,2,1,1,2,2,2,2,2},
			{2,2,2,1,1,1,1,1,1,1,1,2,2,2,1,1,2,2,2,1,1,1,1,1,2,2,1,2},
			{2,2,2,1,2,2,1,1,1,2,1,2,2,2,1,1,2,2,2,2,1,2,2,1,1,1,1,2},
			{2,2,2,2,2,1,1,2,2,2,1,2,2,1,1,1,2,2,2,2,1,2,2,2,2,2,2,2},
			{0,2,2,2,2,1,2,2,2,2,2,2,2,1,2,1,1,1,2,2,2,2,2,2,2,2,2,2},
			{0,0,0,2,2,2,2,2,2,2,2,2,1,1,2,2,2,1,2,2,2,2,2,2,2,2,2,0},
			{0,0,0,0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0,0,2,2,2,0,0,0},
			{0,0,0,2,2,0,0,0,0,2,2,2,2,2,0,0,2,2,0,0,0,0,0,0,0,0,0,0},
		};
		private void TreeSwitch(byte tree, int pointX, int pointY, int x, int y, int X, int Y)
		{
			Tile tile = Framing.GetTileSafely(X, Y);
			switch (tree)
			{
				case 0:
					break;
				case 1:
					tile.TileType = (ushort)treeTile;
					tile.HasTile = true;
					WorldGen.SlopeTile(X, Y, 0);
					break;
				case 2:
					tile.TileType = (ushort)leafTile;
					tile.HasTile = true;
					WorldGen.SlopeTile(X, Y, 0);
					break;
				case 3:
					WorldGen.KillTile(X, Y);
					WorldGen.KillTile(X, Y);
					break;
				default:
					break;
			}
		}
		private void TreeGen(int pointX, int pointY)
		{
			for (int x = 0; x < 28; x++)
			{
				for (int y = 0; y < 32; y++)
				{
					if (x == 28 && y == 32)
					{
						break;
					}
					Tile tile = Framing.GetTileSafely(pointX + x, pointY - y);
					var X = pointX + x;
					var Y = pointY - y;
					if (treeRandom == 0)
					{
						TreeSwitch(TreeTiles0[y, x], pointX, pointY, x, y, X, Y);
						treeChestX = 12;
						treeChestY = 7;
					}
					else
					{
						TreeSwitch(TreeTiles1[y, x], pointX, pointY, x, y, X, Y);
						treeChestX = 12;
						treeChestY = 5;
					}
					WorldGen.PlaceChest(pointX + treeChestX, pointY - treeChestY, (ushort)ModContent.TileType<DeepChest>(), false, 1);
				}
			}
		}
		#endregion
		static readonly byte[,] BossSpawnTiles =
		{
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,1,1,0,1,0,0,1,1,1,0,1,0,0,1,0,0,0,1,1,1,0,0,0,0,0},
			{0,0,0,0,0,0,0,1,0,0,1,1,1,1,1,0,1,1,1,1,1,0,1,1,0,1,1,1,1,1,0,0,0,0,0},
			{0,0,0,0,0,1,1,1,1,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0},
			{0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0},
			{0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,1,1,0,0,0},
			{0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,1,1,1,0,0,0},
			{0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0},
			{0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0},
			{0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
			{0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0},
			{1,1,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0},
			{1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,3,3,3,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0},
			{0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0},
			{0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0},
			{0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0},
			{0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
			{0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
			{0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0},
			{0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0},
			{0,0,1,1,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0},
			{0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0},
			{0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0},
			{0,0,0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,1,1,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,1,0,0,0,1,1,1,1,1,1,1,1,1,1,1,0,0,1,1,1,1,1,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,1,1,1,1,0,1,1,1,1,1,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0},
			{0,0,0,0,0,0,0,0,0,0,0,0,1,1,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
		};
		private void BossSpawnGen(int pointX, int pointY)
		{
			for (int x = 0; x < 35; x++)
			{
				for (int y = 0; y < 29; y++)
				{
					if (x == 35 && y == 29)
					{
						break;
					}
					Tile tile = Framing.GetTileSafely(pointX + x, pointY - y);
					var X = pointX + x;
					var Y = pointY - y;
					switch (BossSpawnTiles[y, x])
					{
						case 0:
							break;
						case 1:
							tile.TileType = (ushort)crystalTile;
							tile.HasTile = true;
							break;
						case 2:
							WorldGen.KillTile(X, Y);
							WorldGen.KillTile(X, Y);
							break;
						case 3:
							tile.TileType = (ushort)crystalTile2;
                            WorldGen.SlopeTile(pointX + x, pointY - y, 0);
                            tile.HasTile = true;
							break;
					}
					WorldGen.PlaceObject(pointX + 17, pointY - 14, (ushort)kronosSpawn, true, 0, -1, -1);
				}
			}
		}
		private void OtherGen(float startX, float startY)
		{
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					WorldGen.GrowTree((int)(startX + x), (int)(startY + y));

					if ((Main.tile[(int)(startX + x), (int)(startY + y - 1)].TileType == dirtTile
					|| Main.tile[(int)(startX + x), (int)(startY + y - 1)].TileType == grassTile
					|| Main.tile[(int)(startX + x), (int)(startY + y - 1)].TileType == stoneTile)
					&& Main.tile[(int)(startX + x), (int)(startY + y - 1)].Slope == 0
					&& Main.tile[(int)(startX + x), (int)(startY + y)].HasTile == false)
					{
						if (Main.rand.Next(100) <= 30)
						{
							WorldGen.PlaceTile((int)(startX + x), (int)(startY + y), (ushort)vine);
						}
					}
					else if (Main.tile[(int)(startX + x), (int)(startY + y - 1)].TileType == vine
						&& Main.tile[(int)(startX + x), (int)(startY + y)].HasTile == false)
					{
						if (Main.rand.Next(100) <= 80)
						{
							WorldGen.PlaceTile((int)(startX + x), (int)(startY + y), (ushort)vine);
						}
					}

					if (Main.tile[(int)(startX + x), (int)(startY + y - 1)].TileType == beamTile
						&& Main.tile[(int)(startX + x), (int)(startY + y)].HasTile == false)
					{
						WorldGen.PlaceTile((int)(startX + x), (int)(startY + y), (ushort)beamTile);
					}

					Cleaning(startX, startY, x, y);
				}
			}
		}

		private void Cleaning(float startX, float startY, int x, int y)
		{
			Tile tile = Framing.GetTileSafely((int)startX + x, (int)startY + y);
			tile.LiquidType = 0;

            if (Vector2.Distance(new Vector2((int)startX + width / 2, (int)startY + height / 2), new Vector2((int)(startX + x), (int)(startY + y))) < width / 2 - 50)
            {
                if (Main.tile[(int)(startX + x), (int)(startY + y)].TileType == 367 ||
                   Main.tile[(int)(startX + x), (int)(startY + y)].TileType == 147 ||
                   Main.tile[(int)(startX + x), (int)(startY + y)].TileType == 190 ||
                   Main.tile[(int)(startX + x), (int)(startY + y)].TileType == 368)
                {
                    Main.tile[(int)(startX + x), (int)(startY + y)].TileType = (ushort)dirtTile;
                }
                if (Main.tile[(int)(startX + x), (int)(startY + y)].WallType != (ushort)grassWall &&
                   Main.tile[(int)(startX + x), (int)(startY + y)].WallType != (ushort)stoneWall &&
                   Main.tile[(int)(startX + x), (int)(startY + y)].WallType != (ushort)dirtWall &&
                   Main.tile[(int)(startX + x), (int)(startY + y)].WallType != (ushort)woodWall &&
                   Main.tile[(int)(startX + x), (int)(startY + y)].WallType != 0)
                {
                    Main.tile[(int)(startX + x), (int)(startY + y)].WallType = (ushort)grassWall;
                }
            }
            else
            {
                if (Main.tile[(int)(startX + x), (int)(startY + y)].TileType == 367 ||
                   Main.tile[(int)(startX + x), (int)(startY + y)].TileType == 147 ||
                   Main.tile[(int)(startX + x), (int)(startY + y)].TileType == 190 ||
                   Main.tile[(int)(startX + x), (int)(startY + y)].TileType == 368)
                {
					Vector2 dir = (new Vector2(startX + x, startY + y) - new Vector2(startX + width / 2, startY + height / 2)).SafeNormalize(Vector2.UnitX);
                    bool isntLongAway = Vector2.Distance(new Vector2(startX + width / 2, startY + height / 2), new Vector2(startX + x, startY + y)) < Vector2.Distance(new Vector2(startX + width / 2, startY + height / 2), new Vector2(startX + width / 2, startY + height / 2) + dir * (float)(width / 2));
                    if (isntLongAway && Main.rand.NextBool(((int)Vector2.Distance(new Vector2(startX + width / 2, startY + height / 2) + dir * (float)(width / 2 - 50), new Vector2(startX + x, startY + y)) + 25) / 20 + 1))
                        Main.tile[(int)(startX + x), (int)(startY + y)].TileType = (ushort)dirtTile;
                }
                if (Main.tile[(int)(startX + x), (int)(startY + y)].WallType != (ushort)grassWall &&
                   Main.tile[(int)(startX + x), (int)(startY + y)].WallType != (ushort)stoneWall &&
                   Main.tile[(int)(startX + x), (int)(startY + y)].WallType != (ushort)dirtWall &&
                   Main.tile[(int)(startX + x), (int)(startY + y)].WallType != (ushort)woodWall &&
                   Main.tile[(int)(startX + x), (int)(startY + y)].WallType != 0)
                {
                    Vector2 dir = (new Vector2(startX + x, startY + y) - new Vector2(startX + width / 2, startY + height / 2)).SafeNormalize(Vector2.UnitX);
					bool isntLongAway = Vector2.Distance(new Vector2(startX + width / 2, startY + height / 2), new Vector2(startX + x, startY + y)) < Vector2.Distance(new Vector2(startX + width / 2, startY + height / 2), new Vector2(startX + width / 2, startY + height / 2) + dir * (float)(width / 2));
                    if (isntLongAway && Main.rand.NextBool(((int)Vector2.Distance(new Vector2(startX + width / 2, startY + height / 2) + dir * (float)(width / 2 - 50), new Vector2(startX + x, startY + y)) + 25) / 20 + 1))
                        Main.tile[(int)(startX + x), (int)(startY + y)].WallType = (ushort)grassWall;
                }
            }

            if ((Main.rand.NextBool(3) && Main.tile[(int)(startX + x), (int)(startY + y)].WallType == grassWall))
            {
                Main.tile[(int)(startX + x), (int)(startY + y)].WallType = (ushort)dirtWall;
            }

            if (Main.tile[(int)(startX + x), (int)(startY + y)].TileType == dirtTile
			|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == crystalTile
			|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == crystalTile2
			|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == stoneTile)
			{
				WorldGen.SlopeTile((int)(startX + x), (int)(startY + y), 0);
			}
			if (Main.tile[(int)(startX + x), (int)(startY + y)].HasTile == true
			&& Main.tile[(int)(startX + x + 1), (int)(startY + y)].HasTile == false
			&& Main.tile[(int)(startX + x - 1), (int)(startY + y)].HasTile == false
			&& Main.tile[(int)(startX + x), (int)(startY + y + 1)].HasTile == false
			&& Main.tile[(int)(startX + x), (int)(startY + y - 1)].HasTile == false)
			{
				WorldGen.KillTile((int)(startX + x), (int)(startY + y));
			}
			if ((Main.tile[(int)(startX + x), (int)(startY + y)].TileType == dirtTile
			|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == crystalTile
			|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == stoneTile)
			&& Main.tile[(int)(startX + x + 1), (int)(startY + y)].HasTile == false
			&& Main.tile[(int)(startX + x), (int)(startY + y - 1)].HasTile == false
			&& Main.tile[(int)(startX + x - 1), (int)(startY + y)].HasTile == true
			&& Main.tile[(int)(startX + x), (int)(startY + y + 1)].HasTile == true
			&& Main.rand.NextBool(2))
			{
				WorldGen.SlopeTile((int)(startX + x), (int)(startY + y), 1);
			}
			if ((Main.tile[(int)(startX + x), (int)(startY + y)].TileType == dirtTile
			|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == crystalTile
			|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == stoneTile)
			&& Main.tile[(int)(startX + x - 1), (int)(startY + y)].HasTile == false
			&& Main.tile[(int)(startX + x), (int)(startY + y - 1)].HasTile == false
			&& Main.tile[(int)(startX + x + 1), (int)(startY + y)].HasTile == true
			&& Main.tile[(int)(startX + x), (int)(startY + y + 1)].HasTile == true
			&& Main.rand.NextBool(3))
			{
				WorldGen.SlopeTile((int)(startX + x), (int)(startY + y), 2);
			}
			if ((Main.tile[(int)(startX + x), (int)(startY + y)].TileType == dirtTile
			|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == crystalTile
			|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == stoneTile)
			&& Main.tile[(int)(startX + x + 1), (int)(startY + y)].HasTile == false
			&& Main.tile[(int)(startX + x), (int)(startY + y + 1)].HasTile == false
			&& Main.tile[(int)(startX + x - 1), (int)(startY + y)].HasTile == true
			&& Main.tile[(int)(startX + x), (int)(startY + y - 1)].HasTile == true
			&& Main.rand.NextBool(3))
			{
				WorldGen.SlopeTile((int)(startX + x), (int)(startY + y), 3);
			}
			if ((Main.tile[(int)(startX + x), (int)(startY + y)].TileType == dirtTile
			|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == crystalTile
			|| Main.tile[(int)(startX + x), (int)(startY + y)].TileType == stoneTile)
			&& Main.tile[(int)(startX + x - 1), (int)(startY + y)].HasTile == false
			&& Main.tile[(int)(startX + x), (int)(startY + y + 1)].HasTile == false
			&& Main.tile[(int)(startX + x + 1), (int)(startY + y)].HasTile == true
			&& Main.tile[(int)(startX + x), (int)(startY + y - 1)].HasTile == true
			&& Main.rand.NextBool(2))
			{
				WorldGen.SlopeTile((int)(startX + x), (int)(startY + y), 4);
			}
			if (Main.tile[(int)(startX + x), (int)(startY + y)].TileType == brickTile)
			{
				WorldGen.SlopeTile((int)(startX + x), (int)(startY + y), 0);
			}
		}

		private void ObjectTilesGen(float startX, float startY)
		{
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					if ((Main.tile[(int)(startX + x), (int)(startY + y + 1)].TileType == dirtTile
					|| Main.tile[(int)(startX + x), (int)(startY + y + 1)].TileType == crystalTile
					|| Main.tile[(int)(startX + x), (int)(startY + y + 1)].TileType == brickTile
					|| Main.tile[(int)(startX + x), (int)(startY + y + 1)].TileType == stoneTile)
					&& (Main.tile[(int)(startX + x + 1), (int)(startY + y + 1)].TileType == dirtTile
					|| Main.tile[(int)(startX + x + 1), (int)(startY + y + 1)].TileType == crystalTile
					|| Main.tile[(int)(startX + x + 1), (int)(startY + y + 1)].TileType == brickTile
					|| Main.tile[(int)(startX + x + 1), (int)(startY + y + 1)].TileType == stoneTile)
					&& Main.tile[(int)(startX + x), (int)(startY + y)].HasTile == false
					&& Main.tile[(int)(startX + x + 1), (int)(startY + y)].HasTile == false
					&& Main.tile[(int)(startX + x + 1), (int)(startY + y - 1)].HasTile == false
					&& Main.tile[(int)(startX + x), (int)(startY + y - 1)].HasTile == false)
					{
						if (Main.rand.NextBool(200))
						{
							WorldGen.SlopeTile((int)(startX + x), (int)(startY + y + 1), 0);
							WorldGen.SlopeTile((int)(startX + x + 1), (int)(startY + y + 1), 0);
							WorldGen.PlaceObject((int)(startX + x), (int)(startY + y), (ushort)pillar, true, 0, -1, -1);
						}
						else if (Main.rand.NextBool(40))
						{
							WorldGen.SlopeTile((int)(startX + x), (int)(startY + y + 1), 0);
							WorldGen.SlopeTile((int)(startX + x + 1), (int)(startY + y + 1), 0);
							WorldGen.PlaceObject((int)(startX + x), (int)(startY + y), (ushort)pot, true, 0, -1, -1);
						}
						else if (Main.rand.NextBool(15))
						{
							WorldGen.SlopeTile((int)(startX + x), (int)(startY + y + 1), 0);
							WorldGen.SlopeTile((int)(startX + x + 1), (int)(startY + y + 1), 0);
							WorldGen.PlaceObject((int)(startX + x), (int)(startY + y), (ushort)Main.rand.Next(new int[] { shardBig1, shardBig2, shardBig3 }), true, 0, -1, -1);
						}
					}

					if ((Main.tile[(int)(startX + x), (int)(startY + y + 1)].TileType == dirtTile
					|| Main.tile[(int)(startX + x), (int)(startY + y + 1)].TileType == crystalTile
					|| Main.tile[(int)(startX + x), (int)(startY + y + 1)].TileType == brickTile
					|| Main.tile[(int)(startX + x), (int)(startY + y + 1)].TileType == stoneTile)
					&& Main.tile[(int)(startX + x), (int)(startY + y)].HasTile == false
					&& Main.tile[(int)(startX + x), (int)(startY + y - 1)].HasTile == false)
					{
						if (Main.rand.Next(100) < 25)
						{
							WorldGen.SlopeTile((int)(startX + x), (int)(startY + y + 1), 0);
							WorldGen.PlaceObject((int)(startX + x), (int)(startY + y), (ushort)shard, true, 0, -1, -1);
						}
						else if (Main.rand.Next(100) < 35 && Main.tile[(int)(startX + x), (int)(startY + y + 1)].TileType == grassTile)
						{
							WorldGen.SlopeTile((int)(startX + x), (int)(startY + y + 1), 0);
							WorldGen.PlaceObject((int)(startX + x), (int)(startY + y), (ushort)herb, true, 0, -1, -1);
						}
					}
				}
			}
		}
	}
}