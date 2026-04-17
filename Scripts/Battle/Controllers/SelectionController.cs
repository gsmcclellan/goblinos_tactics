#nullable enable
using System.Linq;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Core;
using Goblinos.Scripts.Battle.Core.Types;
using Goblinos.Scripts.Battle.Terrain;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Battle.Units;
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
    private GobLogger _logger = GobLogManager.For<SelectionController>();

    private GridCursor _cursor = null!;
    private BattleGrid _grid = null!;
    private UnitRegistry _unitRegistry = null!;

    /** Fields */
    private Vector2I _hoveredCell;

    private TerrainType? _hoveredTerrain;
    private BattleUnit? _hoveredUnit;

    // private Vector2I _selectedCell;
    // private TerrainType _selectedTerrain;
    private BattleUnit? _selectedUnit;

    /** Properties */
    public CellFocus Focus => new CellFocus(_hoveredCell, _hoveredTerrain, _hoveredUnit);
    public Vector2I HoveredCell => _hoveredCell;
    public BattleUnit? HoveredUnit => _hoveredUnit;

    public Vector2I? SelectedCell { 
        get 
        {
            if (_selectedUnit == null) return null;
            
            if (_unitRegistry.TryGetCell(_selectedUnit, out var cell))
                return cell;

            return null;
        }
    }
    public BattleUnit? SelectedUnit => _selectedUnit;

    public bool IsUnitHovered => _hoveredUnit != null;
    public bool IsUnitSelected => _selectedUnit != null;


    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------

    public override void _Ready()
    {
        _logger.Log("Ready", GobLogSeverity.Info, GobLogCategory.Initialization);
    }

    public void Bind(GridCursor cursor, BattleGrid grid, UnitRegistry unitRegistry)
    {
        _logger.Log("Bind", GobLogSeverity.Info, GobLogCategory.Initialization);
        if (_cursor != null! || _grid != null! || _unitRegistry != null!)
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
        if (_cursor != null!)
            _cursor.GridCursorFocusChanged += OnCursorFocusChanged;
        
        _logger.Log("Subscriptions Initialized", GobLogSeverity.Info, GobLogCategory.Initialization);
    }

    private void _UnsubscribeFromEvents()
    {   if (_cursor != null!)
            _cursor.GridCursorFocusChanged -= OnCursorFocusChanged;
        
        _logger.Log("Subscriptions Removed", GobLogSeverity.Info, GobLogCategory.Initialization);
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
            SelectUnit(unit!);
            selectedNode = unit;
            return;
        }

        selectedNode = null;
    }

    public void SelectCell(Vector2I cell) => SelectCell(cell, out _);
    
    public void SelectUnit(BattleUnit unit)
    {
        if (unit == _selectedUnit) return;
        
        if (unit.State == UnitActivationState.Exhausted)
        {
            _logger.Log($"SelectUnit blocked: unit exhausted unit={unit.UnitName}", GobLogSeverity.Trace, GobLogCategory.UiNavigation);
            return;
        }

        if (_selectedUnit != null)
        {
            _selectedUnit?.Deselect();
            _selectedUnit?.SetActivationState(UnitActivationState.Ready);
        }
       
        unit.Select();
        _selectedUnit = unit;
        _logger.Log($"Unit Selected unit={unit.UnitName}", GobLogSeverity.Info, GobLogCategory.UiNavigation);
        EmitSignalSelectedUnitChanged(_selectedUnit);
    }
    
    public void TriggerClearSelection()
    {
        _logger.Log("TriggerSelection", GobLogSeverity.Trace, GobLogCategory.Input);
        DeselectUnit();
        UpdateHovered();
    }
    
    public bool TrySelectHoveredUnit()
    {
        _logger.Log("TriggerSelection", GobLogSeverity.Trace, GobLogCategory.Input);
        
        // TODO - add deselection rules / other types of selection
        if (_hoveredUnit == null)
        {
            _logger.Log("TriggerSelection - No Hovered Unit, return", GobLogSeverity.Trace, GobLogCategory.Input);
            return false;
        }
        
        if (_hoveredUnit.State == UnitActivationState.Exhausted)
        {
            _logger.Log("TriggerSelection - Unit is exhausted, return", GobLogSeverity.Trace, GobLogCategory.Input);
            return false;
        }

        if (_hoveredUnit == _selectedUnit)
        {
            DeselectUnit();
            return false;
        }
        
        SelectUnit(_hoveredUnit);
        return true;
    }

    public bool TrySelectNextUnit(bool reverse = false)
    {
        var units = _unitRegistry.GetSelectableFriendlyUnitsInNavigationOrder();
        
        // At least one selectable unit should exist, else turn/battle should have ended.
        if (!DebugUtil.Require(units.Count > 0,
                $"[{nameof(SelectionController)}] TrySelectNextUnit failed, no selectable units."))
            return false;

        if (_selectedUnit == null)
        {
            SelectUnit(reverse ? units.Last(): units.First());
            return true;
        }
            
        // Ensure selected unit still exists, else it should have been unselected already
        if (!units.Contains(_selectedUnit))
        {
            _logger.Warn($"[{nameof(TrySelectNextUnit)}] - Currently Selected Unit is unselectable");
            SelectUnit(reverse ? units.Last(): units.First());
            return true;
        }
        
        var currentSelectedIndex = units.IndexOf(_selectedUnit!);
        var nextIndex = 0;
        // Get next index.
        if (reverse)
        {
            nextIndex = currentSelectedIndex - 1;
            nextIndex = nextIndex < 0 ? units.Count - 1 : nextIndex;
        }
        else
        {
            nextIndex = currentSelectedIndex + 1;
            nextIndex = nextIndex >= units.Count ? 0 : nextIndex;
        }

        if (units[nextIndex] == _selectedUnit) // Only one unit.
            return false;
        
        SelectUnit(units[nextIndex]);
        return true;
    }

    public void UpdateHovered()
    {
        var cell = _cursor.FocusedCell;
        UpdateHoveredFromCell(cell);
    }
    // ---------------------------------------------------------------------
    // Event Handlers
    // ---------------------------------------------------------------------

    private void OnCursorFocusChanged(Vector2I newCell, Vector2I oldCell, int gridCursorFocusSource)
    {
        _logger.Log($"Cursor focus changed newCell={newCell}, oldCell={oldCell}", GobLogSeverity.Extra, GobLogCategory.UiNavigation);
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
        _logger.Log("Unit Deselected", GobLogSeverity.Info, GobLogCategory.UiNavigation);
        EmitSignalSelectedUnitChanged(_selectedUnit);
    }
    
    private void SetHoveredTerrain(TerrainType? terrain)
    {
        _logger.Log($"Update hovered terrain={terrain?.Id}", GobLogSeverity.Extra, GobLogCategory.UiNavigation);
        _hoveredTerrain = terrain;
        EmitSignalHoveredTerrainChanged(terrain);
    }

    private void SetHoveredUnit(BattleUnit? unit)
    {
        _logger.Log($"Update hovered unit={unit?.Name}", GobLogSeverity.Trace, GobLogCategory.UiNavigation);
        _hoveredUnit = unit;
        EmitSignalHoveredUnitChanged(unit);
    }
    
    private void UpdateHoveredFromCell(Vector2I cell)
    {
        // cell -> unit, terrain
        _hoveredCell = cell;
        _ = _grid.TryGetTerrainAtCell(cell, out var hoveredTerrain);
        _ = _unitRegistry.TryGetUnitAtCell(cell, out var hoveredUnit);

        if (hoveredTerrain != _hoveredTerrain)
            SetHoveredTerrain(hoveredTerrain);

        if (hoveredUnit != _hoveredUnit)
            SetHoveredUnit(hoveredUnit);
        
        _logger.Log($"Update hovered from cell={cell}, terrain={hoveredTerrain?.Id}, unit={hoveredUnit?.Name}", GobLogSeverity.Trace, GobLogCategory.UiNavigation);
    }
}