using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Preview;
using Goblinos.Scripts.Battle.Types;
using Godot;

namespace Goblinos.Scripts.UI.Battle;

public partial class PrimaryActionConfirm : Panel, IBattleHudPanel
{
    [Export] private Label _label;

    private readonly GobLogger _logger = GobLogManager.For<PrimaryActionConfirm>();

    private string _message;

    public string Message
    {
        get => _message;
        set
        {
            _message = value;
            UpdateMessageLabel();
        }
    }

    public override void _Ready()
    {
        Debug.Assert(_label != null, $"[{nameof(PrimaryActionConfirm)}] Initialization failed - missing _label.");
        _logger.Log("Ready", GobLogSeverity.Trace, GobLogCategory.Initialization);
    }

    public void SetContext(UnitActivationContext context)
    {
        var primaryActionType = context.PrimaryAction;

        switch (primaryActionType)
        {
            case PrimaryActionType.Ability:
                Message = $"{context.Unit.Ability.DisplayName}:\n{context.Unit.Ability.Description}";
                break;
            case PrimaryActionType.Attack:
            case PrimaryActionType.Item:
            case PrimaryActionType.Trade:
            case PrimaryActionType.Wait:
                Message = primaryActionType.ToString();
                break;
        }
    }

    private void UpdateMessageLabel()
    {
        _label.Text = _message;
    }
}