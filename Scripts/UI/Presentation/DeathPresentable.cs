using System;
using System.Threading.Tasks;
using Goblinos.Scripts.Battle.Units;
using Godot;
using ReallyGoodIdeas.Presentation;

namespace Goblinos.Scripts.UI.Presentation;

public class DeathPresentable : IPresentable
{
    public event Action OnComplete;
    public PresentationLayer Layer => PresentationLayer.World;
    private readonly BattleUnit _unit;

    public async Task Present(Node parent)
    {
        // await _unit.PlayDeathAnimation();
        
        _unit.QueueFree(); // safe now, animation is done
        OnComplete?.Invoke();
    }

    public Task Skip(Node parent)
    {
        
        _unit.QueueFree(); // no animation, just remove
        OnComplete?.Invoke();
        return Task.CompletedTask;
    }
}