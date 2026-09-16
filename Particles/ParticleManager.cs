using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary.Core;
using ParticleLibrary.Core.V3;
using ParticleLibrary.Core.V3.Particles;
using Terraria;
using Terraria.ModLoader;
using Waybound.Particles;

namespace Waybound.Particles
{
    public class ParticleSystem : ModSystem
    {
        public static ParticleBuffer<MegasparkParticle> MegasparkBuffer;
        public static ParticleBuffer<IceSparkParticle> IceSparkBuffer;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            MegasparkBuffer = new ParticleBuffer<MegasparkParticle>(512);
            MegasparkBuffer.SetBlendState(BlendState.Additive);
            ParticleManagerV3.RegisterUpdatable(MegasparkBuffer);
            ParticleManagerV3.RegisterRenderable(Layer.BeforeNPCs, MegasparkBuffer);

            IceSparkBuffer = new ParticleBuffer<IceSparkParticle>(512);
            IceSparkBuffer.SetBlendState(BlendState.Additive);
            ParticleManagerV3.RegisterUpdatable(IceSparkBuffer);
            ParticleManagerV3.RegisterRenderable(Layer.BeforeNPCs, IceSparkBuffer);

            Mod.Logger.Info("=== Particle buffers SUCCESSFULLY REGISTERED ===");
        }

        public override void Unload()
        {
            MegasparkBuffer = null;
            IceSparkBuffer = null;
        }
    }
}