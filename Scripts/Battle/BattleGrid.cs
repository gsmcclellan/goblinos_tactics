using Godot;
using System;
using System.Diagnostics;
using Goblinos.Scripts.Util;

public partial class BattleGrid : Node2D
{
    public TileMapLayer Tiles;

    public override void _Ready()
    {
        Tiles = GetNode<TileMapLayer>("Tiles");
        DebugUtil.Log("[BattleGrid] Ready", 1, DebugLogCategory.Initialization);
    }
    public Vector2I GetCellAtGlobalPosition(Vector2 globalPos)
    {
        return Tiles.LocalToMap(Tiles.ToLocal(globalPos));
    }
}
