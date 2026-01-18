namespace Goblinos.Scripts.Units.Stats;

public sealed class DerivedStatsCalculator
{
    public DerivedStats Build(UnitStats unitStats)
    {
        // Stage A: apply precompute modifiers to core/base
        // Stage B: compute derived
        // Stage C: apply postcompute modifiers to derived
        // return snapshot
        return new DerivedStats();
    }
}