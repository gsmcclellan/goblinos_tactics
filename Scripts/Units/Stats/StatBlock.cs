using System;
using Goblinos.Scripts.Units.Stats.Types;

namespace Goblinos.Scripts.Units.Stats;

public class StatBlock
{
    /** Core Attributes */
    // Physical power, damage
    public int Might;
    // hit, dodge, crit
    public int Agility;
    // health, defense
    public int Vitality;
    // magic damage, magic def
    public int Mind;
    // status apply, status defense, magic defense
    public int Presence;
    // crit, crit def
    public int Luck;

    /** Base Stats */
    public int Movement;
    public int MaxHitPoints;
    public int Defense;
    public int Resistance;

    // Weapon proficiency
    public StatBlock(
        int might,
        int agility,
        int vitality,
        int mind,
        int presence,
        int luck,
        int movement,
        int maxHitPoints,
        int defense,
        int resistance)
    {
        Might = might;
        Agility = agility;
        Vitality = vitality;
        Mind = mind;
        Presence = presence;
        Luck = luck;
        Movement = movement;
        MaxHitPoints = maxHitPoints;
        Defense = defense;
        Resistance = resistance;
    }

    public int Get(StatName statName)
    {
        // Can only get core / base stats
        if (StatNameInfo.GetTier(statName) == StatTier.Derived)
            throw new ArgumentException(
                $"Stat '{statName}' is a derived stat and cannot be retrieved directly from {nameof(StatBlock)}.",
                nameof(statName));
        
        return statName switch
        {
            // Core
            StatName.Might => Might,
            StatName.Agility => Agility,
            StatName.Vitality => Vitality,
            StatName.Mind => Mind,
            StatName.Presence => Presence,
            StatName.Luck => Luck,

            // Base
            StatName.Movement => Movement,
            StatName.MaxHitPoints => MaxHitPoints,
            StatName.Defense => Defense,
            StatName.Resistance => Resistance,

            _ => throw new ArgumentOutOfRangeException(nameof(statName), statName, null)
        };
    }
}