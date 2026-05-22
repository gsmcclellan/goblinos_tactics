using System;
using System.Threading.Tasks;
using Godot;

namespace Goblinos.Scripts.UI.Presentation;

// public class CombatAnimationPresentable : IPresentable
// {
//     public event Action OnComplete;
//     private readonly PackedScene _scene;
//
//     public async Task Present(Node parent)
//     {
//         await _animationController.PlayCombatAnimation(_combatResult);
//         OnComplete?.Invoke();
//     }

// // Use this if animation needs a specific parent, then this element should own that reference rather than the queue
// public async Task Present(Node _) // ignores the queue-provided parent
// {
//     await _unit.PlayCombatAnimation(...);
// }
// }