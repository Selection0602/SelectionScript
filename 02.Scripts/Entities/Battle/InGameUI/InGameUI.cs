using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private CostUI _playerCost;
    [SerializeField] private CostUI _bossCost;
    [SerializeField] private GameObject[] _battleUIElements;
    [SerializeField] private TextMeshProUGUI _floatingText;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private Image _deckImage;
    [SerializeField] private Image _playerSkillImage;
    [SerializeField] private Image _enemySkillImage;

    [SerializeField] private GameObject[] _fires;

    private BattleDataManager _battleDataManager;

    private void Start()
    {
        _fadeImage.gameObject.SetActive(true);
        _battleDataManager = (SceneBase.Current as BattleSceneController).BattleDataManager;
    }

    public async Task SetBattleUI(NodeType node)
    {
        await SetBackGround(node);
        SetPlayerUI();
        if (node == NodeType.BossBattle)
        {
            SetEnemyUI();
        }
    }

    private async Task SetBackGround(NodeType node)
    {
        _background.sprite = await _battleDataManager.LoadBackGround($"{node}");
        if (node != NodeType.NormalBattle)
        {
            foreach (var fire in _fires)
            {
                fire.gameObject.SetActive(true);
            }
        }
    }

    private void SetPlayerUI()
    {
        var imageData = _battleDataManager.PlayerImageData;
        _playerCost.SetImage(imageData.CostImage);
        _deckImage.sprite = imageData.DeckImage;
        _playerSkillImage.sprite = imageData.SkillIcon;
        _playerCost.SetCostLight(imageData.CostLightColor);
    }

    private void SetEnemyUI()
    {
        var imageData = _battleDataManager.EnemyImageData;

        //코스트 세팅
        _bossCost.SetImage(imageData.CostImage);
        //스킬 이미지 세팅
        _enemySkillImage.sprite = imageData.SkillIcon;
        _bossCost.SetCostLight(imageData.CostLightColor);
    }

    public void InitializePlayerCost(Cost playerCost)
    {
        _playerCost.Initialize(playerCost, true);
    }

    public void InitializeEnemyCost(Cost enemyCost)
    {
        _bossCost.gameObject.SetActive(true);
        _bossCost.Initialize(enemyCost,true);
    }

    public void HideBattleUI()
    {
        foreach (var ui in _battleUIElements)
        {
            ui.SetActive(false);
        }
    }

    public async UniTask ShowFloatingText(string text)
    {
        _floatingText.gameObject.SetActive(true);
        _floatingText.text = text;
        await _floatingText.DOFade(1.0f, 0.5f).AsyncWaitForCompletion();
        await UniTask.Delay(500);
        await _floatingText.DOFade(0.0f, 0.5f).AsyncWaitForCompletion();
        _floatingText.gameObject.SetActive(false);
    }

    public async Task FadeOut()
    {
        await _fadeImage.DOFade(0.0f, 1.0f).AsyncWaitForCompletion();
        _fadeImage.gameObject.SetActive(false);
    }

    public async UniTask FadeInOut_White()
    {
        _fadeImage.gameObject.SetActive(true);
        _fadeImage.color = new Color(1, 1, 1, 0);
        await _fadeImage.DOFade(1.0f, 1.0f).AsyncWaitForCompletion();
        await UniTask.Delay(300);
        await _fadeImage.DOFade(0.0f, 1.0f).AsyncWaitForCompletion();
        _fadeImage.gameObject.SetActive(false);
    }
}
