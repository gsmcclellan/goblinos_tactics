#nullable enable
using System;
using System.Threading.Tasks;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.UI.Combat;
using Goblinos.Scripts.Units;
using Godot;
using ReallyGoodIdeas.Presentation;

namespace Goblinos.Scripts.UI.Presentation;

public class LevelUpPresentable : IPresentable
{
    public event Action OnComplete;
    private readonly UnitLeveledUpEvent _leveledUpEvent;
    private readonly PackedScene _scene = GD.Load<PackedScene>(GlobalSettings.TextAreaDialogScenePath);
    private TextAreaDialog _node;
    private TaskCompletionSource _tcs;
    private readonly BattleUnit? _battleUnit;
    public PresentationLayer Layer => PresentationLayer.Ui;
    
    public LevelUpPresentable(UnitLeveledUpEvent leveledUpEvent, BattleUnit? battleUnit)
    {
        _leveledUpEvent = leveledUpEvent;
        _battleUnit = battleUnit;
    }
    
    public Task Present(Node parent)
    {
        _node = _scene.Instantiate<TextAreaDialog>();
        _node.Show(_leveledUpEvent.Unit.UnitName, _leveledUpEvent.ToString());
        
        parent.AddChild(_node);

        _tcs = new TaskCompletionSource();
        
        _node.Closed += () =>
        {
                _battleUnit?.SyncDisplay();
                _node.QueueFree();
                OnComplete?.Invoke();
                _tcs.SetResult();
            };

        return _tcs.Task;
    }

    public Task Skip(Node parent)
    {
        OnComplete?.Invoke();
        return Task.CompletedTask;
    }
}