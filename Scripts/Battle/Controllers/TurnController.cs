#nullable enable
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Types;
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
    private readonly Logger _logger = LogManager.For<TurnController>();

    private Units.UnitRegistry _unitRegistry;
    
    /** Properties */
    public BattleSide ActiveSide { get; private set; } = BattleSide.Player;
    public int TurnNumber { get; private set; } = 1;
    public TurnPhase Phase { get; private set; } = TurnPhase.PlayerInput;

    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------
    
    public void Bind(Units.UnitRegistry unitRegistry)
    {
        _unitRegistry = unitRegistry;
        Debug.Assert(_unitRegistry != null, "[TurnController] UnitRegistry must be bound.");
        _logger.Log("Bind Complete", LogSeverity.Info, LogCategory.Initialization);
    }

    public void StartBattle()
    {
        _logger.Log("StartBattle", LogSeverity.Info, LogCategory.BattleState);
        BeginPlayerTurn();
    }

    public void NotifyUnitExhausted(Units.BattleUnit unit)
    {
        _logger.Log($"NotifyUnitExhausted unit={unit.UnitName}", LogSeverity.Trace, LogCategory.BattleState);

        if (ActiveSide != BattleSide.Player)
            return;

        TryEndPlayerTurnIfComplete();
    }

    public bool RequestEndPlayerTurn()
    {
        _logger.Log("TryEndPlayerTurnEarly", LogSeverity.Info, LogCategory.BattleState);

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

    private void BeginPlayerTurn()
    {
        if (!DebugUtil.Require(_unitRegistry != null, "[TurnController] No UnitRegistry."))
            return;

        ActiveSide = BattleSide.Player;
        SetPhase(TurnPhase.PlayerInput);

        _unitRegistry.SetFriendlyUnitsActivationState(UnitActivationState.Ready);

        EmitSignal(SignalName.TurnStarted, (int)ActiveSide, TurnNumber);
        _logger.Log($"BeginPlayerTurn turn={TurnNumber}", LogSeverity.Info, LogCategory.BattleState);
    }

    private void EndPlayerTurn()
    {
        EmitSignal(SignalName.TurnEnded, (int)ActiveSide, TurnNumber);
        _logger.Log($"EndPlayerTurn turn={TurnNumber}", LogSeverity.Info, LogCategory.BattleState);

        BeginEnemyTurn();
    }

    private void BeginEnemyTurn()
    {
        ActiveSide = BattleSide.Enemy;
        SetPhase(TurnPhase.EnemyThinking);

        EmitSignal(SignalName.TurnStarted, (int)ActiveSide, TurnNumber);
        _logger.Log($"BeginEnemyTurn turn={TurnNumber}", LogSeverity.Info, LogCategory.BattleState);

        // Next: call your AI runner, then EndEnemyTurn when finished.
        // TODO
        EndEnemyTurn();
    }

    public void EndEnemyTurn()
    {
        EmitSignal(SignalName.TurnEnded, (int)ActiveSide, TurnNumber);
        _logger.Log($"EndEnemyTurn turn={TurnNumber}", LogSeverity.Info, LogCategory.BattleState);

        TurnNumber += 1;
        BeginPlayerTurn();
    }

    private void SetPhase(TurnPhase phase)
    {
        if (phase == Phase)
            return;

        Phase = phase;
        EmitSignal(SignalName.PhaseChanged, (int)Phase);
        _logger.Log($"PhaseChanged phase={Phase}", LogSeverity.Trace, LogCategory.BattleState);
    }
}
