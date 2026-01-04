using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController : Node, IInputHandler
{
    /** Signals */

    /** Actions */

    /** Components */
    [Export] private NodePath _battleGridPath;
    
    public Battle Battle;
    public BattleGrid Grid;

    private Logger _logger = LogManager.For<BattleController>();
    private GridCursor _cursor;
    private BattleUnit _selectedUnit;

    /** Properties */
    
    public override void _Ready()
    {
        CallDeferred(nameof(_DeferredInit));
    }

    private void _DeferredInit()
    {
        _InitializeBattleComponents();
        _SetupSubscriptions();
        
        _Ready_Input();
        _Ready_Events();
        _Ready_Units();
        
        NotifyInitialized();
        
        HandleStartOfBattle();
        
        _logger.Log("Ready", LogSeverity.Info, LogCategory.Initialization);
    }

    public override void _ExitTree()
    {
        _RemoveSubscriptions();
        _ExitTree_Actions();
        base._ExitTree();
    }

    private void _InitializeBattleComponents()
    {
        Battle = GetParent<Battle>();
        Grid = GetNode<BattleGrid>(_battleGridPath);
        _cursor = Battle.Cursor;
        
        DebugUtil.Require(Battle != null, "[BattleController] Battle must be initialized.");
        DebugUtil.Require(Grid != null, "[BattleController] BattleGrid must be initialized.");
        DebugUtil.Require(_cursor != null, "[BattleController] GridCursor must be initialized.");
        
        _logger.Log("Battle Components Initialized", LogSeverity.Info, LogCategory.Initialization);
    }

    private void _SetupSubscriptions()
    {
    }

    private void _RemoveSubscriptions()
    {
    }

    public override void _Process(double delta)
    {
        _Process_Input(delta);
    }
    private void DoEnemyTurn(bool isFirstTurn = false)
    {
    }

    
    public void HandleStartOfBattle()
    {
        
    }

    public void HandleEndOfBattle(bool isVictory)
    {
        _logger.Log("Battle End", LogSeverity.Info, LogCategory.Exit);
        // Show results screen
        // remove self from input router
        _inputRouter.Pop(this);
    }
    
    
    
    
}