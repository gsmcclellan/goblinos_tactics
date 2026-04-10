#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Core;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Services;

public sealed class EnemyActionPlanningService
{
    private readonly GobLogger _logger = GobLogManager.For<EnemyActionPlanningService>();

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
        
        _logger.Log("Constructed", GobLogSeverity.Info, GobLogCategory.Initialization);
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
        _logger.Log($"BuildPlan unit={actingUnit.UnitName}", GobLogSeverity.Trace, GobLogCategory.AiDecision);

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
            return BuildMoveCloserPlan(actingUnit, originCell);

        var moveTarget = attackPreview.AttackOriginsByTargetCell[targetCell].FirstOrDefault();
        
        return new EnemyActionPlan(
            actingUnit, 
            originCell, 
            (moveTarget != originCell) ? moveTarget: null, 
            PrimaryActionType.Attack, 
            targetCell, 
            targetUnit
            );
    }

    public EnemyActionPlan BuildWaitPlan(BattleUnit unit, Vector2I originCell) =>
        new EnemyActionPlan(unit, originCell, null, PrimaryActionType.Wait, null, null);

    public EnemyActionPlan BuildMoveCloserPlan(BattleUnit unit, Vector2I originCell)
    {
        _logger.Log("Building move closer plan.", GobLogSeverity.Trace, GobLogCategory.AiDecision);
        // if can't move return / do something else
        var reachableCells = _moveRangeService.GetMovementPreview(originCell, unit).Cells;
        if (reachableCells.Count == 0)
            return BuildWaitPlan(unit, originCell);
        
        // get closest enemy
        var playerUnits = _unitRegistry.GetFriendlyUnits();
        var closestUnitCell = originCell;
        var closestUnitDistance = int.MaxValue;
        foreach (var playerUnit in playerUnits)
        {
            if (!DebugUtil.Require(_unitRegistry.TryGetCell(playerUnit, out var playerUnitCell), "Unit Registry has unit registered with no cell"))
                continue;
            var distanceTo = ManhattanRangeService.GetDistance(originCell, playerUnitCell);
            if (distanceTo >= closestUnitDistance) 
                continue;
            
            closestUnitDistance = distanceTo;
            closestUnitCell = playerUnitCell;
        }
        
        // get closest cell in movement range to closestUnitCell
        // move towards it.
        var bestDestinationCell = originCell;
        var bestDestinationDistance = ManhattanRangeService.GetDistance(originCell, closestUnitCell);

        foreach (var reachableCell in reachableCells)
        {
            var distanceFromReachableCellToTarget = ManhattanRangeService.GetDistance(reachableCell, closestUnitCell);
            if (distanceFromReachableCellToTarget >= bestDestinationDistance)
                continue;

            bestDestinationDistance = distanceFromReachableCellToTarget;
            bestDestinationCell = reachableCell;
        }

        return new EnemyActionPlan(unit, originCell, bestDestinationCell, PrimaryActionType.Wait, null, null);
    }
}