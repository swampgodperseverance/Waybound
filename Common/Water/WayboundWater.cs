using Terraria.Graphics.Effects;

namespace Waybound.Common.Water;

public abstract class WayboundWater : ModWaterStyle {
    protected string GetTexturePath() => "Waybound/Assets/Textures/Water/" + Name;
    public abstract void Draw(SpriteBatch spriteBatch, Vector2 pos, bool beginSpriteBatch);
    public override void Load() => Core.CustomClassData.Liquids.Add(this);
};
