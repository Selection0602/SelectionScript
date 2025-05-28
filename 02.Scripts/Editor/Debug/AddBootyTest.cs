using System.Linq;
using UnityEditor;

[InitializeOnLoad]
public class AddDataTest
{
    static AddDataTest()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                var testDatas = AssetDatabase.LoadAssetAtPath<TestDataListSO>("Assets/05.ScriptableObjects/TestSO/TestDataListSO.asset");
                foreach (var booty in testDatas.Booties)
                {
                    var reward = new RewardData
                    {
                        FileName = booty.FileName,
                        Image = booty.Image,
                        Icon = booty.Icon,
                        Index = booty.Index,
                        RewardName = booty.RewardName,
                        Desc = booty.Desc,
                        AtkUpValue = booty.AtkUpValue,
                        HealUpValue = booty.HealUpValue,
                        MaxHealValue = booty.MaxHealValue,
                        DrawCard = booty.DrawCard,
                        CostDown = booty.CostDown,
                        NotBurn = booty.NotBurn,
                        NotPoison = booty.NotPoison,
                        NotWeaken = booty.NotWeaken,
                        GetCard = booty.GetCard,
                        GetCardIndex = booty.GetCardIndex,
                        TypeValue = booty.TypeValue,
                        CardDrawIndex = booty.CardDrawIndex
                    };

                    Manager.Instance.DataManager.AddBooty(reward);
                }

                //foreach (var data in testDatas.PlayerCards)
                //{
                //    var cardData = new CardData
                //    {
                //        FileName = data.FileName,
                //        CardName = data.CardName,
                //        Image = data.Image,
                //        Index = data.Index,
                //        RarityType = data.RarityType,
                //        TargetType = data.TargetType,
                //        EffectRange = data.EffectRange,
                //        Cost = data.Cost,
                //        Desc = data.Desc,
                //        Values = data.Values,
                //        ApplyValues = data.ApplyValues.ToList(),
                //        IsDispaly = data.IsDispaly,
                //        ReDraw = data.ReDraw
                //    };

                //    Manager.Instance.DataManager.AddCard(cardData.Index);
                //}

            }
        };
    }
}
