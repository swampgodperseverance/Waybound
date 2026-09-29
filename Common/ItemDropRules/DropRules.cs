using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using Waybound.Common.Systems;

namespace Waybound.Common.ItemDropRules
{
	public class HardModeDrop : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info) => !info.IsInSimulation && Main.hardMode;

		public bool CanShowItemDropInUI() => Main.hardMode;

		public string GetConditionDescription() => Language.GetTextValue("Mods.Waybound.DropRules.HardmodeRule");
	}

	public class SlimeRainDrop : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info) => !info.IsInSimulation && Main.slimeRain;

		public bool CanShowItemDropInUI() => true;

		public string GetConditionDescription() => Language.GetTextValue("Mods.Waybound.DropRules.SlimeRainRule");
	}

	public class PlanteraDownedDrop : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info) => !info.IsInSimulation && NPC.downedPlantBoss;

		public bool CanShowItemDropInUI() => Main.hardMode;

		public string GetConditionDescription() => Language.GetTextValue("Mods.Waybound.DropRules.NoPlanteraRule");
	}

	public class DownedThemis : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info) => !info.IsInSimulation && DownedBossSystem.DownedThemis;

		public bool CanShowItemDropInUI() => true;

		public string GetConditionDescription() => Language.GetTextValue("Mods.Waybound.DropRules.NoThemisRule");
	}

	public class Halloween : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info) => !info.IsInSimulation && Main.halloween;

		public bool CanShowItemDropInUI() => true;

		public string GetConditionDescription() => Language.GetTextValue("Mods.Waybound.DropRules.HalloweenRule");
	}
}