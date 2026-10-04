using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Items.Placeable.Furniture;

namespace Waybound.Content.Tiles.Furniture
{
	public class DeepCrate : ModTile
	{
		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
			TileObjectData.addTile(Type);
			Main.tileSolidTop[Type] = true;
			Main.tileLighted[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileTable[Type] = true;

			HitSound = SoundID.Dig;
			DustType = ModContent.DustType<DeepTreeDust>();

			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("Crate");
			AddMapEntry(new Color(96, 74, 74), name);
		}

		public override void KillMultiTile(int x, int y, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(x, y), x * 16, y * 16, 48, 32, ModContent.ItemType<DeepCrateI>());
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
		{
			offsetY = 2;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.80f;
			g = 0.51f;
			b = 0.56f;
		}
	}
}