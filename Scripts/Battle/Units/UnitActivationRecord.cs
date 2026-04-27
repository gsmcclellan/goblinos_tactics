using Goblinos.Scripts.Battle.Preview;
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
    public Vector2I EndingCell { get; init; }
    public PrimaryActionType PrimaryActionType { get; init; }
    public Vector2I? PrimaryActionTargetCell { get; init; }
    public UnitSnapshot? TargetUnitSnapshot { get; init; }

    public UnitActivationRecord(UnitActivationContext c)
    {
        ActingUnitSnapshot = new UnitSnapshot(
            c.Unit.Id,
            c.Unit.UnitName,
            c.DestinationCell
        );
        OriginCell = c.OriginCell;
        EndingCell = c.DestinationCell;
        PrimaryActionType = c.PrimaryAction;
        PrimaryActionTargetCell = c.PrimaryActionTargetCell;

        TargetUnitSnapshot = c.PrimaryActionTargetUnit != null
            ? new UnitSnapshot(
                c.PrimaryActionTargetUnit.Id,
                c.PrimaryActionTargetUnit.UnitName,
                c.PrimaryActionTargetCell.Value
            )
            : null;
    }
}
