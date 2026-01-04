using System;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Core;

public partial class GlobalSettings : Node
{
    public const int TileSize = 32;

    public const string InputRouterPath = "/root/InputRouter";
    public const string BattlePath = "/root/Battle";
    public const string BattleControllerPath = "/root/Battle/Controllers/BattleController";

    public const string BattleLogScenePath = "res://Nodes/BattleLog.tscn";
    public const string BattleScenePath = "res://Nodes/Battle.tscn";
    public const string BattleResultsScreenScenePath = "res://Nodes/BattleResultsScreen.tscn";

    public const bool AllowInputModeSwitching = true; // TODO move to user settings
    public const InputMode DefaultInputMode = InputMode.Controller; // TODO move to user settings

    public static readonly Random Random = new Random();
}