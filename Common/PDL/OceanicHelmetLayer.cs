using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Waybound.Content.Items.Armor.Magic.Water;

namespace Waybound.Common.PDL;

public class OceanicHelmetLayer : PlayerDrawLayer {
    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);
    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => WUtils.PL.CheckHelmet(drawInfo.drawPlayer, ItemType<OceanicHelmet>()) && !drawInfo.drawPlayer.dead;
    protected override void Draw(ref PlayerDrawSet drawInfo) {
        Player player = drawInfo.drawPlayer;
        if (player.armor[10].type != ItemID.None && player.armor[10].type != ItemType<OceanicHelmet>()) { return; };
        int x = (int)(drawInfo.Position.X + player.width / 2f - Main.screenPosition.X);
        int y = (int)(drawInfo.Position.Y + player.height / 2f - Main.screenPosition.Y - 3);
        drawInfo.DrawDataCache.Add(new(Resources.Textures.Extaras[6].Value, new(x + (player.direction == 1 ? -3 : 3), y - 13), Resources.Textures.Extaras[6].Value.Frame(1, 8, 0, (int)(Main.GlobalTimeWrappedHourly * 12f) % 8), drawInfo.colorArmorHead, 0f, Resources.Textures.Extaras[6].Value.Frame(1, 8, 0, 0).Size() / 2f, 1f, drawInfo.playerEffect, 0) { shader = drawInfo.cHead });
    }
};