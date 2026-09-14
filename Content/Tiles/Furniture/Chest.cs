using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Waybound.Content.Tiles.Furniture;

public abstract class Chest : ModTile
{
    public override void SetStaticDefaults()
    {
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
        VanillaFallbackOnModDeletion = TileID.Containers;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Terraria.Chest.FindEmptyChest, -1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Terraria.Chest.AfterPlacement_Hook, -1, 0, false);
        TileObjectData.newTile.AnchorInvalidTiles = [
            TileID.MagicalIceBlock,
            TileID.Boulder,
            TileID.BouncyBoulder,
            TileID.LifeCrystalBoulder,
            TileID.RollingCactus
        ];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);

        SetChestDefaults();
        TileObjectData.addTile(Type);
    }

    public virtual void SetChestDefaults() { }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        Terraria.Chest.DestroyChest(i, j);
    }

    public override bool RightClick(int i, int j)
    {
        Player player = Main.LocalPlayer;
        Tile tile = Main.tile[i, j];
        Main.mouseRightRelease = false;

        int left = i;
        int top = j;
        if (tile.TileFrameX % 36 != 0)
            left--;
        if (tile.TileFrameY != 0)
            top--;

        player.CloseSign();
        player.SetTalkNPC(-1);
        Main.npcChatCornerItem = 0;
        Main.npcChatText = "";

        if (Main.editChest)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            Main.editChest = false;
            Main.npcChatText = string.Empty;
        }

        if (player.editedChestName)
        {
            NetMessage.SendData(MessageID.SyncPlayerChest, text: NetworkText.FromLiteral(Main.chest[player.chest].name), number: player.chest, number2: 1f);
            player.editedChestName = false;
        }

        bool isLocked = IsLockedChest(i, j);
        if (Main.netMode == NetmodeID.MultiplayerClient && !isLocked)
        {
            if (left == player.chestX && top == player.chestY && player.chest != -1)
            {
                player.chest = -1;
                Recipe.FindRecipes();
                SoundEngine.PlaySound(SoundID.MenuClose);
            }
            else
            {
                NetMessage.SendData(MessageID.RequestChestOpen, number: left, number2: top);
                Main.stackSplit = 600;
            }
        }
        else
        {
            if (!isLocked)
            {
                int chest = Terraria.Chest.FindChest(left, top);
                if (chest != -1)
                {
                    Main.stackSplit = 600;
                    if (chest == player.chest)
                    {
                        player.chest = -1;
                        SoundEngine.PlaySound(SoundID.MenuClose);
                    }
                    else
                    {
                        SoundEngine.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
                        player.OpenChest(left, top, chest);
                    }
                    Recipe.FindRecipes();
                }
            }
        }

        return true;
    }

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        Tile tile = Main.tile[i, j];
        int left = i;
        int top = j;
        if (tile.TileFrameX % 36 != 0)
            left--;
        if (tile.TileFrameY != 0)
            top--;

        int chest = Terraria.Chest.FindChest(left, top);
        player.cursorItemIconID = -1;

        if (chest < 0)
        {
            player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
        }
        else
        {
            string defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY);
            player.cursorItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : defaultName;

            if (player.cursorItemIconText == defaultName)
            {
                player.cursorItemIconID = GetItemDropType();
                player.cursorItemIconText = "";
            }
        }

        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
    }

    public override void MouseOverFar(int i, int j)
    {
        MouseOver(i, j);
        Player player = Main.LocalPlayer;
        if (player.cursorItemIconText == "")
        {
            player.cursorItemIconEnabled = false;
            player.cursorItemIconID = ItemID.None;
        }
    }

    public virtual int GetItemDropType() => 0;

    public virtual bool IsLockedChest(int i, int j) => false;
}