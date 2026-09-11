using Terraria;
using Terraria.UI;

namespace Waybound.Core;

public class PlayerCreationData(Player player) {
    public Player Target => player;

    public UIElement element = null;
    public UIElement middleContainer = null;
    public UIElement topContainer = null;
    public UIElement parent = null;

    public int race = 0;

    public bool openRaceUI;
};