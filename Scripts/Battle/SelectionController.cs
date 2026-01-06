#nullable enable
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Terrain;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class SelectionController : Node
{
    /** Signals */
    [Signal]
    public delegate void HoveredTerrainChangedEventHandler(TerrainType? hoveredTerrain);
    [Signal]
    public delegate void HoveredUnitChangedEventHandler(Node? hoveredUnit);
    [Signal]
    public delegate void SelectedTerrainChangedEventHandler(TerrainType? selectedTerrain);
    [Signal]
    public delegate void SelectedUnitChangedEventHandler(Node? selectedUnit);

    
    // SelectedTerrain
    // HoveredTerrain
    // SelectedCell
    // HoveredCell

    /** Components */
    private Logger _logger = LogManager.For<SelectionController>();

    private GridCursor _cursor;
    private BattleGrid _grid;
    private UnitRegistry _unitRegistry;

    /** Fields */
    private Vector2I _hoveredCell;

    private TerrainType? _hoveredTerrain;
    private BattleUnit? _hoveredUnit;

    private Vector2I _selectedCell;
    private TerrainType _selectedTerrain;
    private BattleUnit? _selectedUnit;

    /** Properties */
    public BattleUnit? HoveredUnit => _hoveredUnit;

    public BattleUnit? SelectedUnit => _selectedUnit;

    public bool IsUnitHovered => _hoveredUnit != null;
    public bool IsUnitSelected => _selectedUnit != null;


    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------


    public override void _Ready()
    {
        _logger.Log("Ready", LogSeverity.Info, LogCategory.Initialization);
    }

    public void Bind(GridCursor cursor, BattleGrid grid, UnitRegistry unitRegistry)
    {
        _logger.Log("Bind", LogSeverity.Info, LogCategory.Initialization);
        _cursor = cursor;
        _grid = grid;
        _unitRegistry = unitRegistry;

        if (!DebugUtil.Require(_cursor != null, "[SelectionController] requires GridCursor binding") ||
            !DebugUtil.Require(_grid != null, "[SelectionController] requires BattleGrid binding") ||
            !DebugUtil.Require(_unitRegistry != null, "[SelectionController] requires SelectionController binding"))
            return;

        _SetupSubscriptions();

        UpdateHoveredFromCell(_cursor.FocusedCell);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
    }

    private void _SetupSubscriptions()
    {
        _cursor.GridCursorFocusChanged += OnCursorFocusChanged;
        
        _logger.Log("Subscriptions Initialized", LogSeverity.Info, LogCategory.Initialization);
    }

    private void _RemoveSubscriptions()
    {
        _cursor.GridCursorFocusChanged -= OnCursorFocusChanged;
        
        _logger.Log("Subscriptions Removed", LogSeverity.Info, LogCategory.Initialization);
    }

    // ---------------------------------------------------------------------
    // Public Methods
    // ---------------------------------------------------------------------

    // SelectHovered()
    // ClearSelection()

    // ---------------------------------------------------------------------
    // Event Handlers
    // ---------------------------------------------------------------------

    private void OnCursorFocusChanged(Vector2I newCell, Vector2I oldCell)
    {
        _logger.Log($"Cursor focus changed newCell={newCell}, oldCell={oldCell}", LogSeverity.Extra, LogCategory.UiNavigation);
        UpdateHoveredFromCell(newCell);
    }

    // ---------------------------------------------------------------------
    // Private Methods
    // ---------------------------------------------------------------------

    private void UpdateHoveredFromCell(Vector2I cell)
    {
        // cell -> unit, terrain
        _hoveredCell = cell;
        var hasTerrain = _grid.TryGetTerrainAtCell(cell, out var hoveredTerrain);
        var hasUnit = _unitRegistry.TryGetUnitAtCell(cell, out var hoveredUnit);

        if (hasTerrain && hoveredTerrain != _hoveredTerrain)
            SetHoveredTerrain(hoveredTerrain);

        if (hasUnit && hoveredUnit != _hoveredUnit)
            SetHoveredUnit(hoveredUnit);
        
        _logger.Log($"Update hovered from cell={cell}, terrain={hoveredTerrain?.Id}, unit={hoveredUnit?.Name}", LogSeverity.Trace, LogCategory.UiNavigation);
    }

    private void SetHoveredTerrain(TerrainType? terrain)
    {
        _logger.Log($"Update hovered terrain={terrain?.Id}", LogSeverity.Extra, LogCategory.UiNavigation);
        _hoveredTerrain = terrain;
        EmitSignalHoveredTerrainChanged(terrain);
    }

    private void SetHoveredUnit(BattleUnit? unit)
    {
        _logger.Log($"Update hovered terrain={unit?.Name}", LogSeverity.Extra, LogCategory.UiNavigation);
        _hoveredUnit = unit;
        EmitSignalHoveredUnitChanged(unit);
    }
}