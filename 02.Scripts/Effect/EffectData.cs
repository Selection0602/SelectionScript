using System.Collections.Generic;
using UnityEngine;

// 임시 이펙트 데이터
public enum EffectType
{
    Damaged,
    
    #region 임시 이펙트
    Attack_01,
    Attack_02,
    Attack_03,
    Attack_04,
    Attack_05,
    Attack_06,
    Attack_07,
    Attack_08,
    Attack_09,
    Attack_10,
    Attack_11,
    Attack_12,
    Attack_13,
    Attack_14,
    Attack_15,
    Attack_16,
    Attack_17,
    Attack_18,
    Attack_19,
    Attack_20,
    Attack_21,
    Attack_23,
    Attack_24,
    Attack_25,
    Attack_26,
    Attack_27,
    Attack_28,
    Attack_29,
    #endregion
}

[CreateAssetMenu(fileName = "EffectData", menuName = "Effect/EffectData")]
public class EffectData : ScriptableObject
{
    public Dictionary<EffectType, int> EffectDataDict = new Dictionary<EffectType, int>()
    {
        { EffectType.Damaged, Animator.StringToHash("Damaged") },
        
        #region 임시 이펙트
        { EffectType.Attack_01, Animator.StringToHash("Attack_01") },
        { EffectType.Attack_02, Animator.StringToHash("Attack_02") },
        { EffectType.Attack_03, Animator.StringToHash("Attack_03") },
        { EffectType.Attack_04, Animator.StringToHash("Attack_04") },
        { EffectType.Attack_05, Animator.StringToHash("Attack_05") },
        { EffectType.Attack_06, Animator.StringToHash("Attack_06") },
        { EffectType.Attack_07, Animator.StringToHash("Attack_07") },
        { EffectType.Attack_08, Animator.StringToHash("Attack_08") },
        { EffectType.Attack_09, Animator.StringToHash("Attack_09") },
        { EffectType.Attack_10, Animator.StringToHash("Attack_10") },
        { EffectType.Attack_11, Animator.StringToHash("Attack_11") },
        { EffectType.Attack_12, Animator.StringToHash("Attack_12") },
        { EffectType.Attack_13, Animator.StringToHash("Attack_13") },
        { EffectType.Attack_14, Animator.StringToHash("Attack_14") },
        { EffectType.Attack_15, Animator.StringToHash("Attack_15") },
        { EffectType.Attack_16, Animator.StringToHash("Attack_16") },
        { EffectType.Attack_17, Animator.StringToHash("Attack_17") },
        { EffectType.Attack_18, Animator.StringToHash("Attack_18") },
        { EffectType.Attack_19, Animator.StringToHash("Attack_19") },
        { EffectType.Attack_20, Animator.StringToHash("Attack_20") },
        { EffectType.Attack_21, Animator.StringToHash("Attack_21") },
        { EffectType.Attack_23, Animator.StringToHash("Attack_23") },
        { EffectType.Attack_24, Animator.StringToHash("Attack_24") },
        { EffectType.Attack_25, Animator.StringToHash("Attack_25") },
        { EffectType.Attack_26, Animator.StringToHash("Attack_26") },
        { EffectType.Attack_27, Animator.StringToHash("Attack_27") },
        { EffectType.Attack_28, Animator.StringToHash("Attack_28") },
        { EffectType.Attack_29, Animator.StringToHash("Attack_29") },
        #endregion
    };
}
