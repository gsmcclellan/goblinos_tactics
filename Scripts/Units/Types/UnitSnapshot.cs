using Godot;

namespace Goblinos.Scripts.Units.Types;

public readonly record struct UnitSnapshot(
    string UnitId,
    string UnitName,
    Vector2I Cell
);