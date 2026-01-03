using System.Diagnostics;
using Goblinos.Scripts.Core;
using Godot;
using Godot.Collections;
using CollectionExtensions = System.Collections.Generic.CollectionExtensions;

namespace Goblinos.Scripts.Util;

public class DebugUtil
{
    private static bool LoggingEnabled = true;
    private static DebugLogSeverity LoggingSeverity = DebugLogSeverity.Trace;
    
    private static readonly Dictionary<DebugLogCategory, bool> LoggingEnabledByCategory =
        new()
        {
            // Core & Engine-Level
            { DebugLogCategory.None, true },
            { DebugLogCategory.Initialization, true },
            { DebugLogCategory.Exit, true },
            { DebugLogCategory.Error, true },
            { DebugLogCategory.Warning, true },
            { DebugLogCategory.Signal, true },

            // Input & Cursor
            { DebugLogCategory.Input, false },          // inputs - Very noisy
            { DebugLogCategory.UiNavigation, false },   // Enable when debugging menus

            // Battle & Gameplay Flow
            { DebugLogCategory.BattleState, true },     // High-value logs
            { DebugLogCategory.CombatResolution, true },

            // Units & AI
            { DebugLogCategory.UnitLifecycle, true },
            { DebugLogCategory.UnitStats, false },      // Spammy during combat
            { DebugLogCategory.AiDecision, true },
            { DebugLogCategory.AiMovement, false },     // Extremely noisy

            // Data & Resources
            { DebugLogCategory.DataLoading, true },
            { DebugLogCategory.Serialization, true },
            { DebugLogCategory.Validation, true },

            // Performance / Diagnostics
            { DebugLogCategory.Performance, false },    // Enable temporarily
            { DebugLogCategory.DebugOnly, false }
        };
    
    public static void Log(string str, DebugLogSeverity severity = DebugLogSeverity.Trace, DebugLogCategory category = DebugLogCategory.None)
    {
        var categoryEnabled = IsCategoryEnabled(category);
        if (!LoggingEnabled || severity < LoggingSeverity || !categoryEnabled)
            return;
        
        switch (severity)
        {
            case >= DebugLogSeverity.Critical:
                GD.PushError($"[CRITICAL] {str}");
                break;
            case >= DebugLogSeverity.Error:
                GD.PushError(str);
                break;
            case >= DebugLogSeverity.Warning:
                GD.PushWarning(str);
                break;
            default:
                GD.Print(str);
                break;
        }
    }
    
    public static void EnableOnly(params DebugLogCategory[] categories)
    {
        SetAllCategories(false);

        foreach (var category in categories)
            SetCategory(category, true);
    }
    
    public static void EnableAll()
    {
        SetAllCategories(true);
    }
    
    public static void DisableAll()
    {
        SetAllCategories(false);
    }
    
    public static bool IsCategoryEnabled(DebugLogCategory category)
    {
        var exists = LoggingEnabledByCategory.TryGetValue(category, out bool enabled);
        Debug.Assert(exists, $"Unable to log, invalid [category]=[{category}]");
        return enabled;
    }
    
    private static void SetAllCategories(bool enabled)
    {
        var keys = new Array<DebugLogCategory>(LoggingEnabledByCategory.Keys);

        foreach (var key in keys)
            LoggingEnabledByCategory[key] = enabled;
    }
    
    public static void SetCategory(DebugLogCategory category, bool enabled)
    {
        bool exists = LoggingEnabledByCategory.ContainsKey(category);
        
        Debug.Assert(exists, $"Unknown DebugLogCategory [{category}]");

        if (!exists)
            return;

        LoggingEnabledByCategory[category] = enabled;
    }

    public static void SetEnabled(bool en)
    {
        LoggingEnabled = en;
    }

    public static void SetSeverity(DebugLogSeverity sev)
    {
        LoggingSeverity = sev;
    }
}

public enum DebugLogCategory
{
    // Core & Engine-Level
    None,               // Default / uncategorized
    Initialization,     // Node setup, _Ready, dependency wiring
    Exit,               // Shutdown, cleanup, scene exit
    Error,              // Non-fatal errors, recoverable failures
    Warning,            // Suspicious but allowed states
    Signal,              // Godot Signals
    
    // Input & Cursor
    Input,              // Raw input events, actions
    UiNavigation,       // Menu focus, UI selection, UI cursor
    
    // Battle & Gameplay flow
    BattleState,        // State machine transitions, turn start/end, phase changes
    CombatResolution,  //  Attacks, abilities, items, damage, hit/miss, crits, status effects
    
    // Units & AI
    UnitLifecycle,      // Spawn, death, removal
    UnitStats,          // HP, buffs, debuffs, stat changes
    AiDecision,         // AI evaluation & choice
    AiMovement,         // Pathing decisions, movement intent
    
    // Data & Resources
    DataLoading,        // Loading resources, JSON, configs
    Serialization,     // Save/load
    Validation,        // Data sanity checks
    
    // Performance / Diagnostics
    Performance,       // Timing, frame-sensitive diagnostics
    DebugOnly           // Temporary or experimental logs
}

public enum DebugLogSeverity
{
    Extra = -1,    // Extremely spammy logs that will dominate the console
    Trace = 0,     // Minor info
    Info = 1,      // Basic info level
    Warning = 2,   // Potential issues, non-gamebreaking
    Error = 3,     // Major issues that cause unintended side effects
    Critical = 4   // Severe game-breaking bugs
}