#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle;
using Goblinos.Scripts.Battle.Terrain;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.UI.Battle;
using Godot;
using Goblinos.Scripts.Util;

namespace Goblinos.Scripts.UI.Battle
{
    public partial class BattleHud : CanvasLayer
    {
        [Export] private NodePath _panelsRootPath;

        private Node _panelsRoot;
        private SelectionController _selectionController;
        private BattleController _battleController;
        
        private Logger _logger = LogManager.For<BattleHud>();
        
        
        private readonly List<IBattleHudPanel> _panels = new();

        // ---------------------------------------------------------------------
        // Lifecycle / Setup Methods
        // ---------------------------------------------------------------------
        public void Bind(SelectionController selectionController, BattleController battleController)
        {
            _logger.Log("Bind", LogSeverity.Info, LogCategory.Initialization);
            
            _selectionController = selectionController;
            _battleController = battleController;
            
            if (!DebugUtil.Require(_selectionController != null, "[BattleHud] requires SelectionController binding") ||
                !DebugUtil.Require(_battleController != null, "[BattleHud] requires BattleController binding") )
                return;
            
            _SubscribeToEvents();
        }
        
        public override void _Ready()
        {
            _panelsRoot = GetNode(_panelsRootPath);
            DebugUtil.Require(_panelsRoot != null, "[BattleHud] Not Initialized. _panelsRoot reference is required.");

            CachePanels();
            WirePanels();
            
            _logger.Log("Ready", LogSeverity.Info, LogCategory.Initialization);
        }
        public override void _ExitTree()
        {
            _logger.Log("_ExitTree", LogSeverity.Info, LogCategory.Exit);
            _UnsubscribeFromEvents();
        }
        
        private void _SubscribeToEvents()
        {
            _logger.Log("ConnectSignals", LogSeverity.Info, LogCategory.Initialization);
            
            _selectionController.HoveredTerrainChanged += OnHoveredTerrainChanged;
            _selectionController.SelectedUnitChanged += OnSelectedUnitChanged;

            _battleController.InputStateChanged += OnBattleControllerInputStateChanged;
        }

        private void _UnsubscribeFromEvents()
        {
            _logger.Log("DisconnectSignals", LogSeverity.Info, LogCategory.Exit);

            _selectionController.HoveredTerrainChanged -= OnHoveredTerrainChanged;
            _selectionController.SelectedUnitChanged -= OnSelectedUnitChanged;
            
            _battleController.InputStateChanged -= OnBattleControllerInputStateChanged;
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
            
            _logger.Log($"CachePanels count={_panels.Count}", LogSeverity.Info, LogCategory.Initialization);
        }

        private void WirePanels()
        {
            _logger.Log("WirePanels count=" + _panels.Count, LogSeverity.Info, LogCategory.Initialization);
        }
        
        // ---------------------------------------------------------------------
        // Signal / Event Callbacks
        // ---------------------------------------------------------------------

        private void OnHoveredTerrainChanged(TerrainType? terrain)
        {
            _logger.Log("OnHoveredTerrainChanged", LogSeverity.Trace, LogCategory.UiNavigation);
            foreach (IBattleHudPanel panel in _panels)
                panel.OnHoveredTerrainChanged(terrain);
        }

        private void OnSelectedUnitChanged(Node? selectedUnit)
        {
            _logger.Log("OnSelectedUnitChanged", LogSeverity.Trace, LogCategory.UiNavigation);
            if (selectedUnit != null && selectedUnit is not BattleUnit)
                throw new InvalidCastException("Unit is wrong type, expect BattleUnit");

            foreach (IBattleHudPanel panel in _panels)
                panel.OnSelectedUnitChanged(selectedUnit as BattleUnit);
        }

        private void OnBattleControllerInputStateChanged(int s)
        {
            var state = (BattleInputState) s;
            _logger.Log("OnBattleControllerInputStateChanged", LogSeverity.Info, LogCategory.UiNavigation);
            var node = GetNode<Label>("BattleControllerInputState");
            node.Text = state.ToString();
        }
    }
}
