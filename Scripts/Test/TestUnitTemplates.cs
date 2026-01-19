
#nullable enable
using System.Collections.Generic;
using System.Linq;
using Goblinos.Scripts.Units;
using Goblinos.Scripts.Units.Stats;

namespace Goblinos.Scripts.Test;

public static class TestUnitTemplates
{
    public static IReadOnlyList<UnitTemplate> All { get; } = new List<UnitTemplate>
    {
        new()
        {
            Id = "gob_sword",
            DisplayName = "Sword Goblin",
            BaseStats = new StatBlock(
                might: 7,
                agility: 6,
                vitality: 5,
                mind: 2,
                presence: 2,
                luck: 3,
                movement: 5,
                maxHealth: 20,
                defense: 4,
                resistance: 2),
            StatGrowthProfile = new StatGrowthProfile(
                might: 45,
                agility: 55,
                vitality: 35,
                mind: 15,
                presence: 20,
                luck: 25)
        },

        new()
        {
            Id = "gob_spear",
            DisplayName = "Spear Goblin",
            BaseStats = new StatBlock(
                might: 6,
                agility: 7,
                vitality: 4,
                mind: 2,
                presence: 3,
                luck: 2,
                movement: 6,
                maxHealth: 18,
                defense: 3,
                resistance: 2),
            StatGrowthProfile = new StatGrowthProfile(
                might: 40,
                agility: 60,
                vitality: 30,
                mind: 20,
                presence: 25,
                luck: 20)
        },
        
        new()
        {
            Id = "hum_spear",
            DisplayName = "Spearman",
            BaseStats = new StatBlock(
                might: 6,
                agility: 7,
                vitality: 4,
                mind: 2,
                presence: 3,
                luck: 2,
                movement: 6,
                maxHealth: 18,
                defense: 3,
                resistance: 2),
            StatGrowthProfile = new StatGrowthProfile(
                might: 40,
                agility: 60,
                vitality: 30,
                mind: 20,
                presence: 25,
                luck: 20)
        },
    };

    public static IReadOnlyDictionary<string, UnitTemplate> Dict = All.ToDictionary(t => t.Id);
}

