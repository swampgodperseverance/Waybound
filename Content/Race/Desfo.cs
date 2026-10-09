using Terraria;
using Terraria.Localization;

namespace Waybound.Content.Race;

public class Desfo : RaceInfo {
    protected override Stat SetStat => new(-40, 20);
    public override Color[] Colors => [Color.Gold, Color.Goldenrod];
}
public static class DesfosRace
{
    public static bool IsDesfo(Player player)
    {
        if (player is null || !player.active)
            return false;

        return Common.WUtils.Race.CheckRace(player, Common.WUtils.Race.ID.Desfo);
    }
}
public class DesfosNoCraftSystem : ModSystem
{
    public override void PostAddRecipes()
    {
        Condition cannotCraft = new Condition(
            Language.GetOrRegister("Mods.Waybound.Conditions.DesfoCannotCraft", () => "Desfos cannot craft"),
            () => Main.gameMenu || Main.LocalPlayer == null || !DesfosRace.IsDesfo(Main.LocalPlayer));

        for (int i = 0; i < Recipe.numRecipes; i++)
        {
            Main.recipe[i].AddCondition(cannotCraft);
        }
    }
}
