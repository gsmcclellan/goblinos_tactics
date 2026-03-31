using System;
using System.Collections;
using System.Threading.Tasks;
using Goblinos.Logging;
using Goblinos.Scripts.Battle;
using Goblinos.Scripts.Battle.Preview;
using Goblinos.Scripts.Battle.Services;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Units.Stats;
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

    public async Task<SimpleCombatResult> Resolve(IUnitActionPlan activationContext)
    {
        // Simplified combat resolution - change later. 
        // Both units deal damage to each other, no hit, crit, order, multi attack.
        var attacker = activationContext.Unit;
        var defender = activationContext.PrimaryActionTargetUnit;

        if (!DebugUtil.Require(attacker != null, $"[{nameof(CombatResolver)}].Resolve failed, Attacker not found.") ||
            !DebugUtil.Require(defender != null, $"[{nameof(CombatResolver)}].Resolve failed, Defender not found.")
           )
            return new SimpleCombatResult();

        var rangeValidationResult = ValidateAttackRange(attacker, defender);

        if (!DebugUtil.Require(rangeValidationResult.AttackerInRange,
                "Error during combat resolution, Attacker not in range."))
            return new SimpleCombatResult();
        
        var attackerStats = DerivedStatsCalculator.Build(attacker.Stats);
        var defenderStats = DerivedStatsCalculator.Build(defender.Stats);
        
        var attackerDamage = _damageCalculator.ComputeDamage(attackerStats, defenderStats);
        var defenderDamage = (rangeValidationResult.DefenderInRange) ? _damageCalculator.ComputeDamage(defenderStats, attackerStats): 0;

        var defTask = defender.ApplyDamage(attackerDamage);
        if (rangeValidationResult.DefenderInRange)
        {
            var attackTask = attacker.ApplyDamage(defenderDamage);
            await Task.WhenAll(defTask, attackTask);
        }
        else
            await defTask;
        
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
    }
    
    public CombatPreview GetCombatPreview(BattleUnit attacker, BattleUnit defender)
    {
        var attackerStats = DerivedStatsCalculator.Build(attacker.Stats); // TODO - these can be cached.
        var defenderStats = DerivedStatsCalculator.Build(defender.Stats);

        var rangeValidationResult = ValidateAttackRange(attacker, defender);
        
        var attackerDamage = _damageCalculator.ComputePreviewDamage(attackerStats, defenderStats);
        var defenderDamage = (rangeValidationResult.DefenderInRange) ? _damageCalculator.ComputePreviewDamage(defenderStats, attackerStats): 0;

        return new CombatPreview()
        {
            Attacker = attacker,
            Defender = defender,
            AttackerDamage = attackerDamage,
            DefenderDamage = defenderDamage
        };
    }
    
    private CombatRangeValidationResult ValidateAttackRange(
        BattleUnit attacker,
        BattleUnit defender)
    {
        var distance = ManhattanRangeService.GetDistance(attacker.Position, defender.Position);

        var attackerInRange = attacker.AttackRange.InRange(distance);
        var defenderInRange = defender.AttackRange.InRange(distance);

        return new CombatRangeValidationResult(attackerInRange, defenderInRange);
    }
}

public struct CombatRangeValidationResult(bool attackerInRange, bool defenderInRange)
{
    public bool AttackerInRange = attackerInRange;
    public bool DefenderInRange = defenderInRange;
}