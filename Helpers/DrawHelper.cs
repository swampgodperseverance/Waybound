using Terraria;
using Terraria.GameContent;

namespace Waybound.Helpers;

public static class DrawHelper
{
    public static void DrawBar(Vector2 position, int value, int maxValue, Color color, float alpha,
        float scale = 1f, bool noFlip = false)
    {
        if (value <= 0)
            return;

        float progress = MathHelper.Clamp((float)value / maxValue, 0, 1);

        int fillWidth = (int)(36f * progress);
        if (fillWidth < 3)
            fillWidth = 3;
        
        float drawX = position.X - 18f * scale;
        float drawY = position.Y;
        
        if ((int)Main.player[Main.myPlayer].gravDir == -1 && !noFlip) {
            drawY -= Main.screenPosition.Y;
            drawY = Main.screenPosition.Y + Main.screenHeight - drawY;
        }


        SpriteBatch spriteBatch = Main.spriteBatch;
        if (fillWidth < 34) {
            spriteBatch.Draw(TextureAssets.Hb2.Value, new Vector2(drawX - Main.screenPosition.X + fillWidth * scale, drawY - Main.screenPosition.Y), new Rectangle(2, 0, 2, TextureAssets.Hb2.Height()), color, 0f, new Vector2(0f, 0f), scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Hb2.Value, new Vector2(drawX - Main.screenPosition.X + (fillWidth + 2) * scale, drawY - Main.screenPosition.Y), new Rectangle(fillWidth + 2, 0, 36 - fillWidth - 2, TextureAssets.Hb2.Height()), color, 0f, new Vector2(0f, 0f), scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Hb1.Value, new Vector2(drawX - Main.screenPosition.X, drawY - Main.screenPosition.Y), new Rectangle(0, 0, fillWidth - 2, TextureAssets.Hb1.Height()), color, 0f, new Vector2(0f, 0f), scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(TextureAssets.Hb1.Value, new Vector2(drawX - Main.screenPosition.X + (fillWidth - 2) * scale, drawY - Main.screenPosition.Y), new Rectangle(32, 0, 2, TextureAssets.Hb1.Height()), color, 0f, new Vector2(0f, 0f), scale, SpriteEffects.None, 0f);
        }
        else {
            if (fillWidth < 36)
                spriteBatch.Draw(TextureAssets.Hb2.Value, new Vector2(drawX - Main.screenPosition.X + fillWidth * scale, drawY - Main.screenPosition.Y), new Rectangle(fillWidth, 0, 36 - fillWidth, TextureAssets.Hb2.Height()), color, 0f, new Vector2(0f, 0f), scale, SpriteEffects.None, 0f);

            spriteBatch.Draw(TextureAssets.Hb1.Value, new Vector2(drawX - Main.screenPosition.X, drawY - Main.screenPosition.Y), new Rectangle(0, 0, fillWidth, TextureAssets.Hb1.Height()), color, 0f, new Vector2(0f, 0f), scale, SpriteEffects.None, 0f);
        }
    }
}