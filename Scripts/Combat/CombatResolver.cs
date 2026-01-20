using System.Collections;
using Goblinos.Logging;
using Goblinos.Scripts.Battle;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Units.Types;
using Goblinos.Scripts.Util;

namespace Goblinos.Scripts.Combat;

/// <summary>
/// Resolves committed combat actions into final gameplay results.
/// This is the authoritative source of combat truth.
/// </summary>
public class CombatResolver
{
    private readonly Logger _logger = LogManager.For<CombatResolver>();
    
    // private readonly HitCalculator _hitCalculator;
    private readonly DamageCalculator _damageCalculator;
    // private readonly StatusEffectResolver _statusEffectResolver;

    public CombatResolver(DamageCalculator damageCalculator)
    {
        _damageCalculator = damageCalculator;
        _logger.Log("Constructed.", LogSeverity.Trace, LogCategory.Initialization);
    }

    public SimpleCombatResult Resolve(UnitActivationContext activationContext)
    {
        // Simplified combat resolution - change later. 
        // Both units deal damage to each other, no hit, crit, order, multi attack.
        var attacker = activationContext.Unit;
        var defender = activationContext.PrimaryActionTargetUnit;

        if (!DebugUtil.Require(attacker != null, $"[{nameof(CombatResolver)}].Resolve failed, Attacker not found.") ||
            !DebugUtil.Require(defender != null, $"[{nameof(CombatResolver)}].Resolve failed, Defender not found.")
           )
            return new SimpleCombatResult();

        var attackerDamage = _damageCalculator.ComputeDamage(attacker, defender);
        var defenderDamage = _damageCalculator.ComputeDamage(defender, attacker);

        attacker.ApplyDamage(defenderDamage);
        defender.ApplyDamage(attackerDamage);

        return new SimpleCombatResult(
            attacker: new UnitSnapshot(attacker.Id, attacker.UnitName),
            defender: new UnitSnapshot(defender.Id, defender.UnitName),
            attackerDamage: attackerDamage,
            defenderDamage: defenderDamage,
            attackerHealthRemaining: attacker.CurrentHitPoints,
            defenderHealthRemaining: defender.CurrentHitPoints
        );
        
        // determine # & order of hits.
        // for each -
            // rolls Hit
            // rolls Crit
            // computes Damage
            // computes if one unit is dead
        // returns a result object you can log / show in UI
        
        // (optionally) applies damage to the defender via a small interface so you aren’t locked to a specific BattleUnit API.

        return new SimpleCombatResult();
    }
    
    
    
    
    
}