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
    public delegate void GridCursorFocusChangedEventHandler(Vector2I newCell, Vector2I oldCell);

    /** Events */

    [ExportGroup("Nodes")]
    
    /** Fields */
    [Export] private NodePath _battleGridPath;
    [Export] private NodePath _unitRegistryPath;

    private Logger _logger = LogManager.For<GridCursor>();

    private Vector2I _lastCellFocused = new(int.MinValue, int.MinValue);
    
    /** Properties */
    public Goblinos.Scripts.Battle.BattleGrid Grid;
    public UnitRegistry UnitRegistry;

    public Vector2I FocusedCell { get; private set; }

    public override void _Ready()
    {
        Grid = GetNode<Goblinos.Scripts.Battle.BattleGrid>(_battleGridPath);
        UnitRegistry = GetNode<UnitRegistry>(_unitRegistryPath);
        
        DebugUtil.Require(Grid != null, "[GridCursor] Grid must be initialized");
        DebugUtil.Require(UnitRegistry != null, "[GridCursor] UnitRegistry must be initialized");
        
        _UpdateFocus();
        
        _logger.Log("Ready", LogSeverity.Info, LogCategory.Initialization);
    }
    
    private void MoveDirection(Vector2I dir)
    {
        _logger.Log("Move " + dir, 0, LogCategory.UiNavigation);
        GlobalPosition += dir * InputUtil.TileSize;
        _UpdateFocus();
    }

    private void MoveToGlobalPosition(Vector2 globalPos)
    {
        _logger.Log("Move To " + globalPos, LogSeverity.Trace, LogCategory.UiNavigation);
        var cell = Grid.GetCellAtGlobalPosition(globalPos);
        MoveToCell(cell);
    }

    private void MoveToCell(Vector2I gridCell)
    {
        _logger.Log("Move To " + gridCell, LogSeverity.Extra, LogCategory.UiNavigation);
        if (gridCell == _lastCellFocused)
        {
            _logger.Log($"MoveTo no move, _lastCellFocused", LogSeverity.Extra, LogCategory.UiNavigation);
            return;
        }
        
        GlobalPosition = gridCell * GlobalSettings.TileSize + new Vector2(GlobalSettings.TileSize * 0.5f, GlobalSettings.TileSize * 0.5f);
        _UpdateFocus();
    }

    /// <summary>
    /// Checks if cursor movement one space in a given direction is possible
    /// according to BattleGrid, then moves cursor
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="cell">out property</param>
    /// <returns>true if able to move</returns>
    public bool TryMoveDirection(InputDirection dir, out Vector2I cell)
    {
        _logger.Log("TryMoveDirection", LogSeverity.Trace, LogCategory.UiNavigation);

        cell = FocusedCell + InputUtil.InputDirectionToVector2I(dir);
        return TryMoveToCell(cell);
    }
    
    public bool TryMoveDirection(InputDirection dir) => TryMoveDirection(dir, out _);

    /// <summary>
    /// Checks if cursor movement to a cell is possible
    /// according to BattleGrid, then moves cursor
    /// </summary>
    /// <param name="cell"></param>
    /// <returns>true if able to move</returns>
    public bool TryMoveToCell(Vector2I cell)
    {
        _logger.Log($"TryMoveToCell [cell]={cell}", LogSeverity.Extra, LogCategory.UiNavigation);

        if (cell == FocusedCell || !Grid.CanFocusCell(cell))
            return false;
        
        MoveToCell(cell);
        return true;
    }

    /// <summary>
    /// Checks if cursor movement to a given global position is possible
    /// according to BattleGrid, then moves cursor
    /// </summary>
    /// <param name="globalPos"></param>
    /// <param name="cell">out property, Vector2I grid cell associated with global pos</param>
    /// <returns>true if able to move</returns>
    public bool TryMoveToGlobalPosition(Vector2 globalPos, out Vector2I cell)
    {
        _logger.Log($"TryMoveToGlobalPosition [globalPos]={globalPos}", LogSeverity.Extra, LogCategory.UiNavigation);

        if (!Grid.CanFocusGlobalPosition(globalPos, out cell) || cell == FocusedCell)
            return false;
        
        MoveToCell(cell);
        return true;
    }

    public bool TryMoveToGlobalPosition(Vector2 globalPos) => TryMoveToGlobalPosition(globalPos, out _);

    private void _UpdateFocus()
    {
        var worldPos = GlobalPosition;
        var cell = Grid.GetCellAtGlobalPosition(worldPos);

        if (cell == _lastCellFocused)
        {
            _logger.Log($"_UpdateFocus no update, _lastCellFocused", LogSeverity.Extra, LogCategory.UiNavigation);
            return;
        }

        FocusedCell = cell;
        EmitSignal(SignalName.GridCursorFocusChanged, cell, _lastCellFocused);
        _lastCellFocused = cell;
        
        _logger.Log($"_UpdateFocus [FocusedCell]={FocusedCell}", LogSeverity.Extra, LogCategory.UiNavigation);
    }
}