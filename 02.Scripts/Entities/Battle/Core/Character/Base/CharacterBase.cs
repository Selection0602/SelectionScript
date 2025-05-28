using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public abstract class CharacterBase : MonoBehaviour
{
    protected Image _characterImage;
    protected Transform _effectPos;
    private StatusBar _sanityBar;

    //스탯
    protected Sanity _sanity; // 정신력
    protected AttackPower _attackPower; //공격력
    protected StatusText _attackPowerText;
    private int _damageTaken; // 받는 피해량
    public float AttackPower => _attackPower.Current;
    public int AttackLevel => _attackPower.GetAttackLevel();
    public int Sanity => _sanity.CurrentSanity;
    public int DamageTaken => _damageTaken;
    public bool IsFullSanity => _sanity.IsFullSanity();

    public List<Debuff> Debuffs => _debuffs;
    private List<Debuff> _debuffs = new();
    private DebuffUI _debuffUI;

    private List<TurnEffect> _turnEffects = new();
    public bool IsDead { get; private set; }

    protected BattleManager _manager;
    protected EffectManager _effectManager;

    private void Start()
    {
        _manager = (SceneBase.Current as BattleSceneController).BattleManager;
        _effectManager = Manager.Instance.EffectManager;
    }

    public virtual void Initialize(BattleCharacterData data)
    {
        _attackPower = new AttackPower(data.BaseAttackPower, data.BounsAttackPower);
        _attackPowerText = GetComponentInChildren<StatusText>();
        _attackPower.OnChangedAttackPower += _attackPowerText.UpdateText;
        _attackPowerText.UpdateText(_attackPower.GetAttackLevel());

        _sanityBar = GetComponentInChildren<StatusBar>();
        _sanity = new Sanity(data.MaxSantity, data.CurrentSantity);
        _sanity.OnChangedCurrentSanity += _sanityBar.UpdateBar;
        _sanityBar.UpdateBar(data.MaxSantity, data.CurrentSantity);

        _debuffUI = GetComponentInChildren<DebuffUI>();
    }

    public virtual async void Heal(int amount)
    {
        _sanity.Heal(amount);
        Manager.Instance.SoundManager.PlaySFX(SFXType.SFX_Heal);
        await PlayDamageEffect(amount,true);
    }

    public virtual async Task Damage(int amount)
    {
        if (IsDead) return; //이미 사망한 상태라면 return

        int damage = _damageTaken + amount; //데미지 계산(받는 피해량 + 데미지)
        _sanity.Damage(damage);
        Manager.Instance.SoundManager.PlaySFX(SFXType.SFX_Damage);

        if (_sanity.CurrentSanity <= 0) //정신력이 0가 되면
        {
            await OnDeathEvent(); //사망 이벤트 호출
            return;
        }

        await PlayDamageEffects(damage); //데미지 연출 재생
    }

    private async Task OnDeathEvent()
    {
        IsDead = true;
        _sanityBar?.gameObject.SetActive(false);
        _debuffUI?.gameObject.SetActive(false);
        await Dead();
    }

    private async Task PlayDamageEffects(int damage)
    {
        if (_effectManager == null) return;
        await _effectManager.PlayEffect(EffectType.Damaged, _effectPos.position);
        await _effectManager.BlinkOnDamage(_characterImage);
        await PlayDamageEffect(damage);
    }

    public async Task PlayDamageEffect(int amount, bool isHeal = false)
    {
        try
        {
            await _effectManager.PlayDamageText(amount,
               _effectPos.position + new Vector3(0, 0.35f), isHeal);
        }
        catch (Exception e)
        {
            Debug.Log("effectManager가 null");
        }
    }

    public void IncreaseMaxSanity(int amount)
    {
        _sanity.IncreaseMaxSanity(amount);
    }

    public abstract Task Dead();

    public void IncreaseDamageTaken(int amount)
    {
        _damageTaken += amount;
        Debug.Log($"{this}의 받는 피해량 : {_damageTaken}");
    }

    public void DecreaseDamageTaken(int amount)
    {
        _damageTaken -= amount;
        Debug.Log($"{this}의 받는 피해량 : {_damageTaken}");
    }

    public bool WillDieFromDamage(int damage)
    {
        return _sanity.CurrentSanity - damage <= 0;
    }

    public void IncreaseAttackPower(int amount)
    {
        _attackPower.IncreaseAttackPower(amount);
        Debug.Log($"{this}의 공격력{amount}상승");
    }

    public void DecreaseAttackPower(int amount)
    {
        _attackPower.DecreaseAttackPower(amount);
        Debug.Log($"{this}의 공격력{amount}");
    }

    public void AddDebuff(Debuff debuff)
    {
        _debuffs.Add(debuff);
        _debuffUI.AddDebuffIcon(debuff);
    }

    public void AddTurnEffect(TurnEffect effect)
    {
        _turnEffects.Add(effect);
    }

    public void AllRemoveDebuff()
    {
        _debuffs.Clear();
        _debuffUI.RemoveAllDebuffIcon();
        _damageTaken = 0;
    }

    public async Task OnTurnEnd()
    {
        var turnEndTask = new TaskCompletionSource<bool>();
        for (int i = _turnEffects.Count - 1; i >= 0; i--)
        {
            var effect = _turnEffects[i];
            effect.EndTurnEffect();
            if (effect.IsExpired)
            {
                _turnEffects.RemoveAt(i);
            }
        }
        turnEndTask.SetResult(true);
        await turnEndTask.Task;
    }

    public async Task OnDebuff()
    {
        for (int i = _debuffs.Count - 1; i >= 0; i--)
        {
            var effect = _debuffs[i];
            await effect.Tick();
            if (effect.IsExpired)
            {
                _debuffs.RemoveAt(i);
                _debuffUI.RemoveDebuffIcon(effect);
            }
        }
    }

    public void OnTurnStart()
    {
        for (int i = _turnEffects.Count - 1; i >= 0; i--)
        {
            var effect = _turnEffects[i];
            effect.StartTurnEffect();
        }
    }
}

