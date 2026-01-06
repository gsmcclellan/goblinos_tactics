// DEPRECATED - using direct signals instead of re emiting here. If re emit, use sub node instead of BattleController partial.

using System;
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController
{
    /** Signals */
    // Battle Level
    [Signal] public delegate void BattleControllerInitializedEventHandler();
    
    // Components
    
    /** Events */
    
    /** Event Listeners */
    
    private void _Ready_Events()
    {
        // GridCursor
        DebugUtil.Require(_cursor != null, "[GridCursor] not initialized. Unable to set up actions.");
        
        _logger.Log("Ready_Events", LogSeverity.Info, LogCategory.Initialization);
    }

    private void _ExitTree_Actions()
    {
        
        _logger.Log("ExitTree_Actions", LogSeverity.Info, LogCategory.Exit);
    }

    private void NotifyInitialized()
    {
        EmitSignal(SignalName.BattleControllerInitialized);
    }
}