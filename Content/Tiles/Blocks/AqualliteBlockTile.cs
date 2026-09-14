using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Waybound.Content.Tiles.Blocks
{
    public class AqualliteBlockTile : ModTile
    {
        private Asset<Texture2D> outlineTexture;

        private const float FadeInTime = 0.7f;
        private const float VisibleTime = 1.4f;
        private const float FadeOutTime = 0.9f;
        private const float MaxAlpha = 0.55f;

        public override void Load()
        {
            outlineTexture = ModContent.Request<Texture2D>("Waybound/Content/Tiles/Blocks/AqualliteBlockTileOutline");
        }

        public sealed override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBrick[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileMerge[Type][TileID.Dirt] = true;
            Main.tileMerge[TileID.Dirt][Type] = true;
            SafeSetDefaults();
        }

        public virtual void SafeSetDefaults()
        {
            AddMapEntry(new Color(26, 46, 55));
            MineResist = 1.5f;
            HitSound = SoundID.Tink;
            DustType = DustID.Silver;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.035f;
            g = 0.08f;
            b = 0.18f;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            if (!tile.HasTile)
                return;

            int seed = i * 397 + j * 7919;
            UnifiedRandom rand = new UnifiedRandom(seed);

            float cycleLength = FadeInTime + VisibleTime + FadeOutTime + 3.5f;
            float time = (float)Main.GameUpdateCount * 0.0167f;
            float localTime = time + rand.NextFloat(0f, 20f);
            float phase = localTime % cycleLength;

            float alpha = 0f;

            if (phase < FadeInTime)
            {
                alpha = phase / FadeInTime;
            }
            else if (phase < FadeInTime + VisibleTime)
            {
                alpha = 1f;
            }
            else if (phase < FadeInTime + VisibleTime + FadeOutTime)
            {
                alpha = 1f - (phase - FadeInTime - VisibleTime) / FadeOutTime;
            }

            if (rand.NextFloat() > 0.32f)
                alpha = 0f;

            if (alpha <= 0.01f)
                return;

            alpha = MathHelper.SmoothStep(0f, 1f, alpha);
            alpha *= MaxAlpha;

            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawPos = new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero;
            Color color = Color.White * alpha;

            spriteBatch.Draw(
                outlineTexture.Value,
                drawPos,
                new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16),
                color,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0f
            );
        }
    }
}