using Godot;
using System;
using Goblinos.Scripts.Battle;
#nullable enable
using System;
using Goblinos.Logging;
using Goblinos.Scripts.Units;
using Goblinos.Scripts.Units.Stats;
using Godot;

namespace Goblinos.Scripts.Test
{
    /// <summary>
    /// Quick in-engine test harness to validate UnitTemplate -> Unit -> BattleUnit wiring.
    /// </summary>
    public partial class TestBattleRunner : Node
    {
        private readonly Logger _logger = LogManager.For<TestBattleRunner>();

        public override void _Ready()
        {
            _logger.Log("Ready", LogSeverity.Info, LogCategory.Initialization);

            var unitFactory = new UnitFactory();
            var battleUnitFactory = new BattleUnitFactory();

            var templateA = CreateTemplate(
                id: "goblin_sword_001",
                name: "Sword Goblin",
                might: 7,
                agility: 6,
                vitality: 5,
                mind: 2,
                presence: 2,
                luck: 3,
                maxHitPoints: 20,
                defense: 4,
                resistance: 2,
                movement: 5);

            var templateB = CreateTemplate(
                id: "goblin_spear_001",
                name: "Spear Goblin",
                might: 6,
                agility: 7,
                vitality: 4,
                mind: 2,
                presence: 3,
                luck: 2,
                maxHitPoints: 18,
                defense: 3,
                resistance: 2,
                movement: 6);

            var unitA = unitFactory.CreateFromTemplate(templateA);
            var unitB = unitFactory.CreateFromTemplate(templateB);

            var battleA = battleUnitFactory.Create(unitA);
            var battleB = battleUnitFactory.Create(unitB);

            RunSimpleDuel(battleA, battleB);
        }

        /// <summary>
        /// Creates a UnitTemplate with a minimal StatBlock.
        /// </summary>
        private static UnitTemplate CreateTemplate(
            string id,
            string name,
            int might,
            int agility,
            int vitality,
            int mind,
            int presence,
            int luck,
            int maxHitPoints,
            int defense,
            int resistance,
            int movement)
        {
            var baseStats = new StatBlock(
                might: might,
                agility: agility,
                vitality: vitality,
                mind: mind,
                presence: presence,
                luck: luck,
                movement: movement,
                maxHitPoints: maxHitPoints,
                defense: defense,
                resistance: resistance);

            return new UnitTemplate
            {
                Id = id,
                DisplayName = name,
                BaseStats = baseStats,
                StatGrowthProfile = new StatGrowthProfile(40, 40, 40, 20, 20, 20)
            };
        }

        /// <summary>
        /// Runs a basic duel using placeholder combat logic.
        /// </summary>
        private void RunSimpleDuel(BattleUnit attacker, BattleUnit defender)
        {
            _logger.Log("RunSimpleDuel", LogSeverity.Info, LogCategory.UnitLifecycle);

            var random = new Random(12345);
            var round = 1;

            while (!attacker.IsDefeated && !defender.IsDefeated && round <= 25)
            {
                _logger.Log("Round " + round, LogSeverity.Info, LogCategory.UnitLifecycle);

                ResolveAttack(attacker, defender, random);
                if (defender.IsDefeated)
                    break;

                ResolveAttack(defender, attacker, random);
                round++;
            }

            var winner = attacker.IsDefeated ? defender.UnitName : attacker.UnitName;
            _logger.Log("Winner: " + winner, LogSeverity.Info, LogCategory.UnitLifecycle);
        }

        /// <summary>
        /// Placeholder attack resolution to validate wiring.
        /// </summary>
        private void ResolveAttack(BattleUnit attacker, BattleUnit defender, Random random)
        {
            _logger.Log("ResolveAttack " + attacker.UnitName + " -> " + defender.UnitName,
                LogSeverity.Info,
                LogCategory.UnitLifecycle);

            // For now, use simple hit chance derived from Agility vs Agility.
            var attackerAccuracy = 70 + attacker.Unit.Stats.BaseStats.Agility * 2;
            var defenderEvasion = defender.Unit.Stats.BaseStats.Agility * 2;
            var hitChance = Math.Clamp(attackerAccuracy - defenderEvasion, 5, 95);

            var roll = random.Next(1, 101);
            if (roll > hitChance)
            {
                _logger.Log("Miss (roll " + roll + " vs " + hitChance + ")",
                    LogSeverity.Info,
                    LogCategory.UnitLifecycle);
                return;
            }

            var attackPower = attacker.Unit.Stats.BaseStats.Might;
            var defense = defender.Unit.Stats.BaseStats.Defense;
            var damage = Math.Max(0, attackPower - defense);

            defender.ApplyDamage(damage);

            _logger.Log("Hit for " + damage + " (HP now " + defender.CurrentHitPoints + ")",
                LogSeverity.Info,
                LogCategory.UnitLifecycle);
        }
    }
}
