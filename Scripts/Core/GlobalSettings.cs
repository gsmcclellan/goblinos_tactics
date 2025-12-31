using System;
using Godot;

namespace Goblinos.Scripts.Core;

public partial class GlobalSettings : Node
{
    public const int TileSize = 32;

    public const string BattlePath = "/root/Battle";
    public const string BattleControllerPath = "/root/Battle/BattleController";

    public const string BattleLogScenePath = "res://Nodes/BattleLog.tscn";
    public const string BattleScenePath = "res://Nodes/Battle.tscn";
    public const string BattleResultsScreenScenePath = "res://Nodes/BattleResultsScreen.tscn";

    public static readonly Random Random = new Random();
}