using System.Drawing;
using Terraria;
using Terraria.ID;
using Waybound.Common.Utils;

namespace Waybound.Content.Tiles.Misc;

public class ErisSwordTile : ModTile {
    public override void SetStaticDefaults() {
        Main.tileSolidTop[Type] = true;
        Main.tileBlockLight[Type] = true;
        AddMapEntry(new(141, 111, 85), CreateMapEntryName());
        DustType = DustID.WoodFurniture;
    }
    public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
    public override bool CanExplode(int i, int j) => false;
    public override bool CanReplace(int i, int j, int tileTypeBeingPlaced) => false;

    
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) {
        Vector2 zero = new(Main.offScreenRange, Main.offScreenRange);
        if (Main.drawToScreen) { zero = Vector2.Zero; }
        spriteBatch.Draw(Resources.Textures.Tiles[0].Value, new Vector2(i * 16f, j * 16f) + zero - Main.screenPosition, null, Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f); ;
        return false;
    }
};
