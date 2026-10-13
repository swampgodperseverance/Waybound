using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Waybound.Content.Items.Accessories.Hardmode;
using Waybound.Content.Items.Weapons.Ranged.Guns.PreHM;

namespace Waybound.Common.GlobalNPCs
{
    public class ArmsDealerNPC : GlobalNPC
    {
        private static readonly Condition DownedMoonLord = new Condition(
            Language.GetOrRegister("Mods.Waybound.Conditions.DownedMoonLord", () => "After defeating the Moon Lord"),
            () => NPC.downedMoonlord);

        public override void ModifyShop(NPCShop shop)
        {
            if (shop.NpcType != NPCID.ArmsDealer)
                return;

            shop.Add<MauserC96>(Condition.DownedEyeOfCthulhu);
            shop.Add<M16>(Condition.DownedSkeletron);
            shop.Add<M249>(Condition.Hardmode);
            shop.Add<ExtendedMagazine>(Condition.DownedMechBossAny);
            shop.Add<SVD>(DownedMoonLord);
        }
    }
}