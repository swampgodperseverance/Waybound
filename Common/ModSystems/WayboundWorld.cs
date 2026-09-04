using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader.IO;
using Waybound.Common.ModSystems.WorldGens;
using Waybound.Helpers;


namespace Waybound.Common.ModSystems {
    public class WayboundWorld : ModSystem {
        public static Dictionary<int, (int, int)> BannerType { get; private set; } = [];

        public static int SwampChestindex { get; set; } = -1;

        public static bool FirstEnterInSnowVillage { get; internal set; }
       // public static bool FirstEnterInHellVillage { get; internal set; }
        //public static bool SpawnDwarf { get; internal set; }

        //internal static bool sinlordDead = false;
        //public static bool SinlordDead { get { return sinlordDead; } private set { sinlordDead = value; } }


        internal static bool cruorDead = false;
        public static bool CruorDead { get { return cruorDead; } private set { cruorDead = value; } }

        //public static bool OpenChest { get; internal set; } = false;

       // public static bool SpawnCristal { get; set; }

        public override void ClearWorld() {
            SwampChestindex = -1;

            FirstEnterInSnowVillage = false;
          //  FirstEnterInHellVillage = false;
            //SpawnDwarf = false;
           // sinlordDead = false;

            cruorDead = false;
         //   SpawnCristal = false;
         //   OpenChest = false;
        }
        public override void OnWorldLoad() {
            SwampChestindex = -1;

            FirstEnterInSnowVillage = false;
           // FirstEnterInHellVillage = false;
           // SpawnDwarf = false;
          //  sinlordDead = false;

            cruorDead = false;
           // SpawnCristal = false;
         //   OpenChest = false;
        }
        public override void SaveWorldData(TagCompound tag) {
            tag["FirstEnterInSnowVillage"] = FirstEnterInSnowVillage;
            //tag["FirstEnterInHellVillage"] = FirstEnterInHellVillage;
            //tag["SpawnDwarf"] = SpawnDwarf;
            //tag["SinlordDead"] = sinlordDead;

            tag["CruorDeadDead"] = cruorDead;
         //   tag["OpenChest"] = OpenChest;
        }
        public override void LoadWorldData(TagCompound tag) {
            FirstEnterInSnowVillage = tag.GetBool("FirstEnterInSnowVillage");
            //FirstEnterInHellVillage = tag.GetBool("FirstEnterInHellVillage");
            //SpawnDwarf = tag.GetBool("SpawnDwarf");
            //sinlordDead = tag.GetBool("SinlordDead");

            cruorDead = tag.GetBool("CruorDeadDead");
           // OpenChest = tag.GetBool("OpenChest");
        }
        sealed public override void NetSend(BinaryWriter writer) {
            writer.Write(FirstEnterInSnowVillage);
            //writer.Write(FirstEnterInHellVillage);
            //writer.Write(SpawnDwarf);
            //writer.Write(SpawnCristal);
            //writer.Write(OpenChest);
        }
        sealed public override void NetReceive(BinaryReader reader) {
            FirstEnterInSnowVillage = reader.ReadBoolean();
            //FirstEnterInHellVillage = reader.ReadBoolean();
            //SpawnDwarf = reader.ReadBoolean();
            //SpawnCristal = reader.ReadBoolean();
            //OpenChest = reader.ReadBoolean();
        }
        public override void PostWorldGen() {
         //   WorldHelper.AddContainersLoot(13, 3, SkyChest, ItemType<Starcaller>());
            //WorldHelper.CleaningLiquid(HellVillageX - 220, HellVillageY - 115, HellVillageX - 57, HellVillageY - 67);
            //WorldHelper.CleaningLiquid(HellLakeX - 214, HellVillageY - 112, HellLakeX, HellVillageY - 80);
        }
        public override void PostUpdateWorld() {
            if (!WayboundGenVars.SnowVillageGen) { FirstEnterInSnowVillage = true; }
            //if (!HellVillageGen) { FirstEnterInHellVillage = true; }
            //if (NPC.downedGolemBoss && !DownedBossSystem.DownedSinlordBoss && !SpawnCristal) {
            //    SynegiaHelper.SpawnNPC((HellArenaPositionX - 198 + 110) * 16, (HellArenaPositionY - 28) * 16, NPCType<HellheartMonolith>());
            //    SpawnCristal = true;
            }
        }
    }

