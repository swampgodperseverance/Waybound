using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.GameContent;

namespace Waybound.Content.Tiles.Furniture.ScorchedWoodFurniture
{
	public class ScorchedWoodBed : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.CanBeSleptIn[Type] = true;
			TileID.Sets.InteractibleByNPCs[Type] = true;
			TileID.Sets.IsValidSpawnPoint[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
			TileObjectData.newTile.CoordinateHeights = new int[] { 16, 18 };
			TileObjectData.addTile(Type);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("Desert Hunter Bed");
			AddMapEntry(new Color(115, 115, 120), name);
            DustType = DustID.Ash;
            AdjTiles = new int[] { TileID.Beds };
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
            r = 0.15f;
            g = 0.15f;
            b = 0.18f;
        }

		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
		public override void ModifySmartInteractCoords(ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY)
		{
			width = 2;
			height = 2;
		}
		public override void ModifySleepingTargetInfo(int i, int j, ref TileRestingInfo info)
		{
			info.VisualOffset.Y += 4f;
		}

	

		public override bool RightClick(int i, int j)
		{
			Player player = Main.LocalPlayer;

			Tile tile = Framing.GetTileSafely(i, j);
			int spawnX = (i - (tile.TileFrameX / 18)) + (tile.TileFrameX >= 72 ? 5 : 2);
			int spawnY = j + 2;
			if (tile.TileFrameY % 38 != 0)
			{
				spawnY--;
			}

			if (!Player.IsHoveringOverABottomSideOfABed(i, j))
			{
				if (player.IsWithinSnappngRangeToTile(i, j, PlayerSleepingHelper.BedSleepingMaxDistance))
				{
					player.GamepadEnableGrappleCooldown();
					player.sleeping.StartSleeping(player, i, j);
				}
			}
			else
			{
				player.FindSpawn();
				if (player.SpawnX == spawnX && player.SpawnY == spawnY)
				{
					player.RemoveSpawn();
					Main.NewText(Language.GetTextValue("Game.SpawnPointRemoved"), byte.MaxValue, 240, 20);
				}
				else if (Player.CheckSpawn(spawnX, spawnY))
				{
					player.ChangeSpawn(spawnX, spawnY);
					Main.NewText(Language.GetTextValue("Game.SpawnPointSet"), byte.MaxValue, 240, 20);
				}
			}
			return true;
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;

			if (!Player.IsHoveringOverABottomSideOfABed(i, j))
			{
				if (player.IsWithinSnappngRangeToTile(i, j, PlayerSleepingHelper.BedSleepingMaxDistance))
				{
					player.noThrow = 2;
					player.cursorItemIconEnabled = true;
					player.cursorItemIconID = ItemID.SleepingIcon;
				}
			}
			else
			{
				player.noThrow = 2;
				player.cursorItemIconEnabled = true;
				player.cursorItemIconID = ModContent.ItemType<Content.Items.Placeable.Furniture.ScorchedWoodFurniture.ScorchedWoodBedI>();
			}
		}
	}
}