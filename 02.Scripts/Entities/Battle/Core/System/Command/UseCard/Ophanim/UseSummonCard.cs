using System;
using System.Threading.Tasks;

public class UseSummonCard : ICommand
{
    private ISummoner _summoner;

    public UseSummonCard(ISummoner summoner)
    {
        _summoner = summoner;
    }

    public async Task Execute()
    {
        _summoner.Summon();
        await Task.CompletedTask;
    }

    public void Undo()
    {

    }
}
