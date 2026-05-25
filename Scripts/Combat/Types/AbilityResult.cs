#nullable enable
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Units.Stats;
using Godot;

namespace Goblinos.Scripts.Combat.Types;

public record AbilityResult
{
    public BattleUnit Actor;
    public BattleUnit? Target;
    public AbilityType AbilityType;
    public bool AbilityResolved;
    public string? ErrorMessage;

    // Only meaningful for damage/heal abilities
    public int? AmountHealed;
    public int? AmountDamaged;

    // Only meaningful for movement abilities
    public Vector2I? ActorDestination;
    public Vector2I? TargetDestination;

    // Only meaningful for stat modifier abilities
    public StatModifier? AppliedModifier;
    public CombatConditionId? AppliedCondition;
    
    // ---------------------------------------------------------------------
    // Factory Methods
    // ---------------------------------------------------------------------

    public static AbilityResult ConditionApplied(BattleUnit actor, BattleUnit target, CombatConditionId conditionId) =>
        new()
        {
            Actor = actor,
            Target = target,
            AbilityType = AbilityType.Condition,
            AbilityResolved = true,
            AppliedCondition = conditionId
        };
    
    public static AbilityResult Healed(BattleUnit actor, BattleUnit target, int amountHealed) => new()
    {
        Actor = actor,
        Target = target,
        AbilityType = AbilityType.Heal,
        AbilityResolved = true,
        AmountHealed = amountHealed
    };

    public static AbilityResult Pushed(BattleUnit actor, BattleUnit target, Vector2I targetDestination, bool success) => new()
    {
        Actor = actor,
        Target = target,
        AbilityType = AbilityType.Push,
        AbilityResolved = success,
        TargetDestination = targetDestination,
        ErrorMessage = success ? null : "Unable to move target"
    };
    
    public static AbilityResult StatModified(BattleUnit actor, BattleUnit target, StatModifier modifier) =>
        new()
        {
            Actor = actor,
            Target = target,
            AbilityType = AbilityType.StatModifier,
            AbilityResolved = true,
            AppliedModifier = modifier
        };

    public static AbilityResult Swapped(BattleUnit actor, BattleUnit target, Vector2I actorDestination,
        Vector2I targetDestination, bool success) =>
        new()
        {
            Actor = actor,
            Target = target,
            ActorDestination = actorDestination,
            TargetDestination = targetDestination,
            AbilityResolved = success,
            ErrorMessage = success ? null : "Unable to swap target"
        };

    public static AbilityResult Failed(BattleUnit actor, BattleUnit? target, AbilityType abilityType, string errorMessage) => new()
    {
        Actor = actor,
        Target = target,
        AbilityType = abilityType,
        AbilityResolved = false,
        ErrorMessage = errorMessage
    };
    
    public static AbilityResult Failed() => new()
    {
        AbilityResolved = false,
        ErrorMessage = "Failed to Resolve Ability."
    };
}