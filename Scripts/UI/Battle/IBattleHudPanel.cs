#nullable enable
using Goblinos.Scripts.Battle;
using Goblinos.Scripts.Battle.Terrain;
using Godot;
using BattleUnit = Goblinos.Scripts.Battle.Units.BattleUnit;

namespace Goblinos.Scripts.UI.Battle
{
    /// <summary>
    /// A HUD panel that can react to BattleController UI events.
    /// </summary>
    public interface IBattleHudPanel
    {
        void OnHoveredCellChanged(Vector2I newCell, Vector2I oldCell) {}
        void OnHoveredTerrainChanged(TerrainType? terrain) {}
        void OnHoveredUnitChanged(BattleUnit? hoveredUnit) {}
        void OnSelectedUnitChanged(BattleUnit? selectedUnit) {}
    }
}