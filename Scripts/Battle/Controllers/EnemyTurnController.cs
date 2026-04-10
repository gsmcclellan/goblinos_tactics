#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Services;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public sealed partial class EnemyTurnController : Node
{
    private readonly GobLogger _logger = GobLogManager.For<EnemyTurnController>();

    [Signal]
    public delegate void EnemyTurnStartedEventHandler();

    [Signal]
    public delegate void EnemyTurnFinishedEventHandler();

    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private BattleController _battleController;
    private UnitRegistry _unitRegistry;
    private EnemyActionPlanningService _enemyActionPlanner;
    #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    
    private bool _isRunning;

    private const float DelayBetweenEnemyActionsSeconds = .5f;

    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------

    public override void _Ready()
    {
        _logger.Log($"[{nameof(EnemyTurnController)}] Ready", GobLogSeverity.Info, GobLogCategory.Initialization);
    }

    /// <summary>
    /// Wires required dependencies for enemy turn orchestration.
    /// </summary>
    public void Bind(
        BattleController battleController,
        EnemyActionPlanningService enemyActionPlanner,
        UnitRegistry unitRegistry
    )
    {
        _logger.Log("Initialize", GobLogSeverity.Info, GobLogCategory.Initialization);

        _battleController = battleController;
        _enemyActionPlanner = enemyActionPlanner;
        _unitRegistry = unitRegistry;
        
        Debug.Assert(_battleController != null, $"[{nameof(EnemyTurnController)}] BattleController must be bound.");
        Debug.Assert(_enemyActionPlanner != null, $"[{nameof(EnemyTurnController)}] EnemyActionPlanningService must be bound.");
        Debug.Assert(_unitRegistry != null, $"[{nameof(EnemyTurnController)}] UnitRegistry must be bound.");
    }

    // ---------------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------------

    /// <summary>
    /// Runs the entire enemy turn sequence and resolves all enemy actions.
    /// </summary>
    public async Task RunEnemyTurnAsync()
    {
        _logger.Log("RunEnemyTurnAsync", GobLogSeverity.Info, GobLogCategory.BattleState);

        if (_isRunning)
        {
            _logger.Log("RunEnemyTurnAsync ignored: already running.", GobLogSeverity.Warn, GobLogCategory.BattleState);
            return;
        }

        if (!DebugUtil.Require(_battleController != null, "EnemyTurnController not initialized: BattleController is null."))
            return;

        if (!DebugUtil.Require(_unitRegistry != null, "EnemyTurnController not initialized: UnitRegistry is null."))
            return;

        if (!DebugUtil.Require(_enemyActionPlanner != null, "EnemyTurnController not initialized: EnemyActionPlanningService is null."))
            return;

        _isRunning = true;

        EmitSignal(SignalName.EnemyTurnStarted);

        try
        {
            await ExecuteAllEnemyUnitTurnsAsync(_unitRegistry, _enemyActionPlanner, _battleController);
        }
        finally
        {
            _isRunning = false;
            EmitSignal(SignalName.EnemyTurnFinished);
        }
    }

    // ---------------------------------------------------------------------
    // Private
    // ---------------------------------------------------------------------

    /// <summary>
    /// Iterates all enemy units that can act, planning and committing actions sequentially.
    /// </summary>
    private async Task ExecuteAllEnemyUnitTurnsAsync(
        UnitRegistry unitRegistry,
        EnemyActionPlanningService enemyActionPlanner,
        BattleController battleController
    )
    {
        _logger.Log("ExecuteAllEnemyUnitsAsync", GobLogSeverity.Info, GobLogCategory.AiDecision);
        
        var enemyUnits = unitRegistry.GetUnitsWhere(unit => !unit.IsFriendly)
            .ToList();
        
        foreach (BattleUnit enemyUnit in enemyUnits)
        {
            _logger.Log($"Enemy unit acting: {enemyUnit.UnitName}", GobLogSeverity.Info, GobLogCategory.AiDecision);
            
            await ToSignal(
                GetTree().CreateTimer(DelayBetweenEnemyActionsSeconds),
                SceneTreeTimer.SignalName.Timeout
            );

            if (!enemyUnit.CanAct)
            {
                _logger.Log($"Enemy unit cannot act: {enemyUnit.UnitName}", GobLogSeverity.Trace, GobLogCategory.AiDecision);
                continue;
            }

            var plan = enemyActionPlanner.BuildSimplePlan(enemyUnit);

            await ExecutePlanAsync(battleController, enemyUnit, plan);
        }
    }

    private Task ExecutePlanAsync(BattleController controller, BattleUnit enemyUnit, EnemyActionPlan enemyPlan)
    {
        controller.CommitUnitActivation(enemyPlan);
        return Task.CompletedTask;
    }
}
