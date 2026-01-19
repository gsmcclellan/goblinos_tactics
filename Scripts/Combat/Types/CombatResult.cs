using System.Collections.Generic;
using Goblinos.Scripts.Units.Types;

namespace Goblinos.Scripts.Combat.Types;

/// <summary>
/// Result of a single resolved attack, suitable for logging and UI.
/// </summary>
public readonly struct CombatResult(
    UnitSnapshot attacker,
    UnitSnapshot defender,
    IReadOnlyList<CombatStrike> strikes)
{
    public readonly UnitSnapshot Attacker = attacker;
    public readonly UnitSnapshot Defender = defender;

    public readonly IReadOnlyList<CombatStrike> Strikes = strikes;
}

public readonly struct CombatStrike(
    string attackerId,
    string defenderId,
    bool isHit,
    bool isCritical,
    int damage,
    DamageType damageType)
{
    public readonly string AttackerId = attackerId;
    public readonly string DefenderId = defenderId;
    public readonly int Damage = damage;
    public readonly bool IsHit = isHit;
    public readonly bool IsCritical = isCritical;
    public readonly DamageType DamageType = damageType;
}

