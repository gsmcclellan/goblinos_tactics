using Goblinos.Scripts.Battle;
using Goblinos.Scripts.Units.Stats;

namespace Goblinos.Scripts.Combat;

public class DamageCalculator
{
    public int ComputeDamage(DerivedStats attackerStats, DerivedStats defenderStats)
    {
        // TODO - damage type - magic vs physical damage.
        return attackerStats.PhysicalDamage - defenderStats.PhysicalProtection;
    }
}