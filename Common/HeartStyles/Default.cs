using Terraria;

namespace Waybound.Common.HeartStyles;

public class Default : HeartStyle {
    public override bool HasAsset => false;
    public override bool Active(Player player, string acyiveStyleName) => false;
    public override int Count(Player player) => 0;
    public override int Priority(Player player) => 0;
};