using System;
using System.Collections.Generic;
using Goblinos.Logging;
using Goblinos.Scripts.Units.Stats;
using Goblinos.Scripts.Units.Stats.Types;
using Godot;

namespace Goblinos.Scripts.Units;

public class UnitProgression(RandomNumberGenerator rng)
{
    /** Events */
    public event Action<UnitLeveledUpEvent> UnitLeveledUp;
    public event Action<ExperienceGainedEvent> ExperienceGained;

    /** Components */
    private PackedScene _experienceProgressDialogPackedScene;
    
    private readonly GobLogger _logger = GobLogManager.For<UnitProgression>();
    private readonly RandomNumberGenerator _rng = rng;
    
    public UnitLeveledUpEvent LevelUp(Unit unit)
    {
        _logger.Log($"{nameof(LevelUp)} unit={unit.UnitName} level={unit.Level}", GobLogSeverity.Info, GobLogCategory.UnitStats);
        unit.Level += 1;
        
        var originalStats = unit.Stats.Copy();
        // Level up stats according to template
        var growthProfile = unit.Template.StatGrowthProfile;
        LevelUpStats(unit.Stats, growthProfile);
        
        var e = new UnitLeveledUpEvent
        {
            Unit = unit,
            OldLevel = unit.Level - 1,
            NewLevel = unit.Level,
            StatsBefore = originalStats.BaseStats,
            StatsAfter = unit.Stats.BaseStats.Copy(),
        };
        
        _logger.Log(e.ToString(), GobLogSeverity.Info, GobLogCategory.UnitStats);
        UnitLeveledUp?.Invoke(e);
        return e;
    }

    public ExperienceGainedEvent AddExperience(Unit unit, int expToAdd)
    {
        var expBefore = unit.Experience;
        var levelUps = new List<UnitLeveledUpEvent>();
        
        unit.Experience += expToAdd;

        while (unit.Experience >= 100)
        {
            unit.Experience -= 100;
            levelUps.Add(LevelUp(unit));
        }
        
        var e = new ExperienceGainedEvent
        {
            Unit = unit,
            ExpBefore = expBefore,
            ExpGained = expToAdd,
            LevelUps = levelUps,
            ExpAfter = unit.Experience,
        };
        
        _logger.Log($"{nameof(AddExperience)} unit={unit.UnitName} {e}", GobLogSeverity.Info, GobLogCategory.UnitStats);
        ExperienceGained?.Invoke(e);
        return e;
    }
    
    public void LevelUpStats(UnitStats stats, StatGrowthProfile growthProfile)
    {
        _logger.Log($"[{nameof(LevelUpStats)}] stats={stats} growth={growthProfile}", GobLogSeverity.Info, GobLogCategory.UnitStats);
        var addedStats = new StatBlock();
        foreach (StatName statName in StatNameInfo.CoreStats)
        {
            var levelUpAmount = 0;
            var growthPercent = growthProfile.Get(statName);

            while (growthPercent > 0)
            {

                var randRes = _rng.RandiRange(1, 100);
                if (randRes < growthPercent)
                    levelUpAmount += 1;
                growthPercent -= 100;
            }

            addedStats.Add(statName, levelUpAmount);
        }
        
        _logger.Log($"{nameof(LevelUpStats)} - Added Stats: \n{addedStats}", GobLogSeverity.Info, GobLogCategory.UnitStats);
        stats.Add(addedStats);
    }
}

public class ExperienceGainedEvent
{
    public Unit Unit;
    public int ExpBefore;       // e.g. 80
    public int ExpGained;       // e.g. 40
    public List<UnitLeveledUpEvent> LevelUps; // 0, 1, or more
    public int ExpAfter;        // e.g. 20 (after level-up resets to 0)
}

public class UnitLeveledUpEvent
{
    public Unit Unit;
    public int OldLevel;
    public int NewLevel;
    public StatBlock StatsBefore;
    public StatBlock StatsAfter;

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"{Unit.UnitName} {OldLevel} -> {NewLevel}\n");
        sb.Append("  Stats:\n");
        foreach (var statName in StatNameInfo.CoreStats)
        {
            var statBefore = StatsBefore.Get(statName);
            var statAfter = StatsAfter.Get(statName);
            sb.Append($"    {statName}: {statBefore}{(statAfter != statBefore ? " -> " + statAfter : "")}\n");
        }
            
        return sb.ToString();
    }
}