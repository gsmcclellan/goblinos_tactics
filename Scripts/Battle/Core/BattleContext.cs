using Goblinos.Scripts.UI.Presentation;
using Goblinos.Scripts.Units;
using Godot;

namespace Goblinos.Scripts.Battle.Core;

public class BattleContext(RandomNumberGenerator combatRng, UnitProgression unitProgression, PresentationQueue presQueue)
{
    public PresentationQueue PresentationQueue = presQueue;
    public RandomNumberGenerator CombatRng = combatRng;
    public UnitProgression UnitProgression = unitProgression;
}