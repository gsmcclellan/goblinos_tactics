using Goblinos.Scripts.Battle.Terrain;
using Godot;

namespace Goblinos.Scripts.Battle.Types;

public readonly struct CellFocus
{
    public CellFocus(Vector2I cell, TerrainType? terrain, BattleUnit? unit)
    {
        Cell = cell;
        Terrain = terrain;
        Unit = unit;
    }

    public Vector2I Cell { get; }
    public TerrainType? Terrain { get; }
    public BattleUnit? Unit { get; }
}