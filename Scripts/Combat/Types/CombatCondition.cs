using System;
using System.Reflection;
using Goblinos.Scripts.Units.Types;

namespace Goblinos.Scripts.Combat.Types;

public class CombatCondition
{
    public CombatConditionId Id;
    public string DisplayName { get; set; }
    public CombatConditionType Type;
    public int Stacks { get; set; } = 1;
    public int MaxStacks { get; set; } = 1;
    public ExpirationTime ExpiresAt = ExpirationTime.EndOfRound;

    public void AddStacks(int numStacks)
    {
        Stacks = Math.Max(Stacks + numStacks, MaxStacks);
    }
}

public static class CombatConditionTemplates
{
    public static CombatCondition Get(CombatConditionId conditionId)
    {
        var property = typeof(CombatConditionTemplates).GetProperty(
            conditionId.ToString(),
            BindingFlags.Public | BindingFlags.Static);

        if (property == null)
            throw new ArgumentException($"Unable to get combat condition for type {conditionId.ToString()}", nameof(conditionId));

        return (CombatCondition)property.GetValue(null)!;
    }

    public static CombatCondition DisableMovement => new CombatCondition()
    {
        Id = CombatConditionId.DisableMovement,
        DisplayName = "Disable",
        Type = CombatConditionType.Debuff,
        Stacks = 1,
        ExpiresAt = ExpirationTime.EndOfRound
    };

    public static CombatCondition Haste => new CombatCondition()
    {
        Id = CombatConditionId.Hasted,
        DisplayName = "Hasted",
        Type = CombatConditionType.Buff,
        Stacks = 1,
        ExpiresAt = ExpirationTime.EndOfRound
    };
}

public enum CombatConditionType
{
    DisableMovement,
    Buff,
    Debuff
}
public enum CombatConditionId
{
    DisableMovement,
    Hasted
}
