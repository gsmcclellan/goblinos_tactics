using System;
using System.Threading.Tasks;
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
    public PresentationLayer Layer => PresentationLayer.Ui;
    
    public LevelUpPresentable(UnitLeveledUpEvent leveledUpEvent)
    {
        _leveledUpEvent = leveledUpEvent;
    }
    
    public Task Present(Node parent)
    {
        _node = _scene.Instantiate<TextAreaDialog>();
        _node.Show(_leveledUpEvent.Unit.UnitName, _leveledUpEvent.ToString());
        
        parent.AddChild(_node);

        _tcs = new TaskCompletionSource();
        
        var closeButton = _node.GetNode<Button>("VBoxContainer/HBoxContainer/CloseButton");
        _node.Closed += () =>
            {
                _node.QueueFree();
                OnComplete?.Invoke();
                _tcs.SetResult();
            };

        return _tcs.Task;
    }

    public Task Skip(Node parent)
    {
        return Task.CompletedTask;
    }
}