using Goblinos.Logging;
using Goblinos.Scripts.Battle.Core.Types;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Core;

public partial class GridCursor : Node2D
{
    
    
    /** Signals */
    [Signal]
    public delegate void GridCursorFocusChangedEventHandler(Vector2I newCell, Vector2I oldCell, int gridCursorFocusSource);

    /** Events */
    
    /** Components */
    private GobLogger _logger = GobLogManager.For<GridCursor>();
    
    /** Fields */
    [ExportGroup("Nodes")]
    [Export] private NodePath _battleGridPath;
    [Export] private NodePath _unitRegistryPath;

    private Vector2I _lastCellFocused = new(int.MinValue, int.MinValue);
    
    /** Properties */
    public BattleGrid Grid;
    public Units.UnitRegistry UnitRegistry;

    public Vector2I FocusedCell { get; private set; }
    
    // ---------------------------------------------------------------------
    // Lifecycle / Init Callbacks
    // ---------------------------------------------------------------------

    public override void _Ready()
    {
        Grid = GetNode<BattleGrid>(_battleGridPath);
        UnitRegistry = GetNode<Units.UnitRegistry>(_unitRegistryPath);
        
        DebugUtil.Require(Grid != null, "[GridCursor] Grid must be initialized");
        DebugUtil.Require(UnitRegistry != null, "[GridCursor] UnitRegistry must be initialized");
        
        _UpdateFocus(GridCursorFocusSource.Programmatic);
        
        _logger.Log("Ready", GobLogSeverity.Info, GobLogCategory.Initialization);
    }
    
    // ---------------------------------------------------------------------
    // Public Methods
    // ---------------------------------------------------------------------

    /// <summary>
    /// Checks if cursor movement one space in a given direction is possible
    /// according to BattleGrid, then moves cursor
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="moveSource"></param>
    /// <param name="cell">out property</param>
    /// <returns>true if able to move</returns>
    public bool TryMoveDirection(InputDirection dir, GridCursorFocusSource moveSource, out Vector2I cell)
    {
        _logger.Log("TryMoveDirection", GobLogSeverity.Extra, GobLogCategory.UiNavigation);

        cell = FocusedCell + InputUtil.InputDirectionToVector2I(dir);
        return TryMoveToCell(cell, moveSource);
    }
    
    public bool TryMoveDirection(InputDirection dir, GridCursorFocusSource moveSource) => TryMoveDirection(dir, moveSource, out _);

    /// <summary>
    /// Checks if cursor movement to a cell is possible
    /// according to BattleGrid, then moves cursor
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="moveSource"></param>
    /// <returns>true if able to move</returns>
    public bool TryMoveToCell(Vector2I cell, GridCursorFocusSource moveSource)
    {
        _logger.Log($"TryMoveToCell [cell]={cell}", GobLogSeverity.Extra, GobLogCategory.UiNavigation);

        if (cell == FocusedCell || !Grid.CanFocusCell(cell))
            return false;
        
        MoveToCell(cell, moveSource);
        return true;
    }

    /// <summary>
    /// Checks if cursor movement to a given global position is possible
    /// according to BattleGrid, then moves cursor
    /// </summary>
    /// <param name="globalPos"></param>
    /// <param name="moveSource"></param>
    /// <param name="cell">out property, Vector2I grid cell associated with global pos</param>
    /// <returns>true if able to move</returns>
    public bool TryMoveToGlobalPosition(Vector2 globalPos, GridCursorFocusSource moveSource, out Vector2I cell)
    {
        _logger.Log($"TryMoveToGlobalPosition [globalPos]={globalPos}", GobLogSeverity.Extra, GobLogCategory.UiNavigation);

        if (!Grid.CanFocusGlobalPosition(globalPos, out cell) || cell == FocusedCell)
            return false;
        
        MoveToCell(cell, moveSource);
        return true;
    }

    public bool TryMoveToGlobalPosition(Vector2 globalPos, GridCursorFocusSource moveSource) => TryMoveToGlobalPosition(globalPos, moveSource, out _);
    
    public void TriggerUpdateFocus(GridCursorFocusSource inputSource)
    {
        _UpdateFocus(inputSource);
    }
    
    // ---------------------------------------------------------------------
    // Private Helper Methods
    // ---------------------------------------------------------------------
    
    private void MoveDirection(Vector2I dir, GridCursorFocusSource inputSource)
    {
        _logger.Log("Move " + dir, GobLogSeverity.Extra, GobLogCategory.UiNavigation);
        GlobalPosition += dir * InputUtil.TileSize;
        _UpdateFocus(inputSource);
    }

    private void MoveToGlobalPosition(Vector2 globalPos, GridCursorFocusSource moveSource)
    {
        _logger.Log("Move To " + globalPos, GobLogSeverity.Trace, GobLogCategory.UiNavigation);
        var cell = Grid.GetCellAtGlobalPosition(globalPos);
        MoveToCell(cell, moveSource);
    }

    private void MoveToCell(Vector2I gridCell, GridCursorFocusSource moveSource)
    {
        _logger.Log("Move To " + gridCell, GobLogSeverity.Extra, GobLogCategory.UiNavigation);
        if (gridCell == _lastCellFocused)
        {
            _logger.Log($"MoveTo no move, _lastCellFocused", GobLogSeverity.Extra, GobLogCategory.UiNavigation);
            return;
        }
        
        GlobalPosition = gridCell * GlobalSettings.TileSize + new Vector2(GlobalSettings.TileSize * 0.5f, GlobalSettings.TileSize * 0.5f);
        _UpdateFocus(moveSource);
    }
    
    private void _UpdateFocus(GridCursorFocusSource inputSource)
    {
        var worldPos = GlobalPosition;
        var cell = Grid.GetCellAtGlobalPosition(worldPos);

        if (cell == _lastCellFocused)
        {
            _logger.Log($"_UpdateFocus no update, _lastCellFocused", GobLogSeverity.Extra, GobLogCategory.UiNavigation);
            return;
        }

        FocusedCell = cell;
        EmitSignal(SignalName.GridCursorFocusChanged, cell, _lastCellFocused, (int)inputSource);
        _lastCellFocused = cell;
        
        _logger.Log($"_UpdateFocus [FocusedCell]={FocusedCell}", GobLogSeverity.Extra, GobLogCategory.UiNavigation);
    }
}

