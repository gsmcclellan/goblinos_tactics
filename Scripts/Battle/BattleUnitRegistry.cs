using System.Collections.Generic;
using Goblinos.Scripts.Units;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

/// <summary>
/// Authoritative registry of all units participating in a single battle.
/// Responsible for unit participation, spatial occupancy, and battle-scoped
/// unit lifecycle events. Commits final results of movement events.
/// </summary>
public partial class BattleUnitRegistry: Node
{
    /** Signals */
    [Signal]
    public delegate void UnitRegisteredEventHandler(BattleUnit unit);
    [Signal]
    public delegate void UnitUnregisteredEventHandler(BattleUnit unit);
    [Signal]
    public delegate void UnitMoveResolvedEventHandler(BattleUnit unit, Vector2I fromCell, Vector2I toCell);
    [Signal]
    public delegate void UnitDiedEventHandler(BattleUnit unit);
    
    /** Fields */

    private readonly List<BattleUnit> _units = new();
    private readonly Dictionary<Vector2I, BattleUnit> _unitsByCell = new();
    private readonly Dictionary<BattleUnit, Vector2I> _cellsByUnit = new();

    /* Properties */

    /// <summary>
    /// Read-only list of all units currently participating in the battle.
    /// </summary>
    public IReadOnlyList<BattleUnit> Units => _units;

    public override void _Ready()
    {
        _SetupSubscriptions();
    }

    public override void _ExitTree()
    {
        _RemoveSubscriptions();
    }

    private void _SetupSubscriptions()
    {
        // Listen to unit death, update registry
    }

    private void _RemoveSubscriptions()
    {
    }

    /** Registration / Lifecycle */

    /// <summary>
    /// Registers a unit as participating in the battle.
    /// <param name="unit"></param>
    /// <param name="initialCell"></param>
    /// </summary>
    public void RegisterUnit(BattleUnit unit, Vector2I initialCell)
    {
        DebugUtil.Log("[BattleUnitRegistry] RegisterUnit " + unit, DebugLogSeverity.Trace, DebugLogCategory.UnitLifecycle);
    }

    /// <summary>
    /// Unregisters a unit from the battle.
    /// </summary>
    public void UnregisterUnit(BattleUnit unit)
    {
        DebugUtil.Log("[BattleUnitRegistry] UnregisterUnit " + unit, DebugLogSeverity.Trace, DebugLogCategory.UnitLifecycle);
    }

    /// <summary>
    /// Marks a unit as dead for the purposes of this battle.
    /// </summary>
    public void NotifyUnitDied(BattleUnit unit)
    {
        DebugUtil.Log("[BattleUnitRegistry] NotifyUnitDied " + unit, DebugLogSeverity.Trace, DebugLogCategory.UnitLifecycle);
    }

    /// <summary>
    /// Clears all registered units. Intended for battle teardown.
    /// </summary>
    public void Clear()
    {
        DebugUtil.Log("[BattleUnitRegistry] Clear", DebugLogSeverity.Info, DebugLogCategory.Exit);
    }
    
    /* Filtering / Queries */

    /// <summary>
    /// Determines if the registry has a given unit
    /// </summary>
    /// <param name="unit"></param>
    /// <returns>true if unit is registered</returns>
    public bool Contains(BattleUnit unit)
    {
        return false;
    }

    /// <summary>
    /// Enumerates units belonging to the given team/faction.
    /// </summary>
    public IEnumerable<BattleUnit> GetUnitsByTeam(UnitTeam team)
    {
        DebugUtil.Log("[BattleUnitRegistry] GetUnitsByTeam " + team, DebugLogSeverity.Extra, DebugLogCategory.DebugOnly);

        yield break;
    }

    /// <summary>
    /// Enumerates all units that are still alive.
    /// </summary>
    public IEnumerable<BattleUnit> GetLivingUnits()
    {
        DebugUtil.Log("[BattleUnitRegistry] GetLivingUnits", DebugLogSeverity.Extra, DebugLogCategory.DebugOnly);

        yield break;
    }

    /// <summary>
    /// Determines if a cell is occupied by a unit
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public bool IsCellOccupied(Vector2I cell)
    {
        var isOccupied = false;
        DebugUtil.Log($"[BattleUnitRegistry] IsCellOccupied [cell]={cell} :: {isOccupied}", DebugLogSeverity.Info, DebugLogCategory.UnitLifecycle);
        
        return isOccupied;
    }

    /// <summary>
    /// Determines if a unit has a cell associated with it, out property is the cell
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="cell">Vector2I of the unit</param>
    /// <returns>true if unit has a cell associated with it</returns>
    public bool TryGetCell(BattleUnit unit, out Vector2I cell)
    {
        var hasCell = false;
        cell = default;
        
        DebugUtil.Log($"[BattleUnitRegistry] TryGetCell [unit]={unit} [hasCell]={hasCell} :: [cell]={cell}", DebugLogSeverity.Info, DebugLogCategory.UnitLifecycle);
        return false;
    }

    /// <summary>
    /// Determines if cell contains a unit, out property is the unit
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="unit"></param>
    /// <returns>true if cell is occupied</returns>
    public bool TryGetUnitAtCell(Vector2I cell, out BattleUnit unit)
    {
        var hasUnit = false;
        unit = null;
        
        DebugUtil.Log($"[BattleUnitRegistry] TryGetUnitAtCell [cell]={cell} [hasUnit]={hasUnit} :: [unit]={unit}", DebugLogSeverity.Info, DebugLogCategory.UnitLifecycle);
        return false;
    }
    
    /** Movement */

    /// <summary>
    /// Updates the registry when a unit moves between cells.
    /// </summary>
    public void ApplyUnitMove(BattleUnit unit, Vector2I fromCell, Vector2I toCell)
    {
        DebugUtil.Log($"[BattleUnitRegistry] ApplyUnitMove [unit]={unit} [from]={fromCell} [to]={toCell}", DebugLogSeverity.Info, DebugLogCategory.UnitLifecycle);
    }
}