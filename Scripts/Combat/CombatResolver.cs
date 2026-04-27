using System;
using System.Collections;
using System.Diagnostics;
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
    private readonly GobLogger _logger = GobLogManager.For<CombatResolver>();
    
    private readonly HitCalculator _hitCalculator;
    private readonly DamageCalculator _damageCalculator;
    // private readonly StatusEffectResolver _statusEffectResolver;

    public CombatResolver(DamageCalculator damageCalculator, HitCalculator hitCalculator)
    {
        _damageCalculator = damageCalculator;
        _hitCalculator = hitCalculator;
        
        Debug.Assert(_damageCalculator != null, $"[{nameof(AbilityResolver)}] {nameof(DamageCalculator)} must be initialized.");
        Debug.Assert(_hitCalculator != null, $"[{nameof(AbilityResolver)}] {nameof(HitCalculator)} must be initialized.");
        _logger.Log("Constructed.", GobLogSeverity.Trace, GobLogCategory.Initialization);
    }

    public async Task<CombatResult> Resolve(IUnitActionPlan activationContext)
    {
        
        // Simplified combat resolution - change later. 
        // Both units deal damage to each other, no hit, crit, order, multi attack.
        var attacker = activationContext.Unit;
        var defender = activationContext.PrimaryActionTargetUnit;
        
        var crb = new CombatResultBuilder(attacker, defender, activationContext.DestinationCell, activationContext.PrimaryActionTargetCell.Value);


        if (!DebugUtil.Require(attacker != null, $"[{nameof(CombatResolver)}].Resolve failed, Attacker not found.") ||
            !DebugUtil.Require(defender != null, $"[{nameof(CombatResolver)}].Resolve failed, Defender not found.") ||
            !DebugUtil.Require(activationContext.PrimaryActionTargetCell != null,
                $"[{nameof(CombatResolver)}].Resolve failed, Target cell not found.")
           )
            return crb.Results();

        
        var rangeValidationResult = ValidateAttackRange(attacker, defender);

        if (!DebugUtil.Require(rangeValidationResult.AttackerInRange,
                "Error during combat resolution, Attacker not in range."))
            return crb.Results();

        var attContext = new CombatContext();
        var attackerHitResult = _hitCalculator.Roll(attacker.Stats, defender.Stats, attContext);
        
        var attackerDamage = attackerHitResult switch {
            HitResult.Miss => 0,
            HitResult.Hit  => _damageCalculator.ComputeDamage(attacker.Stats, defender.Stats),
            HitResult.Crit => _damageCalculator.ComputeCritDamage(attacker.Stats, defender.Stats),
            _ => 0
        };
        
        await defender.ApplyDamage(attackerDamage);
        
        crb.AddStrike(
            attackerId: attacker.Id,
            defenderId: defender.Id,
            hitResult: attackerHitResult,
            damage: attackerDamage,
            defenderHitPointsRemaining: defender.CurrentHitPoints
        );
        
        var defenderCanCounter = !defender.IsDefeated && rangeValidationResult.DefenderInRange;

        var defenderDamage = 0;
        if (!defender.IsDefeated && defenderCanCounter)
        {
            var defContext = new CombatContext();
            var defenderHitResult = _hitCalculator.Roll(defender.Stats, attacker.Stats, defContext);

            defenderDamage = defenderHitResult switch
            {
                HitResult.Miss => 0,
                HitResult.Hit => _damageCalculator.ComputeDamage(defender.Stats, attacker.Stats),
                HitResult.Crit => _damageCalculator.ComputeCritDamage(defender.Stats, attacker.Stats),
                _ => 0
            };
            // defenderDamage = _damageCalculator.ComputeDamage(defender.Stats, attacker.Stats);

            await attacker.ApplyDamage(defenderDamage);
            
            crb.AddStrike(
                attackerId: defender.Id,
                defenderId: attacker.Id,
                hitResult: defenderHitResult,
                damage: defenderDamage,
                defenderHitPointsRemaining: attacker.CurrentHitPoints
            );
            
            
        }

        return crb.Results();
    }
    
    public CombatPreview GetCombatPreview(BattleUnit attacker, BattleUnit defender)
    {

        var rangeValidationResult = ValidateAttackRange(attacker, defender);

        var context = new CombatContext();
        
        
        var attackerDamage = _damageCalculator.ComputePreviewDamage(attacker.Stats, defender.Stats);
        var defenderDamage = (rangeValidationResult.DefenderInRange) ? _damageCalculator.ComputePreviewDamage(defender.Stats, attacker.Stats): 0;
        
        var attackerHitChance = _hitCalculator.HitChance(attacker.Stats, defender.Stats, context);
        var defenderHitChance = (rangeValidationResult.DefenderInRange) ? _hitCalculator.HitChance(defender.Stats, attacker.Stats, context): 0;

        return new CombatPreview()
        {
            Attacker = attacker,
            Defender = defender,
            AttackerHitChance = attackerHitChance,
            DefenderHitChance = defenderHitChance,
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