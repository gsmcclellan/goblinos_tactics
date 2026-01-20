using Goblinos.Scripts.Units.Types;

namespace Goblinos.Scripts.Combat.Types;

public class SimpleCombatResult
{
    public readonly UnitSnapshot Attacker;
    public readonly UnitSnapshot Defender;
    public readonly int AttackerDamage;
    public readonly int DefenderDamage;
    public readonly int AttackerHealthRemaining;
    public readonly int DefenderHealthRemaining;

    public bool AttackerDied => AttackerHealthRemaining <= 0;
    public bool DefenderDied => DefenderHealthRemaining <= 0;

    public SimpleCombatResult()
    {
        
    }
    
    public SimpleCombatResult(
        UnitSnapshot attacker,
        UnitSnapshot defender,
        int attackerDamage,
        int defenderDamage,
        int attackerHealthRemaining,
        int defenderHealthRemaining
    )
    {
        Attacker = attacker;
        Defender = defender;
        AttackerDamage = attackerDamage;
        DefenderDamage = defenderDamage;
        AttackerHealthRemaining = attackerHealthRemaining;
        DefenderHealthRemaining = defenderHealthRemaining;
    }
    
    public override string ToString()
    {
        return
            $"CombatResult:\n" +
            $"  Attacker: <{Attacker.UnitId}> {Attacker.UnitName}\n" +
            $"    Damage Dealt: {AttackerDamage}\n" +
            $"    HP Remaining: {AttackerHealthRemaining}\n" +
            $"    Died: {AttackerDied}\n" +
            $"  Defender: <{Defender.UnitId}> {Defender.UnitName}\n" +
            $"    Damage Dealt: {DefenderDamage}\n" +
            $"    HP Remaining: {DefenderHealthRemaining}\n" +
            $"    Died: {DefenderDied}";
    }
}
