namespace Waybound.ModCompat {
    public abstract class ModSupportsSystem : ModSystem {
        public Mod Target => TargetMod();

        public abstract Mod TargetMod();
        public virtual void PostSetupContent(Mod mod) { }
        public virtual void Load(Mod mod) { }
        public sealed override bool IsLoadingEnabled(Mod mod) => TargetMod() is not null;
        public sealed override void PostSetupContent() => PostSetupContent(TargetMod());
        public sealed override void Load() => Load(TargetMod());
    }
}