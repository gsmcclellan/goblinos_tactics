using Goblinos.Logging;
using Godot;

namespace Goblinos.Scripts.Battle.Core;

public partial class BattleNode : Node2D
{
    /** Signals */

    /** Fields */
    private GobLogger _logger = GobLogManager.For<BattleNode>();

    /** Properties */

    /** Component nodes */
    public GridCursor Cursor;
    
    public override void _Ready()
    {
        GobLogManager.MinimumLoggingSeverity = GobLogSeverity.Info;
        Cursor = GetNode<GridCursor>("Overlays/GridCursor");
        _logger.Log("Ready", GobLogSeverity.Info, GobLogCategory.Initialization);
        
        
    }

    private void _SetupSubscriptions()
    {
        
    }

    
}