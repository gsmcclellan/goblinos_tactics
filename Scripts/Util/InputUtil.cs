using System;
using Goblinos.Scripts.Core;
using Godot;

namespace Goblinos.Scripts.Util;

public class InputUtil
{
    private static readonly Vector2I[] DirectionVectors =
    [
        Vector2I.Up,
        Vector2I.Right,
        Vector2I.Down,
        Vector2I.Left,
        Vector2I.Zero
    ];
    
    public static int TileSize = GlobalSettings.TileSize;
    
    public static Vector2I InputDirectionToVector2I(InputDirection dir)
    {
        return DirectionVectors[(int)dir];
    }
}

public enum InputDirection
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3,
    None = 4
}

public enum InputDeviceMode
{
    AutoDetect,
    Controller,
    MouseAndKeyboard
    
}

public enum BattleInputState
{
    FreeSelect,
    MoveTargeting,
    AttackTargeting
}