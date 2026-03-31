
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
            Id = "gob_stab",
            DisplayName = "Goblin Stabber",
            ImageFileName = "GoblinStabber",
            BaseStats = new StatBlock(
                might: 10,
                agility: 6,
                vitality: 5,
                mind: 2,
                presence: 2,
                luck: 3,
                movement: 5,
                maxHitPoints: 20,
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
            Id = "gob_shield",
            DisplayName = "Goblin Blocker",
            ImageFileName = "GoblinShielder",
            BaseStats = new StatBlock(
                might: 8,
                agility: 4,
                vitality: 4,
                mind: 2,
                presence: 3,
                luck: 2,
                movement: 6,
                maxHitPoints: 18,
                defense: 6,
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
            Id = "gob_sneak",
            DisplayName = "Goblin Sneak",
            ImageFileName = "GoblinSneak",
            MaxRange = 2,
            BaseStats = new StatBlock(
                might: 10,
                agility: 10,
                vitality: 2,
                mind: 2,
                presence: 3,
                luck: 2,
                movement: 6,
                maxHitPoints: 18,
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
            Id = "gob_snipe",
            DisplayName = "Goblin Sniper",
            ImageFileName = "GoblinSniper",
            MinRange = 2,
            MaxRange = 2,
            BaseStats = new StatBlock(
                might: 12,
                agility: 12,
                vitality: 2,
                mind: 2,
                presence: 3,
                luck: 2,
                movement: 5,
                maxHitPoints: 12,
                defense: 2,
                resistance: 1),
            StatGrowthProfile = new StatGrowthProfile(
                might: 75,
                agility: 75,
                vitality: 30,
                mind: 20,
                presence: 25,
                luck: 30)
        },
        
        new()
        {
            Id = "hum_spear",
            DisplayName = "Spearman",
            ImageFileName = "HumanSpearman",
            BaseStats = new StatBlock(
                might: 10,
                agility: 7,
                vitality: 4,
                mind: 2,
                presence: 3,
                luck: 2,
                movement: 6,
                maxHitPoints: 20,
                defense: 4,
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
            Id = "hum_captain",
            DisplayName = "Captain",
            ImageFileName = "HumanCaptain",
            BaseStats = new StatBlock(
                might: 14,
                agility: 10,
                vitality: 8,
                mind: 4,
                presence: 6,
                luck: 4,
                movement: 4,
                maxHitPoints: 36,
                defense: 6,
                resistance: 4),
            StatGrowthProfile = new StatGrowthProfile(
                might: 80,
                agility: 40,
                vitality: 70,
                mind: 40,
                presence: 65,
                luck: 40)
        },
        new()
        {
            Id = "hum_crossbow",
            DisplayName = "Crossbowman",
            ImageFileName = "HumanCrossbowman",
            MinRange = 2,
            MaxRange = 2,
            BaseStats = new StatBlock(
                might: 10,
                agility: 10,
                vitality: 4,
                mind: 2,
                presence: 3,
                luck: 2,
                movement: 6,
                maxHitPoints: 18,
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
            Id = "hum_guard",
            DisplayName = "Guardsman",
            ImageFileName = "HumanGuardsman",
            BaseStats = new StatBlock(
                might: 6,
                agility: 7,
                vitality: 8,
                mind: 2,
                presence: 3,
                luck: 2,
                movement: 6,
                maxHitPoints: 24,
                defense: 7,
                resistance: 4),
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

