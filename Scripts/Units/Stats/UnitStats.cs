using System.Collections.Generic;
using Goblinos.Scripts.Combat;

namespace Goblinos.Scripts.Units.Stats;

public class UnitStats
{
    public StatBlock BaseStats { get; }
    public HashSet<StatModifier> Modifiers { get; } = [];
    
    
}