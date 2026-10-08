using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using Waybound.Content.Tiles.Banners.PreHM;
using Waybound.Content.Tiles.Blocks;
using Waybound.Content.Tiles.BossSummon;
using Waybound.Content.Tiles.Furniture;
using Waybound.Content.Tiles.Furniture.DeepWoodFurniture;
using Waybound.Content.Tiles.Plants;
using Waybound.Content.Tiles.Walls;

namespace Waybound.Common.ModSystems.WorldGens.CrystalDepths
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

		public int waterRand;
		public int treeChestX;
		public int treeChestY;
		public int seed;
		public int maxTreasure;
		public int maxHouse;
		public System.Random randChoice;

		public (int x, int y) BigTreePos;
		public int TreeGrowthRadius;
		public List<StructureRect> Structures = new();

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

			Measure("StoneGen", StoneGen);
			Measure("GenerateCave", GenerateCave);
			Measure("CrystalGen", CrystalGen);
			Measure("PlaceGrid", () => PlaceGrid(startX, startY));
			Measure("OtherGen", () => OtherGen(startX, startY));
			Measure("GrassGen", () => GrassGen(startX, startY));
			Measure("TreeGrowGen", () => TreeGrowGen(startX, startY));
			Measure("ObjectTilesGen", () => ObjectTilesGen(startX, startY));
		}
		
		private void Measure(string name, Action action)
		{
			Stopwatch stopwatch = Stopwatch.StartNew();

			action();

			stopwatch.Stop();
			Console.WriteLine($"{name}: {stopwatch.ElapsedMilliseconds} ms");
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

		private void PlaceGrid(int startX, int startY)
		{
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					var globalX = startX + x;
					var globalY = startY + y;
					Tile tile = Framing.GetTileSafely(globalX, globalY);
					
					
					if (tile.TileType == dirtTile
						|| tile.TileType == crystalTile)
					{
						WorldGen.PlaceWall(globalX, globalY, grassWall);
					}
					if (tile.TileType == stoneTile)
					{
						WorldGen.PlaceWall(globalX, globalY, stoneWall);
					}
					if ((tile.TileType == dirtTile
						|| tile.TileType == stoneTile
						|| tile.TileType == crystalTile)
						&& tile.WallType != grassWall
						&& tile.WallType != stoneWall)
					{
						WorldGen.KillWall(globalX, globalY);
					}

					if (cavePoints[x, y] == 0
						&& (tile.TileType == dirtTile
						|| tile.TileType == stoneTile
						|| tile.TileType == crystalTile
						|| tile.TileType == TileID.SnowBlock
						|| tile.TileType == TileID.Granite
						|| tile.TileType == TileID.Marble))
					{
						WorldGen.KillTile(globalX, globalY);
					}
					if (cavePoints[x, y] == 1)
					{
					}
				}
			}
			
			PlaceStructures(startX, startY);
		}

		private void PlaceStructures(int startX, int startY)
		{
			var bigTreeVariant = Main.rand.Next(0, 2);
			var bigTree = new CrystalDepthBigTree(bigTreeVariant);
			BigTreePos = GetRandomStructurePosition(startX, startY, bigTree.Width, bigTree.Height);
			bigTree.Place(BigTreePos.x, BigTreePos.y);
			
			
			var bossSpawn = new CrystalDepthBossStructure();
			var bossSpawnPosition = GetRandomStructurePosition(startX, startY, bigTree.Width, bigTree.Height);
			bossSpawn.Place(bossSpawnPosition.x, bossSpawnPosition.y);
			
			for (int treasureCount = 0; treasureCount < maxTreasure; treasureCount++)
			{
				var treasureVariant = Main.rand.Next(0, 2);
				var treasure = new CrystalDepthTreasure(treasureVariant);
				var treasureSpawn = GetRandomStructurePosition(startX, startY, treasure.Width, treasure.Height);
				treasure.Place(treasureSpawn.x, treasureSpawn.y);
			}

			for (int houseCount = 0; houseCount < maxHouse; houseCount++)
			{
				var houseVariant = Main.rand.Next(0, 4);
				var house = new CrystalDepthHouse(houseVariant);
				var houseSpawn = GetRandomStructurePosition(startX, startY, house.Width, house.Height);
				house.Place(houseSpawn.x, houseSpawn.y);
			}
		}


		private (int x, int y) GetRandomStructurePosition(int startX, int startY, int w, int h)
		{
			const int maxAttempts = 5;
			
			StructureRect newStructure = default;
			for (int i = 0; i < maxAttempts; i++)
			{
				newStructure = RandomNewStructureRect(startX, startY, w, h);
				if (!Structures.Any(s => s.IntersectsWith(newStructure)))
				{
					break;
				}
			}
			Structures.Add(newStructure);

			return (newStructure.X, newStructure.Y + h - 1);
		}
		
		private StructureRect RandomNewStructureRect(int startX, int startY, int w, int h)
		{
			(int x, int y) = (
				WorldGen.genRand.Next(width / 5, width - width / 5) + startX,
				WorldGen.genRand.Next(width / 5, height - width / 5) + startY
			);

			return new StructureRect(x, y, w, h);
		}
		
		private void GrassGen(int startX, int startY)
		{
			for (int x = startX; x < startX + width; x++)
			{
				for (int y = startY; y < startY + height; y++)
				{
					Tile tile = Main.tile[x, y];
					Tile tileAbove = Main.tile[x, y - 1];
					Tile tileBelow = Main.tile[x, y + 1];
					Tile tileLeft = Main.tile[x - 1, y];
					Tile tileRight = Main.tile[x + 1, y];

					if (tile.TileType == dirtTile && (!tileRight.HasTile || !tileLeft.HasTile || !tileBelow.HasTile ||
					                                  !tileAbove.HasTile))
					{
						tile.TileType = (ushort)grassTile;
					}
				}
			}
		}

		private void TreeGrowGen(int startX, int startY)
		{
			TreeGrowthRadius = Math.Max(width / 3, 150);
			var treeSettings = new WorldGen.GrowTreeSettings()
			{
				TreeTileType = TileID.Trees,
				GroundTest = (tileType) => tileType == grassTile,
				TreeHeightMin = 8,
				TreeHeightMax = 20,
				TreeTopPaddingNeeded = 4,
				SaplingTileType = (ushort)TileType<DeepSapling>(),
				WallTest = (wallType) => true
			};
			

			for (int x = startX; x < startX + width; x++)
			{
				for (int y = startY; y < startY + height; y++)
				{
					Tile tile = Main.tile[x, y];
					Tile tileAbove = Main.tile[x, y - 1];

					if (tile.HasTile && tile.TileType == grassTile && !tileAbove.HasTile)
					{
						var growTreeChance = GetGrowTreeChance(x, y);
						if (Random.Shared.NextDouble() < growTreeChance)
						{
							WorldGen.GrowTreeWithSettings(x, y, treeSettings);

						}
					}
				}
			}
		}
		/// <summary>
		/// Determines whether a tree should spawn based on its distance from the big tree position
		/// </summary>
		private double GetGrowTreeChance(int x, int y)
		{
			var maxChance = 1;
			
			double dx = x - BigTreePos.x;
			double dy = y - BigTreePos.y;

			double distanceSquared = dx * dx + dy * dy;
			double radiusSquared = TreeGrowthRadius * TreeGrowthRadius;

			double chance = Math.Max(0, 1 - distanceSquared / radiusSquared);
			chance = Math.Pow(chance, 2);
			chance = Math.Min(maxChance, chance);

			return chance;
		}

		
		
		private void OtherGen(int startX, int startY)
		{
			for (int x = startX; x < startX + width; x++)
			{
				for (int y = startY; y < startY + height; y++)
				{
					Tile tileAbove = Main.tile[x, y - 1];

					if ((tileAbove.TileType == dirtTile
					     || tileAbove.TileType == grassTile
					     || tileAbove.TileType == stoneTile)
					    && tileAbove.Slope == 0
					    && Main.tile[x, y].HasTile == false
					   )
					{
						if (Main.rand.Next(100) <= 30)
						{
							WorldGen.PlaceTile(x, y, (ushort)vine);
						}
					}
					else if (tileAbove.TileType == vine
					         && Main.tile[x, y].HasTile == false)
					{
						if (Main.rand.Next(100) <= 80)
						{
							WorldGen.PlaceTile(x, y, (ushort)vine);
						}
					}

					if (tileAbove.TileType == beamTile
					    && Main.tile[x, y].HasTile == false)
					{
						WorldGen.PlaceTile(x, y, (ushort)beamTile);
					}
					
					Cleaning(startX, startY, x, y);
				}
			}

		}

		private void Cleaning(int startX, int startY, int x, int y)
		{
			Tile tile = Framing.GetTileSafely(x, y);

			BlendBiomeTiles(startX, startY, x, y);
			
			if ((Main.rand.NextBool(3) && tile.WallType == grassWall))
			{
				tile.WallType = (ushort)dirtWall;
			}

			SlopeTiles(x, y);
		}

		private void BlendBiomeTiles(int startX, int startY, int x, int y)
		{
			Tile tile = Framing.GetTileSafely(x, y);
			tile.LiquidType = 0;

			bool canConvertTile =
				tile.TileType is TileID.Marble or TileID.SnowBlock or TileID.MushroomBlock or TileID.Granite;
			bool canConvertWall = tile.WallType != WallID.None && tile.WallType != (ushort)grassWall &&
			                      tile.WallType != (ushort)stoneWall && tile.WallType != (ushort)dirtWall &&
			                      tile.WallType != (ushort)woodWall && tile.WallType != (ushort)brickWall;

			if (!canConvertTile && !canConvertWall)
			{
				return;
			}

			float halfWidth = width * 0.5f;
			var midVec = new Vector2(startX + halfWidth, startY + height * 0.5f);
			float dist = Vector2.Distance(midVec, new Vector2(x, y));

			bool shouldReplace;
			if (dist < halfWidth - 50)
			{
				shouldReplace = true;
			}
			else if (dist < halfWidth)
			{
				int consequent = ((int)(dist - (halfWidth - 50)) + 25) / 20 + 1;
				shouldReplace = Main.rand.NextBool(Math.Max(1, consequent));
			}
			else
			{
				shouldReplace = false;
			}

			if (shouldReplace)
			{
				if (canConvertTile)
					tile.TileType = (ushort)dirtTile;

				if (canConvertWall)
					tile.WallType = (ushort)grassWall;
			}
		}

		private void SlopeTiles(int x, int y)
		{
			Tile tile = Framing.GetTileSafely(x, y);

			if (tile.TileType == dirtTile || tile.TileType == crystalTile || tile.TileType == crystalTile2 ||
			    tile.TileType == stoneTile)
			{
				WorldGen.SlopeTile(x, y, 0);
			}

			if (tile.HasTile && IsTileAround(x, y, false, false, false, false))
			{
				WorldGen.KillTile(x, y);
			}

			if (tile.TileType == dirtTile || tile.TileType == crystalTile || tile.TileType == stoneTile)
			{
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

			if (tile.TileType == brickTile)
			{
				WorldGen.SlopeTile(x, y, 0);
			}
		}

		private bool IsTileAround(int x, int y, bool above, bool right, bool below, bool left)
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