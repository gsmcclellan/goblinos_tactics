using System;
using System.Threading.Tasks;
using Goblinos.Scripts.UI.Combat;
using Godot;

namespace Goblinos.Scripts.UI.Presentation;

public class ExperiencePresentable : IPresentable
{
    public event Action OnComplete;
    
    private readonly string _unitName;
    private readonly int _from;
    private readonly int _to;
    private readonly PackedScene _scene;
    
    public PresentationLayer Layer => PresentationLayer.Ui;

    public ExperiencePresentable(PackedScene scene, string unitName, int from, int to)
    {
        _scene = scene;
        _unitName = unitName;
        _from = from;
        _to = to;
    }

    public async Task Present(Node parent)
    {
        var dialog = _scene.Instantiate<ExperienceProgress>();
        parent.AddChild(dialog);
        await parent.ToSignal(parent.GetTree(), SceneTree.SignalName.ProcessFrame); // let node settle
        await dialog.Show(_unitName, _from, _to);
        OnComplete?.Invoke();
    }

    public Task Skip(Node parent)
    {
        // Just apply the end state instantly, no animation
        var dialog = _scene.Instantiate<ExperienceProgress>();
        parent.AddChild(dialog);
        dialog.ShowInstant(_unitName, _from, _to);
        return Task.CompletedTask;
    }
}



