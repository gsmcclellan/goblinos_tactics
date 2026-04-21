using System;
using Goblinos.Scripts.Units.Stats.Types;

namespace Goblinos.Scripts.Units.Stats;

public class StatGrowthProfile
{
    public static readonly int MinimumGrowthPercent = 0;
    public static readonly int MaximumGrowthPercent = 200;
    // Contains percent likelihood that each stat goes up when leveling.
    
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

    public StatGrowthProfile(
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
        Might = ClampPercent(might, nameof(might));
        Agility = ClampPercent(agility, nameof(agility));
        Vitality = ClampPercent(vitality, nameof(vitality));
        Mind = ClampPercent(mind, nameof(mind));
        Presence = ClampPercent(presence, nameof(presence));
        Luck = ClampPercent(luck, nameof(luck));
        
        Movement = ClampPercent(movement, nameof(movement));
        MaxHitPoints = ClampPercent(maxHitPoints, nameof(maxHitPoints));
        Defense = ClampPercent(defense, nameof(defense));
        Resistance = ClampPercent(resistance, nameof(resistance));
    }
    
    private static int ClampPercent(int value, string paramName)
    {
        if (value < MinimumGrowthPercent || value > MaximumGrowthPercent)
            throw new ArgumentOutOfRangeException(paramName, value, $"Growth percent must be in [{MinimumGrowthPercent}, {MaximumGrowthPercent}].");
        return value;
    }
    
    public new int Get(StatName statName)
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
}