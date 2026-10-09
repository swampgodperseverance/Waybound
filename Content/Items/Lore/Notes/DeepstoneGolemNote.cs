using Terraria;
using Terraria.ID;
using Waybound.Common.WUtils;

namespace Waybound.Content.Items.Lore.Notes
{
//just copy it
    public class DeepstoneGolemNote : LoreNote
    {
        public override string NoteTitle => Loc.GetLore("DeepstoneGolemNote.Title");
        public override string Speaker => Loc.GetLore("DeepstoneGolemNote.Speaker");

        public override string[] Lines => new[]
        {
            Loc.GetLore("DeepstoneGolemNote.Lines.0"),
            Loc.GetLore("DeepstoneGolemNote.Lines.1"),
            Loc.GetLore("DeepstoneGolemNote.Lines.2"),
            Loc.GetLore("DeepstoneGolemNote.Lines.3"),
            Loc.GetLore("DeepstoneGolemNote.Lines.4"),
            Loc.GetLore("DeepstoneGolemNote.Lines.5"),
            Loc.GetLore("DeepstoneGolemNote.Lines.6"),
        };

        public override void OnFirstRead(Player player)
        {
            player.QuickSpawnItem(player.GetSource_Misc("LoreNote"), ItemID.GoldCoin, 3);
        }

        // Optional
        // public override void SetDefaults()
        // {
        //     base.SetDefaults();
        //     Item.rare = ItemRarityID.Green;
        // }



    }
}