using System;
using System.Linq;
using Goblinos.Scripts.Units.Stats.Types;

namespace Goblinos.Scripts.Units.Stats;

public class StatBlock
{
    /** Core Attributes */
    // Physical power, damage
    public int Might { get; private set; }
    // hit, dodge, crit
    public int Agility { get; private set; }
    // health, defense
    public int Vitality { get; private set; }
    // magic damage, magic def
    public int Mind { get; private set; }
    // status apply, status defense, magic defense
    public int Presence { get; private set; }
    // crit, crit def
    public int Luck { get; private set; }
    
    /** Base Stats */
    public int Movement { get; private set; }
    public int MaxHitPoints { get; private set; }
    public int Defense { get; private set; }
    public int Resistance { get; private set; }
    
    // Weapon proficiency

    public StatBlock() {}

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
        return StatNameInfo.GetTier(statName) switch
        {
            StatTier.Core or StatTier.Base => statName switch
            {
                StatName.Might => Might,
                StatName.Agility => Agility,
                StatName.Vitality => Vitality,
                StatName.Mind => Mind,
                StatName.Presence => Presence,
                StatName.Luck => Luck,
                StatName.Movement => Movement,
                StatName.MaxHitPoints => MaxHitPoints,
                StatName.Defense => Defense,
                StatName.Resistance => Resistance,
                _ => throw new ArgumentOutOfRangeException(nameof(statName), statName, null)
            },

            StatTier.Derived => throw new ArgumentException(
                $"Stat '{statName}' is a derived stat and cannot be retrieved directly from {nameof(StatBlock)}.",
                nameof(statName)),

            _ => throw new ArgumentOutOfRangeException(nameof(statName), statName, null)
        };
    }
    
    public new void Set(StatName statName, int value)
    {
        switch (StatNameInfo.GetTier(statName))
        {
            case StatTier.Core:
            case StatTier.Base:
                switch (statName)
                {
                    // Core
                    case StatName.Might: Might = value; break;
                    case StatName.Agility: Agility = value; break;
                    case StatName.Vitality: Vitality = value; break;
                    case StatName.Mind: Mind = value; break;
                    case StatName.Presence: Presence = value; break;
                    // Base
                    case StatName.Luck: Luck = value; break;
                    case StatName.Movement: Movement = value; break;
                    case StatName.MaxHitPoints: MaxHitPoints = value; break;
                    case StatName.Defense: Defense = value; break;
                    case StatName.Resistance: Resistance = value; break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(statName), statName, null);
                }
                break;
            case StatTier.Derived:
                throw new ArgumentException(
                    $"Stat '{statName}' is a derived stat and cannot be set directly on {nameof(StatBlock)}.",
                    nameof(statName));
            default:
                throw new ArgumentOutOfRangeException(nameof(statName), statName, null);
        }
    }
    
    public new void Add(StatName statName, int amount)
    {
        Set(statName, Get(statName) + amount);
    }
    
    public new void Add(StatBlock stats)
    {
        foreach (StatName statName in StatNameInfo.CoreAndBaseStats)
            Add(statName, stats.Get(statName));
    }
    
    public new StatBlock Copy()
    {
        var statBlock = new StatBlock();
        foreach (var statName in StatNameInfo.CoreAndBaseStats)
            statBlock.Set(statName, Get(statName));
        return statBlock;
    }
    
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"{nameof(StatBlock)}:");
        foreach (var statName in StatNameInfo.CoreAndBaseStats)
            sb.AppendLine($"  {statName}: {Get(statName)}");
        return sb.ToString();
    }
}