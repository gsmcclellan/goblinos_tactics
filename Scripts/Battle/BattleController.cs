using Goblinos.Scripts.Core;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController : Node, IInputHandler
{
    /** Signals */
    
    /** Actions */
    
    /** Components */
    public Battle Battle;
    public BattleGrid Grid;
    
    private GridCursor _cursor;
    private BattleUnit _selectedUnit;
    // private GridTile

    /** Properties */
    
    public override void _Ready()
    {
        CallDeferred(nameof(_DeferredInit));
    }

    private void _DeferredInit()
    {
        _InitializeBattleComponents();
        _SetupSubscriptions();
        
        _Ready_Input();
        _Ready_Actions();
        
        HandleStartOfBattle();
        
        DebugUtil.Log("[BattleController] Ready", DebugLogSeverity.Info, DebugLogCategory.Initialization);
    }

    public override void _ExitTree()
    {
        _RemoveSubscriptions();
        _ExitTree_Actions();
        base._ExitTree();
    }

    private void _InitializeBattleComponents()
    {
        Battle = GetParent<Battle>();
        _cursor = Battle.Cursor;
    }

    private void _SetupSubscriptions()
    {
    }

    private void _RemoveSubscriptions()
    {
    }

    public override void _Process(double delta)
    {
        _Process_Input(delta);
    }
    private void DoEnemyTurn(bool isFirstTurn = false)
    {
    }

    
    public void HandleStartOfBattle()
    {
        
    }

    public void HandleEndOfBattle(bool isVictory)
    {
        // Show results screen
        // remove self from input router
        _inputRouter.Pop(this);
    }
    
    public void MoveCursor(Vector2I dir)
    {
        DebugUtil.Log("[BattleController] MoveCursor", 0, DebugLogCategory.UiNavigation);
        _cursor.Move(dir);
    }

    public void MoveCursorTo(Vector2 globalPos)
    {
        DebugUtil.Log($"[BattleController] MoveCursorTo [globalPos]={globalPos}", 0, DebugLogCategory.UiNavigation);
        _cursor.MoveTo(globalPos);
    }

    public bool TryMoveCursor(InputDirection dir)
    {
        DebugUtil.Log("[BattleController] TryMoveCursor", 0, DebugLogCategory.UiNavigation);

        // TODO - check if able to move. Avoid going off map etc.
        // if can't move return false
        MoveCursor(InputUtil.InputDirectionToVector2I(dir));

        return true;
    }

    public bool TryMoveCursorTo(Vector2 globalPos)
    {
        DebugUtil.Log($"[BattleController] TryMoveCursorTo [globalPos]={globalPos}", 0, DebugLogCategory.UiNavigation);

        // TODO - check if able to move. Avoid going off map etc.
        // if can't move return false
        MoveCursorTo(globalPos);

        return true;
    }
    
    
}