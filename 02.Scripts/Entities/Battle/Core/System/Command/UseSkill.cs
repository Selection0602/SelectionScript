using System.Threading.Tasks;

public class UseSkill : ICommand
{
    private IUniqueSkillUser _user;

    public UseSkill(IUniqueSkillUser user)
    {
        _user = user;
    }

    public async Task Execute()
    {
        await _user.UseSkill();
    }

    public void Undo()
    {

    }
}
