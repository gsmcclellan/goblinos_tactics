using System.Collections.Generic;
using System.Threading.Tasks;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Core;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Combat;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.UI.Battle;
using Goblinos.Scripts.UI.Presentation;
using Goblinos.Scripts.Util;
using Godot;
using ReallyGoodIdeas.Presentation;

namespace Goblinos.Scripts.Battle.Controllers;

public class AnimationController
{
    /** Components */
    private readonly GobLogger _logger = GobLogManager.For<AnimationController>();

    private PresentationQueue _presentationQueue;
    
    private PackedScene _floatingTextScene = GD.Load<PackedScene>(GlobalSettings.FloatingTextScenePath);

    public AnimationController(PresentationQueue presentationQueue)
    {
        _presentationQueue = presentationQueue;
        
        if (!DebugUtil.Require(_presentationQueue != null, $"[{nameof(AnimationController)}] - Initialization failed, {nameof(PresentationQueue)} required."))
        
        _logger.Log($"Constructed ", GobLogSeverity.Info, GobLogCategory.Initialization);
    }

    public Task DisplayFloatingText(Vector2 globalPosition, string message)
    {
        _logger.Log($"{nameof(DisplayFloatingText)} message={message}", GobLogSeverity.Trace, GobLogCategory.CombatResolution);

        var floatingTextPresentable = new FloatingTextPresentable(globalPosition, message);
        return _presentationQueue.PresentOutOfQueue(floatingTextPresentable);
    }

    public async Task PlayCombatAnimation(CombatResult result)
    {
        
        foreach (var strike in result.Strikes)
        {
            var attacker = result.Participant(strike.AttackerId);
            var defender = result.Participant(strike.DefenderId);
            
            await attacker.PlayAttackingAnimation(); // flash attacker
            if (strike.HitResult != HitResult.Miss) 
                await defender.Flash(); // flash defender
            
            _ = DisplayFloatingText(defender.GlobalPosition, strike.HitResult == HitResult.Miss ? "MISS": strike.Damage.ToString());
            await defender.SyncDisplayAnimated();
        }
            
    }
}