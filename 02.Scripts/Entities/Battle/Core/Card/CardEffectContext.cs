using System.Collections.Generic;

public class CardEffectContext
{
    public Card Card { get; set; }
    public CardData CardData { get; set; }
    public ICardUser User { get; set; }
    public ICardDrawer Drawer { get; set; }
    public List<CharacterBase> Targets { get; set; }
    public CharacterBase SingleTarget => Targets[0];
    public int ExecuteCount { get; set; }
    public int Damage { get; set; }
}
