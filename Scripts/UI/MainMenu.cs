using Godot;
using System;
using Goblinos.Scripts.Core;

public partial class MainMenu : Control
{
    /** Signals */
    [Signal] 
    public delegate void StartBattleTriggeredEventHandler();
    
    [Export] private Button _startBattleButton;
    
    

    public override void _Ready()
    {
    }

    public override void _EnterTree()
    {
        base._EnterTree();
        _startBattleButton.Pressed += OnStartBattlePressed;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        _startBattleButton.Pressed -= OnStartBattlePressed;
    }

    public void OnStartBattlePressed()
    {
        EmitSignal(SignalName.StartBattleTriggered);
    }
}
