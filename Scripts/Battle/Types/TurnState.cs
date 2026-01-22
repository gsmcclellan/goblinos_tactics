namespace Goblinos.Scripts.Battle.Types;

public class TurnState
{
    public int TurnNumber { get; private set; } = 1;
    public BattleSide ActiveSide { get; }
    public TurnPhase Phase { get; }
}