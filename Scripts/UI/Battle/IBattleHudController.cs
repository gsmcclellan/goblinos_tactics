namespace Goblinos.Scripts.UI.Battle;

public interface IBattleHudController
{
    void RequestEndTurn();

    void InputStateChanged(int state);
}