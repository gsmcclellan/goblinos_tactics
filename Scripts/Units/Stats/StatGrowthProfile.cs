using System;

namespace Goblinos.Scripts.Units.Stats;

public class StatGrowthProfile
{
    public static readonly int MinimumGrowthPercent = 0;
    public static readonly int MaximumGrowthPercent = 100;
    // Contains percent likelihood that each stat goes up when leveling.
    public int Might { get; }
    public int Agility { get; }
    public int Vitality { get; }
    public int Mind { get; }
    public int Presence { get; }
    public int Luck { get; }
    
    public StatGrowthProfile(
        int might,
        int agility,
        int vitality,
        int mind,
        int presence,
        int luck)
    {
        Might = ClampPercent(might, nameof(might));
        Agility = ClampPercent(agility, nameof(agility));
        Vitality = ClampPercent(vitality, nameof(vitality));
        Mind = ClampPercent(mind, nameof(mind));
        Presence = ClampPercent(presence, nameof(presence));
        Luck = ClampPercent(luck, nameof(luck));
    }
    
    private static int ClampPercent(int value, string paramName)
    {
        if (value < MinimumGrowthPercent || value > MaximumGrowthPercent)
            throw new ArgumentOutOfRangeException(paramName, value, $"Growth percent must be in [{MinimumGrowthPercent}, {MaximumGrowthPercent}].");
        return value;
    }
}