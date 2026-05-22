using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Core;

public partial class GlobalSettings : Node
{
    public const int TileSize = 32;

    public const string GameRootScenePath = "res://Nodes/GameRoot.tscn";
    
    public const string BattleLogScenePath = "res://Nodes/BattleLog.tscn";
    public const string BattleScenePath = "res://Nodes/Battle/Battle.tscn";
    public const string BattleResultsScreenScenePath = "res://Nodes/BattleResultsScreen.tscn";
    public const string FloatingTextScenePath = "res://Nodes/UI/Battle/FloatingText.tscn";
    public const string ExperienceProgressDialogScenePath = "res://Nodes/UI/Combat/ExperienceProgress.tscn";
    public const string LevelUpResultsPanelScenePath = "res://Nodes/UI/Battle/LevelUpResults.tscn";
    
    public const string InputRouterPath = "/root/InputRouter";
    public const string BattlePath = "/root/GameRoot/Battle";
    public const string BattleControllerPath = "/root/Battle/Controllers/BattleController";
    
    public const string UnitImageDirPath = "res://Assets/Images/Units/";

    public const bool AllowInputModeSwitching = true; // TODO move to user settings
    public const InputDeviceMode DefaultInputMode = InputDeviceMode.MouseAndKeyboard; // TODO move to user settings

    public static readonly RandomNumberGenerator RNG = new RandomNumberGenerator();
    
    // Battle Related
    public const int MinimumCombatDamage = 0;

    public GlobalSettings()
    {
        RNG.Randomize();
    }
}