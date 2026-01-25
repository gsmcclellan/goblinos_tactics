#nullable enable
using Goblinos.Scripts.Battle.Types;
using Godot;

namespace Goblinos.Scripts.Battle.Units;

public class EnemyActionPlan: IUnitActionPlan
{
    public BattleUnit Unit  { get; init; }
    public Vector2I? MoveTargetCell  { get; init; }
    public PrimaryActionType PrimaryAction  { get; init; }
    public Vector2I? PrimaryActionTargetCell  { get; init; }
    public BattleUnit? PrimaryActionTargetUnit  { get; init; }

    public EnemyActionPlan(BattleUnit unit, Vector2I? moveTargetCell, PrimaryActionType primaryAction,
        Vector2I? primaryActionTargetCell, BattleUnit? primaryActionTargetUnit)
    {
        Unit = unit;
        MoveTargetCell = moveTargetCell;
        PrimaryAction = primaryAction;
        PrimaryActionTargetCell = primaryActionTargetCell;
        PrimaryActionTargetUnit = primaryActionTargetUnit;
    }
}