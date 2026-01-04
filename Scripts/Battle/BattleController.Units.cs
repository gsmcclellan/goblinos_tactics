using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController
{
    [ExportGroup("Units")]
    /** Components */
    [Export]
    private NodePath _battleUnitRegistryPath;

    /** Fields */
    
    
    /** Properties */
    public BattleUnitRegistry UnitRegistry;
    
    private void _Ready_Units()
    {
        UnitRegistry = GetNode<BattleUnitRegistry>(_battleUnitRegistryPath);
        Debug.Assert(UnitRegistry != null, "[BattleController.Units] unable to register UnitRegistry.");

        _registerExistingBattleUnitNodes();
        
        _logger.Log("Ready", LogSeverity.Info, LogCategory.Initialization);
    }

    /// <summary>
    /// Test function - registers Units in node path. Will be replaced with loading from UnitData.
    /// </summary>
    private void _registerExistingBattleUnitNodes()
    {
        var units = Battle.GetNode("Units").GetChildren();

        foreach (var unit in units)
        {
            if (unit is BattleUnit bUnit)
                UnitRegistry.RegisterUnit(bUnit, default);
        }
        
        _logger.Log($"_registerExistingBattleUnitNodes count={UnitRegistry.Units.Count}", LogSeverity.Info, LogCategory.Initialization);
    }
}