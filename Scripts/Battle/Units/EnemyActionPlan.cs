#nullable enable
using Goblinos.Scripts.Battle.Types;
using Godot;

namespace Goblinos.Scripts.Battle.Units;

public class EnemyActionPlan: IUnitActionPlan
{
    public BattleUnit Unit  { get; init; }
    public Vector2I OriginCell { get; }
    public Vector2I? MoveTargetCell  { get; }
    public PrimaryActionType PrimaryAction  { get; }
    public Vector2I? PrimaryActionTargetCell  { get; }
    public BattleUnit? PrimaryActionTargetUnit  { get; }

    public EnemyActionPlan(BattleUnit unit, Vector2I originCell, Vector2I? moveTargetCell, PrimaryActionType primaryAction,
        Vector2I? primaryActionTargetCell, BattleUnit? primaryActionTargetUnit)
    {
        Unit = unit;
        OriginCell = originCell;
        MoveTargetCell = moveTargetCell;
        PrimaryAction = primaryAction;
        PrimaryActionTargetCell = primaryActionTargetCell;
        PrimaryActionTargetUnit = primaryActionTargetUnit;
    }
}