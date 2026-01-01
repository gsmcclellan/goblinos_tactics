using Goblinos.Scripts.Core;
using Godot;
using Godot.Collections;

namespace Goblinos.Scripts.Util;

public class DebugUtil
{
    private static readonly bool LoggingEnabled = true;
    private static readonly int LoggingSeverity = 0;
    
    private static readonly Dictionary<DebugLogCategory, bool> LoggingEnabledByCategory =
        new()
        {
            { DebugLogCategory.None, true },
            { DebugLogCategory.Input, true },
            { DebugLogCategory.Initialization, true },
            { DebugLogCategory.UiNavigation, true }
        };
    
    public static void Log(string str, int severity = 0, DebugLogCategory category = DebugLogCategory.None)
    {
        if (LoggingEnabled == true && severity >= LoggingSeverity && LoggingEnabledByCategory[category])
            GD.Print(str);
    }
}

public enum DebugLogCategory
{
    None,
    Input,
    Initialization,
    UiNavigation
}