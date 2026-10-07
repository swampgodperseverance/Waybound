using Terraria;
using Terraria.ID;
using Terraria.ObjectData;
using Waybound.Content.Dusts;

namespace Waybound.Content.Tiles.Furniture.DesertHunterFurniture
{
	public class DesertHunterForge : ModTile
	{
        public override void SetStaticDefaults()
        {
            AnimationFrameHeight = 54;
        
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addTile(Type);

            Main.tileSolidTop[Type] = false;
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            HitSound = SoundID.Tink;
            DustType = DustType<DesertHunterDust>();
            AddMapEntry(new Color(96, 74, 74));
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            frameCounter++;
            if (frameCounter > 8)
            {
                frameCounter = 0;
                frame++;
                frame %= 4;
            }
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            int uniqueAnimationFrame = Main.tileFrame[Type] + i + j;
            uniqueAnimationFrame %= 4; 
            frameYOffset = uniqueAnimationFrame * AnimationFrameHeight;
        }
    }
}