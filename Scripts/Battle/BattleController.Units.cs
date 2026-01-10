using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController
{
    // [ExportGroup("Units")]
    /** Components */

    /** Fields */
    
    
    /** Properties */
    
    private void _Ready_Units()
    {
        Debug.Assert(_unitRegistry != null, "[BattleController.Units] Not Initialized. Unable to register UnitRegistry.");

        _registerExistingBattleUnitNodes();
        
        _logger.Log("Ready", LogSeverity.Info, LogCategory.Initialization);
    }

    /// <summary>
    /// Test function - registers Units in node path. Will be replaced with loading from UnitData.
    /// </summary>
    private void _registerExistingBattleUnitNodes()
    {
        var units = _battle.GetNode("Units").GetChildren();

        foreach (var unit in units)
        {
            if (unit is BattleUnit bUnit)
                _unitRegistry.RegisterUnit(bUnit, _grid.GetCellAtGlobalPosition(bUnit.GlobalPosition));
        }
        
        _logger.Log($"_registerExistingBattleUnitNodes count={_unitRegistry.Units.Count}", LogSeverity.Info, LogCategory.Initialization);
    }
}