using System;
using System.Collections.Generic;
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle;
using Goblinos.Scripts.Battle.Types;
using Godot;

namespace Goblinos.Scripts.UI.Battle;

public partial class PrimaryActionSelect : Control
{
    [Signal]
    public delegate void ActionFocusedEventHandler(int action);
    [Signal]
    public delegate void ActionSelectedEventHandler(int action);

    private readonly Logger _logger = LogManager.For<PrimaryActionSelect>();

    private NodePath _buttonContainerPath = "ButtonContainer";
    private Dictionary<PrimaryActionType, Button> _buttons = new();

    private PrimaryActionType? hoveredAction;
    private PrimaryActionType? selectedAction;

    private Button? hoveredButton;
    private Button? selectedButton;

    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------
    
    public override void _Ready()
    {
        CacheButtons();
        _logger.Log("Ready", LogSeverity.Info, LogCategory.Initialization);
    }
    private void CacheButtons()
    {
        _buttons = [];

        foreach (var actionType in Enum.GetValues<PrimaryActionType>())
        {
            if (actionType == PrimaryActionType.None)
                continue;
            var button = GetNode<Button>($"{_buttonContainerPath}/{actionType}Button");
            Debug.Assert(button != null, $"[PrimaryActionSelect].CacheButtons - missing {actionType} button.");
            button.FocusEntered += () => OnButtonFocused(actionType);
            button.Pressed += () => OnButtonPressed(actionType);
            _buttons[actionType] = button;
        }
        
        _logger.Log($"CacheButtons - {_buttons.Count} buttons registered.", LogSeverity.Info, LogCategory.Initialization);
    }
    
    // ---------------------------------------------------------------------
    // Public Methods
    // ---------------------------------------------------------------------

    public bool TryGetButton(PrimaryActionType action, out Button button)
    {
        return _buttons.TryGetValue(action, out button);
    }

    public bool TryFocus(PrimaryActionType action)
    {
        if (!_buttons.TryGetValue(action, out var button))
            return false;
        button.GrabFocus();
        return true;
    }
    
    // ---------------------------------------------------------------------
    // Event Callbacks / Handlers
    // ---------------------------------------------------------------------

    private void OnButtonFocused(PrimaryActionType action)
    {
        _logger.Log($"OnButtonFocused - action={action}", LogSeverity.Trace, LogCategory.UiNavigation);
        EmitSignal(SignalName.ActionFocused, (int)action);
    }

    private void OnButtonPressed(PrimaryActionType action)
    {
        _logger.Log($"OnButtonPressed - action={action}", LogSeverity.Trace, LogCategory.UiNavigation);
        EmitSignal(SignalName.ActionSelected, (int)action);
    }
}