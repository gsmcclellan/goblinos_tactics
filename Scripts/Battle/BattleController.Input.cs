using Goblinos.Scripts.Core;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController: IInputHandler
{
    /** Components, Node references */
    private InputRouter _inputRouter;
    
    /** Properties */
    public bool BlocksLowerInputHandlers => false;

    public InputMode Mode { get; private set; } = GlobalSettings.DefaultInputMode;

    private InputDirection _heldDirection = InputDirection.None;
    private double _repeatMoveTimer = 0.0;
    
    [ExportGroup("Input")]
    [Export] private double _repeatDelay = 0.32;    // initial delay before repeating
    [Export] private double _repeatInterval = 0.16; // time between repeated moves
    
    private void _Ready_Input()
    {
        _inputRouter = GetNode<InputRouter>(GlobalSettings.InputRouterPath);
        _inputRouter.Push(this);
        
        DebugUtil.Log("[BattleController.Input] Ready", DebugLogSeverity.Info, DebugLogCategory.Initialization);
    }

    private void _Process_Input(double delta)
    {
        DebugUtil.Log("[BattleController.Input] _Process_Input", DebugLogSeverity.Extra, DebugLogCategory.Input);
        // Holding direction to repeatedly move cursor
        if (_heldDirection == InputDirection.None) return;

        _heldDirection = ReadHeldDirection();
        if (_heldDirection == InputDirection.None) return;

        _repeatMoveTimer += delta;
        
        while (_repeatMoveTimer >= _repeatInterval)
        {
            _repeatMoveTimer -= _repeatInterval;
            TryMoveCursor(_heldDirection);
        }
    }
    public bool Handle(InputEvent e)
    {
        DebugUtil.Log($"[BattleController.Input] Handle {e.GetType().Name} :: {e.AsText()}", DebugLogSeverity.Extra, DebugLogCategory.Input);

        // If user presses arrow / move buttons handle cursor action
        if (e.IsActionPressed("ui_up"))    { return HandleDirection(InputDirection.Up); }
        if (e.IsActionPressed("ui_right"))    { return HandleDirection(InputDirection.Right); }
        if (e.IsActionPressed("ui_down"))    { return HandleDirection(InputDirection.Down); }
        if (e.IsActionPressed("ui_left"))    { return HandleDirection(InputDirection.Left); }
        
        // Stop repeating when direction released (and no other direction is still held)
        if (e.IsActionReleased("ui_up") || e.IsActionReleased("ui_down") ||
            e.IsActionReleased("ui_left") || e.IsActionReleased("ui_right"))
        {
            return HandleDirectionReleased();
        }

        // Accept / cancel actions
        if (e.IsActionPressed("ui_accept")) { return HandleAccept(e); }
        if (e.IsActionPressed("ui_cancel"))  { return HandleCancel(e); }

        // Mouse - Click
        if (e is InputEventMouseButton mbe && mbe.ButtonIndex == MouseButton.Left && mbe.Pressed)
        {
            return HandleMouseClick(mbe);
        }
        
        // Mouse - Motion
        if (e is InputEventMouseMotion mme)
        {
            return HandleMouseMotion(mme);
        }
        
        return false;
    }

    private bool HandleAccept(InputEvent e)
    {
        DebugUtil.Log("[BattleController.Input] HandleAccept", DebugLogSeverity.Info, DebugLogCategory.Input);
        return true;
    }

    private bool HandleCancel(InputEvent e)
    {
        DebugUtil.Log("[BattleController.Input] HandleCancel", DebugLogSeverity.Info, DebugLogCategory.Input);

        return true;
    }

    private bool HandleDirection(InputDirection? dir)
    {
        DebugUtil.Log("[BattleController.Input] HandleDirection", DebugLogSeverity.Trace, DebugLogCategory.Input);
        if (!dir.HasValue)
            dir = ReadHeldDirection();
        if (dir == InputDirection.None)
            return false;
        
        // Controller/keyboard cursor movement
        if (TryMoveCursor(dir.Value))
        {
            _heldDirection = dir.Value;
            _repeatMoveTimer = -_repeatDelay;
        }
        return true;
    }

    private bool HandleDirectionReleased()
    {
        DebugUtil.Log("[BattleController.Input] HandleDirectionReleased", DebugLogSeverity.Trace, DebugLogCategory.Input);
        _heldDirection = ReadHeldDirection();
        _repeatMoveTimer = 0.0;
        return true;
    }

    private bool HandleMouseClick(InputEventMouseButton e)
    {
        DebugUtil.Log("[BattleController.Input] HandleMouseClick", DebugLogSeverity.Trace, DebugLogCategory.Input);
        // Mouse click: set cursor to clicked tile, then confirm
        // TODO
        return true;
    }

    private bool HandleMouseMotion(InputEventMouseMotion e)
    {
        DebugUtil.Log("[BattleController.Input] HandleMouseMotion", DebugLogSeverity.Extra, DebugLogCategory.Input);
        return TryMoveCursorTo(e.GlobalPosition);
    }

    private InputDirection ReadHeldDirection()
    {
        DebugUtil.Log("[BattleController.Input] ReadHeldDirection", DebugLogSeverity.Extra, DebugLogCategory.Input);
        
        // Priority order if multiple are held
        if (Godot.Input.IsActionPressed("ui_up")) return InputDirection.Up;
        if (Godot.Input.IsActionPressed("ui_down")) return InputDirection.Down;
        if (Godot.Input.IsActionPressed("ui_left")) return InputDirection.Left;
        if (Godot.Input.IsActionPressed("ui_right")) return InputDirection.Right;
        return InputDirection.None;
    }
}

