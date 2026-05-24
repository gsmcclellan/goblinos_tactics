using System.Collections.Generic;
using System.Threading.Tasks;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Core;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Combat;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.UI.Battle;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Controllers;

public class AnimationController
{
    /** Components */
    private readonly GobLogger _logger = GobLogManager.For<AnimationController>();

    private BattleNode _battle;

    private PackedScene _floatingTextScene = GD.Load<PackedScene>(GlobalSettings.FloatingTextScenePath);

    public AnimationController(BattleNode battle)
    {
        _battle = battle;
        
        if (!DebugUtil.Require(_battle != null, $"[{nameof(AnimationController)}] - Initialization failed, {nameof(BattleNode)} required."))
        
        _logger.Log($"Constructed ", GobLogSeverity.Info, GobLogCategory.Initialization);
    }

    public void DisplayFloatingText(Vector2 globalPosition, string message)
    {
        _logger.Log($"{nameof(DisplayFloatingText)} message={message}", GobLogSeverity.Trace, GobLogCategory.CombatResolution);

        var floatingText = _floatingTextScene.Instantiate<FloatingText>();
        _battle.AddChild(floatingText);
        floatingText.GlobalPosition = globalPosition;
        // floatingText.Scale = Vector2.One * 3f; // adjust to match previous inherited scale
        floatingText.Activate(globalPosition, message);
    }

    public async Task PlayCombatAnimation(CombatResult result, IEnumerable<BattleUnit> units)
    {
        Dictionary<string, BattleUnit> unitsById = new();
        foreach (var unit in units)
            unitsById.Add(unit.Id, unit);
        
        foreach (var strike in result.Strikes)
        {
            var attacker = strike.AttackerId;
            var defender = strike.DefenderId;
            
            
            await unitsById[strike.AttackerId].PlayAttackingAnimation(); // flash attacker
            if (strike.HitResult != HitResult.Miss) 
                await unitsById[strike.DefenderId].Flash(); // flash defender
            
            DisplayFloatingText(unitsById[strike.DefenderId].GlobalPosition, strike.HitResult == HitResult.Miss ? "MISS": strike.Damage.ToString());
            await unitsById[strike.DefenderId].SyncDisplay();
        }
            
    }
}