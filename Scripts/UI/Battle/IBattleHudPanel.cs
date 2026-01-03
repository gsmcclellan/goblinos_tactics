#nullable enable
using Goblinos.Scripts.Battle;
using Godot;

namespace Goblinos.Scripts.UI.Battle
{
    /// <summary>
    /// A HUD panel that can react to BattleController UI events.
    /// </summary>
    public interface IBattleHudPanel
    {
        void OnCursorFocusChanged(GridCursorFocus focus);

        void OnSelectedUnitChanged(Scripts.Battle.BattleUnit? selectedUnit);
    }
}