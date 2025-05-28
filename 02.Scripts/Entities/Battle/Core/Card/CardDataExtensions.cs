using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public static class CardDataExtensions
{

    public static int CalculatePriority(this CardData card)
    {
        //부름가 패에 있으면 먼저 쓰도록
        if (card.FileName == "Calling")
        {
            return int.MinValue;
        }
        return card.Cost * 100 - GetTargetPriority(card) * 10;
    }

    public static int GetTargetPriority(CardData card)
    {
        return card.TargetType == TargetType.You ? 1 : 2;
    }

    public static CardData Clone(this CardData card, CardSO data)
    {
        return new CardData()
        {
            FileName = data.FileName,
            Image = data.Image,
            Index = data.Index,
            RarityType = data.RarityType,
            TargetType = data.TargetType,
            EffectRange = data.EffectRange,
            CardName = data.CardName,
            Cost = data.Cost,
            Desc = data.Desc,
            Values = data.Values,
            ApplyValues = data.ApplyValues.ToList(),
            IsDispaly = data.IsDispaly,
            ReDraw = data.ReDraw,
            Weight = data.Weight,
        };
    }

    public static CardData Clone(this CardData card)
    {
        return new CardData()
        {
            FileName = card.FileName,
            Image = card.Image,
            Index = card.Index,
            RarityType = card.RarityType,
            TargetType = card.TargetType,
            EffectRange = card.EffectRange,
            CardName = card.CardName,
            Cost = card.Cost,
            Desc = card.Desc,
            Values = card.Values,
            ApplyValues = card.ApplyValues.ToList(),
            IsDispaly = card.IsDispaly,
            ReDraw = card.ReDraw,
            Weight = card.Weight
        };
    }

    public static async Task ExecuteCardEffect(this CardData card, CardEffectContext cardEffect)
    {
        var effect = CardEffectRegistry.Create(cardEffect);
        await effect.Execute();
        Debug.Log($"카드 사용 : {cardEffect.User}가 {card.CardName}을 사용했다");
    }

    public static void UpdateDesc(this CardData card)
    {
        switch(card.FileName)
        {
            case "Attack":
                card.Desc = $"적 하나에게\n피해를 {GetEnhanceVauleText(card.ApplyValues[0])} 줍니다";
                break;
            case "DrainLife":
                card.Desc = $"적 하나에게 피해를 {GetEnhanceVauleText(card.ApplyValues[0])}줍니다" +
                    $"\n정신력을 {GetEnhanceVauleText(card.ApplyValues[1])} 회복 합니다";
                break;
            case "ContinuousAttack":
                card.Desc = $"적 하나에게 피해를\n3만큼 {GetEnhanceVauleText(card.ApplyValues[1])}번 줍니다";
                break;
        }
    }

    private static string GetEnhanceVauleText(int bouns)
    {
        return $"<color=red>{bouns}</color>";
    }
}
