using System.Collections.Generic;
using Goblinos.Scripts.Battle.Types;

namespace Goblinos.Scripts.Battle.Preview;

public class UnitActivationPreview
{
    public required MovementPreview MovementPreview { get; init; }

    public Dictionary<PrimaryActionType, PrimaryActionPreview> PrimaryActionPreviewsByType { get; } =
        new();

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