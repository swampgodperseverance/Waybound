using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Waybound.Content.Dusts.DeepDusts;
namespace Waybound.Content.Tiles.Furniture
{
    [LegacyName(new string[]
        {
        "PaintTileLarge"
        })]
    public class PaintTile4x3 : ModTile
    {
        // Token: 0x06007859 RID: 30809 RVA: 0x002F53A8 File Offset: 0x002F35A8
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[(int)base.Type] = true;
            Main.tileLavaDeath[(int)base.Type] = true;
            Main.tileSpelunker[(int)base.Type] = true;
            TileID.Sets.FramesOnKillWall[(int)base.Type] = true;
            TileID.Sets.DisableSmartCursor[(int)base.Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.StyleWrapLimit = 111;
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
                16,
                16,
                16
            };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile((int)base.Type);
            base.AddMapEntry(new Color(90, 50, 30), Language.GetText("MapObject.Painting"));
        }

        // Token: 0x0600785A RID: 30810 RVA: 0x002E7996 File Offset: 0x002E5B96
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 0;
        }
    }
}
