using Terraria;

namespace Waybound.Common.Rarities;

public class IceShimer : ModRarity {
    public override Color RarityColor => AnimatedColor([new(79, 195, 247), new(41, 182, 246), new(3, 169, 244), new(3, 155, 229), new(2, 136, 209), new(1, 87, 155)]);
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