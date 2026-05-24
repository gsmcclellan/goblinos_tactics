using System.Collections.Generic;
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Test;
using Goblinos.Scripts.Units;
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

    // private List<(Vector2I Cell, string TemplateId)> enemiesToSpawn = [
    //     (new Vector2I(10, 10), "hum_spear"),
    //     (new Vector2I(11, 11), "hum_spear"),
    //     (new Vector2I(12, 12), "hum_spear"),
    //     (new Vector2I(13, 13), "hum_spear"),
    // ];
    
    
    public void _Ready_Test()
    {
        _unitsRoot = GetNode(_unitsRootPath);
        Debug.Assert(_battleUnitScene != null, $"[{nameof(BattleController)}.Test] Init failed - no Packed scene for {nameof(BattleUnit)}.");
        Debug.Assert(_unitsRootPath != null, $"[{nameof(BattleController)}.Test] Init failed - no Units Root Node.");
        
        SpawnTestUnits();
        // LevelUpUnits();
        _logger.Log("Ready_Test", GobLogSeverity.Info, GobLogCategory.Initialization);
    }
    
    private void SpawnTestUnits()
    {
        List<(Vector2I Cell, string TemplateId)> playerUnitsToSpawn = [
            (new Vector2I(3, 3), "gob_stab"),
            (new Vector2I(3, 5), "gob_stab"),
            (new Vector2I(3, 1), "gob_shield"),
            // (new Vector2I(3, 7), "gob_shield"),
            (new Vector2I(1, 1), "gob_hag"),
            (new Vector2I(1, 7), "gob_sneak"),
            // (new Vector2I(1, 3), "gob_snipe"),
            (new Vector2I(1, 5), "gob_snipe"),
        ];
        
        List<(Vector2I Cell, string TemplateId)> enemiesToSpawn = [
            (new Vector2I(10, 10), "hum_spear"),
            (new Vector2I(12, 12), "hum_spear"),
            (new Vector2I(10, 14), "hum_crossbow"),
            (new Vector2I(2, 12), "hum_spear"),
            (new Vector2I(5, 12), "hum_spear"),
            (new Vector2I(2, 16), "hum_crossbow"),
            (new Vector2I(5, 16), "hum_crossbow"),
            (new Vector2I(18, 4), "hum_crossbow"),
            (new Vector2I(16, 4), "hum_guard"),
            (new Vector2I(18, 2), "hum_spear"),
            (new Vector2I(18, 6), "hum_spear"),
            (new Vector2I(22, 15), "hum_guard"),
            (new Vector2I(22, 18), "hum_spear"),
            (new Vector2I(18, 15), "hum_guard"),
            (new Vector2I(18, 18), "hum_spear"),
            (new Vector2I(20, 17), "hum_crossbow"),
            (new Vector2I(26, 7), "hum_spear"),
            (new Vector2I(24, 5), "hum_guard"),
            (new Vector2I(26, 5), "hum_spear"),
            (new Vector2I(24, 7), "hum_guard"),
            (new Vector2I(21, 6), "hum_crossbow"),
            (new Vector2I(21, 10), "hum_crossbow"),
        ];

        var bossEnemyId = "hum_captain";


        var friendlySpawnCells = _grid.FriendlySpawnPoints.GetEnumerator();
        foreach (var unitSpawnInfo in playerUnitsToSpawn)
        {
            var playerUnit = CreateTestUnit(unitSpawnInfo.TemplateId);
            playerUnit.IsFriendly = true;
            if (friendlySpawnCells.MoveNext())
                Spawn(playerUnit, friendlySpawnCells.Current);
        }

        var enemySpawnCells = _grid.EnemySpawnPoints.GetEnumerator();
        foreach (var enemySpawnInfo in enemiesToSpawn)
        {
            var enemyUnit = CreateTestUnit(enemySpawnInfo.TemplateId);
            if (enemySpawnCells.MoveNext())
                Spawn(enemyUnit, enemySpawnCells.Current);
        }

        var bossEnemy = CreateTestUnit(bossEnemyId);
        if (_grid.BossEnemySpawnPoint != Vector2I.Zero)
            Spawn(bossEnemy, _grid.BossEnemySpawnPoint);
        
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
        _logger.Log("[BattleUnitSpawner] Spawn " + unit.UnitName, GobLogSeverity.Info, GobLogCategory.UnitLifecycle);
        
        var node = _battleUnitScene.Instantiate<BattleUnit>();
        _unitsRoot.AddChild(node);

        node.GlobalPosition = _grid.GetGlobalCenterPositionForCell(cell);
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
        
        _logger.Log($"_registerExistingBattleUnitNodes count={_unitRegistry.Units.Count}", GobLogSeverity.Info, GobLogCategory.Initialization);
    }

    private void LevelUpUnits()
    {
        var units = _unitRegistry.GetFriendlyUnits();

        foreach (var battleUnit in units)
        {
            _context.UnitProgression.LevelUp(battleUnit.Unit);
        }
    }
}