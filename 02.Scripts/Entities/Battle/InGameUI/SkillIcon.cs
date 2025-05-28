using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _skillIcon;
    [SerializeField] private Button _skillUseButton;

    [Header("스킬 설명창")]
    [SerializeField] private Image _skillDescWindow;
    [SerializeField] private TextMeshProUGUI _skillName;
    [SerializeField] private TextMeshProUGUI _skillDesc;

    public IUniqueSkillUser UniqueSkillUser
    {
        get => _uniqueSkillUser;
        set
        {
            _uniqueSkillUser = value;
            SetSkillIcon();
        }
    }

    private IUniqueSkillUser _uniqueSkillUser;

    private void SetSkillIcon()
    {
        //TODO: 유니크 아이콘 설정
        gameObject.SetActive(true);
        _skillName.text = $"{_uniqueSkillUser.SkillData.SkillName}";
        _skillDesc.text = $"{TextUtility.CleanLineBreaks(_uniqueSkillUser.SkillData.Desc)}";
        _skillUseButton.onClick.AddListener(UseSkill);
    }

    public async void UseSkill()
    {
        await _uniqueSkillUser.UseSkill();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _skillDescWindow.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _skillDescWindow.gameObject.SetActive(false);
    }
}
