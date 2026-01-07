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
    [ExportGroup("Label Nodes")]
    [Export] public Label TerrainNameLabel;
    [Export] public Label TerrainDescriptionLabel;
    [Export] public Label CellLabel;
    [Export] public Label DefenseBonusLabel;
    [Export] public Label MovementCostLabel;

    private Vector2I _cell;
    private TerrainType _terrain;

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

    public override void _Ready()
    {
        Debug.Assert(TerrainNameLabel != null, "[TerrainInfoPanel].  Not Initialized. TerrainNameLabel is required.");
        Debug.Assert(TerrainDescriptionLabel != null, "[TerrainInfoPanel].  Not Initialized. TerrainDescriptionLabel is required.");
        Debug.Assert(CellLabel != null, "[TerrainInfoPanel].  Not Initialized. CellLabel is required.");
        Debug.Assert(DefenseBonusLabel != null, "[TerrainInfoPanel].  Not Initialized. DefenseBonusLabel is required.");
        Debug.Assert(MovementCostLabel != null, "[TerrainInfoPanel].  Not Initialized. MovementCostLabel is required.");
    }

    public void OnHoveredTerrainChanged(TerrainType terrain)
    {
        _terrain = terrain;
        UpdateTerrainLabels();
    }

    public void OnSelectedUnitChanged(Goblinos.Scripts.Battle.BattleUnit selectedUnit)
    {
        LogManager.Log("[TerrainInfoPanel] OnSelectedUnitChange - Unused", LogSeverity.Trace, LogCategory.Signal);
    }

    private void UpdateLabels()
    {
        UpdateCellLabels();
        UpdateTerrainLabels();
    }

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
