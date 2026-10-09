using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;

namespace Waybound.Particles;

public class TrailParticle : Behavior<ParticleInfo>
{
    public override string Texture => "Waybound/Assets/Textures/Ring";

    public override void Initialize(ref ParticleInfo info)
    {
        
    }

    public override void Update(ref ParticleInfo info)
    {
        float progress = info.Time / (float)info.Duration;

        info.Color = info.InitialColor * progress;
        info.Scale = info.InitialScale * progress;

        info.Time--;
    }
}