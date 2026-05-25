using System;
using System.Collections.Generic;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Units.Types;
using Godot;

namespace Goblinos.Scripts.Combat.Types;

/// <summary>
/// Result of a single resolved attack, suitable for logging and UI.
/// </summary>
public readonly struct CombatResult(
    BattleUnit attacker,
    BattleUnit defender,
    bool attackerDied,
    bool defenderDied,
    IReadOnlyList<CombatStrike> strikes)
{
    public readonly BattleUnit Attacker = attacker;
    public readonly BattleUnit Defender = defender;

    public readonly bool AttackerDied = attackerDied;
    public readonly bool DefenderDied = defenderDied;

    public readonly IReadOnlyList<CombatStrike> Strikes = strikes;
    
    public BattleUnit Participant(string id)
    {
        if (id == Attacker.Id)
            return Attacker;
       if (id == Defender.Id)
            return Defender;
       throw new ArgumentException("Id does not match attacker or defender.");
    }
}

public readonly struct CombatResultSnapshot(
    UnitSnapshot attacker,
    UnitSnapshot defender,
    bool attackerDied,
    bool defenderDied,
    IReadOnlyList<CombatStrike> strikes)
{
    public readonly UnitSnapshot Attacker = attacker;
    public readonly UnitSnapshot Defender = defender;

    public readonly bool AttackerDied = attackerDied;
    public readonly bool DefenderDied = defenderDied;

    public readonly IReadOnlyList<CombatStrike> Strikes = strikes;
    
    public UnitSnapshot Participant(string id)
    {
        if (id == Attacker.UnitId)
            return Attacker;
        if (id == Defender.UnitId)
            return Defender;
        throw new ArgumentException("Id does not match attacker or defender.");
    }
}

public readonly struct CombatStrike(
    string attackerId,
    string defenderId,
    HitResult hitResult,
    int damage,
    int defenderHitPointsRemaining,
    DamageType damageType)
{
    public readonly string AttackerId = attackerId;
    public readonly string DefenderId = defenderId;
    public readonly HitResult HitResult = hitResult;
    public readonly int Damage = damage;
    public readonly int DefenderHitPointsRemaining = defenderHitPointsRemaining;
    public readonly DamageType DamageType = damageType;

    public readonly int DefenderHealthRemaining;
}

public class CombatResultBuilder(BattleUnit attacker, BattleUnit defender, Vector2I attackerCell, Vector2I defenderCell)
{
    private readonly BattleUnit _attacker = attacker;
    private readonly BattleUnit _defender = defender;

    private readonly Vector2I _attackerCell = attackerCell;
    private readonly Vector2I _defenderCell = defenderCell;

    private int _attackerStartingHealth = attacker.CurrentHitPoints;
    private int _defenderStartingHealth = defender.CurrentHitPoints;

    private bool _attackerDied = false;
    private bool _defenderDied = false;

    public List<CombatStrike> Strikes = new();

    public void AddStrike(string attackerId, string defenderId, HitResult hitResult, int damage, int defenderHitPointsRemaining, DamageType damageType = DamageType.Slash)
    {
        if (defenderHitPointsRemaining == 0)
        {
            if (defenderId == _attacker.Id)
                _attackerDied = true;
            if (defenderId == _defender.Id)
                _defenderDied = true;
        }
        Strikes.Add(new CombatStrike(attackerId, defenderId, hitResult, damage, defenderHitPointsRemaining, damageType));
    }

    public CombatResult Results()
    {
        return new CombatResult(_attacker, _defender, _attackerDied, _defenderDied, Strikes);
    }

    public CombatResultSnapshot ResultsSnapshot()
    {
        var attackerSnapshot = new UnitSnapshot(_attacker.Id, _attacker.UnitName, _attackerCell);
        var defenderSnapshot = new UnitSnapshot(_defender.Id, _defender.UnitName, _defenderCell);
        
        return new CombatResultSnapshot(attackerSnapshot, defenderSnapshot, _attackerDied, _defenderDied, Strikes);
    }
}
