using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Tiles.Blocks;

namespace Waybound.Content.Tiles.Plants.Trees
{
	public class DeepTree : ModTree
	{
		public override TreePaintingSettings TreeShaderSettings => new TreePaintingSettings
		{
			UseSpecialGroups = true,
			SpecialGroupMinimalHueValue = 11f / 72f,
			SpecialGroupMaximumHueValue = 0.25f,
			SpecialGroupMinimumSaturationValue = 0.88f,
			SpecialGroupMaximumSaturationValue = 1f
		};

		public override void SetStaticDefaults()
		{
			GrowsOnTileId = new int[1] { ModContent.TileType<DeepGrass>() };
		}

		public override Asset<Texture2D> GetTexture()
		{
			return ModContent.Request<Texture2D>("Waybound/Content/Tiles/Plants/Trees/DeepTree");
		}

		public override int SaplingGrowthType(ref int style)
		{
			style = 0;
			return ModContent.TileType<DeepSapling>();
		}

		/*public override int TreeLeaf() 
		{
			return ModContent.GoreType<ExampleTreeLeaf>();
		}*/

		public override int CreateDust() => ModContent.DustType<DeepTreeDust>();

		public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight)
		{
		}

		public override Asset<Texture2D> GetBranchTextures()
		{
			return ModContent.Request<Texture2D>("Waybound/Content/Tiles/Plants/Trees/DeepTree_Branches");
		}

		public override Asset<Texture2D> GetTopTextures()
		{
			return ModContent.Request<Texture2D>("Waybound/Content/Tiles/Plants/Trees/DeepTree_Tops");
		}

		public override int DropWood()
		{
			return ModContent.ItemType<DeepTreeItem>();
		}
	}
}