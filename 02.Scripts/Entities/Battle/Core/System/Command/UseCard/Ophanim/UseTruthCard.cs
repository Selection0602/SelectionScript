using System;
using System.Threading.Tasks;

public class UseTruthCard : ICommand
{
    public async Task Execute()
    {
        await Task.CompletedTask;
    }

    public void Undo()
    {
    }
}
