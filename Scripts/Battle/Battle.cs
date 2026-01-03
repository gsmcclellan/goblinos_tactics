using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class Battle : Node2D
{
    /** Signals */

    /** Properties */

    /** Component nodes */
    public GridCursor Cursor;
    
    public override void _Ready()
    {
        Cursor = GetNode<GridCursor>("Overlays/GridCursor");
        DebugUtil.Log("[Battle] Ready", DebugLogSeverity.Info, DebugLogCategory.Initialization);
    }

    public void _Init()
    {
        
    }

    private void _SetupSubscriptions()
    {
        
    }
}