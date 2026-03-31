#nullable enable
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Preview;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Battle.Units;
using Godot;

namespace Goblinos.Scripts.UI.Battle;

public partial class CombatPreviewPanel : Panel, IBattleHudPanel
{
    private readonly Logger _logger = LogManager.For<CombatPreviewPanel>();
    
    [ExportGroup("Label Nodes")]
    [Export] public Label AttackerNameLabel = null!;
    [Export] public Label AttackerCurrentHitPointsLabel = null!;
    [Export] public Label AttackerExpectedHitPointsLabel = null!;
    [Export] public Label DefenderNameLabel = null!;
    [Export] public Label DefenderCurrentHitPointsLabel = null!;
    [Export] public Label DefenderExpectedHitPointsLabel = null!;
   
    private PrimaryActionType? hoveredAction;
    private PrimaryActionType? selectedAction;
    
    private BattleUnit? _hoveredUnit;
    private BattleUnit? _selectedUnit;
    
    public BattleUnit? Unit => _selectedUnit ?? _hoveredUnit;
    
    // ---------------------------------------------------------------------
    // Lifecycle / Init Callbacks
    // ---------------------------------------------------------------------

    public override void _Ready()
    {
        Debug.Assert(AttackerNameLabel != null, "[UnitInfoPanel].  Not Initialized. AttackerNameLabel is required.");
        Debug.Assert(AttackerCurrentHitPointsLabel != null, "[UnitInfoPanel].  Not Initialized. AttackerCurrentHitPointsLabel is required.");
        Debug.Assert(AttackerExpectedHitPointsLabel != null, "[UnitInfoPanel].  Not Initialized. AttackerExpectedHitPointsLabel is required.");
        Debug.Assert(DefenderNameLabel != null, "[UnitInfoPanel].  Not Initialized. DefenderNameLabel is required.");
        Debug.Assert(DefenderCurrentHitPointsLabel != null, "[UnitInfoPanel].  Not Initialized. DefenderCurrentHitPointsLabel is required.");
        Debug.Assert(DefenderExpectedHitPointsLabel != null, "[UnitInfoPanel].  Not Initialized. DefenderExpectedHitPointsLabel is required.");
        
        SetVisible(false);
    }

    public void OnBattleInputStateChanged(int s)
    {
        var state = (BattleInputState) s;
        _logger.Log($"OnBattleInputStateChanged - state={state.ToString()}", LogSeverity.Info, LogCategory.UiNavigation);

        switch (state)
        {
            case BattleInputState.FreeSelect:
            case BattleInputState.MoveTargeting:
            case BattleInputState.PrimaryActionSelect:
                SetVisible(false);
                break;
            case BattleInputState.PrimaryActionTargeting:
            case BattleInputState.PrimaryActionConfirm:
                
            default:
                break;
        }
    }

    public void OnCombatPreviewUpdated(CombatPreview? combatPreview)
    {
        _logger.Log($"OnCombatPreviewUpdated - hasPreview={combatPreview != null}", LogSeverity.Info, LogCategory.UiNavigation);
        if (combatPreview == null)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);
        AttackerNameLabel.Text = combatPreview.Attacker.UnitName;
        AttackerCurrentHitPointsLabel.Text = FormatHitPointsString(combatPreview.Attacker.CurrentHitPoints, combatPreview.Attacker.MaxHitPoints);
        AttackerExpectedHitPointsLabel.Text = FormatHitPointsString(combatPreview.AttackerExpectedHitPoints, combatPreview.Attacker.MaxHitPoints);
        DefenderNameLabel.Text = combatPreview.Defender.UnitName;
        DefenderCurrentHitPointsLabel.Text = FormatHitPointsString(combatPreview.Defender.CurrentHitPoints, combatPreview.Defender.MaxHitPoints);
        DefenderExpectedHitPointsLabel.Text = FormatHitPointsString(combatPreview.DefenderExpectedHitPoints, combatPreview.Defender.MaxHitPoints);
    }

    private string FormatHitPointsString(int num, int denom) => $"{num} / {denom} HP";
}