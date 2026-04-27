using System;
using Goblinos.Scripts.Battle;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.Units.Stats;

namespace Goblinos.Scripts.Combat;

public class DamageCalculator
{
    public int ComputeDamage(DerivedStats attackerStats, DerivedStats defenderStats)
    {
        // TODO - damage type - magic vs physical damage.
        return Math.Max(GlobalSettings.MinimumCombatDamage, attackerStats.PhysicalDamage - defenderStats.PhysicalProtection);
    }

    public int ComputeCritDamage(DerivedStats attackerStats, DerivedStats defenderStats) =>
        ComputeDamage(attackerStats, defenderStats); // TODO
    
    public int ComputePreviewDamage(DerivedStats attackerStats, DerivedStats defenderStats)
    {
        // This is expected damage for preview. Any randomness is ignored, taking min value. ie, assume no crit.
        // TODO - damage type - magic vs physical damage.
        return Math.Max(GlobalSettings.MinimumCombatDamage, attackerStats.PhysicalDamage - defenderStats.PhysicalProtection);
    }
}