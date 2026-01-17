using Godot;
using System;
using Goblinos.Scripts.Battle;
using Goblinos.Scripts.Battle.Terrain;
using Goblinos.Scripts.UI.Battle;

public partial class UnitInfoPanel : Panel, IBattleHudPanel
{
    [ExportGroup("Label Nodes")]
    [Export] public Label UnitNameLabel;
    [Export] public Label HitPointsLabel;
    [Export] public Label PowerLabel;
    
    private Vector2I _cell;
    private TerrainType _terrain;
    
    public void OnHoveredCellChanged(Vector2I newCell, Vector2I oldCell)
    {
        throw new NotImplementedException();
    }

    public void OnHoveredTerrainChanged(TerrainType terrain)
    {
        throw new NotImplementedException();
    }

    public void OnSelectedUnitChanged(BattleUnit selectedUnit)
    {
        throw new NotImplementedException();
    }
}
