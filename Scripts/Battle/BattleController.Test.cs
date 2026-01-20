using System.Collections.Generic;
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Test;
using Goblinos.Scripts.Units;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController
{
    [ExportGroup("Test")] 
    [Export] private PackedScene _battleUnitScene;
    [Export] private NodePath _unitsRootPath;

    private Node _unitsRoot;
    
    public void _Ready_Test()
    {
        _unitsRoot = GetNode(_unitsRootPath);
        Debug.Assert(_battleUnitScene != null, $"[{nameof(BattleController)}.Test] Init failed - no Packed scene for {nameof(BattleUnit)}.");
        Debug.Assert(_unitsRootPath != null, $"[{nameof(BattleController)}.Test] Init failed - no Units Root Node.");
        
        CreateTestUnits();
        _logger.Log("Ready_Test", LogSeverity.Info, LogCategory.Initialization);
    }
    
    private void CreateTestUnits()
    {
        var battleUnitFactory = new BattleUnitFactory();
        var unitFactory = new UnitFactory();
        var templates = TestUnitTemplates.Dict;


        var gob1 = unitFactory.CreateFromTemplate(templates["gob_stab"]);
        var gob2 = unitFactory.CreateFromTemplate(templates["gob_stab"]);
        var gob3 = unitFactory.CreateFromTemplate(templates["gob_shield"]);
        var gob4 = unitFactory.CreateFromTemplate(templates["gob_shield"]);
        var friends = new List<Unit>() {gob1, gob2, gob3, gob4};
        
        var enemy1 = unitFactory.CreateFromTemplate(templates["hum_spear"]);
        var enemy2 = unitFactory.CreateFromTemplate(templates["hum_spear"]);
        var enemy3 = unitFactory.CreateFromTemplate(templates["hum_spear"]);
        var enemy4 = unitFactory.CreateFromTemplate(templates["hum_spear"]);
        var enemies = new List<Unit>() {enemy1, enemy2, enemy3, enemy4};

        for (var i = 0; i < 4; i++)
        {
            friends[i].IsFriendly = true;
            Spawn(friends[i], new Vector2I(i, 0));
            Spawn(enemies[i], new Vector2I(i, 4));
        }
        
        _registerExistingBattleUnitNodes();
    }
    
    /// <summary>
    /// Spawns a BattleUnitNode and binds the given Unit.
    /// </summary>
    private BattleUnit Spawn(Unit unit, Vector2I cell)
    {
        _logger.Log("[BattleUnitSpawner] Spawn " + unit.UnitName, LogSeverity.Info, LogCategory.UnitLifecycle);

        var node = _battleUnitScene.Instantiate<BattleUnit>();
        _unitsRoot.AddChild(node);

        node.GlobalPosition = _grid.GetGlobalPositionForCell(cell);
        node.Bind(unit);

        return node;
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