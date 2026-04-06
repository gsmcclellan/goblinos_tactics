using System;
using System.Collections.Generic;
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Combat;
using Goblinos.Scripts.Units;
using Godot;

namespace Goblinos.Scripts.UI.Battle;

public partial class PrimaryActionSelect : Panel, IBattleHudPanel
{
    /** Signals */
    [Signal]
    public delegate void ActionFocusedEventHandler(int action);
    [Signal]
    public delegate void ActionSelectedEventHandler(int action);
    
    
    /** Components */
    private readonly Logger _logger = LogManager.For<PrimaryActionSelect>();

    private NodePath _buttonContainerPath = "ButtonContainer";
    private Dictionary<PrimaryActionType, Button> _buttons = new();
    
    private Button? hoveredButton;
    private Button? selectedButton;

    private PrimaryActionType? hoveredAction;
    private PrimaryActionType? selectedAction;
    
    
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

    /// <summary>
    /// Enables or disables the specified action button.
    /// Disabled actions cannot be focused or pressed.
    /// </summary>
    public void SetActionEnabled(PrimaryActionType actionType, bool isEnabled)
    {
        _logger.Log($"{nameof(SetActionEnabled)} action={actionType} isEnabled={isEnabled}", LogSeverity.Trace, LogCategory.UiNavigation);

        if (!_buttons.TryGetValue(actionType, out var button))
            return;

        button.Disabled = !isEnabled;

        if (!isEnabled && button.HasFocus())
            ReleaseFocus();
    }

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
    
    /// <summary>
    /// Focuses the first enabled action from the provided priority order.
    /// Returns false if none are enabled.
    /// </summary>
    public bool TryFocusFirstEnabled(IEnumerable<PrimaryActionType> priorityOrder)
    {
        _logger.Log($"{nameof(TryFocusFirstEnabled)}", LogSeverity.Trace, LogCategory.UiNavigation);

        foreach (var action in priorityOrder)
        {
            if (!_buttons.TryGetValue(action, out var button))
                continue;

            if (button.Disabled)
                continue;

            button.GrabFocus();
            return true;
        }

        return false;
    }
    public bool TryFocusFirstEnabled() => TryFocusFirstEnabled(PrimaryActionInfo.PrimaryActionOrder);
    
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

    public void OnSelectedUnitChanged(BattleUnit? selectedUnit)
    {
        _logger.Log($"OnSelectedUnitChanged - unit={selectedUnit?.UnitName}", LogSeverity.Trace, LogCategory.UiNavigation);
        var abilityButton = _buttons[PrimaryActionType.Ability];
        
        abilityButton.Text = selectedUnit?.Ability?.DisplayName ?? "None";
        abilityButton.Disabled = selectedUnit?.Ability?.Type == AbilityType.None;
        
        return;
    }
}