using System.Collections.Generic;
using Godot;

namespace Goblinos.Scripts.Core;

public interface IInputHandler
{
    /// Return true if you consumed the event (router stops), else other handler can consume input
    bool Handle(InputEvent e);
    bool BlocksLowerInputHandlers { get; }
}

public partial class InputRouter : Node
{
    private readonly Stack<IInputHandler> _stack = new();

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always; // still receives input when paused if you want
        GD.Print("[InputRouter] Ready");
    }

    public void Push(IInputHandler handler)
    {
        if (handler == null) return;
        _stack.Push(handler);
        GD.Print($"[InputRouter] Push: {handler.GetType().Name} (depth {_stack.Count})");
    }

    public void Pop(IInputHandler handler = null)
    {
        if (_stack.Count == 0) return;

        if (handler == null)
        {
            var popped = _stack.Pop();
            GD.Print($"[InputRouter] Pop: {popped.GetType().Name} (depth {_stack.Count})");
            return;
        }

        // Pop only if it's the top (simple + predictable)
        if (ReferenceEquals(_stack.Peek(), handler))
        {
            var popped = _stack.Pop();
            GD.Print($"[InputRouter] Pop: {popped.GetType().Name} (depth {_stack.Count})");
        }
    }

    public IInputHandler Peek() => _stack.Count > 0 ? _stack.Peek() : null;

    public override void _UnhandledInput(InputEvent e)
    {
        if (_stack.Count == 0) return;

        foreach (var handler in _stack)
        {
            // Route to the top handler; stop if consumed
            if (handler.Handle(e))
            {
                GetViewport().SetInputAsHandled();
                return;
            }
            
            if (handler.BlocksLowerInputHandlers)
            {
                GetViewport().SetInputAsHandled();
                return;
            }
        }
    }
}