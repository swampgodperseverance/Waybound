using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
using Waybound.Common.WUtils;

namespace Waybound.Common.ItemDropRules;

public class BossHitPlayer : ModPlayer
{
    public int BossHits;

    public override void OnHurt(Player.HurtInfo info)
    {
        if (BossHitGlobalNPC.AnyBossAlive())
            BossHits++;
    }
}

public class BossHitGlobalNPC : GlobalNPC
{
    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (!npc.boss || AnyBossAlive(npc.whoAmI))
            return;

        foreach (Player p in Main.ActivePlayers)
            p.GetModPlayer<BossHitPlayer>().BossHits = 0;
    }

    public static bool AnyBossAlive(int ignoreWho = -1)
    {
        foreach (NPC n in Main.ActiveNPCs)
            if (n.boss && n.whoAmI != ignoreWho)
                return true;
        return false;
    }
}

public class NoHitBonusDrop : IItemDropRuleCondition
{
    public const float NoHitChance = 0.50f;
    public const float FirstHitChance = 0.10f;
    public const float PerHitPenalty = 0.01f;

    public static float GetChance(int hits)
    {
        if (hits <= 0)
            return NoHitChance;
        return System.Math.Max(0f, FirstHitChance - (hits - 1) * PerHitPenalty);
    }

    public bool CanDrop(DropAttemptInfo info)
    {
        if (info.IsInSimulation || info.player is null)
            return false;

        int hits = info.player.GetModPlayer<BossHitPlayer>().BossHits;
        return info.rng.NextFloat() < GetChance(hits);
    }

    public bool CanShowItemDropInUI() => true;

    public string GetConditionDescription() => Loc.GetCond("NoHitBonusRule");
}