#nullable enable
using System.Diagnostics;
using System.Linq;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Preview;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Controllers;

public partial class TurnController : Node
{
    /** Signals */
    [Signal]
    public delegate void TurnStartedEventHandler(BattleSide side, int turnNumber);

    [Signal]
    public delegate void TurnEndedEventHandler(BattleSide side, int turnNumber);

    [Signal]
    public delegate void PhaseChangedEventHandler(TurnPhase phase);

    /** Components */
    private readonly GobLogger _logger = GobLogManager.For<TurnController>();

    private UnitRegistry _unitRegistry;
    private EnemyTurnController _enemyTurnController;
    
    /** Properties */
    public BattleSide ActiveSide { get; private set; } = BattleSide.Player;
    public int TurnNumber { get; private set; } = 1;
    public TurnPhase Phase { get; private set; } = TurnPhase.PlayerInput;

    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------
    
    public void Bind(UnitRegistry unitRegistry, EnemyTurnController enemyTurnController)
    {
        _unitRegistry = unitRegistry;
        _enemyTurnController = enemyTurnController;
        Debug.Assert(_unitRegistry != null, "[TurnController] UnitRegistry must be bound.");
        Debug.Assert(_enemyTurnController != null, "[TurnController] EnemyTurnController must be bound.");
        _logger.Log("Bind Complete", GobLogSeverity.Info, GobLogCategory.Initialization);
    }
    
    // ---------------------------------------------------------------------
    // Public Methods
    // ---------------------------------------------------------------------

    public void HandleUnitExhausted(BattleUnit unit)
    {
        _logger.Log($"NotifyUnitExhausted unit={unit.UnitName}", GobLogSeverity.Trace, GobLogCategory.BattleState);

        if (ActiveSide != BattleSide.Player)
            return;

        TryEndPlayerTurnIfComplete();
    }

    public bool RequestEndPlayerTurn()
    {
        _logger.Log("RequestEndPlayerTurn", GobLogSeverity.Info, GobLogCategory.BattleState);

        if (!DebugUtil.Require(_unitRegistry != null, "[TurnController] No UnitRegistry."))
            return false;

        if (ActiveSide != BattleSide.Player || Phase != TurnPhase.PlayerInput)
            return false;

        // Make the state consistent: any unit that could have acted this turn no longer can.
        _unitRegistry.SetFriendlyUnitsActivationState(UnitActivationState.Exhausted);

        EndPlayerTurn();
        return true;
    }

    public bool TryEndPlayerTurnIfComplete()
    {
        if (!DebugUtil.Require(_unitRegistry != null, "[TurnController] No UnitRegistry."))
            return false;

        if (ActiveSide != BattleSide.Player || Phase != TurnPhase.PlayerInput)
            return false;

        if (!_unitRegistry.AreAllFriendlyUnitsExhausted())
            return false;

        EndPlayerTurn();
        return true;
    }
    
    // ---------------------------------------------------------------------
    // Turn Changes
    // ---------------------------------------------------------------------
    
    public void StartBattle()
    {
        _logger.Log("StartBattle", GobLogSeverity.Info, GobLogCategory.BattleState);
        BeginPlayerTurn();
    }

    private void BeginPlayerTurn()
    {
        if (!DebugUtil.Require(_unitRegistry != null, "[TurnController] No UnitRegistry."))
            return;

        ActiveSide = BattleSide.Player;
        SetPhase(TurnPhase.PlayerInput);
        var nonDormantUnits = _unitRegistry.GetUnitsWhere(unit => unit.State != UnitActivationState.Dormant).ToList();
        foreach (var battleUnit in _unitRegistry.GetUnitsWhere(unit => unit.State != UnitActivationState.Dormant))
        {
            battleUnit.SetActivationState(UnitActivationState.Ready);
        }

        EmitSignal(SignalName.TurnStarted, (int)ActiveSide, TurnNumber);
        _logger.Log($"BeginPlayerTurn turn={TurnNumber}", GobLogSeverity.Info, GobLogCategory.BattleState);
    }

    private void EndPlayerTurn()
    {
        EmitSignal(SignalName.TurnEnded, (int)ActiveSide, TurnNumber);
        _logger.Log($"EndPlayerTurn turn={TurnNumber}", GobLogSeverity.Info, GobLogCategory.BattleState);

        BeginEnemyTurn();
    }

    private async void BeginEnemyTurn()
    {
        ActiveSide = BattleSide.Enemy;
        SetPhase(TurnPhase.EnemyThinking);

        EmitSignal(SignalName.TurnStarted, (int)ActiveSide, TurnNumber);
        _logger.Log($"BeginEnemyTurn turn={TurnNumber}", GobLogSeverity.Info, GobLogCategory.BattleState);

        // Next: call your AI runner, then EndEnemyTurn when finished.
        await _enemyTurnController.RunEnemyTurnAsync();
        
        EndEnemyTurn();
    }

    public void EndEnemyTurn()
    {
        EmitSignal(SignalName.TurnEnded, (int)ActiveSide, TurnNumber);
        _logger.Log($"EndEnemyTurn turn={TurnNumber}", GobLogSeverity.Info, GobLogCategory.BattleState);

        TurnNumber += 1;
        BeginPlayerTurn();
    }
    
    // ---------------------------------------------------------------------
    // Private Helpers / Methods
    // ---------------------------------------------------------------------

    private void SetPhase(TurnPhase phase)
    {
        if (phase == Phase)
            return;

        Phase = phase;
        EmitSignal(SignalName.PhaseChanged, (int)Phase);
        _logger.Log($"PhaseChanged phase={Phase}", GobLogSeverity.Trace, GobLogCategory.BattleState);
    }
}
