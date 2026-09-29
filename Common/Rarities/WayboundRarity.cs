using Terraria;

namespace Waybound.Common.Rarities;

public abstract class WayboundRarity : ModRarity {
    public abstract void Draw(Item item, DrawableTooltipLine line);
    public static Color AnimatedColor(Color[] colors, byte time = 60) {
        int transitionTime = time;
        int colorCount = colors.Length;
        int totalTime = transitionTime * colorCount;

        int timer = (int)(Main.GameUpdateCount % totalTime);
        int index = timer / transitionTime;
        float t = timer % transitionTime / (float)transitionTime;

        Color from = colors[index % colorCount];
        Color to = colors[(index + 1) % colorCount];

        return Color.Lerp(from, to, t);
    }
};