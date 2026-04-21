using Goblinos.Scripts.Units;
using Godot;

namespace Goblinos.Scripts.Battle.Core;

public class BattleContext(RandomNumberGenerator combatRng, UnitProgression unitProgression)
{
    public RandomNumberGenerator CombatRng = combatRng;
    public UnitProgression UnitProgression = unitProgression;
}