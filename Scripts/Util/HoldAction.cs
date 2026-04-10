using System;


namespace Goblinos.Scripts.Util;

/// <summary>
/// Small helper class to use for held inputs.
/// Example: End turn timer
/// HoldAction endTurn = new HoldAction(0.75)
///
/// Sets up a hold action that user must press down for 0.75sec
/// call Start() when event fired to start
/// use Tick() to update progress & check for completion
/// use Cancel() if no longer holding button.
/// </summary>
public sealed class HoldAction
{
    public double HoldSeconds { get; }
    public bool IsHolding { get; private set; }
    public double Progress => HoldSeconds <= 0 ? 1 : Math.Clamp(_held / HoldSeconds, 0, 1);

    private double _held;
    private bool _fired;

    public HoldAction(double holdSeconds)
    {
        HoldSeconds = holdSeconds;
    }

    public void Start()
    {
        IsHolding = true;
        _held = 0;
        _fired = false;
    }

    public void Cancel()
    {
        IsHolding = false;
        _held = 0;
        _fired = false;
    }

    /// Returns true exactly once when the hold completes.
    public bool Tick(double delta)
    {
        if (!IsHolding || _fired) return false;

        _held += delta;
        if (_held >= HoldSeconds)
        {
            _fired = true;
            return true;
        }
        return false;
    }
}