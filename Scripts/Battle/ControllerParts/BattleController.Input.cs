#nullable enable
using System;
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Controllers;
using Goblinos.Scripts.Battle.Core.Types;
using Goblinos.Scripts.Battle.Preview;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController: IInputHandler
{
    /** Components, Node references */
    [ExportGroup("Input")] 
    private InputRouter _inputRouter = null!;
    
    /** Fields */
    [Export] private double _repeatDelay = 0.32;    // initial delay before repeating
    [Export] private double _repeatInterval = 0.16; // time between repeated moves
    
    private InputDirection _heldDirection = InputDirection.None;
    private double _repeatMoveTimer = 0.0;

    /** Properties */
    public bool BlocksLowerInputHandlers => false;

    public Vector2I FocusedCell => _cursor.FocusedCell;
    
    public InputDeviceMode InputDeviceMode = GlobalSettings.DefaultInputMode;
    

    private Units.BattleUnit? ActiveMover => UnitActivation?.Unit;
    
    private void _Ready_Input()
    {
        DebugUtil.Require(_cameraController != null, "[BattleController.Input] Not Initialized. Unable to register BattleCameraController");
        
        _inputRouter = GetNode<InputRouter>(GlobalSettings.InputRouterPath);
        DebugUtil.Require(_inputRouter != null, "[BattleController.Input] Not Initialized. Unable to register input router");
        
        _inputRouter.Push(this);
        _inputRouter.Push(_cameraController);
        
        _logger.Log("Ready", GobLogSeverity.Info, GobLogCategory.Initialization);
    }

    private void _Process_Input(double delta)
    {
        _logger.Log("_Process_Input", GobLogSeverity.Extra, GobLogCategory.Input);
        
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
            if (!_cursor.TryMoveDirection(_heldDirection, GridCursorFocusSource.Unknown))
            {
                // Can't move, stop repeat
                _heldDirection = InputDirection.None;
                _repeatMoveTimer = 0;
                break;
            }
        } // End Held direction
    }
    public bool HandleRoutedInput(InputEvent e)
    {
        _logger.Log($"Handle {e.GetType().Name} :: {e.AsText()}", GobLogSeverity.Extra, GobLogCategory.Input);

        // If user presses arrow / move buttons handle cursor action
        if (e.IsActionPressed("ui_up"))    { return HandleDirection(InputDirection.Up, e); }
        if (e.IsActionPressed("ui_right"))    { return HandleDirection(InputDirection.Right, e); }
        if (e.IsActionPressed("ui_down"))    { return HandleDirection(InputDirection.Down, e); }
        if (e.IsActionPressed("ui_left"))    { return HandleDirection(InputDirection.Left, e); }
        
        // Stop repeating when direction released (and no other direction is still held)
        if (e.IsActionReleased("ui_up") || e.IsActionReleased("ui_down") ||
            e.IsActionReleased("ui_left") || e.IsActionReleased("ui_right"))
        {
            return HandleDirectionReleased();
        }

        // Accept / cancel actions
        if (e.IsActionPressed("ui_accept")) { return HandleAcceptAtFocusedCell(e); }
        if (e.IsActionPressed("ui_cancel"))  { return HandleCancel(e); }

        // Select Next / Select Previous
        if (e.IsActionPressed("select_next", false, true)) { return HandleSelectNext(); }
        if (e.IsActionPressed("select_previous", false, true)) { return HandleSelectPrevious(); }

        // Mouse - Click
        if (e is InputEventMouseButton mbe)
        {
            if (e.IsActionPressed("camera_drag_pan") || e.IsActionReleased("camera_drag_pan"))
            {
                GD.Print($"Camera pan. pressed={mbe.IsActionPressed("camera_drag_pan")}");
            }
            
            if (mbe.ButtonIndex == MouseButton.Left && mbe.Pressed)
            {
                _cursor.TryMoveToGlobalPosition(_cursor.GetGlobalMousePosition(), GridCursorFocusSource.Mouse);
                return HandleAcceptAtFocusedCell(e);
            }
            if (mbe.ButtonIndex == MouseButton.Right && mbe.Pressed)
                return HandleCancel(e);
        }
        
        
        
        // Mouse - Motion
        if (e is InputEventMouseMotion mme)
        {
            return HandleMouseMotion(mme);
        }

        if (e.IsActionPressed("TEST"))
        {
            _logger.Log("DEBUG button pressed - TEST", GobLogSeverity.Warn, GobLogCategory.DebugOnly);
            ClearPreviews();
            return true;
        }
        
        return false;
    }
    
    // ---------------------------------------------------------------------
    // Individual Action Handlers
    // ---------------------------------------------------------------------

    
    /// <summary>
    /// Routes a confirm/click intent based on the current battle input state.
    /// </summary>
    private bool HandleAcceptAtFocusedCell(InputEvent e)
    {
        _logger.Log("HandleAcceptAtFocusedCell", GobLogSeverity.Trace, GobLogCategory.Input);

        var cell = _cursor.FocusedCell;
        var focus = _selectionController.GetFocus(cell);
        _selectionController.UpdateHovered();

        switch (InputState)
        {
            case BattleInputState.FreeSelect:
                return HandleAccept_FreeSelect(focus);
            case BattleInputState.MoveTargeting:
                return HandleAccept_MoveTargeting(focus);
            case BattleInputState.PrimaryActionSelect:
                return HandleAccept_PrimaryActionSelect(focus);
            case BattleInputState.PrimaryActionTargeting:
                return HandleAccept_PrimaryActionTargeting(focus);
            case BattleInputState.PrimaryActionConfirm:
                return HandleAccept_PrimaryActionConfirm(focus);

            default:
                _logger.Warn($"HandleAcceptAtFocusedCell - unhandled BattleInputState={_inputState}");
                return false;
        }
    }
    
    private bool HandleAccept_FreeSelect(CellFocus cellFocus)
    {
        _logger.Log("HandleAccept_FreeSelect", GobLogSeverity.Trace, GobLogCategory.Input);
        
        if (cellFocus.Unit != null)
        {
            if (cellFocus.Unit.IsFriendly)
            {
                _selectionController.SelectCell(cellFocus.Cell);
                EnterMoveTargetingMode(cellFocus.Unit);
            }
            else
            {
                // TODO - toggle enemy inspection.
            }
        }
        return true;
    }

    private bool HandleAccept_MoveTargeting(CellFocus cellFocus)
    {
        _logger.Log("HandleAccept_MoveTargeting", GobLogSeverity.Trace, GobLogCategory.Input);

        if (!DebugUtil.Require(UnitActivation != null,
                "[BattleController.Input].HandleAccept_MoveTargeting - No UnitActivationContext") ||
            !DebugUtil.Require(ActiveMover != null,
                "[BattleController.Input].HandleAccept_MoveTargeting - No ActiveMover"))
        {
            AbortActivationToFreeSelect();
            return true;
        }

        if (cellFocus.Cell == UnitActivation.OriginCell)
        {
            EnterPrimaryActionSelectMode();
            return true;
        }

        if (cellFocus.Unit != ActiveMover && cellFocus.Unit is { IsFriendly: true })
        {
            _selectionController.SelectUnit(cellFocus.Unit);
            ClearUnitActivation();
            InitializeActivationContext();
            ResetPreviews();
            return true;
        }
        
        // TODO - if target valid attack target, move to in range attack square, go to PrimaryActionSelect

        if (!_moveRangeService.CanMoveTo(ActiveMover, UnitActivation.OriginCell, cellFocus.Cell))
        {
            AbortActivationToFreeSelect();
            return true;
        }

        if (_movementController.TryMoveToCell(ActiveMover, cellFocus.Cell))
        {
            UnitActivation.SetMoveTargetCell(cellFocus.Cell);
            EnterPrimaryActionSelectMode();
            return true;
        }
        
        return true;
    }

    private bool HandleAccept_PrimaryActionSelect(CellFocus cellFocus)
    {
        _logger.Log("HandleAccept_PrimaryActionSelect", GobLogSeverity.Trace, GobLogCategory.Input);

        if (!DebugUtil.Require(IsUnitSelected, "[BattleController.Input].HandleAccept_PrimaryActionSelect - No selected attacker"))
        {
            InputState = BattleInputState.FreeSelect;
            return false;
        }

        // TODO - select menu item
        
        return false;
    }

    private bool HandleAccept_PrimaryActionTargeting(CellFocus cellFocus)
    {
        if (!DebugUtil.Require(UnitActivation != null,
                $"[{nameof(BattleController)}].{nameof(HandleAccept_PrimaryActionTargeting)} - No {nameof(UnitActivationContext)}"))
        {
            AbortActivationToFreeSelect();
            return true;
        }
        
        if (_primaryActionTargetingService.IsValidTarget(UnitActivation, cellFocus))
        {
            _unitRegistry.TryGetUnitAtCell(cellFocus.Cell, out var target);
            UnitActivation.SetPrimaryActionTarget(cellFocus.Cell, target);
            EnterPrimaryActionConfirmation();
        }
        else
            AbortActivationToFreeSelect();

        return true;
    }

    private bool HandleAccept_PrimaryActionConfirm(CellFocus cellFocus)
    {
        if (!DebugUtil.Require(UnitActivation != null,
                $"[{nameof(BattleController)}].{nameof(HandleAccept_PrimaryActionConfirm)} - No {nameof(UnitActivationContext)}"))
        {
            AbortActivationToFreeSelect();
            return true;
        }
        
        // TODO - if click target or press enter (or controller confirm button) resolve, else Exit.
        
        CommitUnitActivation();
        return true;
    }
    
    private bool HandleCancel(InputEvent e)
    {
        _logger.Log("HandleCancel", GobLogSeverity.Info, GobLogCategory.Input);
        switch (_inputState)
        {
            case BattleInputState.FreeSelect:
                return true;
            case BattleInputState.MoveTargeting:
            case BattleInputState.PrimaryActionSelect:
                AbortActivationToFreeSelect();
                return true;
            case BattleInputState.PrimaryActionTargeting:
                AbortActivationToFreeSelect();
                return true;
            case BattleInputState.PrimaryActionConfirm:
                AbortActivationToFreeSelect();
                return true;
            default:
                AbortActivationToFreeSelect();
                return true;
        }
    }
    
    private bool HandleDirection(InputDirection? dir, InputEvent e)
    {
        _logger.Log("HandleDirection", GobLogSeverity.Trace, GobLogCategory.Input);
        if (!dir.HasValue)
            dir = ReadHeldDirection();
        if (dir == InputDirection.None)
            return false;
        
        var inputMode = e switch
            {
                InputEventJoypadButton => GridCursorFocusSource.Gamepad,
                InputEventKey => GridCursorFocusSource.Keyboard,
                InputEventMouseButton or InputEventMouseMotion => GridCursorFocusSource.Mouse,
                _ => GridCursorFocusSource.Unknown
            };
        
        // Controller/keyboard cursor movement
        if (_cursor.TryMoveDirection(dir.Value, inputMode))
        {
            _heldDirection = dir.Value;
            _repeatMoveTimer = -_repeatDelay;
        }
        return true;
    }

    private bool HandleDirectionReleased()
    {
        _logger.Log("HandleDirectionReleased", GobLogSeverity.Trace, GobLogCategory.Input);
        _heldDirection = ReadHeldDirection();
        _repeatMoveTimer = 0.0;
        return true;
    }

    private bool HandleMouseClick(InputEventMouseButton e)
    {
        _logger.Log("HandleMouseClick", GobLogSeverity.Trace, GobLogCategory.Input);
        _selectionController.TrySelectHoveredUnit();
        return true;
    }
    
    private bool HandleMouseRightClick(InputEventMouseButton e)
    {
        _logger.Log("HandleMouseRightClick", GobLogSeverity.Trace, GobLogCategory.Input);
        HandleCancel(e);
        return true;
    }

    private bool HandleMouseMotion(InputEventMouseMotion e)
    {
        _logger.Log("HandleMouseMotion", GobLogSeverity.Extra, GobLogCategory.Input);
        _cursor.TryMoveToGlobalPosition(_cursor.GetGlobalMousePosition(), GridCursorFocusSource.Mouse);
        return true;
    }

    private bool HandleSelectNext(bool reverse = false)
    {
        _logger.Log($"{nameof(HandleSelectNext)}", GobLogSeverity.Trace, GobLogCategory.Input);
        switch (InputState)
        {
            case BattleInputState.FreeSelect:
            case BattleInputState.MoveTargeting:
                // Select next non-exhausted player unit.
                _logger.Log($"[{nameof(HandleSelectNext)}] Select {(reverse ? "previous" : "next")} player unit.", GobLogSeverity.Info, GobLogCategory.Input);
                if (_selectionController.TrySelectNextUnit(reverse))
                {
                    _cameraController.CenterOnCell(SelectedCell.Value);
                    EnterMoveTargetingMode(SelectedUnit!);
                }
                    
                break;
            case BattleInputState.PrimaryActionSelect:
                // Pass to menu.
                break;
            case BattleInputState.PrimaryActionTargeting:
                // select next legal target.
                break;
            case BattleInputState.PrimaryActionConfirm:
                // Unknown what to do here, maybe nothing.
                break;

            default:
                _logger.Warn($"HandleAcceptAtFocusedCell - unhandled BattleInputState={_inputState}");
                return false;
        }

        return false;
    }

    private bool HandleSelectPrevious() => HandleSelectNext(true);

    // ---------------------------------------------------------------------
    // Helper Methods
    // ---------------------------------------------------------------------
    
    private InputDirection ReadHeldDirection()
    {
        _logger.Log("ReadHeldDirection", GobLogSeverity.Extra, GobLogCategory.Input);
        // Priority order if multiple are held
        if (Godot.Input.IsActionPressed("ui_up")) return InputDirection.Up;
        if (Godot.Input.IsActionPressed("ui_down")) return InputDirection.Down;
        if (Godot.Input.IsActionPressed("ui_left")) return InputDirection.Left;
        if (Godot.Input.IsActionPressed("ui_right")) return InputDirection.Right;
        return InputDirection.None;
    }
}
