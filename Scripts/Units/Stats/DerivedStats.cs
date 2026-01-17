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
    
    // consider adding these later with more systems 
    // public int StunResist { get; }
    // public int PoisonResist { get; }
    // public int BleedResist { get; }
    // public int FireResist { get; }
    // public int LightningResist { get; }
    // public int FrostResist { get; }

    // public DerivedStats(
    //     StatBlock baseStats,
    //     IEnumerable<IReadonlyStatModifier> modifiers,
    //     StatCaps caps)
    // {
    //     
    // }
}