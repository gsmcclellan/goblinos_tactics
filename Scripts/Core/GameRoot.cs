using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Core;
using Goblinos.Scripts.Units;
using Godot;
using ReallyGoodIdeas.Presentation;

namespace Goblinos.Scripts.Core;

public partial class GameRoot : Node
{
    /** Components */
    private readonly GobLogger _logger = GobLogManager.For<GameRoot>();

    [Export] private CanvasLayer _canvasLayer = null!;
    [Export] private MainMenu _mainMenu = null!;
    
    private RandomNumberGenerator _combatRng;
    private RandomNumberGenerator _progressionRng;
    
    private PresentationQueue _presentationQueue;
    private UnitProgression _unitProgression;
    
    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------
    public override void _Ready()
    {
        _combatRng = new RandomNumberGenerator();
        _progressionRng = new RandomNumberGenerator();

        _presentationQueue = new PresentationQueue();
        _unitProgression = new UnitProgression(_progressionRng);
        
        Debug.Assert(_canvasLayer != null, $"{nameof(GameRoot)}, {nameof(CanvasLayer)} not bound.");
        Debug.Assert(_mainMenu != null, $"{nameof(GameRoot)}, {nameof(MainMenu)} not bound.");

        DisplayMainMenu();
    }

    public override void _EnterTree()
    {
        _SubscribeToEvents();
    }

    public override void _ExitTree()
    {
        _UnsubscribeFromEvents();
    }

    private void _SubscribeToEvents()
    {
        _mainMenu.StartBattleTriggered += OnStartBattleTriggered;
    }
    
    // ---------------------------------------------------------------------
    // Event Handlers
    // ---------------------------------------------------------------------

    private void OnStartBattleTriggered()
    {
        StartBattle();
    }
    
    // ---------------------------------------------------------------------
    // Private Methods
    // ---------------------------------------------------------------------

    private void _UnsubscribeFromEvents()
    {
        _mainMenu.StartBattleTriggered -= OnStartBattleTriggered;
    }

    private void DisplayMainMenu()
    {
        _mainMenu.Visible = true;
    }
    
    private void HideMainMenu()
    {
        _mainMenu.Visible = false;
    }
    
    private void StartBattle()
    {
        var battleContext = new BattleContext(_combatRng, _unitProgression, _presentationQueue);

        var battleNodeScene = GD.Load<PackedScene>(GlobalSettings.BattleScenePath);
        var battle = battleNodeScene.Instantiate<BattleNode>();

        _presentationQueue.Bind(_canvasLayer, battle);

        HideMainMenu();
        
        AddChild(battle);
        battle.Bind(battleContext);
    }
}