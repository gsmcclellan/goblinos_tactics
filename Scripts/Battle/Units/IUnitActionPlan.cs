#nullable enable
using Goblinos.Scripts.Battle.Types;
using Godot;

namespace Goblinos.Scripts.Battle.Units;

public interface IUnitActionPlan
{
    public BattleUnit Unit { get; }
    public Vector2I? MoveTargetCell { get; }
    public PrimaryActionType PrimaryAction { get; }
    public Vector2I? PrimaryActionTargetCell { get; }
    public BattleUnit? PrimaryActionTargetUnit { get; }
}