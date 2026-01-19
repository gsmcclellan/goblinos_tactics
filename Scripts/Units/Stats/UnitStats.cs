#nullable enable
using System;
using System.Collections.Generic;
using Goblinos.Logging;
using Goblinos.Scripts.Combat;

namespace Goblinos.Scripts.Units.Stats;

public sealed class UnitStats
{
    private readonly Logger _logger = LogManager.For<UnitStats>();
    
    public StatBlock BaseStats { get; }
    public HashSet<StatModifier> PermanentModifiers { get; } = [];
    
    public event Action? StatsChanged;
    
    public UnitStats(StatBlock baseStats)
    {
        BaseStats = baseStats;
    }
    
    /// <summary>
    /// Adds a persistent stat modifier and notifies listeners.
    /// </summary>
    public void AddPermanentModifier(StatModifier modifier)
    {
        _logger.Log($"[{nameof(UnitStats)}] {nameof(AddPermanentModifier)} " + modifier.SourceId, LogSeverity.Info, LogCategory.UnitLifecycle);

        PermanentModifiers.Add(modifier);
        StatsChanged?.Invoke();
    }
}