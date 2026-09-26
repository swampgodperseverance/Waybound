    using Microsoft.Xna.Framework.Graphics;
    using ParticleLibrary.Core;
    using ParticleLibrary.Core.V3;
    using ParticleLibrary.Core.V3.Particles;
    using Terraria;
    using Terraria.ModLoader;

    namespace Waybound.Particles
    {
        public class ParticleSystem : ModSystem
        {
            public static ParticleBuffer<MegasparkParticle> MegasparkBuffer;
            public static ParticleBuffer<SnowFlakeParticle> SnowFlakeBuffer;
            public static ParticleBuffer<FlashParticle> FlashBuffer;
            public static ParticleBuffer<FlameParticle> FlameBuffer;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            // Megaspark
            MegasparkBuffer = new ParticleBuffer<MegasparkParticle>(512);
            MegasparkBuffer.SetBlendState(BlendState.Additive);
            ParticleManagerV3.RegisterUpdatable(MegasparkBuffer);
            ParticleManagerV3.RegisterRenderable(Layer.BeforeNPCs, MegasparkBuffer);

            // Snowflake
            SnowFlakeBuffer = new ParticleBuffer<SnowFlakeParticle>(256);
            SnowFlakeBuffer.SetBlendState(BlendState.AlphaBlend);
            ParticleManagerV3.RegisterUpdatable(SnowFlakeBuffer);
            ParticleManagerV3.RegisterRenderable(Layer.BeforeNPCs, SnowFlakeBuffer);

            // Flash
            FlashBuffer = new ParticleBuffer<FlashParticle>(128);
            FlashBuffer.SetBlendState(BlendState.Additive);
            ParticleManagerV3.RegisterUpdatable(FlashBuffer);
            ParticleManagerV3.RegisterRenderable(Layer.BeforeNPCs, FlashBuffer);

            FlameBuffer = new ParticleBuffer<FlameParticle>(256);
            FlameBuffer.SetBlendState(BlendState.Additive); // Делает пламя светящимся
            ParticleManagerV3.RegisterUpdatable(FlameBuffer);
            ParticleManagerV3.RegisterRenderable(Layer.BeforeNPCs, FlameBuffer);

            Mod.Logger.Info("=== Particle buffers SUCCESSFULLY REGISTERED ===");
        }
        public override void Unload()
            {
                MegasparkBuffer = null;
                SnowFlakeBuffer = null;
                FlashBuffer = null;
                FlameBuffer = null;
        }
        }
    }
