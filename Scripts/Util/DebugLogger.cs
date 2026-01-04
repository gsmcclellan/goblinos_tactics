using System;

namespace Goblinos.Scripts.Util;

/// <summary>
/// Lightweight per-class logger that binds a component identity once,
/// so call sites do not need to pass a component parameter.
/// </summary>
public sealed class DebugLogger
{
    private readonly Type _componentType;
    private readonly string _componentName;

    internal DebugLogger(Type componentType, string componentName)
    {
        _componentName = componentName;
        _componentType = componentType;
    }
    
    public void Log(string message, DebugLogSeverity severity, string categoryKey)
    {
        DebugUtil.LogInternal(_componentType, _componentName, message, severity, categoryKey);
    }
    
    public void Log(string message, DebugLogSeverity severity, DebugLogCategory category)
    {
        DebugUtil.LogInternal(_componentType, _componentName, message, severity, category);
    }
    
    public void Extra(string message, string categoryKey)
    {
        Log(message, DebugLogSeverity.Extra, categoryKey);
    }
    
    public void Extra(string message, DebugLogCategory category = DebugLogCategory.None)
    {
        Log(message, DebugLogSeverity.Extra, category);
    }
    
    public void Trace(string message, string categoryKey)
    {
        Log(message, DebugLogSeverity.Trace, categoryKey);
    }

    public void Trace(string message, DebugLogCategory category = DebugLogCategory.None)
    {
        Log(message, DebugLogSeverity.Trace, category);
    }
    
    public void Info(string message, string categoryKey)
    {
        Log(message, DebugLogSeverity.Info, categoryKey);
    }

    public void Info(string message, DebugLogCategory category = DebugLogCategory.None)
    {
        Log(message, DebugLogSeverity.Info, category);
    }
    
    public void Warning(string message, string categoryKey)
    {
        Log(message, DebugLogSeverity.Warning, categoryKey);
    }

    public void Warning(string message, DebugLogCategory category = DebugLogCategory.None)
    {
        Log(message, DebugLogSeverity.Warning, category);
    }
    
    public void Error(string message, string categoryKey)
    {
        Log(message, DebugLogSeverity.Error, categoryKey);
    }

    public void Error(string message, DebugLogCategory category = DebugLogCategory.None)
    {
        Log(message, DebugLogSeverity.Error, category);
    }
    
    public void Critical(string message, string categoryKey)
    {
        Log(message, DebugLogSeverity.Critical, categoryKey);
    }

    public void Critical(string message, DebugLogCategory category = DebugLogCategory.None)
    {
        Log(message, DebugLogSeverity.Critical, category);
    }
}