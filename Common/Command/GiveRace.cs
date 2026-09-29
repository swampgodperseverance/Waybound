using Terraria;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.WUtils;

namespace Waybound.Common.Command;

public class GiveRace : ModCommand {
    public override string Command => "GiveRace";
    public override string Usage => Loc.GetChat("Command.GiveRace.Usage");
    public override string Description => Loc.GetChat("Command.GiveRace.Description");
    public override CommandType Type => CommandType.Chat;
    public override void Action(CommandCaller caller, string input, string[] args) => Main.NewText(Main.LocalPlayer.GetModPlayer<RacePlayer>().Race.RaceName);
};