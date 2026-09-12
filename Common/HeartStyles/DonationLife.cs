using ReLogic.Content;
using Terraria;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.Utils;

namespace Waybound.Common.HeartStyles;

public class DonationLife : HeartStyle {
    public override bool Reaplece => false;
    public override bool Flip => true;
    public override string Text => Loc.GetUI("ModifyHPTexture.Donated");
    public override bool HasAsset => true;

    public override void Draw(SpriteBatch sB, ref Asset<Texture2D> texture, Vector2 position, ref Color color, float scale) => UI.DrawTexture(sB, texture.Value, position, color: Color.Gray, scale: 1f);
    public override bool Active(Player player, string acyiveStyleName) => player.GetModPlayer<BloodyNecklacePlayer>().CursedHP > 0;
    public override int Count(Player player) => player.GetModPlayer<BloodyNecklacePlayer>().CursedHP;
    public override int Priority(Player player) => 0;
};