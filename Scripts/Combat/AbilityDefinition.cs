using System;
using System.Reflection;
using Goblinos.Scripts.Combat.Types;
using Godot;

namespace Goblinos.Scripts.Combat;

public partial class AbilityDefinition: Resource
{
    public string DisplayName = "None";
    public AbilityType Type = AbilityType.None;
    public AbilityTargetMode TargetMode;
    
    public bool CanTargetSelf;
    public bool CanTargetFriends;
    public bool CanTargetEnemies;

    public RangeBand Range;

    public bool RequiresTarget => TargetMode == AbilityTargetMode.SingleTarget ||
                                  TargetMode == AbilityTargetMode.MultiTarget || 
                                  TargetMode == AbilityTargetMode.Area;
}


public enum AbilityTargetMode
{
    None,
    Self,
    SingleTarget,
    MultiTarget,
    Area
}

public static class AbilityDefinitions
{
    public static AbilityDefinition Get(AbilityType type)
    {
        var field = typeof(AbilityDefinitions).GetField(
            type.ToString(),
            BindingFlags.Public | BindingFlags.Static);

        if (field == null)
            return new AbilityDefinition();

        return (AbilityDefinition)field.GetValue(null)!;
    }
    
    public static AbilityDefinition Push = new AbilityDefinition()
    {
        Type = AbilityType.Push,
        DisplayName = "Push",
        TargetMode = AbilityTargetMode.SingleTarget,
        Range = RangeBand.One,
        CanTargetFriends = true,
        CanTargetEnemies = true
    };
}

public enum AbilityType
{
    None,
    Push
}