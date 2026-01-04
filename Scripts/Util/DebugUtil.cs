using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

namespace Goblinos.Scripts.Util;

public class DebugUtil
{
    public static bool LoggingEnabled { get; set; } = true;
    public static DebugLogSeverity MinimumLoggingSeverity { get; set; } = DebugLogSeverity.Trace;
    public static bool ShouldRegisterNewCategories { get; set; } = false;
    public static bool ShouldEnableAutoRegisteredCategories { get; set; } = false;
    
    private static readonly Dictionary<string, bool> LoggingEnabledByCategoryKey =
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Core & Engine-Level
            { nameof(DebugLogCategory.None), true },
            { nameof(DebugLogCategory.Initialization), true },
            { nameof(DebugLogCategory.Exit), true },
            { nameof(DebugLogCategory.Error), true },
            { nameof(DebugLogCategory.Warning), true },
            { nameof(DebugLogCategory.Signal), true },

            // Input & Cursor
            { nameof(DebugLogCategory.Input), true },
            { nameof(DebugLogCategory.UiNavigation), true },

            // Battle & Gameplay Flow
            { nameof(DebugLogCategory.BattleState), true },
            { nameof(DebugLogCategory.CombatResolution), true },

            // Units & AI
            { nameof(DebugLogCategory.UnitLifecycle), true },
            { nameof(DebugLogCategory.UnitStats), false },
            { nameof(DebugLogCategory.AiDecision), true },
            { nameof(DebugLogCategory.AiMovement), false },

            // Data & Resources
            { nameof(DebugLogCategory.DataLoading), true },
            { nameof(DebugLogCategory.Serialization), true },
            { nameof(DebugLogCategory.Validation), true },

            // Performance / Diagnostics
            { nameof(DebugLogCategory.Performance), false },
            { nameof(DebugLogCategory.DebugOnly), false }
        };
    
    private static readonly Dictionary<Type, bool> LoggingEnabledByComponent =
        new();
    
    private static readonly HashSet<string> WarnedUnregisteredCategories =
        new(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>
    /// Creates a logger bound to a specific component class.
    /// </summary>
    public static DebugLogger For<T>()
    {
        return new DebugLogger(typeof(T), typeof(T).Name);
    }

    public static void Log(string str, DebugLogSeverity severity, string categoryKey)
    {
        LogInternal(typeof(DebugUtil), nameof(DebugUtil), str, severity, categoryKey);
    }
    
    public static void Log(string str, DebugLogSeverity severity = DebugLogSeverity.Trace, DebugLogCategory category = DebugLogCategory.None)
    {
        LogInternal(typeof(DebugUtil), nameof(DebugUtil), str, severity, category);
    }
    
    internal static void LogInternal(Type componentType, string componentName, string message, DebugLogSeverity severity, string categoryKey)
    {
        if (!LoggingEnabled || severity < MinimumLoggingSeverity)
            return;

        if (!IsCategoryEnabled(categoryKey))
            return;

        if (!IsComponentEnabled(componentType))
            return;

        var formatted = $"[{componentName}] {message}";

        switch (severity)
        {
            case >= DebugLogSeverity.Critical:
                GD.PushError($"[CRITICAL] {formatted}");
                break;
            case >= DebugLogSeverity.Error:
                GD.PushError(formatted);
                break;
            case >= DebugLogSeverity.Warning:
                GD.PushWarning(formatted);
                break;
            default:
                GD.Print(formatted);
                break;
        }
    }
    
    internal static void LogInternal(Type componentType, string componentName, string message, DebugLogSeverity severity, DebugLogCategory category)
    {
        LogInternal(componentType, componentName, message, severity, ToCategoryKey(category));
    }
    
    public static void ClearComponentFilter()
    {
        LoggingEnabledByComponent.Clear();
    }
    
    public static void DisableAllCategories()
    {
        SetAllCategories(false);
    }
    
    public static void EnableAllCategories()
    {
        SetAllCategories(true);
    }
    
    public static void EnableOnlyCategories(params string[] categories)
    {
        SetAllCategories(false);

        foreach (var categoryKey in categories)
        {
            if (string.IsNullOrWhiteSpace(categoryKey))
                continue;

            SetCategoryEnabled(categoryKey, true);
        }
    }
    
    
    public static void EnableOnlyCategories(params DebugLogCategory[] categories)
    {
        SetAllCategories(false);

        foreach (var category in categories)
            SetCategoryEnabled(category, true);
    }
    
    public static void EnableOnlyComponents(params Type[] components)
    {
        LoggingEnabledByComponent.Clear();

        foreach (var component in components)
            LoggingEnabledByComponent[component] = true;
    }

    public static bool IsCategoryEnabled(string categoryKey)
    {
        bool exists = LoggingEnabledByCategoryKey.TryGetValue(categoryKey, out bool enabled);

        if (!exists && !ShouldRegisterNewCategories)
        {
            if (WarnedUnregisteredCategories.Add(categoryKey))
                GD.PushWarning($"[DebugUtil] Unregistered category [{categoryKey}]. Log will be hidden.");

            return false;
        }

        if (!exists && ShouldRegisterNewCategories)
        {
            if (MinimumLoggingSeverity <= DebugLogSeverity.Trace)
                GD.Print($"[DebugUtil] Unregistered category [{categoryKey}]. Auto registering.");
            RegisterCategory(categoryKey, enabledByDefault: ShouldEnableAutoRegisteredCategories);
            return ShouldEnableAutoRegisteredCategories;
        }
            
        return enabled;
    }
    
    public static bool IsCategoryEnabled(DebugLogCategory category)
    {
        return IsCategoryEnabled(ToCategoryKey(category));
    }

    public static bool IsComponentEnabled(Type componentType)
    {
        if (LoggingEnabledByComponent.Count == 0)
            return true;

        return LoggingEnabledByComponent.TryGetValue(componentType, out bool enabled) && enabled;
    }

    public static void RegisterCategory(string categoryKey, bool enabledByDefault = true)
    {
        if (string.IsNullOrWhiteSpace(categoryKey))
            throw new ArgumentException("Category key must be non-empty.", nameof(categoryKey));

        LoggingEnabledByCategoryKey.TryAdd(categoryKey, enabledByDefault);
    }

    public static void RegisterCategory(DebugLogCategory category, bool enabledByDefault = true)
    {
        RegisterCategory(ToCategoryKey(category), enabledByDefault);
    }

    public static void SetCategoryEnabled(string categoryKey, bool enabled)
    {
        bool exists = LoggingEnabledByCategoryKey.ContainsKey(categoryKey);
        
        // Debug.Assert(exists, $"Unregistered category [{categoryKey}]");
        
        if (!exists && !ShouldRegisterNewCategories)
        {
            if (WarnedUnregisteredCategories.Add(categoryKey))
                GD.PushWarning($"[DebugUtil] Unregistered category [{categoryKey}]. Will not be enabled.");

            return;
        }

        if (!exists && ShouldRegisterNewCategories)
        {
            if (MinimumLoggingSeverity <= DebugLogSeverity.Trace)
                GD.Print($"[DebugUtil] Unregistered category [{categoryKey}]. Auto registering.");
            RegisterCategory(categoryKey, enabledByDefault: ShouldEnableAutoRegisteredCategories);
            exists = true;
        }

        LoggingEnabledByCategoryKey[categoryKey] = enabled;
    }
    
    public static void SetCategoryEnabled(DebugLogCategory category, bool enabled)
    {
        SetCategoryEnabled(ToCategoryKey(category), enabled);
    }
    
    public static void SetComponentEnabled<T>(bool enabled)
    {
        LoggingEnabledByComponent[typeof(T)] = enabled;
    }

    public static void SetEnabled(bool en)
    {
        LoggingEnabled = en;
    }

    public static void SetMinimumSeverity(DebugLogSeverity sev)
    {
        MinimumLoggingSeverity= sev;
    }

    private static void SetAllCategories(bool enabled)
    {
        var keys = new List<string>(LoggingEnabledByCategoryKey.Keys);

        foreach (var key in keys)
            LoggingEnabledByCategoryKey[key] = enabled;
    }

    private static string ToCategoryKey(DebugLogCategory category)
    {
        return category.ToString();
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