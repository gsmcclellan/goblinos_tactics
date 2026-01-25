using System.Collections.Generic;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Combat.Types;
using Godot;

namespace Goblinos.Scripts.Battle.Preview;

internal sealed class PrimaryActionPreviewBuilder
{
    private readonly HashSet<Vector2I> _attackOrigins = new();
    private readonly HashSet<Vector2I> _targets = new();
    private readonly Dictionary<Vector2I, HashSet<Vector2I>> _originsByTarget = new();
    private readonly Dictionary<Vector2I, HashSet<Vector2I>> _targetsByOrigin = new();

    public void AddLink(Vector2I origin, Vector2I target)
    {
        _attackOrigins.Add(origin);
        _targets.Add(target);

        if (!_targetsByOrigin.TryGetValue(origin, out var targets))
        {
            targets = new HashSet<Vector2I>();
            _targetsByOrigin.Add(origin, targets);
        }
        targets.Add(target);

        if (!_originsByTarget.TryGetValue(target, out var origins))
        {
            origins = new HashSet<Vector2I>();
            _originsByTarget.Add(target, origins);
        }
        origins.Add(origin);
    }

    public void AddLinks(Vector2I origin, IEnumerable<Vector2I> targets)
    {
        foreach (var target in targets)
            AddLink(origin, target);
    }

    public PrimaryActionPreview Build(PrimaryActionType type, RangeBand range, Vector2I originCell)
    {
        // After this call, do not mutate builder (or just discard it).
        return new PrimaryActionPreview(

            actionType: type,
            range: range,
            originCell: originCell,
            attackOriginCells: _attackOrigins,
            targetCells: _targets,
            attackOriginsByTargetCell: Freeze(_originsByTarget),
            targetCellsByAttackOrigin: Freeze(_targetsByOrigin)
        );
    }

    private static IReadOnlyDictionary<Vector2I, IReadOnlySet<Vector2I>> Freeze(
        Dictionary<Vector2I, HashSet<Vector2I>> source)
    {
        var result = new Dictionary<Vector2I, IReadOnlySet<Vector2I>>(source.Count);
        foreach (var (key, set) in source)
            result.Add(key, set);
        return result;
    }
}
