#nullable enable
using System;
using System.Collections.Generic;
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
    private readonly UnitActivationPreviewService _unitActivationPreviewService;

    public EnemyActionPlanningService(BattleGrid grid, UnitRegistry unitRegistry)
    {
        _grid = grid;
        _unitRegistry = unitRegistry;
        
        Debug.Assert(_grid != null, $"[{nameof(EnemyTurnController)}] BattleGrid must be set.");
        Debug.Assert(_unitRegistry != null, $"[{nameof(EnemyTurnController)}] UnitRegistry must be set.");

        _moveRangeService = new MoveRangeService(grid, unitRegistry);
        _primaryActionTargetingService = new PrimaryActionTargetingService(grid, unitRegistry);
        _unitActivationPreviewService = new UnitActivationPreviewService(grid, unitRegistry);
        
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
        var unitActivationPreview = _unitActivationPreviewService.BuildPreview(actingUnit, originCell);
        var attackPreview = unitActivationPreview.GetPrimaryActionPreview(PrimaryActionType.Attack);
        // var attackPreview =
            // _primaryActionTargetingService.BuildThreatUnion(unitActivationPreview.MoveCells, actingUnit, PrimaryActionType.Attack);

        // Get self move / attack preview
        // Check for units in range
        // if exists, move & attack.

        Vector2I targetCell = default;
        BattleUnit? targetUnit = null;
        var hasTarget = attackPreview.TargetCells.Any(potentialTargetCell =>
        {
            targetCell = potentialTargetCell;
            return _unitRegistry.TryGetUnitAtCell(potentialTargetCell, out targetUnit) && targetUnit.IsFriendly != actingUnit.IsFriendly;
        });

        if (!hasTarget)
            return BuildWaitPlan(actingUnit, originCell);

        var moveTarget = attackPreview.AttackOriginsByTargetCell[targetCell].FirstOrDefault();
        return new EnemyActionPlan(actingUnit, originCell, moveTarget, PrimaryActionType.Attack, targetCell, targetUnit);
    }

    public EnemyActionPlan BuildWaitPlan(BattleUnit unit, Vector2I originCell) =>
        new EnemyActionPlan(unit, originCell, null, PrimaryActionType.Wait, null, null);

    // private Vector2I? TryBuildMoveTowardNearestPlayer(BattleUnit enemyUnit)
    // {
    //     _logger.Log($"TryBuildMoveTowardNearestPlayer unit={enemyUnit.Name}", LogSeverity.Trace, LogCategory.AiMovement);
    //
    //     
    // }
}