using System;
using System.Threading.Tasks;

public class UseEnhanceCard : ICommand
{
    private EnhanceType _enhanceType;
    private ICardUser _user;
    private int[] _increases;

    public UseEnhanceCard(EnhanceType enhanceType, ICardUser user, int[] increases)
    {
        _enhanceType = enhanceType;
        _user = user;
        _increases = increases;
    }

    public async Task Execute()
    {
        switch (_enhanceType)
        {
            case EnhanceType.CardDamage:
                var attackCard = _user.Decks["Attack"];
                attackCard.ApplyValues[0] += _increases[0];
                attackCard.UpdateDesc();
                break;
            case EnhanceType.DrainLife:
                var drainLifeCard = _user.Decks["DrainLife"];
                drainLifeCard.ApplyValues[0] += _increases[0];
                drainLifeCard.ApplyValues[1] += _increases[1];
                drainLifeCard.UpdateDesc();
                break;
            case EnhanceType.AttackCount:
                var continuousAttackCard = _user.Decks["ContinuousAttack"];
                continuousAttackCard.ApplyValues[1] += _increases[0];
                continuousAttackCard.UpdateDesc();
                break;
        }
        foreach (var card in _user.HandDeck)
        {
            card.UpdateCardDesc();
        }
        await Task.CompletedTask;
    }

    public void Undo()
    {
    }
}
