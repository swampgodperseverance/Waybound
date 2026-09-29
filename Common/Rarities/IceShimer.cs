using System;
using System.Collections.Generic;
using Terraria;
using Waybound.Common.WUtils;

namespace Waybound.Common.Rarities;

public class IceShimer : WayboundRarity {
    private readonly List<Snowflake> snow = [];

    public override Color RarityColor => AnimatedColor([new(79, 195, 247), new(41, 182, 246), new(3, 169, 244), new(3, 155, 229), new(2, 136, 209), new(1, 87, 155)]);
    public override void Draw(Item item, DrawableTooltipLine line) {
        if (line.Name != "ItemName") { return; }
        if (snow.Count < 28 && Main.rand.NextBool(6)){
            snow.Add(new Snowflake {
                pos = new Vector2(Main.rand.NextFloat(line.X - 8f, line.X + line.Font.MeasureString(line.Text).X + 8f), line.Y - Main.rand.NextFloat(4f, 18f)),
                lifeTime = Main.rand.Next(80, 170),
                frame = Main.rand.Next(0, 2),
                scale = Main.rand.NextFloat(0.55f, 1.15f),
                rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                rotationSpeed = Main.rand.NextFloat(-0.045f, 0.045f),
                fallSpeed = Main.rand.NextFloat(0.35f, 0.85f),
                drift = Main.rand.NextFloat(-0.6f, 0.6f),
                wobbleOffset = Main.rand.NextFloat(MathHelper.TwoPi),
            });
            snow[^1].maxLifeTime = snow[^1].lifeTime;
        };
        for (int i = snow.Count - 1; i >= 0; i--) {
            snow[i].Update();
            snow[i].Draw(Main.spriteBatch);
            if (snow[i].ShouldRemove) { snow.RemoveAt(i); }
        }
    }
}

public class Snowflake {
    public bool ShouldRemove => lifeTime <= 0 && alpha <= 0.01f;

    public Vector2 pos = new();
    public int frame = 0;
    public int lifeTime = 0;
    public int maxLifeTime = 0;
    public float scale = 0, alpha = 0, rotation = 0, rotationSpeed = 0, fallSpeed = 0, drift = 0, wobbleOffset = 0;

    public void Update() {
        if (lifeTime > maxLifeTime * 0.75f) { alpha = MathHelper.Clamp(alpha + 0.06f, 0f, 1f); }
        else if (lifeTime < maxLifeTime * 0.25f) { alpha = MathHelper.Clamp(alpha - 0.05f, 0f, 1f); }
        else { alpha = 1f; }
        pos.X += (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.8f + wobbleOffset) * 0.35f + drift * 0.15f;
        pos.Y += fallSpeed;
        rotation += rotationSpeed;
        lifeTime--;
    }
    public void Draw(SpriteBatch spriteBatch) {
        Texture2D tex = Resources.Textures.Extaras[7].Value;
        Rectangle frame2 = tex.Frame(1, 2, 0, frame);
        UI.DrawTexture(spriteBatch, tex, pos, frame2, new Color(180, 230, 255) * (alpha * 0.45f), rotation: rotation, origin: frame2.Size() / 2f, scale: scale * 1.35f);
        UI.DrawTexture(spriteBatch, tex, pos, frame2, Color.White * alpha, rotation: rotation, origin: frame2.Size() / 2f, scale: scale);
    }
}