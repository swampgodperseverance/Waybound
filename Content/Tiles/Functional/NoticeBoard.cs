using System.Drawing;
using Terraria;
using Terraria.ID;
using Waybound.Common.Utils;

namespace Waybound.Content.Tiles.Functional;

public class NoticeBoard : ModTile {
    public override void SetStaticDefaults() {
        Main.tileSolidTop[Type] = true;
        Main.tileBlockLight[Type] = true;
        AddMapEntry(new(141, 111, 85), CreateMapEntryName());
        DustType = DustID.WoodFurniture;
    }
    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
    public override bool CanExplode(int i, int j) => false;
    public override bool CanReplace(int i, int j, int tileTypeBeingPlaced) => false;
    public override void NearbyEffects(int i, int j, bool closer) {
        if (!closer) {
            Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen) { zero = Vector2.Zero; }
            ActivateNoteBoard.X = (int)((i - 10) * 16f + zero.X - Main.screenPosition.X);
            ActivateNoteBoard.Y = (int)((j - 10) * 16f + zero.Y - Main.screenPosition.Y) - 6;
        };
    }
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) { zero = Vector2.Zero; }
        spriteBatch.Draw(Resources.Textures.Tiles[0].Value, new Vector2(i * 16f, j * 16f) + zero - Main.screenPosition, null, Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f); ;
        return false;
    }
};
public class ActivateNoteBoard : ModSystem {
    public static int X = 0, Y = 0;

    public override void PostUpdateWorld() {
        if (X == 0 || Y == 0) { return; };
        if (UI.Hover(new Vector2(X, Y), Resources.Textures.Tiles[0].Value)) {
            if (UI.RightClick()) {
                Main.NewText("Open");
            };
        }
    }
}