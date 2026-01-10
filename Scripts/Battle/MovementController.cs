using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class MovementController: Node
{
    private Logger _logger = LogManager.For<MovementController>();
    
    private BattleGrid _grid;
    private UnitRegistry _unitRegistry;

    public void Bind(BattleGrid grid, UnitRegistry unitRegistry)
    {
        _grid = grid;
        _unitRegistry = unitRegistry;
        
        Debug.Assert(_grid != null, "[MovementController] BattleGrid must be bound.");
        Debug.Assert(_unitRegistry != null, "[MovementController] UnitRegistry must be bound.");

        _logger.Log("Bind Complete", LogSeverity.Info, LogCategory.Initialization);
    }

    public bool TryMoveToCell(BattleUnit unit, Vector2I targetCell)
    {
        _logger.Log("TryMoveToCell", LogSeverity.Info, LogCategory.UnitLifecycle);
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

        CommitMove(unit, fromCell, targetCell);
        return true;
    }

    public void CommitMove(BattleUnit unit, Vector2I fromCell, Vector2I toCell)
    {
        _logger.Log($"CommitMove unit={unit.UnitName} from={fromCell} to={toCell}", LogSeverity.Info, LogCategory.UnitLifecycle);
        // TODO - animations & stuff
        unit.GlobalPosition = _grid.GetGlobalPositionForCell(toCell);
        _unitRegistry.ApplyUnitMove(unit, fromCell, toCell);
    }
}