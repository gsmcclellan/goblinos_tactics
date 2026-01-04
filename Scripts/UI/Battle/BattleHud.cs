using System.Collections.Generic;
using System.Diagnostics;
using Goblinos.Logging;
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
            DebugUtil.Require(_battleController != null, "[BattleHud] Not Initialized. BattleController reference is required.");
            DebugUtil.Require(_battleController != null, "[BattleHud] Not Initialized. _panelsRoot reference is required.");

            CachePanels();
            WirePanels();
            ConnectSignals();
            
            LogManager.Log("[BattleHud] Ready", LogSeverity.Info, LogCategory.Initialization);
        }

        public override void _ExitTree()
        {
            LogManager.Log("[BattleHud] _ExitTree", LogSeverity.Info, LogCategory.Exit);
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
            
            LogManager.Log($"[BattleHud] CachePanels count={_panels.Count}", LogSeverity.Info, LogCategory.Initialization);
        }

        private void WirePanels()
        {
            LogManager.Log("[BattleHud] WirePanels count=" + _panels.Count, LogSeverity.Info, LogCategory.Initialization);
        }

        private void ConnectSignals()
        {
            LogManager.Log("[BattleHud] ConnectSignals", LogSeverity.Info, LogCategory.Initialization);

            _battleController.Connect("GridCursorFocusChanged", new Callable(this, nameof(HandleCursorFocusChanged)));
            // _battleController.Connect("SelectedUnitChanged", new Callable(this, nameof(HandleSelectedUnitChanged))); TODO
        }

        private void DisconnectSignals()
        {
            LogManager.Log("[BattleHud] DisconnectSignals", LogSeverity.Info, LogCategory.Exit);

            if (_battleController == null)
                return;

            if (_battleController.IsConnected("GridCursorFocusChanged", new Callable(this, nameof(HandleCursorFocusChanged))))
                _battleController.Disconnect("GridCursorFocusChanged", new Callable(this, nameof(HandleCursorFocusChanged)));

            // if (_battleController.IsConnected("SelectedUnitChanged", new Callable(this, nameof(HandleSelectedUnitChanged))))
            //     _battleController.Disconnect("SelectedUnitChanged", new Callable(this, nameof(HandleSelectedUnitChanged)));
        }

        private void HandleCursorFocusChanged(GridCursorFocus focus)
        {
            LogManager.Log("[BattleHud] HandleCursorFocusChanged", LogSeverity.Trace, LogCategory.UiNavigation);

            foreach (IBattleHudPanel panel in _panels)
                panel.OnCursorFocusChanged(focus);
        }

        private void HandleSelectedUnitChanged(Scripts.Battle.BattleUnit? selectedUnit)
        {
            LogManager.Log("[BattleHud] SelectedUnitChanged", LogSeverity.Trace, LogCategory.UiNavigation);

            foreach (IBattleHudPanel panel in _panels)
                panel.OnSelectedUnitChanged(selectedUnit);
        }
    }
}
