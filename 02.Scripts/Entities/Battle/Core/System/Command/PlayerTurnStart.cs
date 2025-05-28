using System;
using System.Threading.Tasks;

public class PlayerTurnStart : ICommand
{
    private Player _player;

    public PlayerTurnStart(Player player)
    {
        _player = player;
    }

    public async Task Execute()
    {
        _player.OnTurnStart();
        _player.Cost.RecoveryCost();
        _player.IsCanUseSkill = true;
        Manager.Instance.SoundManager.PlaySFX(SFXType.SFX_GetCost);
        await Task.CompletedTask;
    }

    public void Undo()
    {
        //행동 취소
    }
}
