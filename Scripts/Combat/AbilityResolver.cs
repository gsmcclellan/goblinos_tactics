using System;
using System.Threading.Tasks;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Controllers;
using Goblinos.Scripts.Battle.Preview;
using Goblinos.Scripts.Battle.Services;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Util;

namespace Goblinos.Scripts.Combat;

public class AbilityResolver
{
    private readonly Logger _logger = LogManager.For<AbilityResolver>();
    private MovementController _movementController;

    public AbilityResolver(MovementController movementController)
    {
        _movementController = movementController;
    }

    public Task<bool> Resolve(IUnitActionPlan unitActivation)
    {
        _logger.Log($"{nameof(Resolve)} abilityType={unitActivation.Unit.Ability.DisplayName}", LogSeverity.Info, LogCategory.CombatResolution);
        
        switch (unitActivation.Unit.Ability.Type)
        {
            case AbilityType.Push:
                return ResolvePushAbility(unitActivation);
            case AbilityType.None:
            default:
                throw new NotImplementedException();
                break;
        }
    }

    public async Task<bool> ResolvePushAbility(IUnitActionPlan unitActivation)
    {
        _logger.Log(nameof(ResolvePushAbility), LogSeverity.Info, LogCategory.CombatResolution);
        // check valid positioning of actor and target, 
        var unit = unitActivation.Unit;
        var target = unitActivation.PrimaryActionTargetUnit;
        

        if (!DebugUtil.Require(target != null, "Push failed - no target unit") ||
            !DebugUtil.Require(unitActivation.MoveTargetCell.HasValue, "Push failed - no unit move target") ||
            !DebugUtil.Require(unitActivation.PrimaryActionTargetCell.HasValue, "Push failed - no primary action target"))
            return false;

        var unitCell = unitActivation.MoveTargetCell.Value;
        var targetCell = unitActivation.PrimaryActionTargetCell.Value;
        
        var distance =
            ManhattanRangeService.GetDistance(unitActivation.MoveTargetCell.Value, unitActivation.PrimaryActionTargetCell.Value);
        var dir = targetCell - unitCell;

        if (!DebugUtil.Require(unit.Ability.Range.InRange(distance), "Push failed - target unit not in range."))
            return false;
        // move target one space away from actor
        return _movementController.TryMoveToCell(target, targetCell + dir, true);
    }
}