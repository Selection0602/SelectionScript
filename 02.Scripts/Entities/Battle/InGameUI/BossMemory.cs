using UnityEngine;
using UnityEngine.UI;

public class BossMemory : MonoBehaviour
{
    [SerializeField] private MemorySO _memoryData;
    [SerializeField] private Button _button;

    private void Start()
    {
        _button.onClick.AddListener(SelectMemory);
    }

    public void SelectMemory()
    {
        (SceneBase.Current as BattleSceneController).MoveNextScene(_memoryData);
    }
}