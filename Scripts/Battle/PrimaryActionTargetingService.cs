using System;
using System.Collections.Generic;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Types;
using Godot;

namespace Goblinos.Scripts.Battle;

public class PrimaryActionTargetingService
{
    private readonly Logger _logger = LogManager.For<PrimaryActionTargetingService>();
    
    public bool IsValidTarget(UnitActivationContext unitActivation, CellFocus focus)
    {
        _logger.Log("IsValidTarget", LogSeverity.Extra, LogCategory.Input);
        
        // Example:
        // - unitActivation knows acting unit + selected action + action range rules
        // - focus contains hovered unit / terrain, etc.
        
        throw new NotImplementedException();
    }

    public IReadOnlySet<Vector2I> GetValidTargets(UnitActivationContext unitActivation, CellFocus focus)
    {
        _logger.Log("GetValidTargets", LogSeverity.Extra, LogCategory.Input);
        
        // Example:
        // - unitActivation knows acting unit + selected action + action range rules
        // - focus contains hovered unit / terrain, etc.
        
        throw new NotImplementedException();
    }
}