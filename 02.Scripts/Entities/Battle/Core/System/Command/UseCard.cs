using System.Threading.Tasks;

public class UseCard : ICommand
{
    private CardEffectContext _cardEffect;

    public UseCard(CardEffectContext effect)
    {
        _cardEffect = effect;
    }

    public async Task Execute()
    {
        //카드 사용 가능한지 판단
        if (IsNullifiedByProphecy()) return;
        if (IsBlockedByControl()) return;
        await _cardEffect.CardData.ExecuteCardEffect(_cardEffect);
    }

    private bool IsNullifiedByProphecy()
    {
        //예언카드가 적중했다면
        if (_cardEffect.SingleTarget is IProphet prophet &&
            prophet.IsSuccessProphecy(_cardEffect.CardData))
        {
            //예언 적중한 카드는 덱에서 제외
            _cardEffect.User.DeckHandler.IgnoreCard(_cardEffect.CardData);
            prophet.PredictedCard = null;
            Manager.Instance.SoundManager.PlaySFX(SFXType.SFX_DestroyCard);
            return true;
        }
        return false;
    }

    private bool IsBlockedByControl()
    {
        //카드를 사용한 유저가 조종상태이고, 조종카드가 아닌 카드를 사용하면
        if (_cardEffect.User is IControllable controllable &&
            controllable.IsControlled &&
            _cardEffect.CardData.FileName != "Manipulation")
        {
            return true;
        }
        return false;
    }

    public void Undo()
    {

    }
}
