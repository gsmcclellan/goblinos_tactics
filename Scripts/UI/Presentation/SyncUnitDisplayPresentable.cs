using System;
using System.Threading.Tasks;
using Goblinos.Scripts.Battle.Units;
using Godot;
using ReallyGoodIdeas.Presentation;

namespace Goblinos.Scripts.UI.Presentation;

public class SyncUnitDisplayPresentable(BattleUnit unit) : IPresentable
{
    public event Action OnComplete;
    public PresentationLayer Layer => PresentationLayer.World;
    public async Task Present(Node parent)
    {
        await unit.SyncDisplayAnimated();
        OnComplete?.Invoke();
    }

    public Task Skip(Node parent)
    {
        unit.SyncDisplay();
        OnComplete?.Invoke();
        return Task.CompletedTask;
    }

    
}