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
    // GridCursor
    [Signal]
    public delegate void GridCursorFocusChangedEventHandler(GridCursorFocus focus); // TODO - change name to HoveredCellChanged or HoveredTerrainChanged
    
    /** Events */
    
    /** Event Listeners */
    
    private void _Ready_Events()
    {
        // GridCursor
        DebugUtil.Require(_cursor != null, "[GridCursor] not initialized. Unable to set up actions.");
        _cursor.GridCursorFocusChanged += OnGridCursorFocusChanged;
        
        _logger.Log("Ready_Events", LogSeverity.Info, LogCategory.Initialization);
    }

    private void _ExitTree_Actions()
    {
        // GridCursor
        if (_cursor != null)
            _cursor.GridCursorFocusChanged -= OnGridCursorFocusChanged;
        
        _logger.Log("ExitTree_Actions", LogSeverity.Info, LogCategory.Exit);
    }
    
    private void OnGridCursorFocusChanged(GridCursorFocus focus)
    {
        _logger.Log($"OnGridCursorFocusChanged hasUnit={focus.HasUnit}", LogSeverity.Info, LogCategory.Signal);
        EmitSignal(SignalName.GridCursorFocusChanged, focus);
    }

    private void NotifyInitialized()
    {
        EmitSignal(SignalName.BattleControllerInitialized);
    }
}