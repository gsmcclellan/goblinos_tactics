#nullable enable
using Goblinos.Scripts.Battle;
using Goblinos.Scripts.Battle.Terrain;
using Godot;

namespace Goblinos.Scripts.UI.Battle
{
    /// <summary>
    /// A HUD panel that can react to BattleController UI events.
    /// </summary>
    public interface IBattleHudPanel
    {
        void OnHoveredTerrainChanged(TerrainType? terrain);

        void OnSelectedUnitChanged(BattleUnit? selectedUnit);
    }
}