using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace Waybound.Common.Utils;

public static class UI {
    public static float GetProgress(int startPos, int endPos, bool reflect = false) {
        float progress = (float)endPos <= 0 ? 1f : (float)startPos / (float)endPos;
        progress = MathHelper.Clamp(progress, 0f, 1f);
        if (reflect) { progress = 1f - progress; }
        return progress;
    }
    public static string[] ClearText(string[] arr) {
        for (int i = 0; i < arr.Length; i++) {
            for (int j = 0; j < arr[i].Length; j++) {
                if (arr[i][j] == '[' && arr[i][j + 1] == 'c' && arr[i][j + 2] == '/') {
                    int index = j;
                    int count = 0;
                    for (int k = 0; arr[i][j + k] != ':'; k++) { count = k + 1; }
                    arr[i] = arr[i].Remove(index, count + 1);
                    for (int k = j; arr[i][k] != ']'; k++) { index = k + 1; }
                    arr[i] = arr[i].Remove(index, 1);
                }
            }
        }
        return arr;
    }
    public static string GetButtonName(ModKeybind key) => key.GetAssignedKeys().Count > 0 ? key.GetAssignedKeys()[0] : 
        
        Loc.Get("Keybinds.NotKey");
    public static bool LeftClick() => Main.mouseLeft && Main.mouseLeftRelease;
    public static bool RightClick() => Main.mouseRight && Main.mouseRightRelease;
    public static Vector2 GetMousePos() => new(Main.mouseX, Main.mouseY);
    public static bool Hover(Vector2 pos, Texture2D texture, float drawScale = 1f) {
        Vector2 size = texture.Size() * drawScale;
        Rectangle rect = new((int)(pos.X - size.X / 2f), (int)(pos.Y - size.Y / 2f), texture.Width, texture.Height);
        return rect.Contains(Main.mouseX, Main.mouseY);
    }
    public static bool Hover(Vector2 pos, Rectangle texture, float drawScale = 1f) {
        Vector2 size = texture.Size() * drawScale;
        Rectangle rect = new((int)(pos.X - size.X / 2f), (int)(pos.Y - size.Y / 2f), texture.Width, texture.Height);
        return rect.Contains(Main.mouseX, Main.mouseY);
    }
    public static bool HoverText(Vector2 pos, string text, float scale = 0.9f) => new Vector2(Main.mouseX, Main.mouseY).Between(pos, pos + ChatManager.GetStringSize(FontAssets.MouseText.Value, text, new Vector2(scale)) * new Vector2(scale) * new Vector2(1f).X);
    public static void DrawText(SpriteBatch sB, string text, Vector2 pos, DynamicSpriteFont font = null, Color? color = null, Color? color1 = null, Vector2? orgin = null, Vector2? scale = null) {
        font ??= FontAssets.MouseText.Value;
        color ??= Color.White;
        orgin ??= Vector2.Zero;
        scale ??= Vector2.One;
        if (color1 != null) { ChatManager.DrawColorCodedStringWithShadow(sB, font, text, pos, (Color)color, (Color)color1, 0f, (Vector2)orgin, (Vector2)scale); }
        else { ChatManager.DrawColorCodedStringWithShadow(sB, font, text, pos, (Color)color, 0f, (Vector2)orgin, (Vector2)scale); }
    }
    public static void DrawTexture<T>(SpriteBatch sB, Texture2D texture, T VectorOrRect, Rectangle? sourceRectangle = null, Color? color = null, float rotation = 0f, Vector2? origin = null, float scale = 1f, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0) { 
        color ??= Color.White;
        if (origin == null) { origin = sourceRectangle == null ? texture.Size() / 2f : sourceRectangle.Value.Size() / 2f; }
        if (VectorOrRect is Vector2 pos) { sB.Draw(texture, pos, sourceRectangle, color.Value, rotation, origin.Value, scale, effects, layerDepth); }
        else if (VectorOrRect is Rectangle rec) { sB.Draw(texture, rec, sourceRectangle, color.Value, rotation, origin.Value, effects, layerDepth); }
        else { throw new System.Exception("Params <T> is not Vector or Rectangle"); };
    }
    public static Vector2 X(this Vector2 pos, float value) => new(pos.X + value, pos.Y);
    public static Vector2 Y(this Vector2 pos, float value) => new(pos.X, pos.Y + value);
};