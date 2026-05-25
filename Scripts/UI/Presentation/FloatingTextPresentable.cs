using System;
using System.Threading.Tasks;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.UI.Battle;
using Goblinos.Scripts.UI.Combat;
using Godot;
using ReallyGoodIdeas.Presentation;

namespace Goblinos.Scripts.UI.Presentation;

public class FloatingTextPresentable(Vector2 globalPos, string message) : IPresentable
{
    private readonly PackedScene _floatingTextScene = GD.Load<PackedScene>(GlobalSettings.FloatingTextScenePath);

    public event Action OnComplete;
    public PresentationLayer Layer => PresentationLayer.World;

    public async Task Present(Node parent)
    {
        var floatingText = _floatingTextScene.Instantiate<FloatingText>();
        parent.AddChild(floatingText);
        floatingText.GlobalPosition = globalPos;

        await floatingText.Activate(globalPos, message);
        OnComplete?.Invoke();
    }

    public Task Skip(Node parent)
    {
        var floatingText = _floatingTextScene.Instantiate<FloatingText>();
        parent.AddChild(floatingText);
        floatingText.GlobalPosition = globalPos;
        OnComplete?.Invoke();
        return Task.CompletedTask;
    }
}