#nullable enable
using System;
using System.Diagnostics;
using System.Linq;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Core;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Battle.Units;
using Godot;

namespace Goblinos.Scripts.Battle.Services;

public sealed class EnemyActionPlanningService
{
    private readonly Logger _logger = LogManager.For<EnemyActionPlanningService>();

    private readonly BattleGrid _grid;
    private readonly UnitRegistry _unitRegistry;
    private readonly MoveRangeService _moveRangeService;
    private readonly PrimaryActionTargetingService _primaryActionTargetingService;

    public EnemyActionPlanningService(BattleGrid grid, UnitRegistry unitRegistry)
    {
        _grid = grid;
        _unitRegistry = unitRegistry;
        
        Debug.Assert(_grid != null, $"[{nameof(EnemyTurnController)}] BattleGrid must be set.");
        Debug.Assert(_unitRegistry != null, $"[{nameof(EnemyTurnController)}] UnitRegistry must be set.");

        _moveRangeService = new MoveRangeService(grid, unitRegistry);
        _primaryActionTargetingService = new PrimaryActionTargetingService(grid, unitRegistry);
        
        _logger.Log("Constructed", LogSeverity.Info, LogCategory.Initialization);
    }

    public void StartEnemyTurn()
    {
        // Build context
        // Get Player units, compute a general threat value for each one.
        // Get Enemy units, compute a general value for each one.
    }

    /// <summary>
    /// Builds a simple plan: if any attack is available now, attack; otherwise move toward nearest player.
    /// </summary>
    public EnemyActionPlan BuildSimplePlan(BattleUnit actingUnit)
    {
        _logger.Log($"BuildPlan unit={actingUnit.UnitName}", LogSeverity.Trace, LogCategory.AiDecision);

        if (!_unitRegistry.TryGetCell(actingUnit, out var originCell))
            throw new Exception($"Unable to generate action plan - unit={actingUnit.UnitName} origin cell unavailable");
        var movePreview = _moveRangeService.GetMovementPreview(originCell, actingUnit);
        var attackPreview =
            _primaryActionTargetingService.BuildThreatUnion(movePreview.Cells, actingUnit, PrimaryActionType.Attack);

        // Get self move / attack preview
        // Check for units in range
        // if exists, move & attack.

        Vector2I targetCell;
        BattleUnit targetUnit;
        var hasTarget = attackPreview.Any(potentialTargetCell =>
        {
            targetCell = potentialTargetCell;
            return _unitRegistry.TryGetUnitAtCell(potentialTargetCell, out targetUnit);
        });

        if (!hasTarget)
            return BuildWaitPlan(actingUnit);

        return BuildWaitPlan(actingUnit); // TODO - make simple attack plan.
    }

    public EnemyActionPlan BuildWaitPlan(BattleUnit unit) =>
        new EnemyActionPlan(unit, null, PrimaryActionType.Wait, null, null);

    // private Vector2I? TryBuildMoveTowardNearestPlayer(BattleUnit enemyUnit)
    // {
    //     _logger.Log($"TryBuildMoveTowardNearestPlayer unit={enemyUnit.Name}", LogSeverity.Trace, LogCategory.AiMovement);
    //
    //     
    // }
}