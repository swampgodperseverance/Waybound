using Terraria.ID;

namespace Waybound.Common.Water;

public class CryoFluid : ModWaterStyle {
    public override string Texture => "Waybound/Assets/Textures/Water/CryoFluid";
    public override int ChooseWaterfallStyle() => GetInstance<CryoFluidFall>().Slot;
    public override int GetSplashDust() => DustID.IceTorch;
    public override int GetDropletGore() => GoreID.WaterDripIce;
    public override void LightColorMultiplier(ref float r, ref float g, ref float b) {
        r = 0.85f;
        g = 0.92f;
        b = 1.05f;
    }
    public override Color BiomeHairColor() => new(140, 210, 255);
}