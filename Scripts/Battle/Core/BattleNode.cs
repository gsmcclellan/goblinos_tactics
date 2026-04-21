using System.Diagnostics;
using Goblinos.Logging;
using Godot;

namespace Goblinos.Scripts.Battle.Core;

public partial class BattleNode : Node2D
{
    /** Signals */
    
    /** Components */
    private GobLogger _logger = GobLogManager.For<BattleNode>();

    [Export] private BattleController _controller;
    [Export] public GridCursor Cursor;

    /** Fields */
    private BattleContext _context;

    /** Properties */

    /** Component nodes */
   
    
    public override void _Ready()
    {
        GobLogManager.MinimumLoggingSeverity = GobLogSeverity.Info;
        
        Debug.Assert(Cursor != null, $"[{nameof(Battle)}] - {nameof(Cursor)} not bound");
        Debug.Assert(_controller != null, $"[{nameof(Battle)}] - {nameof(BattleController)} not bound");
        
        _logger.Log("Ready", GobLogSeverity.Info, GobLogCategory.Initialization);
        
    }

    public void Bind(BattleContext context)
    {
        _context = context;
        _controller.Bind(this, context);
    }

    private void _SetupSubscriptions()
    {
        
    }

    
}