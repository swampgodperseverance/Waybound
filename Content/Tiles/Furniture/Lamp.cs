using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Waybound.Content.Tiles.Furniture;

public abstract class Lamp : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;

        //TileID.Sets.HasOutlines[Type] = true; FUCK IT
        TileID.Sets.DisableSmartCursor[Type] = true;

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);

        AdjTiles = [TileID.Lamps];
        VanillaFallbackOnModDeletion = TileID.Lamps;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16];
        TileObjectData.newTile.Origin = new Point16(0, 2);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = true;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);

        SetLampDefaults();
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(80, 160, 220), Language.GetText("MapObject.FloorLamp"));
    }

    public virtual void SetLampDefaults() { }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override void HitWire(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        int topY = j - tile.TileFrameY / 18 % 3;

        short frameAdjustment = (short)(tile.TileFrameX != 0 ? -18 : 18);

        Main.tile[i, topY].TileFrameX += frameAdjustment;
        Main.tile[i, topY + 1].TileFrameX += frameAdjustment;
        Main.tile[i, topY + 2].TileFrameX += frameAdjustment;

        Wiring.SkipWire(i, topY);
        Wiring.SkipWire(i, topY + 1);
        Wiring.SkipWire(i, topY + 2);

        if (Main.netMode != NetmodeID.SinglePlayer)
            NetMessage.SendTileSquare(-1, i, topY + 1, 3);
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
    {
        if (i % 2 == 1)
            spriteEffects = SpriteEffects.FlipHorizontally;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        Tile tile = Main.tile[i, j];
        if (tile.TileFrameX == 0) 
        {
            Vector3 color = GetLightColor();
            r = color.X;
            g = color.Y;
            b = color.Z;
        }
    }

    public virtual Vector3 GetLightColor() => new Vector3(0.35f, 0.75f, 1.15f); 

    public virtual int GetItemDropType() => 0;

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = GetItemDropType();
    }

    //  GLOW SUPPORT

    public virtual Asset<Texture2D> GlowTexture => null;

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        if (GlowTexture == null)
            return;

        Tile tile = Main.tile[i, j];
        if (tile.TileFrameX != 0) 
            return;

        Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
        Vector2 position = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero;

        Rectangle frame = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);

        float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.2f) * 0.15f + 0.85f;

        Color glowColor = new Color(120, 200, 255) * pulse; 

        spriteBatch.Draw(
            GlowTexture.Value,
            position,
            frame,
            glowColor,
            0f,
            Vector2.Zero,
            1f,
            SpriteEffects.None,
            0f
        );
    }
}