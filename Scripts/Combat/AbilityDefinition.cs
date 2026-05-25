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
    public string Description = "";
    public AbilityTargetMode TargetMode = AbilityTargetMode.None;
    public int Magnitude = 1;
    public StatName? MagnitudeStat;
    public StatName? TargetStat;
    public CombatConditionId? CombatConditionId;
    
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
        Id = AbilityId.DisableMovement,
        Type = AbilityType.StatModifier,
        DisplayName = "Disable",
        Description = "Disable movement until next turn.",
        TargetMode = AbilityTargetMode.SingleTarget,
        Range = RangeBand.One,
        CanTargetEnemies = true,
        Magnitude = -99,
        CanTarget = (self, target) => !target.HasStatModifier("DisableMovement")
    };
    
    public static AbilityDefinition Push => new()
    {
        Id = AbilityId.Push,
        Type = AbilityType.Push,
        DisplayName = "Push",
        Description = "Push target back one square.",
        TargetMode = AbilityTargetMode.SingleTarget,
        Range = RangeBand.One,
        CanTargetFriends = true,
        CanTargetEnemies = true
    };

    public static AbilityDefinition Swap => new()
    {
        Id = AbilityId.Swap,
        Type = AbilityType.Swap,
        DisplayName = "Swap",
        Description = "Swap positions with target.",
        TargetMode = AbilityTargetMode.SingleTarget,
        Range = RangeBand.One,
        CanTargetFriends = true,
        CanTargetEnemies = true
    };

    public static AbilityDefinition Haste => new AbilityDefinition()
    {
        Id = AbilityId.Haste,
        Type = AbilityType.StatModifier,
        DisplayName = "Haste",
        Description = "Increase target's movement range.",
        TargetMode = AbilityTargetMode.SingleTarget,
        Range = RangeBand.One,
        CanTargetFriends = true,
        Magnitude = 2,
        TargetStat = StatName.Movement,
        CanTarget = (self, target) =>
        {
            var can = target.CanAct && !target.HasStatModifier("Haste");
            return target.CanAct && !target.HasStatModifier("Haste");
        }
    };
    
    public static AbilityDefinition Heal => new AbilityDefinition()
    {
        Id = AbilityId.Heal,
        Type = AbilityType.Heal,
        DisplayName = "Heal",
        Description = "Restore hit points",
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
    Condition,
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