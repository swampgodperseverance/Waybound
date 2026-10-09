namespace Waybound.Common.WUtils;

public static class WColors {
    public static Color OutLineColor(Color dark, Color white, float pulse, float alpha) {
        Color color = Color.Lerp(dark * alpha, white * alpha, pulse);
        return color * alpha;
    }
};
