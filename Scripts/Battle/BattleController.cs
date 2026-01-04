using System.Diagnostics;
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
    
    private GridCursor _cursor;
    private BattleUnit _selectedUnit;
    // private GridTile

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
        
        DebugUtil.Log("[BattleController] Ready", DebugLogSeverity.Info, DebugLogCategory.Initialization);
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
        
        Debug.Assert(Battle != null, "[BattleController] Battle must be initialized.");
        Debug.Assert(Grid != null, "[BattleController] BattleGrid must be initialized.");
        Debug.Assert(_cursor != null, "[BattleController] GridCursor must be initialized.");
        
        DebugUtil.Log("[BattleController] Battle Components Initialized", DebugLogSeverity.Info, DebugLogCategory.Initialization);
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
        // Show results screen
        // remove self from input router
        _inputRouter.Pop(this);
    }
    
    
    
    
}