using System.Collections.Generic;
using Godot;

namespace Goblinos.Scripts.Battle.Types;

public sealed class MovementPreviewResults
{
    public required IReadOnlySet<Vector2I> Cells { get; init; }
    public required IReadOnlyDictionary<Vector2I, int> CostByCell { get; init; }
    public required IReadOnlyDictionary<Vector2I, Vector2I> ParentCells { get; init; }
    public required Vector2I StartCell { get; init; }
}