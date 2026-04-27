using System;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Units.Stats;
using Godot;

namespace Goblinos.Scripts.Combat;

public class HitCalculator
{
    private readonly RandomNumberGenerator _rng;

    public HitCalculator(RandomNumberGenerator rng)
    {
        _rng = rng;
    }

    public HitResult Roll(DerivedStats attacker, DerivedStats defender, CombatContext context)
    {
        // TODO CombatContext
        var rng = _rng.RandiRange(-5, 5);
        var hitChance = HitChance(attacker, defender, context) + rng;

        var roll = _rng.RandfRange(1, 100);
        var isHit = roll <= hitChance;
        
        if (!isHit) return HitResult.Miss;

        // var critRoll = _rng.Next(1, 101);
        // var isCrit = critRoll <= attacker.CritChance;
        var isCrit = false;
        
        return isCrit ? HitResult.Crit : HitResult.Hit;
    }

    public int HitChance(DerivedStats attacker, DerivedStats defender, CombatContext context)
    {
        return  Math.Clamp(
            attacker.PhysicalAccuracy - defender.Evasion + context.AccuracyBonus,
            5, 95);
    }
}

public enum HitResult { Miss, Hit, Crit }