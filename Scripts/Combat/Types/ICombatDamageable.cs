namespace Goblinos.Scripts.Combat.Types;

/// <summary>
/// Optional interface for applying damage directly inside CombatResolver.
/// If your BattleUnit already has ApplyDamage(int), implement this and pass the BattleUnit in as defender.
/// </summary>
public interface ICombatDamageable
{
    void ApplyDamage(int damage);
}