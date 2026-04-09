using System;
using System.Reflection;

namespace Goblinos.Scripts.Combat.Types;

public class CombatCondition
{
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
    public static CombatCondition Get(CombatConditionType type)
    {
        var property = typeof(CombatConditionTemplates).GetProperty(
            type.ToString(),
            BindingFlags.Public | BindingFlags.Static);

        if (property == null)
            throw new ArgumentException($"Unable to get combat condition for type {type.ToString()}", nameof(type));

        return (CombatCondition)property.GetValue(null)!;
    }

    public static CombatCondition DisableMovement => new CombatCondition()
    {
        DisplayName = "Disable",
        Type = CombatConditionType.DisableMovement,
        Stacks = 1,
        ExpiresAt = ExpirationTime.EndOfRound
    };
}

public enum ExpirationTime
{
    EndOfAction,
    EndOfRound,
    EndOfBattle
}

public enum CombatConditionType
{
    DisableMovement
}