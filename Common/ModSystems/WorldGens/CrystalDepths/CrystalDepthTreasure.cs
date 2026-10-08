using Terraria;

namespace Waybound.Common.ModSystems.WorldGens.CrystalDepths;

internal class CrystalDepthTreasure
{
	private static readonly byte[,] _variant0 =
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

	private static readonly byte[,] _variant1 =
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
	
	
		public int Variant { get; private set; }

		public int Height => _targetTiles.GetLength(0);
		public int Width => _targetTiles.GetLength(1);

		private byte[,] _targetTiles;


		public CrystalDepthTreasure(int variant)
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
			}
		}

		public void Place(int startX, int startY)
		{
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
			WorldGen.PlaceChest(startX + 7, startY - 5, DepthsTiles.Chest, false, 0);

		}
		
		private void PlaceBlock(byte block, int globalX, int globalY)
		{
			Tile tile = Framing.GetTileSafely(globalX, globalY);
			switch (block)
			{
				case 0:
					WorldGen.KillTile(globalX, globalY);
					break;
				case 1:
					tile.TileType = DepthsTiles.Dirt;
					tile.HasTile = true;
					WorldGen.SlopeTile(globalX, globalY, 0);
					break;
				case 2:
					tile.TileType = DepthsTiles.Crystal;
					tile.HasTile = true;
					WorldGen.SlopeTile(globalX, globalY, 0);
					break;
				case 3:
					break;
				case 4:
					tile.TileType = DepthsTiles.Brick;
					tile.HasTile = true;
					WorldGen.SlopeTile(globalX, globalY, 0);
					break;
			}
		}

}