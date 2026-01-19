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
}