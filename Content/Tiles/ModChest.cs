using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Waybound.Content.Tiles;

public abstract class ModChest : ModTile {
    public abstract Color MapColor { get; }
    public abstract int Dust { get; }
    public abstract void MouseIcon(Player player, int style);

    public override void Load() => _ = this.GetLocalization("MazeChest").Value;
    public override void SetStaticDefaults() {
        Main.tileSpelunker[Type] = true;
        Main.tileContainer[Type] = true;
        Main.tileShine2[Type] = true;
        Main.tileShine[Type] = 1200;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileOreFinderPriority[Type] = 500;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.BasicChest[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.AvoidedByNPCs[Type] = true;
        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.IsAContainer[Type] = true;
        TileID.Sets.FriendlyFairyCanLureTo[Type] = true;
        TileID.Sets.GeneralPlacementTiles[Type] = false;
        AdjTiles = [TileID.Containers];
        AddMapEntry(MapColor, CreateMapEntryName());
        DustType = Dust;
        RegisterItemDrop(ItemID.Chest);
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
        TileObjectData.newTile.AnchorInvalidTiles = [TileID.MagicalIceBlock, TileID.Boulder, TileID.BouncyBoulder, TileID.LifeCrystalBoulder, TileID.RollingCactus];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
        TileObjectData.addTile(Type);
    }
    public override ushort GetMapOption(int i, int j) => (ushort)(Main.tile[i, j].TileFrameX / 36);
    public override LocalizedText DefaultContainerName(int frameX, int frameY) => this.GetLocalization("MazeChest");
    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
    public override bool IsLockedChest(int i, int j) {
        return Main.tile[i, j].TileFrameX / 36 == 1;
    }
    public override bool UnlockChest(int i, int j, ref short frameXAdjustment, ref int dustType, ref bool manual) {
        if (Main.dayTime) {
            Main.NewText("The chest stubbornly refuses to open in the light of the day. Try again at night.", Color.Orange);
            return false;
        }

        DustType = dustType;
        return true;
    }
    public override bool LockChest(int i, int j, ref short frameXAdjustment, ref bool manual) {
        int style = TileObjectData.GetTileStyle(Main.tile[i, j]);
        if (style == 0) {
            return true;
        }
        return false;
    }
    public static string MapChestName(string name, int i, int j) {
        int left = i;
        int top = j;
        Tile tile = Main.tile[i, j];

        if (tile.TileFrameX % 36 != 0) { left--; };
        if (tile.TileFrameY != 0) { top--; };
        int chest = Chest.FindChest(left, top);
        if (chest < 0) { return Language.GetTextValue("LegacyChestType.0"); };
        if (Main.chest[chest].name == "") { return name; };

        return name + ": " + Main.chest[chest].name;
    }
    public override void NumDust(int i, int j, bool fail, ref int num) => num = 1;
    public override void KillMultiTile(int i, int j, int frameX, int frameY) => Chest.DestroyChest(i, j);
    public override bool RightClick(int i, int j) {
        Player player = Main.LocalPlayer;
        Tile tile = Main.tile[i, j];

        Main.mouseRightRelease = false;

        int left = i;
        int top = j;

        if (tile.TileFrameX % 36 != 0) { left--; };
        if (tile.TileFrameY != 0) { top--; };

        player.CloseSign();
        player.SetTalkNPC(-1);
        Main.npcChatCornerItem = 0;
        Main.npcChatText = string.Empty;
        if (Main.editChest) {
            SoundEngine.PlaySound(SoundID.MenuTick);
            Main.editChest = false;
            Main.npcChatText = string.Empty;
        };
        if (player.editedChestName) {
            NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f);
            player.editedChestName = false;
        };

        bool isLocked = Chest.IsLocked(left, top);

        if (Main.netMode == NetmodeID.MultiplayerClient && !isLocked) {
            if (left == player.chestX && top == player.chestY && player.chest != -1) {
                player.chest = -1;
                Recipe.FindRecipes();
                SoundEngine.PlaySound(SoundID.MenuClose);
            } else {
                NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, top);
                Main.stackSplit = 600;
            };
        }
        else {
            if (isLocked) {
               //int key = ModContent.ItemType<Key>();
                //if (player.HasItemInInventoryOrOpenVoidBag(key) && Chest.Unlock(left, top) && player.ConsumeItem(key, includeVoidBag: true)) {
                //    if (Main.netMode == NetmodeID.MultiplayerClient) {
                //        NetMessage.SendData(MessageID.LockAndUnlock, -1, -1, null, player.whoAmI, 1f, left, top);
                //    }
                //}
            } else {
                int chestID = Chest.FindChest(left, top);
                if (chestID != -1) {
                    Main.stackSplit = 600;
                    if (chestID == player.chest) {
                        player.chest = -1;
                        SoundEngine.PlaySound(SoundID.MenuClose);
                    } else {
                        SoundEngine.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
                        player.OpenChest(left, top, chestID);
                    };
                    Recipe.FindRecipes();
                };
            };
        };
        return true;
    }
    public override void MouseOver(int i, int j) {
        Player player = Main.LocalPlayer;
        Tile tile = Main.tile[i, j];
        int left = i;
        int top = j;

        if (tile.TileFrameX % 36 != 0) { left--; };
        if (tile.TileFrameY != 0) { top--; };

        int chest = Chest.FindChest(left, top);
        player.cursorItemIconID = -1;
        if (chest < 0) { player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0"); }
        else {
            string defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY);
            player.cursorItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : defaultName;
            if (player.cursorItemIconText == defaultName) {
                MouseIcon(player, 0);
                player.cursorItemIconText = "";
            }
        }

        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
    }
    public override void MouseOverFar(int i, int j) {
        MouseOver(i, j);
        Player player = Main.LocalPlayer;
        int style = Main.tile[i, j].TileFrameX;
        MouseIcon(player, style);
        //if (style == 72 || style == 90) {
        //    //player.cursorItemIconID = ModContent.ItemType<BlueKey>();
        //    player.cursorItemIconText = "";
        //}
        //else {
        //    //player.cursorItemIconID = ModContent.ItemType<Items.Placeable.BlueMazeChest>();
        //}
    }
};