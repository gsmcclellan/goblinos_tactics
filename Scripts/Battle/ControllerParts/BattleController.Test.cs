using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Test;
using Goblinos.Scripts.Units;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController
{
    /** Components */
    [ExportGroup("Test")] 
    [Export] private PackedScene _battleUnitScene;
    [Export] private NodePath _unitsRootPath;
    private Node _unitsRoot;

    private UnitFactory _unitFactory = new UnitFactory();
    private RandomNumberGenerator _random = new RandomNumberGenerator();

    
    
    public void _Ready_Test()
    {
        _unitsRoot = GetNode(_unitsRootPath);
        Debug.Assert(_battleUnitScene != null, $"[{nameof(BattleController)}.Test] Init failed - no Packed scene for {nameof(BattleUnit)}.");
        Debug.Assert(_unitsRootPath != null, $"[{nameof(BattleController)}.Test] Init failed - no Units Root Node.");
        
        SpawnTestUnits();
        _logger.Log("Ready_Test", LogSeverity.Info, LogCategory.Initialization);
    }
    
    private void SpawnTestUnits()
    {
        List<(string, int)> friendlyUnitTypes = new()
        {
            ("gob_stab", 1),
            ("gob_shield", 1),
            ("gob_sneak", 1),
            ("gob_snipe", 1)
        };
        List<(string, int)> enemyUnitTypes = new()
        {
            ("hum_spear", 1),
            ("hum_captain", 1),
            ("hum_crossbow", 1),
            ("hum_guard", 1)
        };

        var friends = friendlyUnitTypes.SelectMany(fut => CreateTestUnits(fut.Item1, fut.Item2)).ToList();
        var enemies = enemyUnitTypes.SelectMany(fut => CreateTestUnits(fut.Item1, fut.Item2)).ToList();

        for (var i = 0; i < friends.Count; i++)
        {
            friends[i].IsFriendly = true;
            Spawn(friends[i], new Vector2I(i, _random.RandiRange(0, 15)));
        }
        for (var i = 0; i < enemies.Count; i++)
        {
            Spawn(enemies[i], new Vector2I(8+i, _random.RandiRange(0, 15)));
        }
        
        _registerExistingBattleUnitNodes();
    }

    private IEnumerable<Unit> CreateTestUnits(string unitTemplateId, int numUnits = 1)
    {
        var units = new List<Unit>();
        for (var i = 0; i < numUnits; i++)
        {
            units.Add(CreateTestUnit(unitTemplateId));
        }

        return units;
    }

    private Unit CreateTestUnit(string unitTemplateId)
    {
        var templates = TestUnitTemplates.Dict;
        return _unitFactory.CreateFromTemplate(templates[unitTemplateId]);
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