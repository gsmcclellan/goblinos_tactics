using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController: IInputHandler
{
    /** Components, Node references */
    [ExportGroup("Input")] 
    private InputRouter _inputRouter;
    
    /** Fields */
    [Export] private double _repeatDelay = 0.32;    // initial delay before repeating
    [Export] private double _repeatInterval = 0.16; // time between repeated moves
    
    private InputDirection _heldDirection = InputDirection.None;
    private double _repeatMoveTimer = 0.0;
    
    /** Properties */
    public bool BlocksLowerInputHandlers => false;

    public InputMode Mode { get; private set; } = GlobalSettings.DefaultInputMode;

    public Vector2I FocusedCell => _cursor.FocusedCell;
    
    private void _Ready_Input()
    {
        _inputRouter = GetNode<InputRouter>(GlobalSettings.InputRouterPath);
        DebugUtil.Require(_inputRouter != null, "[BattleController.Input] Not Initialized. Unable to register input router");
        _inputRouter.Push(this);
        
        _logger.Log("Ready", LogSeverity.Info, LogCategory.Initialization);
    }

    private void _Process_Input(double delta)
    {
        _logger.Log("_Process_Input", LogSeverity.Extra, LogCategory.Input);
        
        // Holding direction to repeatedly move cursor
        if (_heldDirection == InputDirection.None) return;
        var previousDirection = _heldDirection;
        _heldDirection = ReadHeldDirection();
        if (_heldDirection == InputDirection.None) return;

        if (previousDirection != _heldDirection)
            _repeatMoveTimer = -_repeatDelay;
        _repeatMoveTimer += delta;
        
        // If delta is large, move multiple times.
        while (_repeatMoveTimer >= _repeatInterval)
        {
            _repeatMoveTimer -= _repeatInterval;
            if (!TryMoveCursor(_heldDirection))
            {
                // Can't move, stop repeat
                _heldDirection = InputDirection.None;
                _repeatMoveTimer = 0;
                break;
            }
        } // End Held direction
    }
    public bool Handle(InputEvent e)
    {
        _logger.Log($"Handle {e.GetType().Name} :: {e.AsText()}", LogSeverity.Extra, LogCategory.Input);

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
        _logger.Log("HandleAccept", LogSeverity.Info, LogCategory.Input);
        return true;
    }

    private bool HandleCancel(InputEvent e)
    {
        _logger.Log("HandleCancel", LogSeverity.Info, LogCategory.Input);

        return true;
    }

    private bool HandleDirection(InputDirection? dir)
    {
        _logger.Log("HandleDirection", LogSeverity.Trace, LogCategory.Input);
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
        _logger.Log("HandleDirectionReleased", LogSeverity.Trace, LogCategory.Input);
        _heldDirection = ReadHeldDirection();
        _repeatMoveTimer = 0.0;
        return true;
    }

    private bool HandleMouseClick(InputEventMouseButton e)
    {
        _logger.Log("HandleMouseClick", LogSeverity.Trace, LogCategory.Input);
        // Mouse click: attempt to select what is focused by cursor
        // TODO
        return true;
    }

    private bool HandleMouseMotion(InputEventMouseMotion e)
    {
        _logger.Log("HandleMouseMotion", LogSeverity.Extra, LogCategory.Input);
        TryMoveCursorToGlobalPosition(e.GlobalPosition);
        return true;
    }

    private InputDirection ReadHeldDirection()
    {
        _logger.Log("ReadHeldDirection", LogSeverity.Extra, LogCategory.Input);
        // Priority order if multiple are held
        if (Godot.Input.IsActionPressed("ui_up")) return InputDirection.Up;
        if (Godot.Input.IsActionPressed("ui_down")) return InputDirection.Down;
        if (Godot.Input.IsActionPressed("ui_left")) return InputDirection.Left;
        if (Godot.Input.IsActionPressed("ui_right")) return InputDirection.Right;
        return InputDirection.None;
    }
    
    public void MoveCursorToCell(Vector2I cell) // TODO - move cursor movement logic to cursor with try functions, battle controller only triggers
    {
        _logger.Log($"MoveCursorTo [cell]={cell}", LogSeverity.Trace, LogCategory.UiNavigation);
        _cursor.MoveTo(cell);
    }

    /// <summary>
    /// Checks if cursor movement one space in a given direction is possible
    /// according to BattleGrid, then moves cursor
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="cell">out property</param>
    /// <returns>true if able to move</returns>
    public bool TryMoveCursor(InputDirection dir, out Vector2I cell)
    {
        _logger.Log("TryMoveCursor", LogSeverity.Trace, LogCategory.UiNavigation);

        cell = _cursor.Focus.Cell + InputUtil.InputDirectionToVector2I(dir);
        return TryMoveCursorToCell(cell);
    }
    
    public bool TryMoveCursor(InputDirection dir) => TryMoveCursor(dir, out _);

    /// <summary>
    /// Checks if cursor movement to a cell is possible
    /// according to BattleGrid, then moves cursor
    /// </summary>
    /// <param name="cell"></param>
    /// <returns>true if able to move</returns>
    public bool TryMoveCursorToCell(Vector2I cell)
    {
        _logger.Log($"TryMoveCursorTo [cell]={cell}", LogSeverity.Extra, LogCategory.UiNavigation);

        if (cell == FocusedCell || !Grid.CanFocusCell(cell))
            return false;
        
        MoveCursorToCell(cell);
        return true;
    }

    /// <summary>
    /// Checks if cursor movement to a given global position is possible
    /// according to BattleGrid, then moves cursor
    /// </summary>
    /// <param name="globalPos"></param>
    /// <param name="cell">out property, Vector2I grid cell associated with global pos</param>
    /// <returns>true if able to move</returns>
    public bool TryMoveCursorToGlobalPosition(Vector2 globalPos, out Vector2I cell)
    {
        _logger.Log($"TryMoveCursorToGlobalPosition [globalPos]={globalPos}", LogSeverity.Extra, LogCategory.UiNavigation);

        if (!Grid.CanFocusGlobalPosition(globalPos, out cell) || cell == FocusedCell)
            return false;
        
        MoveCursorToCell(cell);
        return true;
    }

    public bool TryMoveCursorToGlobalPosition(Vector2 globalPos) => TryMoveCursorToGlobalPosition(globalPos, out _);
}

