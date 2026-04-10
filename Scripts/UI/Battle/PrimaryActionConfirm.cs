using System.Diagnostics;
using Goblinos.Logging;
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

    private void UpdateMessageLabel()
    {
        _label.Text = _message;
    }
}