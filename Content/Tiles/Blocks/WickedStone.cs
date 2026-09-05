using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace Waybound.Content.Tiles.Blocks {
    public class WickedStone : ModTile
    {
        // Token: 0x06001CA2 RID: 7330 RVA: 0x000C25EC File Offset: 0x000C07EC
        public override void SetStaticDefaults()
        {
            TileID.Sets.HellSpecial[(int)base.Type] = true;
            TileID.Sets.ChecksForMerge[(int)base.Type] = true;
            Main.tileBrick[(int)base.Type] = true;
            Main.tileSolid[(int)base.Type] = true;
            Main.tileBlockLight[(int)base.Type] = true;
            base.AddMapEntry(new Color(10, 10, 20), null);
            base.DustType = DustID.Blood;
            base.HitSound = new SoundStyle?(SoundID.Tink);
        }
    }
}
