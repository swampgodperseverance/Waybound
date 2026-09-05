using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.Utils;

namespace Waybound.Common.PDL;

public class ThunderShield : PlayerDrawLayer {
    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.IceBarrier);
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => drawInfo.drawPlayer.GetModPlayer<ThunderSigilPlayer>().equipped;
    protected override void Draw(ref PlayerDrawSet drawInfo) {
        Player drawPlayer = drawInfo.drawPlayer;
        ThunderSigilPlayer modPlayer = drawPlayer.GetModPlayer<ThunderSigilPlayer>();
        if (drawInfo.shadow != 0f) { return; }

        Asset<Texture2D>[] asset = Resources.Textures.Extaras;

        Vector2 Position = drawInfo.Position;
        Vector2 pos = new((int)(Position.X - Main.screenPosition.X + (drawPlayer.width / 2)), (int)(Position.Y - Main.screenPosition.Y + (drawPlayer.height / 2) - 2f * drawPlayer.gravDir) + 30);
        Vector2 barrierPos = new(pos.X - 24, pos.Y - 72);
        Vector2 barrierElementPos = new(pos.X - 13, pos.Y - 90);
        barrierElementPos.Y += (float)System.Math.Sin(Main.GlobalTimeWrappedHourly * 4);

        DrawData cristalSkletBg;
        if (modPlayer.activeEffect) { cristalSkletBg = new(asset[7].Value, barrierPos.X(4).Y(14), new Color(180, 180, 180, 200)); }
        else { cristalSkletBg = new(asset[7].Value, barrierPos.X(4).Y(14), new Color(120, 120, 120, 140)); };
        drawInfo.DrawDataCache.Add(cristalSkletBg);

        DrawData cristalSklet = new(asset[6].Value, barrierPos, new(255, 255, 255, 255));
        drawInfo.DrawDataCache.Add(cristalSklet);

        //DrawData elementTop = new(asset[5].Value, barrierElementPos, asset[5].Value.Frame(1, 2, 0, 0), Color.White);
        //drawInfo.DrawDataCache.Add(elementTop);

        //DrawData elementBot = new(asset[5].Value, barrierElementPos.Y(110).X(2), asset[5].Value.Frame(1, 2, 0, 1), Color.White);
        //drawInfo.DrawDataCache.Add(elementBot);
    }
};