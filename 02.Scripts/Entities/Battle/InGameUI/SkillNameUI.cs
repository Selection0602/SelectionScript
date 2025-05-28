using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class SkillNameUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _skillName;

    [Header("스킬 사용 연출")]
    [SerializeField] private float _duration;
    [SerializeField] private int _delay;

    public async Task ShowSkillNameUI(string skillName)
    {
        _skillName.text = skillName;
        _canvasGroup.alpha = 0;
        _canvasGroup.gameObject.SetActive(true);

        await _canvasGroup.DOFade(1f, _duration).SetEase(Ease.OutQuad).AsyncWaitForCompletion();
        await UniTask.Delay(_delay);
        await _canvasGroup.DOFade(0f, _duration).SetEase(Ease.InQuad).AsyncWaitForCompletion();

        _canvasGroup.gameObject.SetActive(false);
    }
}
