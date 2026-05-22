using System.Reflection;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Core;
using Godot;

namespace Goblinos.Scripts.Battle.Controllers;

public partial class EnvironmentController: Node
{
    private readonly GobLogger _logger = GobLogManager.For<EnvironmentController>();

    private BattleGrid _grid;

    private readonly PackedScene _fireNodeScene = GD.Load<PackedScene>("res://Nodes/Battle/FireNode.tscn");
    
    public void Bind(BattleGrid grid)
    {
        _grid = grid;

        // Test();
    }

    public void Test()
    {
        SpawnFireNode(new Vector2I(5, 5));
    }

    private void SpawnFireNode(Vector2I cell)
    {
        var fire = _fireNodeScene.Instantiate<FireNode>();
        AddChild(fire);
        fire.GlobalPosition = _grid.GetGlobalCenterPositionForCell(cell);
    }
}