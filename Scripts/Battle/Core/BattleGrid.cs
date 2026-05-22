#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Terrain;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Core;

public partial class BattleGrid : Node2D
{
    /** Components */
    [ExportGroup("Tiles")]
    [Export] private TileMapLayer _terrainLayer = null!;
    [Export] private TileMapLayer _actionPreviewLayer = null!;
    [Export] private Godot.Collections.Array<TerrainType> _terrainResources = new();

    [Export] private PackedScene _map = null!;

    private readonly GobLogger _logger = GobLogManager.For<BattleGrid>();
    
    /** Fields */
    private const int BasicGroundTilesAtlasId = 0;
    private const int ActionOverlayTilesAtlasId = 1;
    
    private readonly Dictionary<string, TerrainType> _terrainById = new(StringComparer.Ordinal);
    private TerrainType? _defaultTerrain;

    // per-cell cache for querying TerrainType
    private readonly Dictionary<Vector2I, TerrainType?> _terrainAtCellCache = new();
    
    // Preview Data
    private IReadOnlySet<Vector2I>? _movementPreview;
    private (IReadOnlySet<Vector2I> Cells, int TileType)? _actionPreview;
    private IReadOnlySet<Vector2I>? _hoveredThreatPreview;
    // private HashSet<Vector2I> _interactCells = new();
    private static readonly HashSet<Vector2I> EmptyCells = new();

    /** Properties */
    public HashSet<Vector2I> EnemySpawnPoints = new();
    public Vector2I BossEnemySpawnPoint = new();
    public HashSet<Vector2I> FriendlySpawnPoints = new();
    
    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------
    
    public override void _Ready()
    {
        Debug.Assert(_terrainLayer != null, "[BattleGrid] Terrain Layer not initialized");
        Debug.Assert(_actionPreviewLayer != null, "[BattleGrid] Action Preview Layer not initialized");

        ClearOverlays();
        _loadMap();
        _loadTerrainDb();
        
        // Pick a default: either explicit ID (recommended) or first loaded
        _defaultTerrain = _terrainById.TryGetValue("default", out var t) ? t
            : (_terrainById.Count > 0 ? FirstTerrain() : null);
        
        GD.Print($"[BattleGrid] Terrain DB loaded count={_terrainById.Count}");
        _logger.Log("Ready", GobLogSeverity.Info, GobLogCategory.Initialization);
    }

    private void _loadMap()
    {
        var bossSpawnAtlasCoord = new Vector2I(3, 0);
        var enemySpawnAtlasCoord = new Vector2I(1, 0);
        var friendSpawnAtlasCoord = new Vector2I(2, 0);
        
        Node map = _map.Instantiate();
        var terrain = map.GetNode<TileMapLayer>("TerrainLayer");
        var spawnPointsLayer = map.GetNode<TileMapLayer>("SpawnPoints");
        var spawnPoints = spawnPointsLayer.GetUsedCells();
        foreach (var cell in spawnPoints)
        {
            var overlayType = (SpawnOverlayType)spawnPointsLayer.GetCellAtlasCoords(cell).X;
            switch (overlayType)
            {
                case SpawnOverlayType.Friend:
                    FriendlySpawnPoints.Add(cell);
                    break;
                case SpawnOverlayType.Enemy:
                    EnemySpawnPoints.Add(cell);
                    break;
                case SpawnOverlayType.Boss:
                    BossEnemySpawnPoint = cell;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(overlayType), overlayType, "Cannot resolve overlay type used.");
            }
        }
        
        _terrainLayer.TileMapData = terrain.TileMapData;
    }
    
    /// <summary>
    /// Loads all TerrainType resources in terrain folder
    /// </summary>
    /// <param name="folder"></param>
    private void _loadTerrainDb()
    {
        _terrainById.Clear();

        foreach (var res in _terrainResources)
        {
            if (res == null)
            {
                _logger.Warn("Null entry in _terrainResources");
                continue;
            }

            if (string.IsNullOrWhiteSpace(res.Id))
            {
                _logger.Warn("TerrainType missing Id");
                continue;
            }

            if (_terrainById.ContainsKey(res.Id))
            {
                _logger.Warn($"Duplicate TerrainType Id '{res.Id}'");
                continue;
            }

            _terrainById.Add(res.Id, res);
        }

        _logger.Log($"Loaded TerrainTypes: {_terrainById.Count}", 0, GobLogCategory.Initialization);
    }
    
    // ---------------------------------------------------------------------
    // Public Methods
    // ---------------------------------------------------------------------

    /// <summary>Gets TerrainType for a cell (uses per-cell cache).</summary>
    public bool CanFocusCell(Vector2I cell)
    {
        var hasTerrain = TryGetTerrainAtCell(cell, out var terrain);
        var canFocus = hasTerrain && terrain is { BlocksCursor: false };
        
        _logger.Log($"CanFocusCell cell={cell}, hasTerrain={hasTerrain}, terrain={terrain} :: {canFocus}", GobLogSeverity.Extra, GobLogCategory.UiNavigation);
        return canFocus;
    }

    public bool CanFocusGlobalPosition(Vector2 globalPos, out Vector2I cell)
    {
        cell = GetCellAtGlobalPosition(globalPos);
        return CanFocusCell(cell);
    }

    /// <summary>Clears preview data and overlay.</summary>
    public void ClearTurnPreviews()
    {
        _movementPreview = null;
        _actionPreview = null;
    }
    
    /// <summary>Clears preview overlay. Use if you change lots of tiles at once.</summary>
    public void ClearOverlays()
    {
        _actionPreviewLayer.Clear();
    }
    
    /// <summary>Clears terrain cache. Use if you change lots of tiles at once.</summary>
    public void ClearTerrainCache()
    {
        _terrainAtCellCache.Clear();
    }

    public Vector2 GetGlobalCenterPositionForCell(Vector2I cell)
    {
        var localPos = _terrainLayer.MapToLocal(cell);
        return _terrainLayer.ToGlobal(localPos);
    }

    public Vector2 GetGlobalTopLeftPositionForCell(Vector2I cell)
    {
        var localPos = _terrainLayer.MapToLocal(cell);
        return _terrainLayer.ToGlobal(localPos) - new Vector2I(GlobalSettings.TileSize / 2, GlobalSettings.TileSize / 2);
    }
    
    /// <summary>
    /// Returns cell coordinates for a given globalPos
    /// </summary>
    /// <param name="globalPos"></param>
    /// <returns></returns>
    public Vector2I GetCellAtGlobalPosition(Vector2 globalPos)
    {
        var localPos = _terrainLayer.ToLocal(globalPos);
        return _terrainLayer.LocalToMap(localPos);
    }

    /// <summary>
    /// Returns Bounding rect used by map layer.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public Rect2 GetMapRectGlobal()
    {
        Vector2 halfTile = _terrainLayer.TileSet.TileSize / 2;
        var rectInCells = _terrainLayer.GetUsedRect();
        Vector2I pos = rectInCells.Position;
        Vector2I size = rectInCells.Size;

        Vector2 topLeft = GetGlobalCenterPositionForCell(pos) - halfTile;
        Vector2 bottomRight = GetGlobalCenterPositionForCell(pos + size) - halfTile;
        
        
        
        return new Rect2(topLeft, bottomRight - topLeft);
    }
    
    /// <summary>
    /// Returns TerrainType data for a given Vector2I cell
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public TerrainType? GetTerrainAtCell(Vector2I cell)
    {
        if (_terrainAtCellCache.TryGetValue(cell, out var cached))
            return cached;

        var tileData = _terrainLayer.GetCellTileData(cell);
        if (tileData == null)
            return Cache(cell, null);

        // TileSet custom data key added to terrain in TileMapLayer
        var v = tileData.GetCustomData("terrain_id");
        if (!DebugUtil.Require(v.VariantType == Variant.Type.String, $"Tile at {cell} is missing a valid 'terrain_id' custom data."))
            return Cache(cell, _defaultTerrain);

        var id = v.AsString();
        if (!string.IsNullOrEmpty(id) && _terrainById.TryGetValue(id, out var terrain))
            return Cache(cell, terrain);

        // If the tile is painted but missing terrain_id, use default
        return Cache(cell, _defaultTerrain);
    }
    
    /// <summary>Call this if the tile at a cell changes, to refresh cached terrain.</summary>
    public void InvalidateTerrainCacheAt(Vector2I cell)
    {
        _terrainAtCellCache.Remove(cell);
    }
    
    public void SetMovementPreview(IReadOnlySet<Vector2I> preview)
    {
        ClearTurnPreviews();
        _movementPreview = preview;
        RedrawOverlay();
    }

    public void SetActionPreview(IReadOnlySet<Vector2I> preview, PrimaryActionType actionType)
    {
        ClearTurnPreviews();
        _actionPreview = (preview, (int)PrimaryActionTypeToOverlayType(actionType));
        RedrawOverlay();
    }
    
    public void SetAttackPreview(IReadOnlySet<Vector2I> preview)
    {
        ClearTurnPreviews();
        _actionPreview = (preview, (int)ActionOverlayType.Attack);
        RedrawOverlay();
    }

    public void SetHoveredThreatPreview(IReadOnlySet<Vector2I> preview)
    {
        ClearTurnPreviews();
        _hoveredThreatPreview = preview;
        RedrawOverlay();
    }

    public void SetUnitStartOfTurnPreviews(IReadOnlySet<Vector2I> movePreview, IReadOnlySet<Vector2I> attackPreview)
    {
        ClearTurnPreviews();
        _movementPreview = movePreview;
        _actionPreview = (attackPreview, (int)ActionOverlayType.Attack);
        RedrawOverlay();
    }

    /// <summary>
    /// Returns true if TerrainType terrain exists at given cell coordindates, out var terrain
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="terrain"></param>
    /// <returns></returns>
    public bool TryGetTerrainAtCell(Vector2I cell, out TerrainType? terrain)
    {
        var t = GetTerrainAtCell(cell);
        if (t == null)
        {
            terrain = null;
            return false;
        }

        terrain = t;
        return true;
    }
    
    // ---------------------------------------------------------------------
    // Private Methods
    // ---------------------------------------------------------------------
    
    /// <summary>Add TerrainType associated with cell to cache for quick access when making many queries</summary>
    /// <param name="cell">Vector2I position of terrain</param>
    /// <param name="terrain">TerrainType object containing data</param>
    /// <returns></returns>
    private TerrainType? Cache(Vector2I cell, TerrainType? terrain)
    {
        _terrainAtCellCache[cell] = terrain;
        return terrain;
    }

    /// <summary>
    /// Returns first TerrainType from file in directory
    /// </summary>
    /// <returns></returns>
    private TerrainType? FirstTerrain()
    {
        foreach (var kv in _terrainById) return kv.Value;
        return null;
    }

    private static Vector2I OverlayTypeToVector2I(ActionOverlayType t)
    {
        return new Vector2I((int)t, 0);
    }

    private static ActionOverlayType PrimaryActionTypeToOverlayType(PrimaryActionType actionType)
    {
        switch (actionType)
        {
            case PrimaryActionType.Attack:
                return ActionOverlayType.Attack;
            case PrimaryActionType.Ability:
                // TODO - depends on type.
                return ActionOverlayType.Attack;
            case PrimaryActionType.Item:
                return ActionOverlayType.Interact;
            case PrimaryActionType.Trade:
                return ActionOverlayType.Interact;
            case PrimaryActionType.Wait:
            case PrimaryActionType.None:
            default:
                return ActionOverlayType.Attack;
        }
    }

    private void RedrawOverlay()
    {
        ClearOverlays();
        // Attack cells
        // Movement cells
        // Interaction cells
        if (!DebugUtil.Require(_actionPreviewLayer != null, "Unable to draw overlay, _actionPreviewLayer not initialized")) 
            return;

        _actionPreviewLayer.Visible = true;
        
        var moveCells = _movementPreview ?? EmptyCells;
        var (attackCells, actionTileType) = _actionPreview ?? (EmptyCells, 1);
        var hoveredThreatCells = _hoveredThreatPreview ?? EmptyCells;
        
        // Movement takes priority - cell legal for move & attack show as movement
        var attackOnly = new HashSet<Vector2I>(attackCells);
        attackOnly.ExceptWith(moveCells);
        
        // Combined set of cells to draw
        var renderedCells = new HashSet<Vector2I>(moveCells);
        renderedCells.UnionWith(attackCells);
        renderedCells.UnionWith(hoveredThreatCells);
        // renderedCells.UnionWith(_interactCells); TODO
        
        ClearOverlays(); // TODO - clear selectively by passing renderedCells & ignoring everything else

        foreach (var cell in renderedCells)
        {
            ActionOverlayType? overlayType;
            // if (_interactCells.Contains(cell))
            //     overlayType = ActionOverlayType.Interact;
            /*else*/
            if (moveCells.Contains(cell))
                overlayType = ActionOverlayType.Movement;
            else if (attackOnly.Contains(cell))
                overlayType = (ActionOverlayType)actionTileType;
            else if (hoveredThreatCells.Contains(cell))
                overlayType = ActionOverlayType.EnemyThreat;
            else
            {
                _logger.Warn($"Redraw Overlay - Unknown type for cell={cell}");
                overlayType = null;
            } 
            
            if (overlayType.HasValue)
                _actionPreviewLayer.SetCell(cell, ActionOverlayTilesAtlasId, OverlayTypeToVector2I(overlayType.Value));
        }

        if (renderedCells.Count == 0)
            _actionPreviewLayer.Visible = false; // hide empty overlay
        
        _logger.Log($"RedrawOverlay cellCount={renderedCells.Count}", GobLogSeverity.Extra, GobLogCategory.BattleState);
    }
}

public enum GridOverlayType
{
    MoveRange,
    AttackRange,
    MoveAndAttackRange,
    EnemyAttackRange,
    AbilityRange
}

public enum ActionOverlayType
{
    Movement = 0,
    Attack = 1,
    Interact = 2,
    Warning = 3,
    EnemyThreat = 4
}

public enum SpawnOverlayType
{
    None = 0,
    Enemy = 1,
    Friend = 2,
    Neutral = 3,
    Boss = 4
}