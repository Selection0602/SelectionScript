using System;
using System.Threading.Tasks;

public class EnqueueCommand : ICommand
{
    private CommandInvoker _invoker;

    public EnqueueCommand(CommandInvoker invoker)
    {
        _invoker = invoker;
    }

    public async Task Execute()
    {
        await _invoker.ExecuteNext();
    }

    public void Undo()
    {
        
    }
}
