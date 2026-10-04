using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Items.Ammo.Arrows;
using Waybound.Content.Items.Materials.Misc;
using Waybound.Content.Items.Placeable.Furniture;

namespace Waybound.Content.Tiles.Furniture
{
    public class DeepPot : ModTile
	{
		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
			TileObjectData.addTile(Type);
			Main.tileSolidTop[Type] = false;
			Main.tileLighted[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileSpelunker[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;

			HitSound = SoundID.Shatter;

			DustType = ModContent.DustType<DeepStoneDust>();

			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("Pot");
			AddMapEntry(new Color(68, 94, 100), name);
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.80f;
			g = 0.51f;
			b = 0.56f;
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
		{
			offsetY = 4;
		}

		public override bool CanExplode(int i, int j)
		{
			return true;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			int rand = Main.rand.Next(1, 5);
			//ModContent.ItemType<Content.Items.Ammo.crystalArrow>()
			switch (rand)
			{
				case 1:
					Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 48, ModContent.ItemType<CrystalArrow>(), Main.rand.Next(10, 20));
					break;
				case 2:
					Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 48, ModContent.ItemType<CrystalMushroomItem>());
					break;
				case 3:
					Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 48, ModContent.ItemType<DeepCrystalShardItem>(), Main.rand.Next(1, 4));
					break;
				default:
					Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 48, 72, Main.rand.Next(1, 50));
					break;
			}
		}
	}
}