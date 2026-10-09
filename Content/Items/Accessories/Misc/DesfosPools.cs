using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Waybound.Common.WUtils;
using Waybound.Content.Items.Accessories.Hardmode;

namespace Waybound.Content.Items.Accessories.Misc
{
    //  NEW POOL guide
    //
    //  Add(ItemID.StoneBlock, 150)                      // What do we put in the bag and how much
    //      .Common  (WEIGHT, MIN AMOUNT, MAX AMOUNT, ID, ID, ID etc...)       // Common      58%
    //      .Uncommon(WEIGHT, MIN AMOUNT, MAX AMOUNT, ID, ID, ID etc...)       // Uncommon    28%
    //      .Rare    (WEIGHT, MIN AMOUNT, MAX AMOUNT, ID, ID, ID etc...)       // rare        11% There's a guarant 
    //      .Jackpot (WEIGHT, MIN AMOUNT, MAX AMOUNT, ID, ID, ID etc...)       // JACKPOT     3%  also a guarant
    //      .When(DesfosCond.Hardmode)                   // (NOT IMPORTANT) Conditions
    //      .Once()                                      // (NOT IMPORTANT) It drops only once in the whole game. Use this wisely
    //                                                   //         
    //      .Pity                                        // (NOT IMPORTANT) your own guarant chance
    //
    //  random id is chosen
    //
    //  Conditions can be found in DesfosCond (Hardmode, AfterMechBoss, AfterPlantera, ...).
    //  Conditions, almost every:  .When(p => p.ZoneSnow && Main.hardMode)
    //  Completely removes old drop from the pool with the progress:  .When(DesfosCond.Not(DesfosCond.Hardmode))

    public enum DesfosTier { Common = 0, Uncommon = 1, Rare = 2, Jackpot = 3 }

    public class DesfosEntry
    {
        public int Weight;
        public int Min;
        public int Max;
        public int[] Types;
        public bool Once;
        public Func<Player, bool> Condition; // null = always available
        public string Key => ItemID.Search.GetName(Types[0]);
        public bool IsAvailable(DesfosBagPlayer bag)
        {
            if (Once && bag.obtained.Contains(Key)) { return false; }
            return Condition == null || Condition(bag.Player);
        }
    }

    /// the place where conditions should be added
    public static class DesfosCond
    {
        public static bool Hardmode(Player p) => Main.hardMode;
        public static bool AfterEvilBoss(Player p) => NPC.downedBoss2;       // evil boss
        public static bool AfterSkeletron(Player p) => NPC.downedBoss3;
        public static bool AfterMechBoss(Player p) => NPC.downedMechBossAny;
        public static bool AfterAllMechs(Player p) => NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3;
        public static bool AfterPlantera(Player p) => NPC.downedPlantBoss;
        public static bool AfterGolem(Player p) => NPC.downedGolemBoss;
        public static bool AfterMoonLord(Player p) => NPC.downedMoonlord;

        public static Func<Player, bool> Not(Func<Player, bool> cond) => p => !cond(p);
    }

    public class DesfosPool
    {
        public readonly int InputType;
        public readonly int Required;
        public readonly List<DesfosEntry>[] Tiers = { new(), new(), new(), new() };

        // after SP, without good drops chance goes upwards
        // after HP next reward 100% rare or better
        public int SoftPity = 10;
        public int HardPity = 20;
        public int JackpotPity = 40;

        DesfosEntry _last;

        public DesfosPool(int input, int required)
        {
            InputType = input;
            Required = required;
        }

        public string Key => ItemID.Search.GetName(InputType);

        public DesfosPool Common(int weight, int min, int max, params int[] types) => AddEntry(DesfosTier.Common, weight, min, max, types);
        public DesfosPool Uncommon(int weight, int min, int max, params int[] types) => AddEntry(DesfosTier.Uncommon, weight, min, max, types);
        public DesfosPool Rare(int weight, int min, int max, params int[] types) => AddEntry(DesfosTier.Rare, weight, min, max, types);
        public DesfosPool Jackpot(int weight, int min, int max, params int[] types) => AddEntry(DesfosTier.Jackpot, weight, min, max, types);

        public DesfosPool Once()
        {
            if (_last != null) { _last.Once = true; }
            return this;
        }

        public DesfosPool When(Func<Player, bool> condition)
        {
            if (_last != null) { _last.Condition = condition; }
            return this;
        }

        public DesfosPool Pity(int soft, int hard, int jackpot = 40)
        {
            SoftPity = soft;
            HardPity = hard;
            JackpotPity = jackpot;
            return this;
        }

        DesfosPool AddEntry(DesfosTier tier, int weight, int min, int max, int[] types)
        {
            _last = new DesfosEntry { Weight = Math.Max(1, weight), Min = min, Max = Math.Max(min, max), Types = types };
            Tiers[(int)tier].Add(_last);
            return this;
        }

        bool HasAvailable(DesfosTier tier, DesfosBagPlayer bag)
        {
            foreach (DesfosEntry e in Tiers[(int)tier])
            {
                if (e.IsAvailable(bag)) { return true; }
            }
            return false;
        }
        public bool CanRoll(DesfosBagPlayer bag)
        {
            for (int t = 0; t < Tiers.Length; t++)
            {
                if (HasAvailable((DesfosTier)t, bag)) { return true; }
            }
            return false;
        }
        public List<DesfosEntry> AvailableEntries(DesfosTier tier, DesfosBagPlayer bag)
            => Tiers[(int)tier].Where(e => e.IsAvailable(bag)).ToList();

        static int Smooth(int min, int max)
        {
            if (max <= min) { return min; }
            return (Main.rand.Next(min, max + 1) + Main.rand.Next(min, max + 1) + 1) / 2;
        }

        public Item Roll(DesfosBagPlayer bag, out DesfosTier tier)
        {
            bag.pityRare.TryGetValue(Key, out int missRare);
            bag.pityJackpot.TryGetValue(Key, out int missJackpot);

            float cJackpot = 3f, cRare = 11f, cUncommon = 28f;
            if (missRare >= SoftPity) { cRare += (missRare - SoftPity + 1) * 8f; }

            if (missJackpot >= JackpotPity && HasAvailable(DesfosTier.Jackpot, bag))
            {
                tier = DesfosTier.Jackpot;
            }
            else if (missRare >= HardPity && (HasAvailable(DesfosTier.Rare, bag) || HasAvailable(DesfosTier.Jackpot, bag)))
            {
                tier = Main.rand.NextFloat() < 0.2f ? DesfosTier.Jackpot : DesfosTier.Rare; 
            }
            else
            {
                float roll = Main.rand.NextFloat() * 100f;
                if (roll < cJackpot) { tier = DesfosTier.Jackpot; }
                else if (roll < cJackpot + cRare) { tier = DesfosTier.Rare; }
                else if (roll < cJackpot + cRare + cUncommon) { tier = DesfosTier.Uncommon; }
                else { tier = DesfosTier.Common; }
            }

            while (tier > DesfosTier.Common && !HasAvailable(tier, bag)) { tier--; }
            while (tier < DesfosTier.Jackpot && !HasAvailable(tier, bag)) { tier++; }
            List<DesfosEntry> candidates = AvailableEntries(tier, bag);
            Item reward = new Item();
            reward.TurnToAir(true);
            if (candidates.Count == 0) { return reward; }

            int total = candidates.Sum(e => e.Weight);
            int pick = Main.rand.Next(total);
            DesfosEntry chosen = candidates[0];
            foreach (DesfosEntry e in candidates)
            {
                pick -= e.Weight;
                if (pick < 0) { chosen = e; break; }
            }

            int type = chosen.Types[Main.rand.Next(chosen.Types.Length)];
            reward.SetDefaults(type);
            reward.stack = reward.maxStack > 1 ? Smooth(chosen.Min, chosen.Max) : 1;
            bag.pityRare[Key] = tier >= DesfosTier.Rare ? 0 : missRare + 1;
            bag.pityJackpot[Key] = tier == DesfosTier.Jackpot ? 0 : missJackpot + 1;
            if (chosen.Once) { bag.obtained.Add(chosen.Key); }

            return reward;
        }
    }

    public static class DesfosPools
    {
        static Dictionary<int, DesfosPool> _pools;

        public static bool TryGet(int type, out DesfosPool pool)
        {
            EnsureInit();
            return _pools.TryGetValue(type, out pool);
        }

        public static void Reset() => _pools = null;

        public static Color TierColor(DesfosTier tier) => tier switch
        {
            DesfosTier.Uncommon => new Color(150, 255, 170),
            DesfosTier.Rare => new Color(170, 140, 255),
            DesfosTier.Jackpot => new Color(255, 110, 200),
            _ => new Color(255, 225, 150),
        };

        static void EnsureInit()
        {
            if (_pools != null) { return; }
            _pools = new Dictionary<int, DesfosPool>();
            RegisterAll();
        }

        static DesfosPool Add(int input, int required)
        {
            var pool = new DesfosPool(input, required);
            _pools[input] = pool;
            return pool;
        }
        static void Smelt(int ore, int required, int bar, int count) => Add(ore, required).Common(1, count, count, bar);
        //method for ores

        static void RegisterAll()
        {
            // stone
            Add(ItemID.StoneBlock, 150)
                .Common(40, 20, 40, ItemID.CopperOre, ItemID.TinOre, ItemID.IronOre, ItemID.LeadOre)
                .Common(20, 12, 25, ItemID.SilverOre, ItemID.TungstenOre)
                .Uncommon(15, 10, 20, ItemID.GoldOre, ItemID.PlatinumOre)
                .Uncommon(20, 3, 6, ItemID.Amethyst, ItemID.Topaz, ItemID.Sapphire)
                .Rare(10, 3, 5, ItemID.Emerald, ItemID.Ruby)
                .Rare(6, 1, 1, ItemID.MiningHelmet, ItemID.Shackle, ItemID.EnchantedBoomerang)
                .Jackpot(3, 2, 4, ItemID.Diamond)
                .Jackpot(2, 1, 1, ItemID.EnchantedSword);
            //cactus
            Add(ItemID.Cactus, 60)
                .Common(40, 20, 40, ItemID.SandBlock)
                .Uncommon(20, 1, 1, ItemID.CactusHelmet, ItemID.CactusBreastplate, ItemID.CactusLeggings)
                .Rare(10, 1, 1, ItemID.CactusSword, ItemID.CactusPickaxe);


            // here i think we should go without randomness
            Smelt(ItemID.CopperOre, 15, ItemID.CopperBar, 5);
            Smelt(ItemID.IronOre, 15, ItemID.IronBar, 5);
            Smelt(ItemID.GoldOre, 15, ItemID.GoldBar, 5);
        }
    }

    public class DesfosPoolTooltips : GlobalItem
    {
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (Main.gameMenu || Main.LocalPlayer == null || item.IsAir)
                return;

            if (!DesfosPools.TryGet(item.type, out DesfosPool pool))
                return;

            DesfosBagPlayer bag = Main.LocalPlayer.GetModPlayer<DesfosBagPlayer>();
            if (!bag.equippedBag && !bag.bagActive)
                return;

            // Progress
            tooltips.Add(new TooltipLine(Mod, "DesfosBag0",
                string.Format(Loc.GetTips("DesfosBag.Progress"),
                    Math.Min(item.stack, pool.Required),
                    pool.Required))
            {
                OverrideColor = DesfosPools.TierColor(DesfosTier.Common)
            });

            // Rare list
            var rares = pool.Tiers[(int)DesfosTier.Rare]
                .Concat(pool.Tiers[(int)DesfosTier.Jackpot])
                .SelectMany(e => e.Types)
                .Distinct()
                .Take(6)
                .Select(t => Lang.GetItemNameValue(t))
                .ToList();

            if (rares.Count > 0)
            {
                tooltips.Add(new TooltipLine(Mod, "DesfosBag1",
                    string.Format(Loc.GetTips("DesfosBag.Rare"),
                        string.Join(", ", rares)))
                {
                    OverrideColor = DesfosPools.TierColor(DesfosTier.Rare)
                });
            }

            // Pity
            bag.pityRare.TryGetValue(pool.Key, out int miss);
            int left = pool.HardPity - miss;

            string pity = left <= 0
                ? Loc.GetTips("DesfosBag.PityReady")
                : string.Format(Loc.GetTips("DesfosBag.PityLeft"),
                    left,
                    left == 1 ? "" : "s");

            tooltips.Add(new TooltipLine(Mod, "DesfosBag2", pity)
            {
                OverrideColor = new Color(190, 190, 190)
            });
        }
    }

    public class DesfosPoolsSystem : ModSystem
    {
        public override void Unload() => DesfosPools.Reset();
    }
}