using System.Collections.Generic;
using System.Diagnostics;
using Goblinos.Scripts.Battle;
using Goblinos.Scripts.UI.Battle;
using Godot;
using Goblinos.Scripts.Util;

namespace Goblinos.Scripts.UI.Battle
{
    public partial class BattleHud : CanvasLayer
    {
        [Export] private NodePath _battleControllerPath;
        [Export] private NodePath _panelsRootPath;

        private BattleController _battleController;
        private Node _panelsRoot;
        private readonly List<IBattleHudPanel> _panels = new();

        public override void _Ready()
        {
            _battleController = GetNode<BattleController>(_battleControllerPath);
            _panelsRoot = GetNode(_panelsRootPath);
            Debug.Assert(_battleController != null, "BattleController reference is required.");
            Debug.Assert(_battleController != null, "_panelsRoot reference is required.");

            CachePanels();
            WirePanels();
            ConnectSignals();
            
            DebugUtil.Log("[BattleHud] Ready", DebugLogSeverity.Info, DebugLogCategory.Initialization);
        }

        public override void _ExitTree()
        {
            DebugUtil.Log("[BattleHud] _ExitTree", DebugLogSeverity.Info, DebugLogCategory.Exit);
            DisconnectSignals();
        }

        private void CachePanels()
        {
            _panels.Clear();
            _panelsRoot ??= GetNode(_panelsRootPath);
            
            foreach (Node child in _panelsRoot.GetChildren())
            {
                if (child is IBattleHudPanel panel)
                    _panels.Add(panel);
            }
            
            DebugUtil.Log($"[BattleHud] CachePanels count={_panels.Count}", DebugLogSeverity.Info, DebugLogCategory.Initialization);
        }

        private void WirePanels()
        {
            DebugUtil.Log("[BattleHud] WirePanels count=" + _panels.Count, DebugLogSeverity.Info, DebugLogCategory.Initialization);
        }

        private void ConnectSignals()
        {
            DebugUtil.Log("[BattleHud] ConnectSignals", DebugLogSeverity.Info, DebugLogCategory.Initialization);

            _battleController.Connect("GridCursorFocusChanged", new Callable(this, nameof(HandleCursorFocusChanged)));
            // _battleController.Connect("SelectedUnitChanged", new Callable(this, nameof(HandleSelectedUnitChanged))); TODO
        }

        private void DisconnectSignals()
        {
            DebugUtil.Log("[BattleHud] DisconnectSignals", DebugLogSeverity.Info, DebugLogCategory.Exit);

            if (_battleController == null)
                return;

            if (_battleController.IsConnected("GridCursorFocusChanged", new Callable(this, nameof(HandleCursorFocusChanged))))
                _battleController.Disconnect("GridCursorFocusChanged", new Callable(this, nameof(HandleCursorFocusChanged)));

            if (_battleController.IsConnected("SelectedUnitChanged", new Callable(this, nameof(HandleSelectedUnitChanged))))
                _battleController.Disconnect("SelectedUnitChanged", new Callable(this, nameof(HandleSelectedUnitChanged)));
        }

        private void HandleCursorFocusChanged(GridCursorFocus focus)
        {
            DebugUtil.Log("[BattleHud] HandleCursorFocusChanged", DebugLogSeverity.Trace, DebugLogCategory.UiNavigation);

            foreach (IBattleHudPanel panel in _panels)
                panel.OnCursorFocusChanged(focus);
        }

        private void HandleSelectedUnitChanged(Scripts.Battle.BattleUnit? selectedUnit)
        {
            DebugUtil.Log("[BattleHud] SelectedUnitChanged", DebugLogSeverity.Trace, DebugLogCategory.UiNavigation);

            foreach (IBattleHudPanel panel in _panels)
                panel.OnSelectedUnitChanged(selectedUnit);
        }
    }
}
