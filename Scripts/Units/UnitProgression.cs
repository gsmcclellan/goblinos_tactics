using System;
using Goblinos.Logging;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.Units.Stats;
using Goblinos.Scripts.Units.Stats.Types;
using Godot;

namespace Goblinos.Scripts.Units;

public class UnitProgression(RandomNumberGenerator rng)
{
    /** Events */
    public event Action<UnitLeveledUpEvent> UnitLeveledUp;
    
    /** Components */
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