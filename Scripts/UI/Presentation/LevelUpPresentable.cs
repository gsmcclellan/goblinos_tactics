using System;
using System.Threading.Tasks;
using Godot;

namespace Goblinos.Scripts.UI.Presentation;

// public class LevelUpPresentable : IPresentable
// {
//     public event Action OnComplete;
//     private readonly string _unitName;
//     private readonly int _from;
//     private readonly int _to;
//     private readonly PackedScene _scene;
//     public async Task Present(Node parent)
//     {
//         var panel = _scene.Instantiate<LevelUpPanel>();
//         parent.AddChild(panel);
//         await panel.ShowAndWaitForDismissal(_levelUpEvent);
//         OnComplete?.Invoke();
//     }
// }