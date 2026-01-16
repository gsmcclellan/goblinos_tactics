using System;
using System.Collections.Generic;
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Services;


/// <summary>
/// Computes legal target cells for primary actions and builds cached previews
/// for use during the PrimaryActionTargeting phase.
/// </summary>
public class PrimaryActionTargetingService
{
    private readonly Logger _logger = LogManager.For<PrimaryActionTargetingService>();

    private readonly BattleGrid _grid;
    private readonly TargetRangeService _targetRangeService;
    private readonly UnitRegistry _unitRegistry;

    public PrimaryActionTargetingService(BattleGrid grid, UnitRegistry unitRegistry)
    {
        _grid = grid;
        _targetRangeService = new TargetRangeService(grid);
        _unitRegistry = unitRegistry;
    }
    
    // ---------------------------------------------------------------------
    // Public Methods
    // ---------------------------------------------------------------------
    
    /// <summary>
    /// Builds and returns cached target previews for every available primary action type.
    /// Call once when entering the PrimaryActionTargeting phase.
    /// </summary>
    public PrimaryActionPreviewResults BuildPrimaryActionPreviews(
        UnitActivationContext unitActivation,
        IEnumerable<PrimaryActionType> primaryActionTypes)
    {
        _logger.Log($"{nameof(BuildPrimaryActionPreviews)}", LogSeverity.Info, LogCategory.UiNavigation);

        var previewResults = new PrimaryActionPreviewResults();

        foreach (var primaryActionType in primaryActionTypes)
        {
            var validTargets = GetValidTargets(unitActivation.MoveTargetCell ?? unitActivation.OriginCell, unitActivation.Unit, primaryActionType);
            previewResults.SetTargetPreview(primaryActionType, validTargets);
        }

        return previewResults;
    }

    /// <summary>
    /// Returns all valid targets based on origin cell, unit & action type.
    /// </summary>
    public IReadOnlySet<Vector2I> GetValidTargets(Vector2I originCell, BattleUnit unit, PrimaryActionType primaryActionType)
    {
        _logger.Log($"{nameof(GetValidTargets)}", LogSeverity.Info, LogCategory.UiNavigation);
        var targetableCells = new HashSet<Vector2I>();

        if (!DebugUtil.Require(unit != null,
                $"[{nameof(PrimaryActionTargetingService)}].{nameof(GetValidTargets)} - Missing Unit"))
            return targetableCells;
        
        // TODO - get range for other types of primary 
        RangeBand range = GetRange(unit, primaryActionType);
        var inRangeCells = _targetRangeService.BuildTargetRangeFromCell(originCell, range);

        foreach (var inRangeCell in inRangeCells)
            AddIfValidTarget(inRangeCell, unit, primaryActionType, targetableCells);

        return targetableCells;
    }

    /// <summary>
    /// Determines if target cell is valid given acting unit & action type. Does not check for range.
    /// </summary>
    public bool IsValidTarget(Vector2I cell, BattleUnit actingUnit, PrimaryActionType actionType)
    {
        _logger.Log($"{nameof(IsValidTarget)} cell={cell} actionType={actionType}", LogSeverity.Extra, LogCategory.Input);
        
        bool requiresUnit;
        bool mustBeEnemies;
        bool mustBeFriends;
        switch (actionType)
        {
            case PrimaryActionType.Attack:
                requiresUnit = true;
                mustBeEnemies = true;
                mustBeFriends = false;
                break;
            default:
                _logger.Warn($"[{nameof(PrimaryActionTargetingService)}].{nameof(AddIfValidTarget)} - No case for PrimaryActionType={actionType}.");
                requiresUnit = false;
                mustBeEnemies = false;
                mustBeFriends = false;
                break;
        }

        var hasUnit = _unitRegistry.TryGetUnitAtCell(cell, out var targetUnit);
        if (!hasUnit)
            return !requiresUnit;

        return (!mustBeEnemies || actingUnit.IsFriendly != targetUnit.IsFriendly) && (!mustBeFriends || actingUnit.IsFriendly == targetUnit.IsFriendly);
    }

    /// <summary>Determines if target cell is valid given acting unit & action type. Does not check for range.</summary>
    public bool IsValidTarget(UnitActivationContext unitActivation, Vector2I cell)
    {
        if (!DebugUtil.Require(unitActivation != null,
                $"[{nameof(PrimaryActionTargetingService)}].{nameof(IsValidTarget)} - Missing UnitActivationContext"))
            return false;
        
        return IsValidTarget(cell, unitActivation.Unit, unitActivation.PrimaryAction);
    }
    
    /// <summary>Determines if target cell is valid given acting unit & action type. Does not check for range.</summary>
    public bool IsValidTarget(UnitActivationContext unitActivation, CellFocus focus) =>
        IsValidTarget(unitActivation, focus.Cell);

    /// <summary>Determines if target cell is valid given acting unit & action type. Does not check for range.</summary>
    public IReadOnlySet<Vector2I> GetValidTargets(UnitActivationContext unitActivation) => 
        GetValidTargets(unitActivation.MoveTargetCell ?? unitActivation.OriginCell, unitActivation.Unit, unitActivation.PrimaryAction);
    
    // ---------------------------------------------------------------------
    // Private Helpers
    // ---------------------------------------------------------------------
    private void AddIfValidTarget(Vector2I cell, BattleUnit actingUnit, PrimaryActionType actionType, HashSet<Vector2I> output)
    {
        if (IsValidTarget(cell, actingUnit, actionType))
            output.Add(cell);
    }

    private RangeBand GetRange(BattleUnit unit, PrimaryActionType actionType)
    {
        switch (actionType)
        {
            case PrimaryActionType.Attack:
                return unit.AttackRange;
            default:
                _logger.Warn($"[{nameof(PrimaryActionTargetingService)}].{nameof(GetRange)} - No case for PrimaryActionType={actionType}.");
                return new RangeBand(0, 0);
        }
    }
}