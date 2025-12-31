using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController : Node
{
    /** Signals */
    
    /** Actions */
    public event Action BattleStart;
    
    /** Components */
    public Battle Battle;
    
    /** Properties */
    
    public override void _Ready()
    {
        CallDeferred(nameof(_DeferredInit));
    }

    public override void _ExitTree()
    {
        _RemoveSubscriptions();
        base._ExitTree();
    }

    private void _DeferredInit()
    {
        _InitializeBattleComponents();
        _SetupSubscriptions();
        
        HandleStartOfBattle();
    }

    private void _InitializeBattleComponents()
    {
    }

    private void _SetupSubscriptions()
    {
    }

    private void _RemoveSubscriptions()
    {
    }
    private void DoEnemyTurn(bool isFirstTurn = false)
    {
    }
    public void HandleStartOfBattle()
    {
        
    }

    public void HandleEndOfBattle(bool isVictory)
    {
        
    }
}