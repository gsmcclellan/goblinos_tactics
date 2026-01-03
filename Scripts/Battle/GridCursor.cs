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

    /** Nodes */
    [ExportGroup("Nodes")]
    [Export] public BattleController Controller;
    [Export] public BattleGrid Grid;
    
    /** Properties */
    public GridCursorFocus Focus;
    
    
    private Vector2I _lastCellFocused = new(int.MinValue, int.MinValue);

    public override void _Ready()
    {
        _UpdateFocus();
    }
    
    public void Move(Vector2I dir)
    {
        DebugUtil.Log("[GridCursor] Move " + dir, 0, DebugLogCategory.UiNavigation);
        GlobalPosition += dir * InputUtil.TileSize;
        _UpdateFocus();
    }

    public void MoveTo(Vector2 globalPos)
    {
        DebugUtil.Log("[GridCursor] Move To" + globalPos, DebugLogSeverity.Trace, DebugLogCategory.UiNavigation);
        var cell = Grid.GetCellAtGlobalPosition(globalPos);
        if (cell == _lastCellFocused)
        {
            DebugUtil.Log($"[GridCursor] MoveTo no move, _lastCellFocused", DebugLogSeverity.Extra, DebugLogCategory.UiNavigation);
            return;
        }
        
        
        GlobalPosition = cell * GlobalSettings.TileSize + new Vector2(GlobalSettings.TileSize * 0.5f, GlobalSettings.TileSize * 0.5f);
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
        
        var nextFocus = new GridCursorFocus
        {
            Cell = cell,
            Terrain = Grid.GetTerrainAtCell(cell)
            // Unit = Grid.TryGetUnitAt(cell),   // or however you query units
            // TopNode = Grid.TryGetUnitAt(cell) // placeholder; later pick priority from Nodes
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