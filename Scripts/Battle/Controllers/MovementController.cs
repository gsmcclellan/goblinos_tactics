using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Core;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Controllers;

public partial class MovementController: Node
{
    /** Components */
    private GobLogger _logger = GobLogManager.For<MovementController>();
    
    private BattleGrid _grid;
    private UnitRegistry _unitRegistry;
    
    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------
    public void Bind(BattleGrid grid, Units.UnitRegistry unitRegistry)
    {
        _grid = grid;
        _unitRegistry = unitRegistry;
        
        Debug.Assert(_grid != null, "[MovementController] BattleGrid must be bound.");
        Debug.Assert(_unitRegistry != null, "[MovementController] UnitRegistry must be bound.");

        _logger.Log("Bind Complete", GobLogSeverity.Info, GobLogCategory.Initialization);
    }
    
    // ---------------------------------------------------------------------
    // Public Methods
    // ---------------------------------------------------------------------

    public bool TryMoveToCell(BattleUnit unit, Vector2I targetCell, bool commitMovement = false)
    {
        _logger.Log("TryMoveToCell", GobLogSeverity.Info, GobLogCategory.UnitLifecycle);
        if (!DebugUtil.Require(unit != null, "[MovementController] Unable to move, no unit."))
            return false;

        if (!_grid.TryGetTerrainAtCell(targetCell, out var terrain))
            return false;

        if (terrain.BlocksMovement)
            return false;

        if (!DebugUtil.Require(
                _unitRegistry.TryGetCell(unit, out var fromCell),
                $"[MovementController] Unable to move unit={unit.Name}, no cell."))
            return false;

        if (_unitRegistry.IsCellOccupied(targetCell))
            return false;

        if (commitMovement)
            CommitMove(unit, fromCell, targetCell);
        else
            CreatePendingMove(unit, fromCell, targetCell);
        return true;
    }

    public bool TrySwapUnits(BattleUnit actingUnit, Vector2I actingUnitCell, Vector2I targetCell)
    {
        _logger.Log("TryMoveToCell", GobLogSeverity.Info, GobLogCategory.UnitLifecycle);
        if (!DebugUtil.Require(actingUnit != null, "[MovementController] Unable to swap, no unit."))
            return false;

        var hasTargetUnit = _unitRegistry.TryGetUnitAtCell(targetCell, out var targetUnit);
        
        actingUnit.GlobalPosition = _grid.GetGlobalCenterPositionForCell(targetCell);
        if (hasTargetUnit)
            targetUnit.GlobalPosition = _grid.GetGlobalCenterPositionForCell(actingUnitCell);
        
        _unitRegistry.ApplyUnitMove(actingUnit, actingUnitCell, targetCell, true);
        return true;
    }

    public void CommitMove(BattleUnit unit, Vector2I fromCell, Vector2I toCell)
    {
        _logger.Log($"CommitMove unit={unit.UnitName} from={fromCell} to={toCell}", GobLogSeverity.Info, GobLogCategory.UnitLifecycle);
        // TODO - animations & stuff
        unit.GlobalPosition = _grid.GetGlobalCenterPositionForCell(toCell);
        _unitRegistry.ApplyUnitMove(unit, fromCell, toCell);
    }

    public void CreatePendingMove(BattleUnit unit, Vector2I fromCell, Vector2I toCell)
    {
        _logger.Log($"CreatePendingMove unit={unit.UnitName} from={fromCell} to={toCell}", GobLogSeverity.Info, GobLogCategory.UnitLifecycle);
        // TODO - animations & stuff

        unit.GlobalPosition = _grid.GetGlobalCenterPositionForCell(toCell);
        _unitRegistry.AddPendingMove(unit, fromCell, toCell);
    }

    public void UndoPendingMove(BattleUnit unit)
    {
        if (!_unitRegistry.TryGetCell(unit, out var originCell))
            return;
        // TODO - check that unit can undo (should be current unit activation, not exhausted)
        unit.GlobalPosition = _grid.GetGlobalCenterPositionForCell(originCell);
        _unitRegistry.ClearPendingMove();
    }
    
    // ---------------------------------------------------------------------
    // Private Helper Methods
    // ---------------------------------------------------------------------
}