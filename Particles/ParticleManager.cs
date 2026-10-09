    using Microsoft.Xna.Framework.Graphics;
    using ParticleLibrary.Core;
    using ParticleLibrary.Core.V3;
    using ParticleLibrary.Core.V3.Particles;
    using Terraria;

    namespace Waybound.Particles;

public static class ParticleSystem
{
    public static ParticleBuffer<MegasparkParticle> MegasparkBuffer;
    public static ParticleBuffer<SnowFlakeParticle> SnowFlakeBuffer;
    public static ParticleBuffer<FlashParticle> FlashBuffer;
    public static ParticleBuffer<FlameParticle> FlameBuffer;
    public static ParticleBuffer<TrailParticle> TrailBuffer;
    public static ParticleBuffer<BubbleParticle> BubbleBuffer;
    public static ParticleBuffer<CrystalParticle> CrystalBuffer;
    public static ParticleBuffer<DeepGlowParticle> DeepGlowBuffer;
    public static ParticleBuffer<FireParticle> FireBuffer;
    public static ParticleBuffer<FlameBoomParticle> FlameBoomBuffer;
    public static void Load()
    {
        if (Main.dedServ) return;

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

        // Flame
        FlameBuffer = new ParticleBuffer<FlameParticle>(256);
        FlameBuffer.SetBlendState(BlendState.Additive);
        ParticleManagerV3.RegisterUpdatable(FlameBuffer);
        ParticleManagerV3.RegisterRenderable(Layer.BeforeNPCs, FlameBuffer);

        // Trail
        TrailBuffer = new ParticleBuffer<TrailParticle>(512);
        TrailBuffer.SetBlendState(BlendState.Additive);
        ParticleManagerV3.RegisterUpdatable(TrailBuffer);
        ParticleManagerV3.RegisterRenderable(Layer.BeforeNPCs, TrailBuffer);

        // Bubble
        BubbleBuffer = new ParticleBuffer<BubbleParticle>(256);
        BubbleBuffer.SetBlendState(BlendState.AlphaBlend);
        ParticleManagerV3.RegisterUpdatable(BubbleBuffer);
        ParticleManagerV3.RegisterRenderable(Layer.BeforeNPCs, BubbleBuffer);

        // Crystal
        CrystalBuffer = new ParticleBuffer<CrystalParticle>(512);
        CrystalBuffer.SetBlendState(BlendState.Additive);
        ParticleManagerV3.RegisterUpdatable(CrystalBuffer);
        ParticleManagerV3.RegisterRenderable(Layer.BeforeNPCs, CrystalBuffer);

        //DeepGlow
        DeepGlowBuffer = new ParticleBuffer<DeepGlowParticle>(1024);
        DeepGlowBuffer.SetBlendState(BlendState.Additive);
        ParticleManagerV3.RegisterUpdatable(DeepGlowBuffer);
        ParticleManagerV3.RegisterRenderable(Layer.BeforeNPCs, DeepGlowBuffer);

        //Fire
        FireBuffer = new ParticleBuffer<FireParticle>(512);
        FireBuffer.SetBlendState(BlendState.Additive);
        ParticleManagerV3.RegisterUpdatable(FireBuffer);
        ParticleManagerV3.RegisterRenderable(Layer.BeforeNPCs, FireBuffer);

        //FlameBoom
        FlameBoomBuffer = new ParticleBuffer<FlameBoomParticle>(64);
        FlameBoomBuffer.SetBlendState(BlendState.Additive);
        ParticleManagerV3.RegisterUpdatable(FlameBoomBuffer);
        ParticleManagerV3.RegisterRenderable(Layer.BeforeNPCs, FlameBoomBuffer);
    }

    public static void Unload()
    {
        MegasparkBuffer = null;
        SnowFlakeBuffer = null;
        FlashBuffer = null;
        FlameBuffer = null;
        TrailBuffer = null;
        BubbleBuffer = null;
        CrystalBuffer = null;
        DeepGlowBuffer = null;
        FireBuffer = null;
        FlameBoomBuffer = null;
    }
}