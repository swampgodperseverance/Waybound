using Terraria;
using Terraria.GameContent.ItemDropRules;
using Waybound.Common.Systems;
using Waybound.Common.WUtils;

namespace Waybound.Common.ItemDropRules;

public interface IColorDropInfo { public Color DropColor { get; } };
public class HardModeDrop : IItemDropRuleCondition, IColorDropInfo {
	public bool CanDrop(DropAttemptInfo info) => !info.IsInSimulation && Main.hardMode;
	public bool CanShowItemDropInUI() => Main.hardMode;
	public string GetConditionDescription() => Loc.GetCond("HardmodeRule");
	public Color DropColor => Color.Red;
}
public class SlimeRainDrop : IItemDropRuleCondition {
	public bool CanDrop(DropAttemptInfo info) => !info.IsInSimulation && Main.slimeRain;
	public bool CanShowItemDropInUI() => true;
	public string GetConditionDescription() => Loc.GetCond("SlimeRainRule");
}
public class PlanteraDownedDrop : IItemDropRuleCondition {
	public bool CanDrop(DropAttemptInfo info) => !info.IsInSimulation && NPC.downedPlantBoss;
	public bool CanShowItemDropInUI() => Main.hardMode;
	public string GetConditionDescription() => Loc.GetCond("NoPlanteraRule");
}
public class DownedThemis : IItemDropRuleCondition {
	public bool CanDrop(DropAttemptInfo info) => !info.IsInSimulation && DownedBossSystem.DownedThemis;
	public bool CanShowItemDropInUI() => true;
	public string GetConditionDescription() => Loc.GetCond("NoThemisRule");
}
public class Halloween : IItemDropRuleCondition {
	public bool CanDrop(DropAttemptInfo info) => !info.IsInSimulation && Main.halloween;
	public bool CanShowItemDropInUI() => true;
	public string GetConditionDescription() => Loc.GetCond("HalloweenRule");
}