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
    /** Events */
    public event Action<IReadOnlyList<StatName>> StatsChanged;
    
    /** Components */
    private readonly GobLogger _logger = GobLogManager.For<UnitStats>();

    /** Fields */
    private StatBlock _baseStats;
    
    /** Properties */
    public StatBlock BaseStats
    {
        get => _baseStats;
        set
        {
            if (_baseStats != null)
                _baseStats.StatsChanged -= RaiseStatsChanged;
            _baseStats = value;
            if (_baseStats != null)
                _baseStats.StatsChanged += RaiseStatsChanged;
        }
    }
    public HashSet<StatModifier> PermanentModifiers { get; } = [];
    
    public UnitStats()
    {
        BaseStats = new StatBlock();
    }
    
    public UnitStats(StatBlock baseStats)
    {
        BaseStats = baseStats;
    }
    
    // ---------------------------------------------------------------------
    // Public Methods
    // ---------------------------------------------------------------------

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

            // Derived - accessed via DerivedStats class, not UnitStats
            

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


    private void RaiseStatsChanged(IReadOnlyList<StatName> changed) 
        => StatsChanged?.Invoke(changed);
}