using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/MiniGame Data")]
public class MiniGameDataSO : ScriptableObject
{
    public string title;
    public int index;

    [SerializeField]
    private bool isLocked = true;
    public bool IsLocked
    {
        get => isLocked;
        set => isLocked = value;
    }

    public string sceneName;
}