#nullable enable
using Godot;
using System;
using System.Diagnostics;
using System.Globalization;
using Goblinos.Logging;
using Goblinos.Scripts.Battle;
using Goblinos.Scripts.Battle.Terrain;
using Goblinos.Scripts.UI.Battle;
using Goblinos.Scripts.Util;

public partial class TerrainInfoPanel : Panel, IBattleHudPanel
{
    private readonly Logger _logger = LogManager.For<TerrainInfoPanel>();
    
    [ExportGroup("Label Nodes")]
    [Export] public Label TerrainNameLabel;
    [Export] public Label TerrainDescriptionLabel;
    [Export] public Label CellLabel;
    [Export] public Label DefenseBonusLabel;
    [Export] public Label MovementCostLabel;

    private Vector2I _cell;
    private TerrainType? _terrain;

    public Vector2I Cell
    {
        get => _cell;
        set
        {
            _cell = value;
            UpdateCellLabels();
        }
    }

    public TerrainType Terrain
    {
        get => _terrain;
        set
        {
            _terrain = value;
            UpdateTerrainLabels();
        }
    }
    
    // ---------------------------------------------------------------------
    // Lifecycle / Init Callbacks
    // ---------------------------------------------------------------------

    public override void _Ready()
    {
        Debug.Assert(TerrainNameLabel != null, "[TerrainInfoPanel].  Not Initialized. TerrainNameLabel is required.");
        Debug.Assert(TerrainDescriptionLabel != null, "[TerrainInfoPanel].  Not Initialized. TerrainDescriptionLabel is required.");
        Debug.Assert(CellLabel != null, "[TerrainInfoPanel].  Not Initialized. CellLabel is required.");
        Debug.Assert(DefenseBonusLabel != null, "[TerrainInfoPanel].  Not Initialized. DefenseBonusLabel is required.");
        Debug.Assert(MovementCostLabel != null, "[TerrainInfoPanel].  Not Initialized. MovementCostLabel is required.");
    }
    
    // ---------------------------------------------------------------------
    // Signal / Event Callbacks
    // ---------------------------------------------------------------------

    public void OnHoveredCellChanged(Vector2I newCell, Vector2I oldCell)
    {
        _logger.Log($"[{nameof(OnHoveredCellChanged)}] newCell={newCell}, oldCell={oldCell}", LogSeverity.Trace, LogCategory.UiNavigation);
        _cell = newCell;
        UpdateCellLabels();
    }
    
    public void OnHoveredTerrainChanged(TerrainType? terrain)
    {
        _terrain = terrain;
        UpdateTerrainLabels();
    }

    public void OnSelectedUnitChanged(BattleUnit? selectedUnit)
    {
        // Intentionally unused for this panel.
    }

    private void UpdateLabels()
    {
        UpdateCellLabels();
        UpdateTerrainLabels();
    }
    
    // ---------------------------------------------------------------------
    // Label Update Methods
    // ---------------------------------------------------------------------

    private void UpdateCellLabels()
    {
        CellLabel.Text = _cell.ToString();
    }

    private void UpdateTerrainLabels()
    {
        TerrainNameLabel.Text = _terrain?.DisplayName ?? "";
        TerrainDescriptionLabel.Text = _terrain?.Description ?? "";
        DefenseBonusLabel.Text = _terrain?.DefenseBonus.ToString() ?? "";
        MovementCostLabel.Text = _terrain?.MoveCost.ToString(CultureInfo.CurrentCulture) ?? "";
    }
    
    
}
