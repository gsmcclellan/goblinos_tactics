using System.Diagnostics;

namespace Goblinos.Scripts.Util;

public class DebugUtil
{
    public static bool Require(bool condition, string message)
    {
        Debug.Assert(condition, message);
        return condition;
    }
}