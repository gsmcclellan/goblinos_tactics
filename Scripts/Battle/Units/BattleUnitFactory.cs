using Goblinos.Logging;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Units;

namespace Goblinos.Scripts.Battle.Units;

/// <summary>
/// Creates BattleUnit instances from persistent Units.
/// </summary>
public sealed class BattleUnitFactory
{
    private readonly GobLogger _logger = GobLogManager.For<BattleUnitFactory>();

    public BattleUnit Create(Unit unit)
    {
        _logger.Log("[BattleUnitFactory] Create " + unit.UnitName, GobLogSeverity.Info, GobLogCategory.Initialization);

        // Starting health is pulled from persistent BaseStats.MaxHitPoints for now.
        return new BattleUnit(unit);
    }
}
