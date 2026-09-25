using Microsoft.Xna.Framework;
using ParticleLibrary;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Particles;
using SysVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Tiles.Blocks {
  
        public class HellmutBrickTile : ModTile
        {
            public override void SetStaticDefaults()
            {
                Main.tileSolid[(int)base.Type] = true;
                Main.tileBrick[(int)base.Type] = true;
                Main.tileMergeDirt[(int)base.Type] = true;
                Main.tileBlockLight[(int)base.Type] = true;
                Main.tileLighted[(int)base.Type] = true;
                base.DustType = 90;
                base.HitSound = new SoundStyle?(SoundID.Tink);
                base.AddMapEntry(new Color(255, 180, 180), null);
            }

            // Token: 0x060079AF RID: 31151 RVA: 0x002FA051 File Offset: 0x002F8251
            public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
            {
                r = 0.55f;
                g = 0.37f;
                b = 0.4f;
            }

            // Token: 0x060079B0 RID: 31152 RVA: 0x002E60E2 File Offset: 0x002E42E2
            public override void NumDust(int i, int j, bool fail, ref int num)
            {
                num = (fail ? 1 : 3);
            }
        }
    
}   
