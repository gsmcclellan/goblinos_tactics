#nullable enable
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Services;
using Goblinos.Scripts.Combat;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.UI.Battle;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController : Node
{
    /** Signals */

    /** Actions */

    /** Components */
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [Export] private NodePath _cursorPath;

    [Export] private NodePath _gridPath;
    [Export] private NodePath _hudPath;
    [Export] private NodePath _movementControllerPath;
    [Export] private NodePath _selectionControllerPath;
    [Export] private NodePath _turnControllerPath;
    [Export] private NodePath _unitRegistryPath;

    private Core.Battle _battle;
    private Core.GridCursor _cursor;
    private Core.BattleGrid _grid;
    private BattleHud _hud;
    private Controllers.MovementController _movementController;
    private Controllers.SelectionController _selectionController;
    private Controllers.TurnController _turnController;
    private Units.UnitRegistry _unitRegistry;

    private readonly Logger _logger = LogManager.For<BattleController>();
    
    private CombatResolver _combatResolver;
    private MoveRangeService _moveRangeService;
    private PrimaryActionTargetingService _primaryActionTargetingService;
    private TargetRangeService _targetRangeService;
    
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /** Fields */


    /** Properties */


    public override void _Ready()
    {
        CallDeferred(nameof(_DeferredInit));
    }

    private void _DeferredInit()
    {
        _InitializeBattleComponents();
        _BindBattleComponents();
        _SubscribeToEvents();

        _Ready_Input();
        _Ready_Events();
        _Ready_State();
        _Ready_Test();

        NotifyInitialized();

        HandleStartOfBattle();

        _logger.Log("Ready", LogSeverity.Info, LogCategory.Initialization);
    }

    public override void _ExitTree()
    {
        _ExitTree_Actions();
        base._ExitTree();
    }

    private void _InitializeBattleComponents()
    {
        _battle = GetNode<Core.Battle>(GlobalSettings.BattlePath);
        _cursor = _battle.Cursor;
        _grid = GetNode<Core.BattleGrid>(_gridPath);
        _hud = GetNode<BattleHud>(_hudPath);
        _movementController = GetNode<Controllers.MovementController>(_movementControllerPath);
        _selectionController = GetNode<Controllers.SelectionController>(_selectionControllerPath);
        _turnController = GetNode<Controllers.TurnController>(_turnControllerPath);
        _unitRegistry = GetNode<Units.UnitRegistry>(_unitRegistryPath);

        Debug.Assert(_battle != null, "[BattleController] Battle must be initialized.");
        Debug.Assert(_cursor != null, "[BattleController] GridCursor must be initialized.");
        Debug.Assert(_grid != null, "[BattleController] BattleGrid must be initialized.");
        Debug.Assert(_hud != null, "[BattleController] BattleHud must be initialized.");
        Debug.Assert(_movementController != null, "[BattleController] MovementController must be initialized.");
        Debug.Assert(_selectionController != null, "[BattleController] SelectionController must be initialized.");
        Debug.Assert(_turnController != null, "[BattleController] TurnController must be initialized.");
        Debug.Assert(_unitRegistry != null, "[BattleController] UnitRegistry must be initialized.");

        // Non-Node Components
        _combatResolver = new CombatResolver(new DamageCalculator());
        _moveRangeService = new MoveRangeService(_grid, _unitRegistry);
        _primaryActionTargetingService = new PrimaryActionTargetingService(_grid, _unitRegistry);
        _targetRangeService = new TargetRangeService(_grid);

        _logger.Log("Battle Components Initialized", LogSeverity.Info, LogCategory.Initialization);
    }

    private void _BindBattleComponents()
    {
        _hud.Bind(this, _cursor, _selectionController, _turnController);
        _movementController.Bind(_grid, _unitRegistry);
        _selectionController.Bind(_cursor, _grid, _unitRegistry);
        _turnController.Bind(_unitRegistry);
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
        _turnController.StartBattle();
    }

    public void HandleEndOfBattle(bool isVictory)
    {
        _logger.Log("Battle End", LogSeverity.Info, LogCategory.Exit);
        // Show results screen
        // remove self from input router
        _inputRouter.Pop(this);
    }
}