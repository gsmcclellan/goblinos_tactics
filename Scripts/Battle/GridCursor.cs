using System.Diagnostics;
using Goblinos.Logging;
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

    private Logger _logger = LogManager.For<GridCursor>();

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
    }
    
    public void MoveDirection(Vector2I dir)
    {
        _logger.Log("Move " + dir, 0, LogCategory.UiNavigation);
        GlobalPosition += dir * InputUtil.TileSize;
        _UpdateFocus();
    }

    public void MoveToGlobalPosition(Vector2 globalPos)
    {
        _logger.Log("Move To" + globalPos, LogSeverity.Trace, LogCategory.UiNavigation);
        var cell = Grid.GetCellAtGlobalPosition(globalPos);
        MoveTo(cell);
    }

    public void MoveTo(Vector2I gridCell)
    {
        _logger.Log("Move To" + gridCell, LogSeverity.Trace, LogCategory.UiNavigation);
        if (gridCell == _lastCellFocused)
        {
            _logger.Log($"MoveTo no move, _lastCellFocused", LogSeverity.Extra, LogCategory.UiNavigation);
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
            _logger.Log($"_UpdateFocus no update, _lastCellFocused", LogSeverity.Extra, LogCategory.UiNavigation);
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
        _logger.Log($"_UpdateFocus [Focus]={nextFocus}", LogSeverity.Info, LogCategory.UiNavigation);
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