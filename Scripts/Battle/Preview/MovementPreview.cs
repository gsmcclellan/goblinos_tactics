using System.Collections.Generic;
using Goblinos.Scripts.Battle.Types;
using Godot;

namespace Goblinos.Scripts.Battle.Preview;

public sealed class MovementPreview
{
    public required IReadOnlySet<Vector2I> Cells { get; init; }
    public required IReadOnlyDictionary<Vector2I, int> CostByCell { get; init; }
    public required IReadOnlyDictionary<Vector2I, Vector2I> ParentCells { get; init; }
    public required Vector2I OriginCell { get; init; }
}