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
    int attackSpeed,
    int accuracy,
    int evasion,
    int critChance,
    int critDefense,
    int physicalProtection,
    int magicProtection,
    int armorPierce,
    int magicPenetration)
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
    public int AttackSpeed { get; } = attackSpeed;
    public int Accuracy { get; } = accuracy;
    public int Evasion { get; } = evasion;
    public int CritChance { get; } = critChance;
    public int CritDefense { get; } = critDefense;
    public int PhysicalProtection { get; } = physicalProtection;
    public int MagicProtection { get; } = magicProtection;
    public int ArmorPierce { get; } = armorPierce;
    public int MagicPenetration { get; } = magicPenetration;
}