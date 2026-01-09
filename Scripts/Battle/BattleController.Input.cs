#nullable enable
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

    public Vector2I FocusedCell => _cursor.FocusedCell;
    
    public InputDeviceMode InputDeviceMode = GlobalSettings.DefaultInputMode;
    private BattleInputState _inputState = BattleInputState.FreeSelect;
    public BattleInputState InputState
    {
        get => _inputState;
        private set
        {
            if (value == _inputState) return; 
            _inputState = value;
            NotifyInputStateChanged();
        }
    }

    private BattleUnit? ActiveMover => _selectionController.SelectedUnit;
    
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
            if (!_cursor.TryMoveDirection(_heldDirection))
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
        if (e.IsActionPressed("ui_accept")) { return HandleAcceptAtFocusedCell(e); }
        if (e.IsActionPressed("ui_cancel"))  { return HandleCancel(e); }

        // Mouse - Click
        if (e is InputEventMouseButton mbe )
        {
            if (mbe.ButtonIndex == MouseButton.Left && mbe.Pressed)
                return HandleAcceptAtFocusedCell(mbe);
            else if (mbe.ButtonIndex == MouseButton.Right && mbe.Pressed)
                return HandleMouseRightClick(mbe);
        }
        
        // Mouse - Motion
        if (e is InputEventMouseMotion mme)
        {
            return HandleMouseMotion(mme);
        }
        
        return false;
    }
    // private bool HandleAccept(InputEvent e)
    // {
    //     _logger.Log("HandleAccept", LogSeverity.Info, LogCategory.Input);
    //     // attempt to select via SelectionController
    //     _selectionController.TriggerSelection();
    //     return true;
    // }

    
    
    /// <summary>
    /// Routes a confirm/click intent based on the current battle input state.
    /// </summary>
    private bool HandleAcceptAtFocusedCell(InputEvent e)
    {
        _logger.Log("HandleAcceptAtFocusedCell", LogSeverity.Trace, LogCategory.Input);

        var targetCell = _cursor.FocusedCell;
        _unitRegistry.TryGetUnitAtCell(targetCell, out var unitAtCell);

        switch (_inputState)
        {
            case BattleInputState.FreeSelect:
                return HandleAccept_FreeSelect(targetCell, unitAtCell);

            case BattleInputState.MoveTargeting:
                return HandleAccept_MoveTargeting(targetCell, unitAtCell);

            case BattleInputState.AttackTargeting:
                return HandleAccept_AttackTargeting(targetCell, unitAtCell);

            default:
                return false;
        }
    }
    
    private bool HandleAccept_FreeSelect(Vector2I targetCell, BattleUnit? unitAtCell)
    {
        _logger.Log("HandleAccept_FreeSelect", LogSeverity.Trace, LogCategory.Input);

        if (unitAtCell != null)
        {
            _selectionController.SelectUnit(unitAtCell);
            return true;
        }

        _selectionController.SelectCell(targetCell);
        return true;
    }

    private bool HandleAccept_MoveTargeting(Vector2I targetCell, BattleUnit? unitAtCell)
    {
        _logger.Log("HandleAccept_MoveTargeting", LogSeverity.Trace, LogCategory.Input);

        if (unitAtCell != null && unitAtCell.IsFriendly)
        {
            _selectionController.SelectUnit(unitAtCell);
            return true;
        }

        if (ActiveMover == null)
        {
            _logger.Log("No active mover in MoveTargeting", LogSeverity.Warn, LogCategory.Input);
            InputState = BattleInputState.FreeSelect;
            return true;
        }

        _movementController.TryMoveToCell(ActiveMover, targetCell);
        return true;
    }

    private bool HandleAccept_AttackTargeting(Vector2I targetCell, BattleUnit? unitAtCell)
    {
        _logger.Log("HandleAccept_AttackTargeting", LogSeverity.Trace, LogCategory.Input);

        if (_selectionController.SelectedUnit == null)
        {
            _logger.Log("No selected attacker in AttackTargeting", LogSeverity.Warn, LogCategory.Input);
            InputState = BattleInputState.FreeSelect;
            return true;
        }

        if (unitAtCell == null || unitAtCell.IsFriendly)
        {
            _logger.Log("Invalid attack target", LogSeverity.Trace, LogCategory.Input);
            return true;
        }

        // _attackController.TryAttack(_selectionController.SelectedUnit, unitAtCell); TODO
        return true;
    }
    
    private bool HandleCancel(InputEvent e)
    {
        _logger.Log("HandleCancel", LogSeverity.Info, LogCategory.Input);
        switch (_inputState)
        {
            case BattleInputState.FreeSelect:
                _selectionController.TriggerClearSelection();
                return true;

            case BattleInputState.MoveTargeting:
            case BattleInputState.AttackTargeting:
                ExitTargetingMode();
                return true;

            default:
                ExitTargetingMode();
                return true;
        }
    }

    private bool HandleDirection(InputDirection? dir)
    {
        _logger.Log("HandleDirection", LogSeverity.Trace, LogCategory.Input);
        if (!dir.HasValue)
            dir = ReadHeldDirection();
        if (dir == InputDirection.None)
            return false;
        
        // Controller/keyboard cursor movement
        if (_cursor.TryMoveDirection(dir.Value))
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
        _selectionController.TriggerSelection();
        return true;
    }
    
    private bool HandleMouseRightClick(InputEventMouseButton e)
    {
        _logger.Log("HandleMouseRightClick", LogSeverity.Trace, LogCategory.Input);
        _selectionController.TriggerClearSelection();
        return true;
    }

    private bool HandleMouseMotion(InputEventMouseMotion e)
    {
        _logger.Log("HandleMouseMotion", LogSeverity.Extra, LogCategory.Input);
        _cursor.TryMoveToGlobalPosition(e.GlobalPosition);
        return true;
    }

    private void EnterMoveTargetingMode(BattleUnit unit)
    {
        _logger.Log("EnterMovementMode", LogSeverity.Trace, LogCategory.Input);
        InputState = BattleInputState.MoveTargeting;

        var cell = _selectionController.SelectedCell;
        var movement = _selectionController.SelectedUnit?.Movement;
        if (!DebugUtil.Require(cell != null, "[BattleController.Input] failed to enter MovementMode, no selected cell") ||
            !DebugUtil.Require(movement != null, "[BattleController.Input] failed to enter MovementMode, no movement value")
            )
            return;
        
        var movementPreview = _moveRangeService.BuildMovementPreview(cell.Value, movement.Value);
        _grid.SetMovementPreview(movementPreview);
    }
    private void ExitTargetingMode()
    {
        _logger.Log("ExitTargetingMode", LogSeverity.Trace, LogCategory.Input);

        InputState = BattleInputState.FreeSelect;
        _selectionController.TriggerClearSelection();
        _grid.ClearOverlays();
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
    
    
}
