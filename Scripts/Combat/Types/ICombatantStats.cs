namespace Goblinos.Scripts.Combat.Types;

/// <summary>
/// Minimal interface for reading combat-relevant stats.
/// This prevents CombatResolver from depending on BattleUnit, Unit, or UnitStats shapes.
/// </summary>
public interface ICombatantStats
{
    int Might { get; }
    int Agility { get; }
    int Luck { get; }
    int Defense { get; }
}