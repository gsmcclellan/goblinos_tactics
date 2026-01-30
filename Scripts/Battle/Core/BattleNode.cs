using Goblinos.Logging;
using Godot;

namespace Goblinos.Scripts.Battle.Core;

public partial class BattleNode : Node2D
{
    /** Signals */

    /** Fields */
    private Logger _logger = LogManager.For<BattleNode>();

    /** Properties */

    /** Component nodes */
    public GridCursor Cursor;
    
    public override void _Ready()
    {
        LogManager.MinimumLoggingSeverity = LogSeverity.Info;
        Cursor = GetNode<GridCursor>("Overlays/GridCursor");
        _logger.Log("Ready", LogSeverity.Info, LogCategory.Initialization);
        
        
    }

    private void _SetupSubscriptions()
    {
        
    }

    
}