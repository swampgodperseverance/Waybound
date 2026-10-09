namespace Waybound.Common.Biome.Water;

public abstract class WayboundWater : ModWaterStyle {
    protected string GetTexturePath() => "Waybound/Assets/Textures/Biome/Water/" + Name;
    public abstract void Draw(SpriteBatch spriteBatch, Vector2 pos, bool beginSpriteBatch);
    public override void Load() => Core.CustomClassData.Liquids.Add(this);
};
