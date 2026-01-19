using Goblinos.Logging;
using Goblinos.Scripts.Units;

namespace Goblinos.Scripts.Battle;

/// <summary>
/// Creates BattleUnit instances from persistent Units.
/// </summary>
public sealed class BattleUnitFactory
{
    private readonly Logger _logger = LogManager.For<BattleUnitFactory>();

    public BattleUnit Create(Unit unit)
    {
        _logger.Log("[BattleUnitFactory] Create " + unit.UnitName, LogSeverity.Info, LogCategory.Initialization);

        // Starting health is pulled from persistent BaseStats.MaxHealth for now.
        return new BattleUnit(unit);
    }
}
