public enum SkillType
{
    Heal = 1,   // 회복
    AtkBuff,    // 공격력 버프
    Attack,     // 공격
    DrawCard    // 드로우
}

public enum TargetType
{
    Me,
    You,
}

public enum EffectRange
{
    NoneTarget,   // 타겟 없음
    SingleTarget, // 단일 타겟
    TwoTarget,    // 2개 타겟
    ThreeTarget,  // 3개 타겟
    AllTarget,    // 전체 타겟
}

public enum CardEffectType
{
    Attack = 1,          // 공격
    AllyAttack,          // 아군 공격
    Heal,                // 치유
    AllyHeal,            // 아군 치유
    AtkBuff,                // 버프
    AllyAtkBuff,            // 아군 버프
    DrawCard,            // 카드 뽑기
    LifeSteal,           // 생명력 흡수
    Debuff,              // 디버프
    EnemyKillConditionalAtkBuff,
    DamageConditionalBuff,
    RemoveDebuff,        // 디버프 제거
    IncreaseMaxCost,     // 최대 코스트 증가
    MultiAttack,         // 다중 공격
    ImmediateUse,        // 즉시 사용
    SummonAlly,          // 소환 동료
    AddCardToDeck,       // 덱에 카드 추가
    FirstTurnUse,        // 첫 턴 사용
    ReplaceCard,         // 카드 교체
    StealCard,           // 카드 훔치기
    Damaged,
    NormalAttackEnhance,
    LifeStealEnhance,
    MultiAttackEnhance,
    BurnDebuff,
    PoisonDebuff,
    WeakenDebuff,
    EnemyCardInvalid,
    TurnStartCard,
    
}


public enum CardRarityType
{
    Common = 1,          // 일반
    Rare,                // 레어
    Epic,                // 에픽
    Ophanim ,            // 오파님
    Demon,              // 데몬
    Legendary
}

public enum BuffType
{
    Power,
    Cost,
}

public enum DebuffType
{
    Burn = 1,       // 화상
    Poison,         // 독
    Weaken,          // 약화
    None,
}

public enum MonsterType
{
    Normal,
    Elite,
    Boss,
    Summon,
}

public enum NodeType
{
    None,
    Start,
    NormalBattle,
    EliteBattle,
    Rest,
    Trap,
    BossBattle,
    MiniGame_01,
    MiniGame_02,
    MiniGame_03,
    RandomEvent,
}

public enum PanelKey
{
    Title,
    Desc,
    Select,
    Camp,
    CharacterSelect,
    Credits
}

public enum GameState
{
    None,
    GameStart,
    PlayerTurnStart,
    PlayerPlay,
    PlayerTurnEnd,
    EnemyTurnStart,
    EnemyPlay,
    EnemyTurnEnd,
    GameClear,
    GameOver
}

public enum EnhanceType
{
    CardDamage,
    DrainLife,
    AttackCount,
}

public enum CharacterType
{
    Maria,
    Elios,
    Ophanim,
    Demon,
    Ally
}

public enum AssetType
{
    ValueSO,
    SkillSO,
    CardSO,
    MonsterSO,
    Prefab,
    Texture2D,
    EndingSO,
    _NodeTypeDataSO,
    CharacterSO,
    AudioClip,
    MiniGameDataSO,
    DebuffSO,
    MemorySO,
    RewardSO,
}

public enum RewardType
{
    Immediately,
    Booty,
    Card,
    Memory
}

public enum PanelState
{
    None,
    Memory,
    Card
}
public enum NodeEventType
{
    None,
    Battle,
    Shop,
    Event,
    Boss
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right,
    None
}

public enum RoomType
{
    Corridor,
    Library,
    Piano,
    Porch
}