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
using Goblinos.Scripts.UI.Presentation;
using Goblinos.Scripts.Units.Stats;
using Goblinos.Scripts.Units.Stats.Types;
using Goblinos.Scripts.Units.Types;
using Goblinos.Scripts.Util;
using Godot;
using ReallyGoodIdeas.Presentation;

namespace Goblinos.Scripts.Combat;

public class AbilityResolver
{
    private readonly GobLogger _logger = GobLogManager.For<AbilityResolver>();
    private readonly MovementController _movementController;
    private readonly PresentationQueue _presentationQueue;
    private UnitRegistry _unitRegistry;

    public AbilityResolver(MovementController movementController, PresentationQueue presentationQueue, UnitRegistry unitRegistry)
    {
        _movementController = movementController;
        _presentationQueue = presentationQueue;
        _unitRegistry = unitRegistry;
        
        Debug.Assert(_movementController != null, $"[{nameof(AbilityResolver)}] {nameof(MovementController)} must be initialized.");
        Debug.Assert(_presentationQueue != null, $"[{nameof(AbilityResolver)}] {nameof(PresentationQueue)} must be initialized.");
        Debug.Assert(_unitRegistry != null, $"[{nameof(AbilityResolver)}] {nameof(UnitRegistry)} must be initialized.");
    }

    public async Task<AbilityResult> Resolve(IUnitActionPlan unitActivation)
    {
        _logger.Log($"{nameof(Resolve)} abilityType={unitActivation.Unit.Ability.DisplayName}", GobLogSeverity.Info, GobLogCategory.CombatResolution);

        try
        {
            switch (unitActivation.Unit.Ability.Type)
            {
                case AbilityType.Push:
                {
                    if (!TryGetSingleTargets(unitActivation, out var targetUnit, out var targetCell))
                        throw new Exception(
                            $"Unable to resolve {unitActivation.Unit.Ability.Type} ability - Invalid targeting info");
                    return await ResolvePushAbility(unitActivation.Unit, targetUnit, unitActivation.DestinationCell,
                        targetCell);
                }
                case AbilityType.Condition:
                {
                    if (!TryGetSingleTargets(unitActivation, out var targetUnit, out var targetCell))
                        throw new Exception(
                            $"Unable to resolve {unitActivation.Unit.Ability.Type} ability - Invalid targeting info");
                    return ResolveApplyConditionAbility(unitActivation.Unit, targetUnit, unitActivation.DestinationCell,
                        targetCell);
                }
                case AbilityType.StatModifier:
                {
                    if (!TryGetSingleTargets(unitActivation, out var targetUnit, out var targetCell))
                        throw new Exception(
                            $"Unable to resolve {unitActivation.Unit.Ability.Type} ability - Invalid targeting info");
                    return ResolveStatModifierAbility(unitActivation.Unit, targetUnit, unitActivation.DestinationCell,
                        targetCell);
                }
                case AbilityType.Swap:
                {
                    if (!TryGetSingleTargets(unitActivation, out var targetUnit, out var targetCell))
                        throw new Exception(
                            $"Unable to resolve {unitActivation.Unit.Ability.Type} ability - Invalid targeting info");
                    return await ResolveSwapAbility(unitActivation.Unit, targetUnit, unitActivation.DestinationCell,
                        targetCell);
                }
                case AbilityType.Heal:
                {
                    if (!TryGetSingleTargets(unitActivation, out var targetUnit, out var targetCell))
                        throw new Exception(
                            $"Unable to resolve {unitActivation.Unit.Ability.Type} ability - Invalid targeting info");
                    return ResolveHealAbility(unitActivation.Unit, targetUnit, unitActivation.DestinationCell,
                        targetCell);
                }
                case AbilityType.DisableMovement:
                case AbilityType.None:
                default:
                    throw new NotImplementedException();
                    break;
            }
        }
        catch (Exception e)
        {
            return AbilityResult.Failed(
                unitActivation.Unit,
                unitActivation.PrimaryActionTargetUnit,
                unitActivation.Unit.Ability.Type,
                e.Message
            );
        }
    }
    
    // ---------------------------------------------------------------------
    // Individual ability resolvers.
    // ---------------------------------------------------------------------
    private AbilityResult ResolveApplyConditionAbility(BattleUnit actingUnit, BattleUnit targetUnit, Vector2I actingUnitCell, Vector2I targetCell)
    {
        _logger.Log(nameof(ResolveApplyConditionAbility), GobLogSeverity.Info, GobLogCategory.CombatResolution);
        if (!DebugUtil.Require(actingUnit.Ability.CombatConditionId.HasValue,
                "Apply condition failed - Ability does not have CombatConditionId value."))
            throw new Exception("Apply condition failed - Ability does not have CombatConditionId value.");
        
        // apply disabled condition
        targetUnit.ApplyCondition(CombatConditionTemplates.Get(actingUnit.Ability.CombatConditionId.Value));

        return AbilityResult.ConditionApplied(actingUnit, targetUnit, actingUnit.Ability.CombatConditionId.Value);
    }

    private Task<AbilityResult> ResolvePushAbility(BattleUnit actingUnit, BattleUnit targetUnit, Vector2I actingUnitCell, Vector2I targetCell)
    {
        _logger.Log(nameof(ResolvePushAbility), GobLogSeverity.Info, GobLogCategory.CombatResolution);
        // check valid positioning of actor and target, 
        
        var distance =
            ManhattanRangeService.GetDistance(actingUnitCell, targetCell);
        var dir = targetCell - actingUnitCell;

        if (!DebugUtil.Require(actingUnit.Ability.Range.InRange(distance), "Push failed - target unit not in range."))
            throw new Exception("Push failed - target unit not in range.");
        
        // move target one space away from actor
        var pushDestination = targetCell + dir;
        var canMove = _movementController.TryMoveToCell(targetUnit, pushDestination, true);

        return Task.FromResult(
            AbilityResult.Pushed(actingUnit, targetUnit, pushDestination, canMove)
        );
    }

    private AbilityResult ResolveStatModifierAbility(BattleUnit actingUnit, BattleUnit targetUnit, Vector2I actingUnitCell, Vector2I targetCell)
    {
        _logger.Log(nameof(ResolveStatModifierAbility), GobLogSeverity.Info, GobLogCategory.CombatResolution);
        if (!actingUnit.Ability.TargetStat.HasValue)
            throw new Exception("Missing target stat");

        var statMod = new StatModifier(actingUnit.Ability.Id.ToString(),actingUnit.Id, actingUnit.Ability.TargetStat.Value, actingUnit.Ability.Magnitude,
            ExpirationTime.EndOfRound);
        targetUnit.ApplyStatModifier(statMod);

        return AbilityResult.StatModified(actingUnit, targetUnit, statMod);
    }
    
    private AbilityResult ResolveHealAbility(BattleUnit actingUnit, BattleUnit targetUnit, Vector2I actingUnitCell, Vector2I targetCell)
    {
        _logger.Log(nameof(ResolveHealAbility), GobLogSeverity.Info, GobLogCategory.CombatResolution);
        var amountHealed = targetUnit.ApplyHealing(actingUnit.AbilityMagnitude);

        // var syncDisplayPresentable = new SyncUnitDisplayPresentable(targetUnit);
        // var floatingTextPresentable = new FloatingTextPresentable(targetUnit.GlobalPosition, amountHealed.ToString());
        // var queued = _presentationQueue.EnqueueAndWait(syncDisplayPresentable);
        // _ = _presentationQueue.PresentOutOfQueue(floatingTextPresentable);

        // await queued;

        return AbilityResult.Healed(actingUnit, targetUnit, amountHealed);
    }
    
    private Task<AbilityResult> ResolveSwapAbility(BattleUnit actingUnit, BattleUnit targetUnit, Vector2I actingUnitCell, Vector2I targetCell)
    {
        _logger.Log(nameof(ResolveSwapAbility), GobLogSeverity.Info, GobLogCategory.CombatResolution);
        if (!_movementController.TrySwapUnits(actingUnit, actingUnitCell, targetCell))
            throw new Exception("Swap failed - Cannot swap units.");
            
        return Task.FromResult(
            AbilityResult.Swapped(actingUnit, targetUnit, targetCell, actingUnitCell, true)
        );
        
        
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
            if (!DebugUtil.Require(rangeValidated, "TryGetSingleTargets failed - target unit not in range."))
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