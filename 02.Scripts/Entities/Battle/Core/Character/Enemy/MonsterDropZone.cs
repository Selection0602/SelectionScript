using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MonsterDropZone : MonoBehaviour, IDropHandler
{
    public Action<Card, CharacterBase> OnDropCardEvent = delegate { };

    private BattleManager _battleManager;
    private CharacterBase _enemy;

    private Image _image;

    private void Awake()
    {
        _battleManager = (SceneBase.Current as BattleSceneController).BattleManager;
        _image = transform.parent.GetComponent<Image>();
    }

    public void Initialize(CharacterBase enemy)
    {
        _enemy = enemy;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag.TryGetComponent(out PlayerCard card))
        {
            _image.material = null;
            if (IsCanUseCard(card))
            {
                card.CardAnimation.IsUseCard = true;
                var enemies = new List<CharacterBase> { _enemy };
                if (card.Data.EffectRange == EffectRange.AllTarget)
                {
                    enemies = _battleManager.GetIsAliveEnemies();
                }
                _battleManager.Player.UseCard(enemies, card);
            }
            else
            {
                card.CardAnimation.IsUseCard = false;
            }
        }
    }

    private bool IsCanUseCard(PlayerCard card)
    {
        if (!card.IsCanUse)
        {
            return false;
        }
        if (card.Data.TargetType != TargetType.You)
        {
            return false;
        }
        if (_enemy is Ophanim ophanim && !ophanim.IsCanDamaged)
        {
            return false;
        }
        return true;
    }
}

