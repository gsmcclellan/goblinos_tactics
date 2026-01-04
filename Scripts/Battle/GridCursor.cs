using System.Diagnostics;
using Godot;
using Goblinos.Scripts.Battle;
using Goblinos.Scripts.Battle.Terrain;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.Util;
using Godot.Collections;

public partial class GridCursor : Node2D
{
    
    
    /** Signals */
    [Signal]
    public delegate void GridCursorFocusChangedEventHandler(GridCursorFocus focus);

    /** Events */

    [ExportGroup("Nodes")]
    
    /** Fields */
    [Export] private NodePath _battleGridPath;
    [Export] private NodePath _battleUnitRegistryPath;

    private Vector2I _lastCellFocused = new(int.MinValue, int.MinValue);
    
    /** Properties */
    public GridCursorFocus Focus;
    public BattleGrid Grid;
    public BattleUnitRegistry UnitRegistry;

    public Vector2I FocusedCell => Focus.Cell;

    public override void _Ready()
    {
        Grid = GetNode<BattleGrid>(_battleGridPath);
        UnitRegistry = GetNode<BattleUnitRegistry>(_battleUnitRegistryPath);
        
        Debug.Assert(Grid != null, "[GridCursor] Grid must be initialized");
        Debug.Assert(UnitRegistry != null, "[GridCursor] UnitRegistry must be initialized");
        
        _UpdateFocus();
        
        DebugUtil.EnableOnlyCategories("UiNavigation", "Input");
    }
    
    public void MoveDirection(Vector2I dir)
    {
        DebugUtil.Log("[GridCursor] Move " + dir, 0, DebugLogCategory.UiNavigation);
        GlobalPosition += dir * InputUtil.TileSize;
        _UpdateFocus();
    }

    public void MoveToGlobalPosition(Vector2 globalPos)
    {
        DebugUtil.Log("[GridCursor] Move To" + globalPos, DebugLogSeverity.Trace, DebugLogCategory.UiNavigation);
        var cell = Grid.GetCellAtGlobalPosition(globalPos);
        MoveTo(cell);
    }

    public void MoveTo(Vector2I gridCell)
    {
        DebugUtil.Log("[GridCursor] Move To" + gridCell, DebugLogSeverity.Trace, DebugLogCategory.UiNavigation);
        if (gridCell == _lastCellFocused)
        {
            DebugUtil.Log($"[GridCursor] MoveTo no move, _lastCellFocused", DebugLogSeverity.Extra, DebugLogCategory.UiNavigation);
            return;
        }
        
        GlobalPosition = gridCell * GlobalSettings.TileSize + new Vector2(GlobalSettings.TileSize * 0.5f, GlobalSettings.TileSize * 0.5f);
        _UpdateFocus();
    }

    private void _UpdateFocus()
    {
        var worldPos = GlobalPosition;
        var cell = Grid.GetCellAtGlobalPosition(worldPos);

        if (cell == _lastCellFocused)
        {
            DebugUtil.Log($"[GridCursor] _UpdateFocus no update, _lastCellFocused", DebugLogSeverity.Extra, DebugLogCategory.UiNavigation);
            return;
        }

        var terrain = Grid.GetTerrainAtCell(cell);
        UnitRegistry.TryGetUnitAtCell(cell, out var unit);
            
        var nextFocus = new GridCursorFocus
        {
            Cell = cell,
            Terrain = terrain,
            Unit = unit,
            TopNode = unit // placeholder; TODO pick priority from Nodes
        };
        
        Focus = nextFocus;
        _lastCellFocused = cell;
        
        EmitSignal(SignalName.GridCursorFocusChanged, nextFocus);
        DebugUtil.Log($"[GridCursor] _UpdateFocus [Focus]={nextFocus}", DebugLogSeverity.Info, DebugLogCategory.UiNavigation);
    }
}

public partial class GridCursorFocus: RefCounted
{
    public Vector2I Cell { get; init; }
    public TerrainType Terrain { get; init; }
    public Goblinos.Scripts.Battle.BattleUnit? Unit { get; init; }
    public Node? TopNode { get; init; }
    public Godot.Collections.Array<Node> Nodes { get; init; } = new();
    public bool HasUnit => Unit != null;
}