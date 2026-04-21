#nullable enable
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Units.Stats.Types;
using Godot;
using BattleUnit = Goblinos.Scripts.Battle.Units.BattleUnit;

namespace Goblinos.Scripts.UI.Battle;

public partial class UnitInfoPanel : Panel, IBattleHudPanel
{
    private readonly GobLogger _logger = GobLogManager.For<UnitInfoPanel>();
    [ExportGroup("Label Nodes")]
    [Export] public Label UnitNameLabel = null!;
    [Export] public Label IsFriendlyLabel = null!;
    [Export] public Label HitPointsLabel = null!;
    [Export] public Label MightLabel = null!;
    [Export] public Label AgilityLabel = null!;
    [Export] public Label VitalityLabel = null!;
    [Export] public Label MindLabel = null!;
    [Export] public Label PresenceLabel = null!;
    [Export] public Label LevelLabel = null!;
    [Export] public Label LuckLabel = null!;
    [Export] public Label DefenseLabel = null!;
    [Export] public Label ResistanceLabel = null!;
    [Export] public Label MovementLabel = null!;

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
        Debug.Assert(MightLabel != null, "[UnitInfoPanel].  Not Initialized. MightLabel is required.");
        Debug.Assert(AgilityLabel != null, "[UnitInfoPanel].  Not Initialized. AgilityLabel is required.");
        Debug.Assert(VitalityLabel != null, "[UnitInfoPanel].  Not Initialized. VitalityLabel is required.");
        Debug.Assert(MindLabel != null, "[UnitInfoPanel].  Not Initialized. MindLabel is required.");
        Debug.Assert(PresenceLabel != null, "[UnitInfoPanel].  Not Initialized. PresenceLabel is required.");
        Debug.Assert(LevelLabel != null, "[UnitInfoPanel].  Not Initialized. LevelLabel is required.");
        Debug.Assert(LuckLabel != null, "[UnitInfoPanel].  Not Initialized. LuckLabel is required.");
        Debug.Assert(DefenseLabel != null, "[UnitInfoPanel].  Not Initialized. DefenseLabel is required.");
        Debug.Assert(ResistanceLabel != null, "[UnitInfoPanel].  Not Initialized. ResistanceLabel is required.");
        Debug.Assert(MovementLabel != null, "[UnitInfoPanel].  Not Initialized. MovementLabel is required.");
        
        SetVisible();
    }
    // ---------------------------------------------------------------------
    // Signal / Event Callbacks
    // ---------------------------------------------------------------------

    public void OnHoveredUnitChanged(BattleUnit? hoveredUnit)
    {
        _logger.Log($"{nameof(OnHoveredUnitChanged)} - hoveredUnit={hoveredUnit?.UnitName}", GobLogSeverity.Extra, GobLogCategory.Signal);
        SetHoveredUnit(hoveredUnit);
    }

    public void OnSelectedUnitChanged(BattleUnit? selectedUnit)
    {
        _logger.Log($"{nameof(OnSelectedUnitChanged)} - selectedUnit={selectedUnit?.UnitName}", GobLogSeverity.Extra, GobLogCategory.Signal);
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
        SetVisible();
    }
    private void SetSelectedUnit(BattleUnit? unit)
    {
        _selectedUnit = unit;
        UpdateUnitLabels();
        SetVisible();
    }

    private void SetVisible()
    {
        Visible = Unit != null;
    }

    private void UpdateUnitLabels() {
        _logger.Log($"{nameof(UpdateUnitLabels)} - Unit={Unit?.UnitName}", GobLogSeverity.Extra, GobLogCategory.UiNavigation);
        UpdateNameLabel(Unit);
        UpdateIsFriendlyLabel(Unit);
        UpdateHitPointsLabel(Unit);
        UpdateLevelLabel(Unit);
        
        foreach(var statName in StatNameInfo.Stats)
            UpdateStatLabel(Unit, statName);
    }


    private void UpdateLevelLabel(BattleUnit? unit)
    {
        var text = "";
        if (unit != null)
            text = $"Lv. {unit.Level}";
        LevelLabel.Text = text;
    }

    private void UpdateNameLabel(BattleUnit? unit)
    {
        var text = "";
        if (unit != null)
            text = unit.UnitName;
        UnitNameLabel.Text = text;
    }

    private void UpdateIsFriendlyLabel(BattleUnit? unit)
    {
        var text = "";
        if (unit != null)
            text = unit.IsFriendly ? "(Friend)" : "(Foe)";
        IsFriendlyLabel.Text = text;
    }

    private void UpdateHitPointsLabel(BattleUnit? unit)
    {
        var text = $"{unit?.CurrentHitPoints.ToString() ?? ""} / {unit?.MaxHitPoints.ToString() ?? ""} hp";
        HitPointsLabel.Text = text;
    }

    private void UpdateStatLabel(BattleUnit? unit, StatName statName)
    {
        var label = GetStatLabel(statName);
        if (label == null)
            return;
        
        label.Text = (unit != null) ? $"{StatNameInfo.GetAbbreviatedDisplayName(statName)}: {unit.GetStat(statName)}" : "";
    }
    
    private Label? GetStatLabel(StatName statName)
    {
        return statName switch
        {
            StatName.Might => MightLabel,
            StatName.Agility => AgilityLabel,
            StatName.Vitality => VitalityLabel,
            StatName.Mind => MindLabel,
            StatName.Presence => PresenceLabel,
            StatName.Luck => LuckLabel,
            
            StatName.Defense => DefenseLabel,
            StatName.Resistance => ResistanceLabel,
            StatName.Movement => MovementLabel,
            _ => null
        };
    }
}