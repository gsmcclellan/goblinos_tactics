using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Controllers;
using Goblinos.Scripts.Battle.Preview;
using Goblinos.Scripts.Battle.Services;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Units.Stats;
using Goblinos.Scripts.Units.Stats.Types;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Combat;

public class AbilityResolver
{
    private readonly Logger _logger = LogManager.For<AbilityResolver>();
    private MovementController _movementController;
    private UnitRegistry _unitRegistry;

    public AbilityResolver(MovementController movementController, UnitRegistry unitRegistry)
    {
        _movementController = movementController;
        _unitRegistry = unitRegistry;
        
        Debug.Assert(_movementController != null, $"[{nameof(AbilityResolver)}] {nameof(MovementController)} must be initialized.");
        Debug.Assert(_unitRegistry != null, $"[{nameof(AbilityResolver)}] {nameof(UnitRegistry)} must be initialized.");
    }

    public Task<bool> Resolve(IUnitActionPlan unitActivation)
    {
        _logger.Log($"{nameof(Resolve)} abilityType={unitActivation.Unit.Ability.DisplayName}", LogSeverity.Info, LogCategory.CombatResolution);
        
        switch (unitActivation.Unit.Ability.Type)
        {
            case AbilityType.Push:
            {
                if (!TryGetSingleTargets(unitActivation, out var targetUnit, out var targetCell))
                    throw new Exception($"Unable to resolve {unitActivation.Unit.Ability.Type} ability - Invalid targeting info");
                return ResolvePushAbility(unitActivation.Unit, targetUnit, unitActivation.DestinationCell, targetCell);
            }
            case AbilityType.DisableMovement:
            {
                if (!TryGetSingleTargets(unitActivation, out var targetUnit, out var targetCell))
                    throw new Exception($"Unable to resolve {unitActivation.Unit.Ability.Type} ability - Invalid targeting info");
                return ResolveDisableMovementAbility(unitActivation.Unit, targetUnit, unitActivation.DestinationCell, targetCell);
            }
            case AbilityType.StatModifier:
            {
                if (!TryGetSingleTargets(unitActivation, out var targetUnit, out var targetCell))
                    throw new Exception($"Unable to resolve {unitActivation.Unit.Ability.Type} ability - Invalid targeting info");
                return ResolveStatModifierAbility(unitActivation.Unit, targetUnit, unitActivation.DestinationCell, targetCell);
            }
            case AbilityType.Swap:
            {
                if (!TryGetSingleTargets(unitActivation, out var targetUnit, out var targetCell))
                    throw new Exception($"Unable to resolve {unitActivation.Unit.Ability.Type} ability - Invalid targeting info");
                return ResolveSwapAbility(unitActivation.Unit, targetUnit, unitActivation.DestinationCell, targetCell);
            }
            case AbilityType.None:
            default:
                throw new NotImplementedException();
                break;
        }
    }
    
    // ---------------------------------------------------------------------
    // Individual ability resolvers.
    // ---------------------------------------------------------------------
    
    private async Task<bool> ResolveDisableMovementAbility(BattleUnit actingUnit, BattleUnit targetUnit, Vector2I actingUnitCell, Vector2I targetCell)
    {
        _logger.Log(nameof(ResolveDisableMovementAbility), LogSeverity.Info, LogCategory.CombatResolution);
        
        // apply disabled condition
        targetUnit.ApplyCondition(CombatConditionTemplates.Get(CombatConditionType.DisableMovement));

        return true;
    }

    private async Task<bool> ResolvePushAbility(BattleUnit actingUnit, BattleUnit targetUnit, Vector2I actingUnitCell, Vector2I targetCell)
    {
        _logger.Log(nameof(ResolvePushAbility), LogSeverity.Info, LogCategory.CombatResolution);
        // check valid positioning of actor and target, 
        
        var distance =
            ManhattanRangeService.GetDistance(actingUnitCell, targetCell);
        var dir = targetCell - actingUnitCell;

        if (!DebugUtil.Require(actingUnit.Ability.Range.InRange(distance), "Push failed - target unit not in range."))
            return false;
        
        // move target one space away from actor
        return _movementController.TryMoveToCell(targetUnit, targetCell + dir, true);
    }

    private async Task<bool> ResolveStatModifierAbility(BattleUnit actingUnit, BattleUnit targetUnit, Vector2I actingUnitCell, Vector2I targetCell)
    {
        _logger.Log(nameof(ResolveStatModifierAbility), LogSeverity.Info, LogCategory.CombatResolution);
        var statMod = new StatModifier(actingUnit.Id, StatName.Movement, actingUnit.Ability.Magnitude,
            StatModifierExpiration.EndOfRound);
        targetUnit.ApplyStatModifier(statMod);
        return true;
    }

    private async Task<bool> ResolveSwapAbility(BattleUnit actingUnit, BattleUnit targetUnit, Vector2I actingUnitCell, Vector2I targetCell)
    {
        _logger.Log(nameof(ResolveSwapAbility), LogSeverity.Info, LogCategory.CombatResolution);
        return _movementController.TrySwapUnits(actingUnit, actingUnitCell, targetCell);
    }
    
    // ---------------------------------------------------------------------
    // Private Helpers
    // ---------------------------------------------------------------------
    private static bool TryGetSingleTargets(
        IUnitActionPlan unitActionPlan,
        out BattleUnit targetUnit,
        out Vector2I targetCell)
    {
        if (unitActionPlan.PrimaryActionTargetUnit is { } unit &&
            unitActionPlan.PrimaryActionTargetCell is { } cell)
        {
            var rangeValidated = ValidateRange(unitActionPlan.Unit.Ability.Range, unitActionPlan.DestinationCell, cell);
            if (!DebugUtil.Require(rangeValidated, "DisableMovement failed - target unit not in range."))
            {
                targetUnit = null!;
                targetCell = default;
                return false;
            }
            
            targetUnit = unit;
            targetCell = cell;
            return true;
        }

        targetUnit = null!;
        targetCell = default;
        return false;
    }

    private static bool ValidateRange(RangeBand range, Vector2I actingCell, Vector2I targetCell) =>
        range.InRange(ManhattanRangeService.GetDistance(actingCell, targetCell));
}