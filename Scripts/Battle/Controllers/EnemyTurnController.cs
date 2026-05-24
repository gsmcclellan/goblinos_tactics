#nullable enable
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Core;
using Goblinos.Scripts.Battle.Services;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Util;
using Godot;
using ReallyGoodIdeas.Presentation;

namespace Goblinos.Scripts.Battle;

public sealed partial class EnemyTurnController : Node
{
    private readonly GobLogger _logger = GobLogManager.For<EnemyTurnController>();

    [Signal]
    public delegate void EnemyTurnStartedEventHandler();

    [Signal]
    public delegate void EnemyTurnFinishedEventHandler();

    private BattleController _battleController = null!;
    private BattleGrid _grid = null;
    private PresentationQueue _presentationQueue = null!;
    private UnitRegistry _unitRegistry = null!;
    
    
    private EnemyActionPlanningService _enemyActionPlanner = null!;
    private PrimaryActionTargetingService _primaryActionTargetingService = null!;
    private UnitActivationPreviewService _unitActivationPreviewService = null!;
    
    private bool _isRunning;

    private const int DelayBetweenEnemyActionsMilliseconds = 500;

    private const int AwakenNeighborDistance = 4; // If unit becomes awakened, also awakens teammates in an area.

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
        BattleGrid grid,
        EnemyActionPlanningService enemyActionPlanner,
        PresentationQueue presentationQueue,
        UnitRegistry unitRegistry
    )
    {
        _logger.Log("Initialize", GobLogSeverity.Info, GobLogCategory.Initialization);

        _battleController = battleController;
        _grid = grid;
        _enemyActionPlanner = enemyActionPlanner;
        _presentationQueue = presentationQueue;
        _unitRegistry = unitRegistry;
        
        Debug.Assert(_battleController != null, $"[{nameof(EnemyTurnController)}] BattleController must be bound.");
        Debug.Assert(_grid != null, $"[{nameof(EnemyTurnController)}] {nameof(BattleGrid)} must be bound.");
        Debug.Assert(_enemyActionPlanner != null, $"[{nameof(EnemyTurnController)}] EnemyActionPlanningService must be bound.");
        Debug.Assert(_presentationQueue != null, $"[{nameof(EnemyTurnController)}] PresentationQueue must be bound.");
        Debug.Assert(_unitRegistry != null, $"[{nameof(EnemyTurnController)}] UnitRegistry must be bound.");
        
        _primaryActionTargetingService = new PrimaryActionTargetingService(grid, unitRegistry);
        _unitActivationPreviewService = new UnitActivationPreviewService(grid, unitRegistry);
    }

    // ---------------------------------------------------------------------
    // Public API
    // ---------------------------------------------------------------------

    /// <summary>
    /// Runs the entire enemy turn sequence and resolves all enemy actions.
    /// </summary>
    public async Task RunEnemyTurnAsync()
    {
        _logger.Log($"{nameof(RunEnemyTurnAsync)}", GobLogSeverity.Info, GobLogCategory.BattleState);

        if (_isRunning)
        {
            _logger.Log($"{nameof(RunEnemyTurnAsync)} ignored: already running.", GobLogSeverity.Warn, GobLogCategory.BattleState);
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

        AwakenUnits();

        try
        {
            await ExecuteAllEnemyUnitTurnsAsync();
        }
        catch (Exception e)
        {
            GD.Print(e);
            return;
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
    /// Iterates all enemy dormant units to check if they should wake up.
    /// </summary>
    private void AwakenUnits()
    {
        var enemyDormantUnits =
            _unitRegistry.GetUnitsWhere(unit => !unit.IsFriendly && unit.State == UnitActivationState.Dormant)
                .ToList();
        var awakenedCount = 0;
        // Awaken if in move range of enemy
        enemyDormantUnits.ForEach(actingUnit =>
        {
            if (actingUnit.State == UnitActivationState.Ready) // May have been activated by neighbor.
                return;

            bool shouldAwaken;
            
            _ = _unitRegistry.TryGetCell(actingUnit, out var originCell);
            var unitActivationPreview = _unitActivationPreviewService.BuildPreview(actingUnit, originCell);
            var attackPreview = unitActivationPreview.GetPrimaryActionPreview(PrimaryActionType.Attack);

            
            var hasTarget = _unitRegistry.AnyUnits(unit =>
                unit.IsFriendly && _unitRegistry.TryGetCell(unit, out var unitCell) &&
                attackPreview.CanTargetCell(unitCell));

            shouldAwaken = hasTarget;
            
            if (!shouldAwaken) return;
            
            actingUnit.SetActivationState(UnitActivationState.Ready);
            awakenedCount++;
        
            var dormantNeighbors = enemyDormantUnits.Where(unit =>
                _unitRegistry.TryGetCell(unit, out var unitCell) &&
                ManhattanRangeService.GetDistance(unitActivationPreview.OriginCell, unitCell) <= AwakenNeighborDistance);
            
            foreach (var dormantNeighbor in dormantNeighbors)
            {
                dormantNeighbor.SetActivationState(UnitActivationState.Ready);
                awakenedCount++;
            }
                
            // Not cascading - can change if that's desired...
        });
        
        _logger.Log($"{nameof(AwakenUnits)} - awakened {awakenedCount} / {enemyDormantUnits.Count} units.", GobLogSeverity.Info, GobLogCategory.AiDecision);
    }
    
    /// <summary>
    /// Iterates all enemy units that can act, planning and committing actions sequentially.
    /// </summary>
    private async Task ExecuteAllEnemyUnitTurnsAsync()
    {
        _logger.Log("ExecuteAllEnemyUnitsAsync", GobLogSeverity.Info, GobLogCategory.AiDecision);
        
        var enemyUnits = _unitRegistry.GetUnitsWhere(unit => !unit.IsFriendly)
            .ToList();
        
        foreach (BattleUnit enemyUnit in enemyUnits)
        {
            _logger.Log($"Enemy unit acting: {enemyUnit.UnitName}", GobLogSeverity.Info, GobLogCategory.AiDecision);
            
            if (!enemyUnit.CanAct)
            {
                _logger.Log($"Enemy unit cannot act: {enemyUnit.UnitName}", GobLogSeverity.Trace, GobLogCategory.AiDecision);
                continue;
            }
            
            _presentationQueue.Enqueue(new DelayPresentable(DelayBetweenEnemyActionsMilliseconds));
            
            var plan = _enemyActionPlanner.BuildSimplePlan(enemyUnit);

            await ExecutePlan(_battleController, enemyUnit, plan);
        }
    }
    
    private Task ExecutePlan(BattleController controller, BattleUnit enemyUnit, EnemyActionPlan enemyPlan)
    {
        return controller.CommitUnitActivation(enemyPlan);
    }
}
