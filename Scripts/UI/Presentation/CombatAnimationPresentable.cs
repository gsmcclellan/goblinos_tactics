using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Goblinos.Scripts.Battle.Controllers;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Combat.Types;
using Godot;
using ReallyGoodIdeas.Presentation;

namespace Goblinos.Scripts.UI.Presentation;

public class CombatAnimationPresentable(
    AnimationController animationController,
    CombatResult combatResult,
    IEnumerable<BattleUnit> battleUnits)
    : IPresentable
{
    public event Action OnComplete;

    public PresentationLayer Layer => PresentationLayer.World;

    public async Task Present(Node parent)
    {
        await animationController.PlayCombatAnimation(combatResult);
        OnComplete?.Invoke();
    }

    public Task Skip(Node parent)
    {
        throw new NotImplementedException();
    }

    // // Use this if animation needs a specific parent, then this element should own that reference rather than the queue
    // public async Task Present(Node _) // ignores the queue-provided parent
    // {
    //     await _unit.PlayCombatAnimation(...);
    // }
}