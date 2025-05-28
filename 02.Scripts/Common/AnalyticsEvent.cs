// Analytics 이벤트 이름  
public static class EventName
{
    /// <summary>  
    /// 선택한 캐릭터 (엘리오스/마리아) - character_name  
    /// </summary>  
    public const string CHARACTER_SELECTED = "character_selected";

    /// <summary>  
    /// 시작화면에서 함정 파훼, 엔딩 관람 등 - category_name  
    /// </summary>  
    public const string MAIN_MENU_FEATURE = "main_menu_feature";

    /// <summary>  
    /// 게임 진행 중 뒤로가기 버튼 누른 횟수 - scene_name  
    /// </summary>  
    public const string BACK_BUTTON_CLICKED = "back_button_clicked";

    /// <summary>  
    /// 특수 노드 사용 횟수 - ex_node_type_name
    /// </summary>  
    public const string USED_EXTRA_NODE = "used_extra_node";

    /// <summary>  
    /// 플레이어가 사망한 스테이지 - stage_number  
    /// </summary>  
    public const string PLAYER_DEATH = "player_death";

    /// <summary>  
    /// 사망 시 배틀했던 몬스터 종류 - monster_list  
    /// </summary>  
    public const string DEATH_BATTLE_MONSTERS = "death_battle_monsters";

    /// <summary>  
    /// 배틀에서 사용한 카드 이름 - card_name  
    /// </summary>  
    public const string CARD_USED = "card_used";

    /// <summary>  
    /// 드로우된 카드 이름 - card_name  
    /// </summary>  
    public const string CARD_DRAWN = "card_drawn";

    /// <summary>  
    /// 전투 타입(일반/엘리트/보스) - battle_type  
    /// 소요 시간(초) - duration_seconds  
    /// </summary>  
    public const string BATTLE_COMPLETED_TIME = "battle_completed_time";

    /// <summary>  
    /// 사용한 고유스킬 이름 - skill_name  
    /// </summary>  
    public const string SKILL_USED = "skill_used";

    /// <summary>  
    /// 턴 종료 시 남은 카드 수 - remaining_cards  
    /// </summary>  
    public const string TURN_ENDED = "turn_ended";

    /// <summary>  
    /// 휴식에서 선택한 옵션 - option_name  
    /// </summary>  
    public const string REST_SELECTED = "rest_selected";

    /// <summary>  
    /// 선택한 보상 이름 - reward_name  
    /// </summary>  
    public const string NORMAL_REWARD_SELECTED = "normal_reward_selected";
    public const string ELITE_REWARD_SELECTED = "elite_reward_selected";

    /// <summary>  
    /// 게임 클리어 횟수 - 없음  
    /// </summary>  
    public const string GAME_CLEARED = "game_cleared";

    /// <summary>  
    /// 미니게임 이름/번호 - minigame_name  
    /// 소요 시간(초) - duration_seconds  
    /// </summary>  
    public const string MINIGAME_PLAYED = "minigame_played";
}

// Analytics 이벤트 파라미터  
public static class EventParam
{
    /// <summary>  
    /// character_selected - 선택한 캐릭터 (엘리오스/마리아)  
    /// </summary>  
    public const string CHARACTER_NAME = "character_name";

    /// <summary>  
    /// main_menu_feature - 시작화면에서 함정 파훼, 엔딩 관람 등  
    /// </summary>  
    public const string CATEGORY_NAME = "category_name";

    /// <summary>  
    /// back_button_pressed - 게임 진행 중 뒤로가기 버튼 누른 횟수  
    /// </summary>
    public const string SCENE_NAME = "scene_name";

    /// <summary>
    /// used_extra_node - 특수 노드 사용 횟수
    /// </summary>
    public const string EX_NODE_TYPE_NAME = "ex_node_type_name";

    /// <summary>  
    /// player_death - 플레이어가 사망한 스테이지  
    /// </summary>  
    public const string STAGE_NUMBER = "stage_number";

    /// <summary>  
    /// death_battle_monsters - 사망 시 배틀했던 몬스터 종류  
    /// </summary>  
    public const string MONSTER_LIST = "monster_list";

    /// <summary>  
    /// card_used, card_drawn - 배틀에서 사용한 카드 이름, 드로우된 카드 이름  
    /// </summary>  
    public const string CARD_NAME = "card_name";

    /// <summary>  
    /// battle_completed - 전투 타입(일반/엘리트/보스)  
    /// </summary>  
    public const string BATTLE_TYPE = "battle_type";

    /// <summary>  
    /// battle_completed, minigame_played - 소요 시간(초)  
    /// </summary>  
    public const string CLEAR_TIME = "duration_seconds";

    /// <summary>  
    /// unique_skill_used - 사용한 고유스킬 이름  
    /// </summary>  
    public const string SKILL_NAME = "skill_name";

    /// <summary>  
    /// turn_ended - 턴 종료 시 남은 카드 수  
    /// </summary>  
    public const string REMAINING_CARDS = "remaining_cards";

    /// <summary>  
    /// rest_option_selected - 휴식에서 선택한 옵션  
    /// </summary>  
    public const string SELECT_OPTION = "select_option";

    /// <summary>  
    /// normal_reward_selected, elite_reward_selected - 선택한 보상 이름
    /// </summary>  
    public const string REWARD_NAME = "reward_name";

    public const string IS_CLEARED = "is_cleared";

    /// <summary>  
    /// minigame_played - 미니게임 이름/번호  
    /// </summary>  
    public const string MINIGAME_NAME = "minigame_name";

    public const string NODE_TYPE = "node_type";
}
