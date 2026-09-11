using Terraria.UI;

namespace Waybound.UIs;

public class Race : UIElement {
    public override void OnInitialize() {

    }
    public override bool ContainsPoint(Vector2 point) {
        return false;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        CalculatedStyle dimensions = GetDimensions();

        Vector2 vector2 = new Vector2(dimensions.X, dimensions.Y);
    }
};