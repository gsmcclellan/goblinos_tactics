using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Units.Types;
using Godot;

namespace Goblinos.Scripts.Battle.Units;

/// <summary>
/// This combines all actions of a unit's per turn command.
/// Includes move + primary action
/// </summary>
public sealed class UnitActivationRecord
{
    public UnitSnapshot ActingUnitSnapshot { get; init; }
    public Vector2I OriginCell { get; init; }
    public Vector2I? MoveTargetCell { get; init; }
    public PrimaryActionType PrimaryActionType { get; init; }
    public Vector2I? PrimaryActionTargetCell { get; init; }
    public UnitSnapshot? TargetUnitSnapshot { get; init; }

    public UnitActivationRecord(UnitActivationContext c)
    {
        ActingUnitSnapshot = new UnitSnapshot(
            c.Unit.Id,
            c.Unit.UnitName
        );
        OriginCell = c.OriginCell;
        MoveTargetCell = c.MoveTargetCell;
        PrimaryActionType = c.PrimaryAction;
        PrimaryActionTargetCell = c.PrimaryActionTargetCell;

        TargetUnitSnapshot = c.PrimaryActionTargetUnit != null
            ? new UnitSnapshot(
                c.PrimaryActionTargetUnit.Id,
                c.PrimaryActionTargetUnit.UnitName
            )
            : null;
    }
}
