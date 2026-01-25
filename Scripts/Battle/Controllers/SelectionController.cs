#nullable enable
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Preview;
using Goblinos.Scripts.Battle.Terrain;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Controllers;

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

    private Core.GridCursor _cursor;
    private Core.BattleGrid _grid;
    private Units.UnitRegistry _unitRegistry;

    /** Fields */
    private Vector2I _hoveredCell;

    private TerrainType? _hoveredTerrain;
    private Units.BattleUnit? _hoveredUnit;

    // private Vector2I _selectedCell;
    // private TerrainType _selectedTerrain;
    private Units.BattleUnit? _selectedUnit;

    /** Properties */
    public CellFocus Focus => new CellFocus(_hoveredCell, _hoveredTerrain, _hoveredUnit);
    public Vector2I HoveredCell => _hoveredCell;
    public Units.BattleUnit? HoveredUnit => _hoveredUnit;

    public Vector2I? SelectedCell { 
        get 
        {
            if (_selectedUnit == null) return null;
            
            if (_unitRegistry.TryGetCell(_selectedUnit, out var cell))
                return cell;

            return null;
        }
    }
    public Units.BattleUnit? SelectedUnit => _selectedUnit;

    public bool IsUnitHovered => _hoveredUnit != null;
    public bool IsUnitSelected => _selectedUnit != null;


    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------

    public override void _Ready()
    {
        _logger.Log("Ready", LogSeverity.Info, LogCategory.Initialization);
    }

    public void Bind(Core.GridCursor cursor, Core.BattleGrid grid, Units.UnitRegistry unitRegistry)
    {
        _logger.Log("Bind", LogSeverity.Info, LogCategory.Initialization);
        if (_cursor != null || _grid != null || _unitRegistry != null)
            _UnsubscribeFromEvents();
        
        _cursor = cursor;
        _grid = grid;
        _unitRegistry = unitRegistry;

        if (!DebugUtil.Require(_cursor != null, "[SelectionController] requires GridCursor binding") ||
            !DebugUtil.Require(_grid != null, "[SelectionController] requires BattleGrid binding") ||
            !DebugUtil.Require(_unitRegistry != null, "[SelectionController] requires SelectionController binding"))
            return;

        _SubscribeToEvents();

        UpdateHoveredFromCell(_cursor.FocusedCell);
    }

    public override void _ExitTree()
    {
        _UnsubscribeFromEvents();
        base._ExitTree();
    }

    private void _SubscribeToEvents()
    {
        if (_cursor != null)
            _cursor.GridCursorFocusChanged += OnCursorFocusChanged;
        
        _logger.Log("Subscriptions Initialized", LogSeverity.Info, LogCategory.Initialization);
    }

    private void _UnsubscribeFromEvents()
    {   if (_cursor != null)
            _cursor.GridCursorFocusChanged -= OnCursorFocusChanged;
        
        _logger.Log("Subscriptions Removed", LogSeverity.Info, LogCategory.Initialization);
    }

    // ---------------------------------------------------------------------
    // Public Methods
    // ---------------------------------------------------------------------

    public CellFocus GetFocus(Vector2I cell)
    {
        _grid.TryGetTerrainAtCell(cell, out var terrain);
        _unitRegistry.TryGetUnitAtCell(cell, out var unit);
        return new CellFocus(cell, terrain, unit);
    }
    public void SelectCell(Vector2I cell, out Node? selectedNode)
    {
        if (_unitRegistry.TryGetUnitAtCell(cell, out var unit))
        {
            SelectUnit(unit);
            selectedNode = unit;
            return;
        }

        selectedNode = null;
    }

    public void SelectCell(Vector2I cell) => SelectCell(cell, out _);
    
    public void SelectUnit(Units.BattleUnit unit)
    {
        if (unit == _selectedUnit) return;
        
        if (unit.State == UnitActivationState.Exhausted)
        {
            _logger.Log($"SelectUnit blocked: unit exhausted unit={unit.UnitName}", LogSeverity.Trace, LogCategory.UiNavigation);
            return;
        }
        
        _selectedUnit?.Deselect();
        unit.Select();
        _selectedUnit = unit;
        _logger.Log($"Unit Selected unit={unit?.UnitName}", LogSeverity.Info, LogCategory.UiNavigation);
        EmitSignalSelectedUnitChanged(_selectedUnit);
    }
    
    public void TriggerClearSelection()
    {
        _logger.Log("TriggerSelection", LogSeverity.Trace, LogCategory.Input);
        DeselectUnit();
    }
    
    public void TriggerSelection()
    {
        _logger.Log("TriggerSelection", LogSeverity.Trace, LogCategory.Input);
        
        // TODO - add deselection rules / other types of selection
        if (_hoveredUnit == null)
        {
            _logger.Log("TriggerSelection - No Hovered Unit, return", LogSeverity.Trace, LogCategory.Input);
            return;
        }
        
        if (_hoveredUnit.State == UnitActivationState.Exhausted)
        {
            _logger.Log("TriggerSelection - Unit is exhausted, return", LogSeverity.Trace, LogCategory.Input);
            return;
        }
        
        if (_hoveredUnit != _selectedUnit)
            SelectUnit(_hoveredUnit);
        else
            DeselectUnit();
    }

    public void UpdateHovered()
    {
        var cell = _cursor.FocusedCell;
        UpdateHoveredFromCell(cell);
    }
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

    private void DeselectUnit()
    {
        if (_selectedUnit == null)
            return;
        
        _selectedUnit.Deselect();
        _selectedUnit = null;
        _logger.Log("Unit Deselected", LogSeverity.Info, LogCategory.UiNavigation);
        EmitSignalSelectedUnitChanged(_selectedUnit);
    }
    
    private void SetHoveredTerrain(TerrainType? terrain)
    {
        _logger.Log($"Update hovered terrain={terrain?.Id}", LogSeverity.Extra, LogCategory.UiNavigation);
        _hoveredTerrain = terrain;
        EmitSignalHoveredTerrainChanged(terrain);
    }

    private void SetHoveredUnit(Units.BattleUnit? unit)
    {
        _logger.Log($"Update hovered unit={unit?.Name}", LogSeverity.Trace, LogCategory.UiNavigation);
        _hoveredUnit = unit;
        EmitSignalHoveredUnitChanged(unit);
    }

    private void TryMoveSelectedUnitTo(Vector2I cell)
    {
        
    }
    
    private void UpdateHoveredFromCell(Vector2I cell)
    {
        // cell -> unit, terrain
        _hoveredCell = cell;
        var hasTerrain = _grid.TryGetTerrainAtCell(cell, out var hoveredTerrain);
        var hasUnit = _unitRegistry.TryGetUnitAtCell(cell, out var hoveredUnit);

        if (hoveredTerrain != _hoveredTerrain)
            SetHoveredTerrain(hoveredTerrain);

        if (hoveredUnit != _hoveredUnit)
            SetHoveredUnit(hoveredUnit);
        
        _logger.Log($"Update hovered from cell={cell}, terrain={hoveredTerrain?.Id}, unit={hoveredUnit?.Name}", LogSeverity.Trace, LogCategory.UiNavigation);
    }
}