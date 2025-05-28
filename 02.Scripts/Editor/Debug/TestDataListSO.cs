using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "TestDataListSO", menuName = "GameData/TestDataListSO")]
public class TestDataListSO : ScriptableObject
{
    public List<RewardSO> Booties;
    public List<RewardSO> Memories;
    public List<CardSO> PlayerCards;
}
