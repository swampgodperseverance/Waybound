namespace Waybound.Common.HeartStyles;

public abstract class HeartStyle : ILoadable {
    public virtual string Name => GetType().Name;
    public virtual string Text => null;

    public bool DrawCustomText => Text != null;
    public virtual bool Flip => false;
    public virtual bool Reaplece => true;
    public virtual bool HasAsset => true;

    public abstract int Count(Terraria.Player player);
    public abstract int Priority(Terraria.Player player); 
    public abstract bool Active(Terraria.Player player, string acyiveStyleName);
    public virtual void Draw(SpriteBatch sB, ref ReLogic.Content.Asset<Texture2D> texture, Vector2 position, ref Color color, float scale) { }

    public void Load(Mod mod) {
        if (HasAsset) { Resources.Textures.RegisterHeart(mod, Name); };
        Core.CustomClassData.Heart.Add(this); 
    }
    public void Unload() { }
};