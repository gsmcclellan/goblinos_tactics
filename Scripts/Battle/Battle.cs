using Goblinos.Logging;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class Battle : Node2D
{
    /** Signals */

    /** Fields */
    private Logger _logger = LogManager.For<Battle>();

    /** Properties */

    /** Component nodes */
    public GridCursor Cursor;
    
    public override void _Ready()
    {
        Cursor = GetNode<GridCursor>("Overlays/GridCursor");
        _logger.Log("Ready", LogSeverity.Info, LogCategory.Initialization);
    }

    public void _Init()
    {
        
    }

    private void _SetupSubscriptions()
    {
        
    }
}