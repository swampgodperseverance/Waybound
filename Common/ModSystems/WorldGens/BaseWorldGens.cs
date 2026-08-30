// Code by SerNik
using System.Collections.Generic;
using System.IO;
using Terraria.GameContent.Generation;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace Waybound.Common.ModSystems.WorldGens {
    public abstract class BaseWorldGens : ModSystem {
        public const byte a = 10, b = 11, c = 12, d = 13, e = 14, f = 15, g = 16, h = 17, i = 18, j = 19, k = 20, l = 21;
        public string Favorit = "Final Cleanup";

        public virtual int Index => 0;
        public virtual string SaveName => GetType().Name;
        public virtual string NameGen => "Error";
        public virtual string VanillaIndexName => "Final Cleanup";
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
            int index = tasks.FindIndex(x => x.Name == VanillaIndexName);
            if (index != -1) { tasks.Insert(index + Index, new PassLegacy(NameGen, (progress, config) => AddGen(progress))); }
        }
        void AddGen(GenerationProgress progress = null) {
            if (GensBool) return;
            bool Success = Do_MakeGen(progress);
            if (Success) GensBool = true;
        }
        public abstract bool GensBool { get; set; }
        public abstract bool Do_MakeGen(GenerationProgress progress);
        public override void OnWorldLoad() => GensBool = false;
        public override void SaveWorldData(TagCompound tag) => tag[SaveName] = GensBool;
        public override void LoadWorldData(TagCompound tag) => GensBool = tag.GetBool(SaveName);
        public override void NetSend(BinaryWriter writer) => writer.Write(GensBool);
        public override void NetReceive(BinaryReader reader) => GensBool = reader.ReadBoolean();
        public override void ClearWorld() => GensBool = false;
    }
}