#nullable enable
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle;
using Godot;

namespace Goblinos.Scripts.UI.Battle;

public partial class UnitInfoPanel : Panel, IBattleHudPanel
{
    private readonly Logger _logger = LogManager.For<UnitInfoPanel>();
    [ExportGroup("Label Nodes")]
    [Export] public Label? UnitNameLabel;
    [Export] public Label? IsFriendlyLabel;
    [Export] public Label? HitPointsLabel;
    [Export] public Label? PowerLabel;

    private BattleUnit? _hoveredUnit;
    private BattleUnit? _selectedUnit;
    
    public BattleUnit? Unit => _selectedUnit ?? _hoveredUnit;
    
    // ---------------------------------------------------------------------
    // Lifecycle / Init Callbacks
    // ---------------------------------------------------------------------

    public override void _Ready()
    {
        Debug.Assert(UnitNameLabel != null, "[UnitInfoPanel].  Not Initialized. UnitNameLabel is required.");
        Debug.Assert(IsFriendlyLabel != null, "[UnitInfoPanel].  Not Initialized. IsFriendlyPanel is required.");
        Debug.Assert(HitPointsLabel != null, "[UnitInfoPanel].  Not Initialized. HitPointsLabel is required.");
        Debug.Assert(PowerLabel != null, "[UnitInfoPanel].  Not Initialized. PowerLabel is required.");
    }
    // ---------------------------------------------------------------------
    // Signal / Event Callbacks
    // ---------------------------------------------------------------------

    public void OnHoveredUnitChanged(BattleUnit? hoveredUnit)
    {
        _logger.Log($"{nameof(OnHoveredUnitChanged)} - hoveredUnit={hoveredUnit?.UnitName}", LogSeverity.Extra, LogCategory.Signal);
        SetHoveredUnit(hoveredUnit);
    }

    public void OnSelectedUnitChanged(BattleUnit? selectedUnit)
    {
        _logger.Log($"{nameof(OnSelectedUnitChanged)} - selectedUnit={selectedUnit?.UnitName}", LogSeverity.Extra, LogCategory.Signal);
        SetSelectedUnit(selectedUnit);
    }
    
    // ---------------------------------------------------------------------
    // Label Update Methods
    // ---------------------------------------------------------------------

    private void SetHoveredUnit(BattleUnit? unit)
    {
        _hoveredUnit = unit;
        
        if (_selectedUnit != null)
            return;
        
        UpdateUnitLabels();
    }
    private void SetSelectedUnit(BattleUnit? unit)
    {
        _selectedUnit = unit;
        UpdateUnitLabels();
    }

    private void UpdateUnitLabels() {
        _logger.Log($"{nameof(UpdateUnitLabels)} - Unit={Unit?.UnitName}", LogSeverity.Extra, LogCategory.UiNavigation);
        if (UnitNameLabel != null) UnitNameLabel.Text = Unit?.UnitName ?? "";
        if (IsFriendlyLabel != null) IsFriendlyLabel.Text = Unit?.IsFriendly.ToString() ?? "";
        // if (HitPointsLabel != null) HitPointsLabel.Text = _unit?.Stats.HitPoints.ToString() ?? "";
        // if (PowerLabel != null) PowerLabel.Text = _unit?.Stats.Power.ToString() ?? "";
    }
}