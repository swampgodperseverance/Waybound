using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Enums;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.GameContent.ObjectInteractions;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Items.Placeable.Furniture.DeepWoodFurniture;

namespace Waybound.Content.Tiles.Furniture.DeepWoodFurniture
{
	public class DeepWoodDresser : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolidTop[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileTable[Type] = true;
			Main.tileContainer[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.HasOutlines[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.BasicDresser[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.Origin = new Point16(1, 1);
			TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(new Func<int, int, int, int, int, int, int>(Chest.FindEmptyChest), -1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(new Func<int, int, int, int, int, int, int>(Chest.AfterPlacement_Hook), -1, 0, false);
			TileObjectData.newTile.AnchorInvalidTiles = new int[] { 127 };
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.addTile(Type);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
			
			LocalizedText name = CreateMapEntryName();
			// name.SetDefault("Deep Wood Dresser");
			AddMapEntry(new Color(96, 74, 74), name);
			// ContainerName/* tModPorter Note: Removed. Override DefaultContainerName instead */.SetDefault("Deep Wood Dresser");

			DustType = ModContent.DustType<DeepTreeDust>();
			AdjTiles = new int[] { TileID.Dressers };
		}

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.60f;
            g = 0.375f;
            b = 0.36f;
        }

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;


				public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
		public override void ModifySmartInteractCoords(ref int width, ref int height, ref int frameWidth, ref int frameHeight, ref int extraY)
		{
			width = 3;
			height = 1;
			extraY = 0;
		}

		public override bool RightClick(int i, int j) {
			Player player = Main.LocalPlayer;
			int left = Main.tile[i, j].TileFrameX / 18;
			left %= 3;
			left = i - left;
			int top = j - Main.tile[i, j].TileFrameY / 18;
			if (Main.tile[i, j].TileFrameY == 0) {
				Main.CancelClothesWindow(true);
				Main.mouseRightRelease = false;
				player.CloseSign();
				player.SetTalkNPC(-1);
				Main.npcChatCornerItem = 0;
				Main.npcChatText = "";
				if (Main.editChest) {
					SoundEngine.PlaySound(SoundID.MenuTick);
					Main.editChest = false;
					Main.npcChatText = string.Empty;
				}
				if (player.editedChestName) {
					NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f);
					player.editedChestName = false;
				}
				if (Main.netMode == NetmodeID.MultiplayerClient) {
					if (left == player.chestX && top == player.chestY && player.chest != -1) {
						player.chest = -1;
						Recipe.FindRecipes();
						SoundEngine.PlaySound(SoundID.MenuClose);
					}
					else {
						NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, top);
						Main.stackSplit = 600;
					}
				}
				else {
					player.piggyBankProjTracker.Clear();
					player.voidLensChest.Clear();
					int chestIndex = Chest.FindChest(left, top);
					if (chestIndex != -1) {
						Main.stackSplit = 600;
						if (chestIndex == player.chest) {
							player.chest = -1;
							Recipe.FindRecipes();
							SoundEngine.PlaySound(SoundID.MenuClose);
						}
						else if (chestIndex != player.chest && player.chest == -1) {
							player.OpenChest(left, top, chestIndex);
							SoundEngine.PlaySound(SoundID.MenuOpen);
						}
						else {
							player.OpenChest(left, top, chestIndex);
							SoundEngine.PlaySound(SoundID.MenuTick);
						}
						Recipe.FindRecipes();
					}
				}
			}
			else {
				Main.playerInventory = false;
				player.chest = -1;
				Recipe.FindRecipes();
				player.SetTalkNPC(-1);
				Main.npcChatCornerItem = 0;
				Main.npcChatText = "";
				Main.interactedDresserTopLeftX = left;
				Main.interactedDresserTopLeftY = top;
				Main.OpenClothesWindow();
			}
			return true;
		}

		public override void MouseOverFar(int i, int j)
		{
			Player player = Main.LocalPlayer;
			Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
			int left = Player.tileTargetX;
			int top = Player.tileTargetY;
			left -= tile.TileFrameX % 54 / 18;
			if (tile.TileFrameY % 36 != 0)
			{
				top--;
			}
			int chestIndex = Chest.FindChest(left, top);
			player.cursorItemIconID = -1;
			if (chestIndex < 0)
			{
				player.cursorItemIconText = Language.GetTextValue("LegacyDresserType.0");
			}
			else
			{
				if (Main.chest[chestIndex].name != "")
				{
					player.cursorItemIconText = Main.chest[chestIndex].name;
				}
				else
				{
					player.cursorItemIconText = "Deep Wood Dresser";
				}
				if (player.cursorItemIconText == "Deep Wood Dresser")
				{
					player.cursorItemIconID = ModContent.ItemType<DeepWoodDresserI>();
					player.cursorItemIconText = "";
				}
			}
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			if (player.cursorItemIconText == "")
			{
				player.cursorItemIconEnabled = false;
				player.cursorItemIconID = 0;
			}
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);
			int left = Player.tileTargetX;
			int top = Player.tileTargetY;
			left -= tile.TileFrameX % 54 / 18;
			if (tile.TileFrameY % 36 != 0)
			{
				top--;
			}
			int num138 = Chest.FindChest(left, top);
			player.cursorItemIconID = -1;
			if (num138 < 0)
			{
				player.cursorItemIconText = Language.GetTextValue("LegacyDresserType.0");
			}
			else
			{
				if (Main.chest[num138].name != "")
				{
					player.cursorItemIconText = Main.chest[num138].name;
				}
				else
				{
					player.cursorItemIconText = "Deep Wood Dresser";
				}
				if (player.cursorItemIconText == "Deep Wood Dresser")
				{
					player.cursorItemIconID = ModContent.ItemType<DeepWoodDresserI>();
					player.cursorItemIconText = "";
				}
			}
			player.noThrow = 2;
			player.cursorItemIconEnabled = true;
			if (Main.tile[Player.tileTargetX, Player.tileTargetY].TileFrameY > 0)
			{
				player.cursorItemIconID = ItemID.FamiliarShirt;
				player.cursorItemIconText = "";
			}
		}
	}
}