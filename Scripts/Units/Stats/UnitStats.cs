#nullable enable
using System;
using System.Collections.Generic;
using Goblinos.Logging;
using Goblinos.Scripts.Combat;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.Units.Stats.Types;
using Godot;

namespace Goblinos.Scripts.Units.Stats;

public sealed class UnitStats
{
    private readonly GobLogger _logger = GobLogManager.For<UnitStats>();
    
    public StatBlock BaseStats { get; }
    public HashSet<StatModifier> PermanentModifiers { get; } = [];
    
    public event Action? StatsChanged;

    public UnitStats()
    {
        BaseStats = new StatBlock();
    }
    
    public UnitStats(StatBlock baseStats)
    {
        BaseStats = baseStats;
    }

    public UnitStats Copy()
    {
        var stats = new UnitStats(BaseStats.Copy());
        foreach (var permanentModifier in PermanentModifiers)
        {
            stats.AddPermanentModifier(permanentModifier.Copy());
        }

        return stats;
    }
    
    /// <summary>
    /// Adds a persistent stat modifier and notifies listeners.
    /// </summary>
    public void AddPermanentModifier(StatModifier modifier)
    {
        _logger.Log($"[{nameof(UnitStats)}] {nameof(AddPermanentModifier)} " + modifier.SourceId, GobLogSeverity.Info, GobLogCategory.UnitLifecycle);

        PermanentModifiers.Add(modifier);
        StatsChanged?.Invoke();
    }

    public int Get(StatName statName)
    {
        return statName switch
        {
            // Core
            StatName.Might or
                StatName.Agility or
                StatName.Vitality or
                StatName.Mind or
                StatName.Presence or
                StatName.Luck => BaseStats.Get(statName),

            // Base
            StatName.Movement or
                StatName.MaxHitPoints or
                StatName.Defense or
                StatName.Resistance => BaseStats.Get(statName),

            // Derived
            StatName.AttackSpeed => throw new NotImplementedException("AttackSpeed is derived and not stored directly."),
            StatName.Accuracy => throw new NotImplementedException("Accuracy is derived and not stored directly."),
            StatName.Evasion => throw new NotImplementedException("Evasion is derived and not stored directly."),
            StatName.CritChance => throw new NotImplementedException("CritChance is derived and not stored directly."),
            StatName.CritDefense => throw new NotImplementedException("CritDefense is derived and not stored directly."),
            StatName.PhysicalProtection => throw new NotImplementedException("PhysicalProtection is derived and not stored directly."),
            StatName.MagicProtection => throw new NotImplementedException("MagicProtection is derived and not stored directly."),
            StatName.ArmorPierce => throw new NotImplementedException("ArmorPierce is derived and not stored directly."),
            StatName.MagicPenetration => throw new NotImplementedException("MagicPenetration is derived and not stored directly."),

            _ => throw new ArgumentOutOfRangeException(nameof(statName), statName, null)
        };
    }

    public void Add(StatName statName, int amount)
    {
        BaseStats.Add(statName, amount);
    }

    public void Add(StatBlock stats)
    {
        BaseStats.Add(stats);
    }
    
    public override string ToString()
    {
        var statBlockString = BaseStats.ToString();
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Modifiers:");
        foreach (var statModifier in PermanentModifiers)
            sb.AppendLine($"  {statModifier.StatName}: {statModifier.Value}"); 
        return statBlockString + '\n' + sb;
    }
}