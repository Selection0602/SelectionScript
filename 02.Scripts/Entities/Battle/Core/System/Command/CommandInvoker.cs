using System.Collections.Generic;
using System.Threading.Tasks;

public class CommandInvoker
{
    private Queue<ICommand> _commandQueue = new();
    private Stack<ICommand> _executedCommands = new(); 

    private bool _isRunning = false;

    public void Enqueue(ICommand command)
    {
        _commandQueue.Enqueue(command);
    }

    public async Task ExecuteNext()
    {
        if (_isRunning || _commandQueue.Count == 0) return;
        
        _isRunning = true;

        while (_commandQueue.Count > 0)
        {
            ICommand current = _commandQueue.Dequeue();
            await current.Execute();  // 비동기 명령 실행
        }

        _isRunning = false;
    }

    public async Task ExecuteAllAsync()
    {
        while (_commandQueue.Count > 0)
        {
            ICommand command = _commandQueue.Dequeue();
            await command.Execute();
        }
    }

    public void UndoLastCommandAsync()
    {
        if (_executedCommands.Count > 0)
        {
            var command = _executedCommands.Pop();
            command.Undo();
        }
    }

    public void UndoAllAsync()
    {
        while (_executedCommands.Count > 0)
        {
            var command = _executedCommands.Pop();
            command.Undo();
        }
    }
}
