using System.Threading.Tasks;
using UnityEngine;

public class SkillHandler : MonoBehaviour
{
    [SerializeField] private SkillIcon _skillIcon;
    [SerializeField] private SkillNameUI _skillNameUI;

    public void Initialize(IUniqueSkillUser uniqueSkillUser)
    {
        _skillIcon.UniqueSkillUser = uniqueSkillUser;
    }

    public async Task ShowSkillNameUI(string skillName)
    {
        await _skillNameUI.ShowSkillNameUI(skillName);
    }
}
