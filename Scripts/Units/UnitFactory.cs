using System;
using Goblinos.Logging;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Units.Stats;

namespace Goblinos.Scripts.Units
{
    /// <summary>
    /// Creates persistent Units from UnitTemplates.
    /// </summary>
    public sealed class UnitFactory
    {
        private readonly Logger _logger = LogManager.For<UnitFactory>();

        /// <summary>
        /// Instantiates a Unit from a template.
        /// </summary>
        public Unit CreateFromTemplate(UnitTemplate template)
        {
            _logger.Log("[UnitFactory] CreateFromTemplate " + template.Id, LogSeverity.Info, LogCategory.Initialization);

            var unit = new Unit
            {
                Id = Guid.NewGuid().ToString(),
                TemplateId = template.Id,
                UnitName = template.DisplayName,
                Level = 1,
                Experience = 0,
                Stats = new UnitStats(template.BaseStats),
                AttackRange = new RangeBand(template.MinRange, template.MaxRange)
            };

            // Growth profile is typically stored on Unit, or referenced via class/template.
            // For this test, you can store it on Unit if you want; or keep it external.

            return unit;
        }
    }
}