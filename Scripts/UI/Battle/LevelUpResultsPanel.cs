using Godot;
using System;
using Goblinos.Logging;
using Goblinos.Scripts.Units;

public partial class LevelUpResultsPanel : Panel
{
    private readonly GobLogger _logger = GobLogManager.For<LevelUpResultsPanel>();

    [Export] private Node _labelsRoot;
    
    private UnitLeveledUpEvent _levelUpResults;
    
    public override void _Ready()
    {
        _logger.Log($"{nameof(_Ready)}", GobLogSeverity.Trace, GobLogCategory.Initialization);
    }

    public void Bind(UnitLeveledUpEvent leveledUpEvent)
    {
        _levelUpResults = leveledUpEvent;
    }
}
