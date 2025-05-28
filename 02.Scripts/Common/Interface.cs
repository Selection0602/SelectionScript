using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICommand
{
    public Task Execute();
    public void Undo();  //행동 취소
}

public interface ICharacter<T>
{
    public void Setup(T data);
}

public interface ICardUser
{
    public CharacterBase Character { get; }
    public DeckHandler DeckHandler { get; }
    public List<Card> HandDeck { get; }
    public Dictionary<string, CardData> Decks { get; }
    public Cost Cost { get; set; }
    public Task UseCard();
}

public interface ICardDrawer
{
    public bool IsFullDeck();
    public Task DrawCard(int count, bool isFirstTurn); //카드 뽑기
}

public interface ICardDistributor
{
    public void SetDeck(DeckHandler handler, int cardIndex); // 덱 설정 
    public void DistributeCards(Enemy enemy); //카드 분배
}

public interface IProphet //예언자
{
    public void Prophecy(ICardUser target); //예언
    public void MissedProphecy(); //예언 빗나감
    public void SucceededProphecy(); //예언 적중
    public bool IsSuccessProphecy(CardData data) => data == PredictedCard;
    public CardData PredictedCard { get; set; } //예언한 카드
}

public interface IControllable //조종당하는 자
{
    public bool IsControlled { get; }
}

public interface ISummoner //소환자
{
    public void Summon();                 // 기본 소환
    public void DeathSummon(Summon valkyrie);            // 소환수 사망
    public int GetSummonBestAttackPower(); //소환수중에 가장 높은 공격력 반환
    public Summon GetlowSanitySummon(); //정신력이 가장 낮은 소환수 반환
    public Summon GetlowAttackPowerSummon(); //정신력이 가장 낮은 소환수 반환
    List<Summon> Summons { get; }    // 현재 소환한 유닛들
    public bool IsCanSummon { get; }
}

public interface IUniqueSkillUser
{
    public SkillSO SkillData { get; }
    public bool IsCanUseSkill { get; set; }
    public Task UseSkill();
}

public interface ICardUseCondition
{
    public bool CanUse(Card card, CharacterBase chara = null, int remainCost = 0);
}