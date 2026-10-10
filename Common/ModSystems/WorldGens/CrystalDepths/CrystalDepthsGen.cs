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
using Waybound.Helpers;

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
		public int XPos;
		public int YPos;
		public int width;
		public int height;
		public int smoothCycles = 4;
		private int[,] cavePoints;
		public int randFillPercent = 49;
		
		
		public int threshold = 4;

		public int seed;
		public int maxTreasure;
		public int maxHouse;
		public Random randChoice;

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

			WorldGen.TileRunner(XPos, YPos, width, (int)(height / 2.3f), DepthsTiles.Dirt, true, 0f, 0f, false, true);

			GenHelper.Measure("StoneGen", StoneGen);
			GenHelper.Measure("GenerateCave", GenerateCave);
			GenHelper.Measure("CrystalGen", CrystalGen);
			GenHelper.Measure("PlaceGrid", () => PlaceGrid(startX, startY));
			GenHelper.Measure("SlopePass", () => SlopePass(startX, startY));

			GenHelper.Measure("OtherGen", () => OtherGen(startX, startY));
			Cleaning(startX, startY);

			GenHelper.Measure("GrassGen", () => GrassGen(startX, startY));
			GenHelper.Measure("TreeGrowGen", () => TreeGrowGen(startX, startY));
			GenHelper.Measure("ObjectTilesGen", () => ObjectTilesGen(startX, startY));

			RangeFrame(startX, startY);
		}

		private void SlopePass(int startX, int startY)
		{
			for (int x = startX; x < startX + width; x++)
			for (int y = startY; y < startY + height; y++)
			{
				SlopeTiles(x, y);
			}
		}
		
		private void Cleaning(int startX, int startY)
		{
			for (int x = startX; x < startX + width; x++)
			for (int y = startY; y < startY + height; y++)

			{
				Tile tile = Framing.GetTileSafely(x, y);

				BlendBiomeTiles(startX, startY, x, y);

				if ((Main.rand.NextBool(3) && tile.WallType == DepthsTiles.GrassWall))
				{
					tile.WallType = DepthsTiles.DirtWall;
				}
			}
		}


		private void RangeFrame(int startX, int startY)
		{
			WorldGen.RangeFrame(startX, startY, startX + width, startY + height);
		}

		private void StoneGen()
		{
			for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 10E-04); k++)
			{
				int x = WorldGen.genRand.Next(0, Main.maxTilesX);

				int y = WorldGen.genRand.Next((int)GenVars.worldSurface, Main.maxTilesY);

				Tile tile = Framing.GetTileSafely(x, y);
				if (tile.TileType == DepthsTiles.Dirt)
				{
					WorldGen.TileRunner(x, y, WorldGen.genRand.Next(8, 18), WorldGen.genRand.Next(8, 18), DepthsTiles.Stone, true, 0f, 0f, false, true);
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
				if (tile.TileType == DepthsTiles.Dirt || tile.TileType == DepthsTiles.Stone)
					WorldGen.TileRunner(x, y, WorldGen.genRand.Next(6, 8), WorldGen.genRand.Next(6, 8), DepthsTiles.Crystal, true, 0f, 0f, false, true);
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
					
					
					if (tile.TileType == DepthsTiles.Dirt
						|| tile.TileType == DepthsTiles.Crystal)
					{
						tile.WallType = DepthsTiles.GrassWall;
					}
					if (tile.TileType == DepthsTiles.Stone)
					{
						tile.WallType = DepthsTiles.StoneWall;
					}
					if ((tile.TileType == DepthsTiles.Dirt
						|| tile.TileType == DepthsTiles.Stone
						|| tile.TileType == DepthsTiles.Crystal)
						&& tile.WallType != DepthsTiles.GrassWall
						&& tile.WallType != DepthsTiles.StoneWall)
					{
						tile.WallType = WallID.None;
					}

					if (cavePoints[x, y] == 0
						&& (tile.TileType == DepthsTiles.Dirt
						|| tile.TileType == DepthsTiles.Stone
						|| tile.TileType == DepthsTiles.Crystal
						|| tile.TileType == TileID.SnowBlock
						|| tile.TileType == TileID.Granite
						|| tile.TileType == TileID.Marble))
					{
						tile.HasTile = false;
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

					if (tile.TileType == DepthsTiles.Dirt && (!tileRight.HasTile || !tileLeft.HasTile || !tileBelow.HasTile ||
					                                     !tileAbove.HasTile))
					{
						tile.TileType = DepthsTiles.Grass;
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
				GroundTest = (tileType) => tileType == DepthsTiles.Grass,
				TreeHeightMin = 8,
				TreeHeightMax = 20,
				TreeTopPaddingNeeded = 4,
				SaplingTileType = DepthsTiles.Sapling,
				WallTest = (wallType) => true
			};
			

			for (int x = startX; x < startX + width; x++)
			{
				for (int y = startY; y < startY + height; y++)
				{
					Tile tile = Main.tile[x, y];
					Tile tileAbove = Main.tile[x, y - 1];

					if (tile.HasTile && tile.TileType == DepthsTiles.Grass && !tileAbove.HasTile)
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
					Tile tile = Main.tile[x, y];

					if (!tileAbove.HasTile)
					{
						continue;
					}

					if ((tileAbove.TileType == DepthsTiles.Dirt
					     || tileAbove.TileType == DepthsTiles.Grass
					     || tileAbove.TileType == DepthsTiles.Stone)
					    && tileAbove.Slope == 0
					    && !tile.HasTile
					   )
					{
						if (Main.rand.Next(100) <= 30)
						{
							WorldGen.PlaceTile(x, y, DepthsTiles.Vine);
						}
					}
					else if (tileAbove.TileType == DepthsTiles.Vine
					         && !tile.HasTile)
					{
						if (Main.rand.Next(100) <= 80)
						{
							WorldGen.PlaceTile(x, y, DepthsTiles.Vine);
						}
					}

					if (tileAbove.TileType == DepthsTiles.Beam
					    && !tile.HasTile)
					{
						WorldGen.PlaceTile(x, y, DepthsTiles.Beam);
					}
				}
			}
		}


		private void BlendBiomeTiles(int startX, int startY, int x, int y)
		{
			Tile tile = Framing.GetTileSafely(x, y);
			tile.LiquidType = 0;

			bool canConvertTile =
				tile.TileType is TileID.Marble or TileID.SnowBlock or TileID.MushroomBlock or TileID.Granite;
			bool canConvertWall = tile.WallType != WallID.None && tile.WallType != DepthsTiles.GrassWall &&
			                      tile.WallType != DepthsTiles.StoneWall && tile.WallType != DepthsTiles.DirtWall &&
			                      tile.WallType != DepthsTiles.WoodWall && tile.WallType != DepthsTiles.BrickWall;

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
					tile.TileType = DepthsTiles.Dirt;

				if (canConvertWall)
					tile.WallType = DepthsTiles.GrassWall;
			}
		}

		private void SlopeTiles(int x, int y)
		{
			Tile tile = Framing.GetTileSafely(x, y);
			if (!tile.HasTile) return;

			ushort type = tile.TileType;
			bool natural = type == DepthsTiles.Dirt || type == DepthsTiles.Crystal || type == DepthsTiles.Stone;

			if (natural || type == DepthsTiles.Crystal2 || type == DepthsTiles.Brick)
			{
				GenHelper.SlopeZero(x, y);
			}

			GenHelper.RemoveAlone(x, y);

			if (!tile.HasTile) return;

			if (natural)
			{
				GenHelper.SlopeNaturally(x, y);
			}
		}


		private static readonly HashSet<int> SolidTiles = new()
			{ DepthsTiles.Dirt, DepthsTiles.Crystal, DepthsTiles.Grass, DepthsTiles.Brick, DepthsTiles.Stone };

		private static bool IsEmpty(int x, int y) => !Framing.GetTileSafely(x, y).HasTile;
		private static bool IsSolid(Tile t) => t.HasTile && SolidTiles.Contains(t.TileType);

		private void ObjectTilesGen(int startX, int startY)
		{
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					var gx = startX + x;
					var gy = startY + y;

					if (!IsEmpty(gx, gy) || !IsEmpty(gx, gy - 1))
					{
						continue;
					}

					Tile below = Framing.GetTileSafely(gx, gy + 1);
					if (!IsSolid(below))
					{
						continue;
					}

					bool wideSpot = IsSolid(Framing.GetTileSafely(gx - 1, gy))
					                && IsEmpty(gx + 1, gy)
					                && IsEmpty(gx + 1, gy - 1);

					if (wideSpot)
					{
						int? id = Main.rand.NextBool(100) ? DepthsTiles.Pillar
							: Main.rand.NextBool(20) ? DepthsTiles.Pot
							: Main.rand.NextBool(15) ? Main.rand.Next([DepthsTiles.ShardBig1, DepthsTiles.ShardBig2, DepthsTiles.ShardBig3])
							: null;

						if (id != null)
						{
							WorldGen.SlopeTile(gx, gy + 1);
							WorldGen.SlopeTile(gx + 1, gy + 1);
							WorldGen.PlaceObject(gx, gy, (ushort)id.Value, true, 0, -1, -1);
							continue;
						}
					}

					if (below.TileType == DepthsTiles.Grass)
					{
						if (Main.rand.Next(100) < 35)
						{
							WorldGen.SlopeTile(gx, gy + 1, 0, true);
							if (WorldGen.PlaceObject(gx, gy, DepthsTiles.Herb, true, 0, -1, -1))
							{
								continue;
							}
						}
					}

					if (Main.rand.NextBool(4)) // 25%
					{
						WorldGen.SlopeTile(gx, gy + 1);
						WorldGen.PlaceObject(gx, gy, DepthsTiles.Shard, true, 0, -1, -1);
					}
				}
			}
		}
	}
}