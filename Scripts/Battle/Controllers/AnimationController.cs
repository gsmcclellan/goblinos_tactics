using System.Threading.Tasks;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Core;
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
}