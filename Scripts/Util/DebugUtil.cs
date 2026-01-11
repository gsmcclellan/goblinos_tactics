using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Goblinos.Scripts.Util;

public static class DebugUtil
{
    public static bool Require(
        [DoesNotReturnIf(false)] bool condition,        
        string message)
    {
        Debug.Assert(condition, message);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        return condition;
    }
    
    public static bool Check(
        bool condition,        
        string message)
    {
        Debug.Assert(condition, message);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        return condition;
    }
}