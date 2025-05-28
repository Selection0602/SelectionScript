using System;
using System.Threading.Tasks;

public class UseManipulationCard : ICommand
{
    public async Task Execute()
    {
        await Task.CompletedTask;
    }

    public void Undo()
    {
    }
}
