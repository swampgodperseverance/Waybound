using Terraria;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.WUtils;

namespace Waybound.Common.Command;

internal class SetRace : ModCommand {
    public override string Command => "SetRace";
    public override string Usage => Loc.GetChat("Command.SetRace.Usage");
    public override string Description => Loc.GetChat("Command.SetRace.Description");
    public override CommandType Type => CommandType.Chat;
    public override void Action(CommandCaller caller, string input, string[] args) => Main.LocalPlayer.GetModPlayer<RacePlayer>().race = Race.GetRace(int.Parse(args[0]));
};