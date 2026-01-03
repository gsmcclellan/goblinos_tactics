using System;
using System.Diagnostics;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController
{
    /** Signals */
    [Signal]
    public delegate void GridCursorFocusChangedEventHandler(GridCursorFocus focus);
    
    /** Events */
    
    /** Event Listeners */
    
    private void _Ready_Actions()
    {
        // GridCursor
        Debug.Assert(_cursor != null, "GridCursor not initialized. Unable to set up actions.");
        _cursor.GridCursorFocusChanged += OnGridCursorFocusChanged;
        
        DebugUtil.Log("[BattleController.Actions] Ready", DebugLogSeverity.Info, DebugLogCategory.Initialization);
    }

    private void _ExitTree_Actions()
    {
        // GridCursor
        if (_cursor != null)
            _cursor.GridCursorFocusChanged -= OnGridCursorFocusChanged;
        
        DebugUtil.Log("[BattleController.Actions] Exit", DebugLogSeverity.Info, DebugLogCategory.Exit);
    }
    
    private void OnGridCursorFocusChanged(GridCursorFocus focus)
    {
        DebugUtil.Log("[BattleController.Actions] OnGridCursorFocusChanged", DebugLogSeverity.Info, DebugLogCategory.Signal);
        EmitSignal(SignalName.GridCursorFocusChanged, focus);
    }
}