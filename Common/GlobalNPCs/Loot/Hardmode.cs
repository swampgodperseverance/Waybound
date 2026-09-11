using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Waybound.Content.Items.Accessories.Hardmode;

namespace Waybound.Common.GlobalNPCs.Loot;

public class Hardmode : GlobalNPC {
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
        if (npc.type == NPCID.Vampire || npc.type == NPCID.VampireBat) {
            npcLoot.Add(ItemDropRule.Common(ItemType<BloodyNecklace>(), 10));
        }
    }
};