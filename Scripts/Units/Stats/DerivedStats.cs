using System;
using System.Collections.Generic;
using Goblinos.Scripts.Units.Stats.Types;

namespace Goblinos.Scripts.Units.Stats;

public sealed class DerivedStats(
    int might,
    int agility,
    int vitality,
    int mind,
    int presence,
    int luck,
    int movement,
    int maxHitPoints,
    int defense,
    int resistance,
    int physicalDamage,
    int magicDamage,
    int physicalAccuracy,
    int magicAccuracy,
    // int attackSpeed,
    int evasion,
    // int critChance,
    // int critDefense,
    int physicalProtection,
    int magicResistance
    // int armorPierce,
    // int magicPenetration
    )
{
    /** Core Attributes */
    public int Might { get; } = might;
    public int Agility { get; } = agility;
    public int Vitality { get; } = vitality;
    public int Mind { get; } = mind;
    public int Presence { get; } = presence;
    public int Luck { get; } = luck;
    
    /** Base Stats */
    public int Movement { get; } = movement;
    public int MaxHitPoints { get; } = maxHitPoints;
    public int Defense { get; } = defense;
    public int Resistance { get; } = resistance;

    /** Derived Stats */
    public int PhysicalDamage { get; } = physicalDamage;
    public int MagicDamage { get; } = magicDamage;

    public int PhysicalAccuracy { get; } = physicalAccuracy;
    public int MagicAccuracy { get; } = magicAccuracy;
    // public int AttackSpeed { get; } = attackSpeed;
    public int Evasion { get; } = evasion;
    // public int CritChance { get; } = critChance;
    // public int CritDefense { get; } = critDefense;
    public int PhysicalProtection { get; } = physicalProtection;
    public int MagicProtection { get; } = magicResistance;
    // public int ArmorPierce { get; } = armorPierce;
    // public int MagicPenetration { get; } = magicPenetration;
    
    
    public int Get(StatName statName)
    {
        return statName switch
        {
            // Core Stats
            StatName.Might => Might,
            StatName.Agility => Agility,
            StatName.Vitality => Vitality,
            StatName.Mind => Mind,
            StatName.Presence => Presence,
            StatName.Luck => Luck,
            
            // Base Stats
            StatName.Movement => Movement,
            StatName.MaxHitPoints => MaxHitPoints,
            StatName.Defense => Defense,
            StatName.Resistance => Resistance,
            
            // Derived Stats
            // StatName.AttackSpeed => AttackSpeed,
            StatName.PhysicalAccuracy => PhysicalAccuracy,
            StatName.MagicAccuracy => MagicAccuracy,
            StatName.Evasion => Evasion,
            // StatName.CritChance => CritChance,
            // StatName.CritDefense => CritDefense,
            StatName.PhysicalProtection => PhysicalProtection,
            StatName.MagicProtection => MagicProtection,
            // StatName.ArmorPierce => ArmorPierce,
            // StatName.MagicPenetration => MagicPenetration,
            
            
            
            _ => throw new NotImplementedException($"[{nameof(DerivedStats)}] Getter for {nameof(StatName)} {statName} not implemented.")
        };
    }
}