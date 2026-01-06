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
    
    [Export] private NodePath _cursorPath;
    [Export] private NodePath _gridPath;
    [Export] private NodePath _selectionControllerPath;
    [Export] private NodePath _unitRegistryPath;
    
    private Battle _battle;
    private GridCursor _cursor;
    private BattleGrid _grid;
    private SelectionController _selectionController;
    private UnitRegistry _unitRegistry;

    /** Fields */
    
    
    /** Properties */
    
    
    
    private Logger _logger = LogManager.For<BattleController>();
    
    private BattleUnit _selectedUnit;

    /** Properties */
    
    public override void _Ready()
    {
        CallDeferred(nameof(_DeferredInit));
    }

    private void _DeferredInit()
    {
        _InitializeBattleComponents();
        _BindBattleComponents();
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
        _battle = GetParent<Battle>();
        _cursor = _battle.Cursor;
        _grid = GetNode<BattleGrid>(_gridPath);
        _selectionController = GetNode<SelectionController>(_selectionControllerPath);
        _unitRegistry = GetNode<UnitRegistry>(_unitRegistryPath);
        
        Debug.Assert(_battle != null, "[BattleController] Battle must be initialized.");
        Debug.Assert(_grid != null, "[BattleController] BattleGrid must be initialized.");
        Debug.Assert(_cursor != null, "[BattleController] GridCursor must be initialized.");
        Debug.Assert(_selectionController != null, "[BattleController] SelectionController must be initialized.");
        Debug.Assert(_unitRegistry != null, "[BattleController] UnitRegistry must be initialized.");
        
        _logger.Log("Battle Components Initialized", LogSeverity.Info, LogCategory.Initialization);
    }

    private void _BindBattleComponents()
    {
        _selectionController.Bind(_cursor, _grid, _unitRegistry);
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