using Terraria;
using Terraria.DataStructures;
using Waybound.Common.GlobalPlayer;

namespace Waybound.Common.PDL;

public class EnergyShieldLayer : PlayerDrawLayer {
    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.IceBarrier);
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
        Player drawPlayer = drawInfo.drawPlayer;
        return drawPlayer.GetModPlayer<ThunderSigilPlayer>().activeEffect;
    }

    protected override void Draw(ref PlayerDrawSet drawInfo) {
        Player drawPlayer = drawInfo.drawPlayer;
        Texture2D texture = Resources.Textures.Extaras[0].Value;
        int num1 = (int)((double)drawInfo.Position.X + (double)drawPlayer.width / 2.0 - (double)Main.screenPosition.X);
        int num2 = (int)((double)drawInfo.Position.Y + 50 - (double)Main.screenPosition.Y);
        DrawData drawData = new(texture, new Vector2((float)num1, (float)num2), null, new Color(255, 255, 255, 140), 0.0f, new Vector2((float)texture.Width / 2f, (float)texture.Height), 1f, SpriteEffects.None, 0);
        //drawInfo.DrawDataCache.Add(drawData);
    }
};