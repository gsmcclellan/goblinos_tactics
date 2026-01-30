using System.Collections.Generic;
using Goblinos.Scripts.Battle.Types;
using Godot;

namespace Goblinos.Scripts.Battle.Preview;

public class UnitActivationPreview
{
    public required MovementPreview MovementPreview { get; init; }

    public Dictionary<PrimaryActionType, PrimaryActionPreview> PrimaryActionPreviewsByType { get; } =
        new();

    // public required IReadOnlySet<Vector2I> Cells { get; init; }
    // public required IReadOnlyDictionary<Vector2I, int> CostByCell { get; init; }
    // public required IReadOnlyDictionary<Vector2I, Vector2I> ParentCells { get; init; }
    // public required Vector2I OriginCell { get; init; }

    public IReadOnlySet<Vector2I> AttackCells => GetPrimaryActionPreview(PrimaryActionType.Attack)?.TargetCells ?? new HashSet<Vector2I>();
    public IReadOnlySet<Vector2I> MoveCells => MovementPreview?.Cells ?? new HashSet<Vector2I>();
    public Vector2I OriginCell => MovementPreview.OriginCell;
    
    public void AddPrimaryActionPreview(PrimaryActionPreview preview)
    {
        PrimaryActionPreviewsByType[preview.ActionType] = preview;
    }

    public PrimaryActionPreview GetPrimaryActionPreview(PrimaryActionType actionType)
    {
        return PrimaryActionPreviewsByType[actionType];
    }

    public bool TryGetPrimaryActionPreview(PrimaryActionType actionType, out PrimaryActionPreview primaryActionPreview)
    {
        return PrimaryActionPreviewsByType.TryGetValue(actionType, out primaryActionPreview);
    }
}