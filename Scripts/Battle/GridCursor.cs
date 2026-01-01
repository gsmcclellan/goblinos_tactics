using Godot;
using System;
using System.Collections.Generic;
using Goblinos.Scripts.Battle;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.Util;

public partial class GridCursor : Node2D
{
    /** Signals */
    
    /** Events */
    public event Action<GridCursorFocus> GridCursorFocusChanged;

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
        DebugUtil.Log("[GridCursor] Move To" + globalPos, 0, DebugLogCategory.UiNavigation);
        var cell = Grid.GetCellAtGlobalPosition(globalPos);
        if (cell == _lastCellFocused) return;
        
        GlobalPosition = cell * GlobalSettings.TileSize + new Vector2(GlobalSettings.TileSize * 0.5f, GlobalSettings.TileSize * 0.5f);
        _UpdateFocus();
    }

    private void _UpdateFocus()
    {
        var worldPos = GlobalPosition;
        var cell = Grid.GetCellAtGlobalPosition(worldPos);

        if (cell == _lastCellFocused) return;
        
        var nextFocus = new GridCursorFocus
        {
            Cell = cell,
            // Unit = Grid.TryGetUnitAt(cell),   // or however you query units
            // TopNode = Grid.TryGetUnitAt(cell) // placeholder; later pick priority from Nodes
        };
        
        Focus = nextFocus;
        _lastCellFocused = cell;
        DebugUtil.Log($"[GridCursor] _UpdateFocus [Focus]={nextFocus}", 0, DebugLogCategory.UiNavigation);
    }
}

public sealed class GridCursorFocus
{
    public Vector2I Cell { get; init; }
    // public Tile Tile { get; init; }          // or TileData / your tile wrapper
    public Unit? Unit { get; init; }
    public Node? TopNode { get; init; }      // optional “best candidate”
    public IReadOnlyList<Node> Nodes { get; init; } = Array.Empty<Node>();

    // public bool HasTile => Tile != null;
    public bool HasUnit => Unit != null;
}