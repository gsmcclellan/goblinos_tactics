using System;
using Goblinos.Scripts.Units.Stats.Types;
using Goblinos.Scripts.Units.Types;

namespace Goblinos.Scripts.Units.Stats;

public sealed class StatModifier
{
    public string Id { get; }
    public string SourceId { get;  }
    public StatName StatName { get; }
    public int Value { get; private set; }
    public ExpirationTime ExpiresAt { get; }

    public StatTier StatTier { get; }
    public StatModifierStage ModifierStage { get; }
    
    public StatModifier(string id,
        string sourceId, 
        StatName statName, 
        int value,
        ExpirationTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            throw new ArgumentException("SourceId is required.", nameof(sourceId));

        Id = id;
        SourceId = sourceId;
        StatName = statName;
        Value = value;
        ExpiresAt = expiresAt;
        
        StatTier = StatNameInfo.GetTier(statName);
        ModifierStage = StatNameInfo.GetTier(statName) switch
        {
            StatTier.Core or StatTier.Base => StatModifierStage.PreCompute,
            StatTier.Derived => StatModifierStage.PostCompute,
            _ => throw new ArgumentOutOfRangeException(nameof(statName), statName, "Unhandled StatName.")
        };
    }

    public StatModifier Copy()
    {
        var statModifier = new StatModifier(Id, SourceId, StatName, Value, ExpiresAt);
        return statModifier;
    }

    public void Add(StatModifier statMod)
    {
        Value += statMod.Value;
    }

    public bool Equals(StatModifier statMod)
    {
        return StatName == statMod.StatName           &&
               ModifierStage == statMod.ModifierStage &&
               ExpiresAt == statMod.ExpiresAt;
    }
}