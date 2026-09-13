using ReLogic.Content;
using Terraria;
using Terraria.UI;
using Waybound.Content.Race;

namespace Waybound.Core;

public class PlayerCreationData(Player player) {
    public Player Target => player;
    public Asset<Texture2D> asset = null;

    public UIElement raceConfirmUI = null;
    public UIElement element = null;
    public UIElement middleContainer = null;
    public UIElement topContainer = null;
    public UIElement parent = null;

    public RaceInfo race = null;

    public bool openRaceUI = false;
};