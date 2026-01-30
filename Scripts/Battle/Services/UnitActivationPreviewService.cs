using Goblinos.Scripts.Battle.Core;
using Goblinos.Scripts.Battle.Preview;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Battle.Units;
using Godot;

namespace Goblinos.Scripts.Battle.Services;

public class UnitActivationPreviewService(BattleGrid grid, UnitRegistry unitRegistry)
{
    private readonly BattleGrid _grid = grid;
    private readonly UnitRegistry _unitRegistry = unitRegistry;
    private readonly MoveRangeService _moveRangeService = new(grid, unitRegistry);
    private readonly PrimaryActionTargetingService _primaryActionTargetingService = new(grid, unitRegistry);

    public UnitActivationPreview BuildPreview(BattleUnit actingUnit, Vector2I originCell)
    {
        var movePreview = _moveRangeService.GetMovementPreview(originCell, actingUnit);

        var unitActivationPreview = new UnitActivationPreview()
        {
            MovementPreview = movePreview
        };
        
        foreach (var actionType in PrimaryActionInfo.PrimaryActionOrder)
        {
            if (!PrimaryActionInfo.RequiresTarget(actionType))
                continue;

            var actionPreview =
                _primaryActionTargetingService.BuildPrimaryActionPreview(originCell, movePreview.Cells, actingUnit,
                    actionType);
            
            unitActivationPreview.AddPrimaryActionPreview(actionPreview);
        }

        return unitActivationPreview;
    }
}