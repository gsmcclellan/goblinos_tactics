#nullable enable
using System;
using System.Reflection;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Units.Stats.Types;
using Godot;

namespace Goblinos.Scripts.Combat;

public partial class AbilityDefinition: Resource
{
    public AbilityId Id = AbilityId.None;
    public AbilityType Type = AbilityType.None;
    public string DisplayName = "None";
    public AbilityTargetMode TargetMode = AbilityTargetMode.None;
    public int Magnitude = 1;
    public StatName? MagnitudeStat;
    
    public bool CanTargetSelf;
    public bool CanTargetFriends;
    public bool CanTargetEnemies;

    public RangeBand Range;

    public Func<BattleUnit, BattleUnit, bool>? CanTarget;

    public bool RequiresTarget => TargetMode is AbilityTargetMode.SingleTarget or AbilityTargetMode.MultiTarget or AbilityTargetMode.Area;
}


public enum AbilityTargetMode
{
    None,
    Self,
    SingleTarget,
    MultiTarget,
    Area
}

public static class AbilityDefinitionTemplates
{
    public static AbilityDefinition Get(AbilityId id) => id switch
    {
        AbilityId.Haste => Haste,
        // AbilityId.Shield => Shield,
        AbilityId.DisableMovement => DisableMovement,
        AbilityId.Push => Push,
        AbilityId.Swap => Swap,
        AbilityId.Heal => Heal,
        AbilityId.None => new AbilityDefinition(),
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
    };
    
    public static AbilityDefinition DisableMovement => new()
    {
        Type = AbilityType.DisableMovement,
        DisplayName = "Disable",
        TargetMode = AbilityTargetMode.SingleTarget,
        Range = RangeBand.One,
        CanTargetEnemies = true
    };
    
    public static AbilityDefinition Push => new()
    {
        Type = AbilityType.Push,
        DisplayName = "Push",
        TargetMode = AbilityTargetMode.SingleTarget,
        Range = RangeBand.One,
        CanTargetFriends = true,
        CanTargetEnemies = true
    };

    public static AbilityDefinition Swap => new()
    {
        Type = AbilityType.Swap,
        DisplayName = "Swap",
        TargetMode = AbilityTargetMode.SingleTarget,
        Range = RangeBand.One,
        CanTargetFriends = true,
        CanTargetEnemies = true
    };

    public static AbilityDefinition Haste => new AbilityDefinition()
    {
        Type = AbilityType.StatModifier,
        DisplayName = "Haste",
        TargetMode = AbilityTargetMode.SingleTarget,
        Range = RangeBand.One,
        CanTargetFriends = true,
        Magnitude = 2
    };
    
    public static AbilityDefinition Heal => new AbilityDefinition()
    {
        Type = AbilityType.Heal,
        DisplayName = "Heal",
        TargetMode = AbilityTargetMode.SingleTarget,
        Range = RangeBand.One,
        CanTargetFriends = true,
        MagnitudeStat = StatName.Presence,
        CanTarget = (self, target) => target.CurrentHitPoints < target.MaxHitPoints
    };
}

public enum AbilityType
{
    None,
    DisableMovement,
    Push,
    StatModifier,
    Swap, 
    Heal
}

public enum AbilityId
{
    None,
    Haste,
    Shield,
    DisableMovement,
    Push,
    Swap,
    Heal
}