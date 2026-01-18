using System;
using System.Collections.Generic;
using Goblinos.Scripts.Units.Stats.Types;

namespace Goblinos.Scripts.Units.Stats;

public sealed class DerivedStats
{
    public int AttackSpeed { get; }
    public int Accuracy { get; }
    public int Evasion { get; }
    public int CritChance { get; }
    public int CritDefense { get; }
    public int PhysicalProtection { get; }
    public int MagicProtection { get; }
    public int ArmorPierce { get; }
    public int MagicPenetration { get; }
    
    public DerivedStats(
        /* inputs: final core/base stats + weapon + context + modifiers */)
    {
        // compute once
    }
}