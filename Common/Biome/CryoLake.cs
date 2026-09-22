using Terraria;
using Waybound.Common.ModSystems.WorldGens;
using Waybound.Helpers;

namespace Waybound.Common.Biome;

public class CryoLake : ModBiome {
    public override bool IsBiomeActive(Player player) {
        for (int i = 0; i < WayboundGenVars.CryoSpringPos.Count; i++) {
            GenVector gen = WayboundGenVars.CryoSpringPos[i];

            float x2 = gen.End.X - gen.Start.X;
            float y2 = gen.End.Y - gen.Start.Y;

            bool flag = WorldHelper.CheckBiome(player, (int)x2, (int)y2, (int)gen.Start.X, (int)gen.Start.Y);
            if (flag) { return true; };
        }
        return false;
    }
    public override void OnEnter(Player player) {
        Main.NewText("[c/ff0000:System.ArgumentNullException]");
    }
};
