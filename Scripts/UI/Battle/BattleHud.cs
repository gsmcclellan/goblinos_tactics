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
        /** Signals */
        [Signal]
        public delegate void PrimaryActionFocusedEventHandler(int action);
        [Signal]
        public delegate void PrimaryActionSelectedEventHandler(int action);
        
        /** Components */
        [Export] private NodePath _panelsRootPath;
        [Export] private NodePath _primaryActionSelectPath;
        [Export] private NodePath _primaryActionConfirmPath;

        private BattleController _battleController;
        private GridCursor _cursor;
        private Node _panelsRoot;
        private PrimaryActionConfirm _primaryActionConfirm;
        private PrimaryActionSelect _primaryActionSelect;
        private SelectionController _selectionController;
        
        private Logger _logger = LogManager.For<BattleHud>();
        
        /** Fields */
        private readonly List<IBattleHudPanel> _panels = new();

        /** Properties */
        public bool IsPrimaryActionSelectMenuActive => _primaryActionSelect.Visible;

        // ---------------------------------------------------------------------
        // Lifecycle / Setup Methods
        // ---------------------------------------------------------------------
        public void Bind(SelectionController selectionController, BattleController battleController, GridCursor cursor)
        {
            _logger.Log("Bind", LogSeverity.Info, LogCategory.Initialization);
            
            
            _battleController = battleController;
            _cursor = cursor;
            _selectionController = selectionController;
            
            if (!DebugUtil.Require(_battleController != null, "[BattleHud] requires BattleController binding") ||
                !DebugUtil.Require(_cursor != null, "[BattleHud] requires GridCursor binding") ||
                !DebugUtil.Require(_selectionController != null, "[BattleHud] requires SelectionController binding")
               )
                return;
            
            _SubscribeToEvents();
        }
        
        public override void _Ready()
        {
            _panelsRoot = GetNode(_panelsRootPath);
            _primaryActionConfirm = GetNode<PrimaryActionConfirm>(_primaryActionConfirmPath);
            _primaryActionSelect = GetNode<PrimaryActionSelect>(_primaryActionSelectPath);
            DebugUtil.Require(_panelsRoot != null, "[BattleHud] Not Initialized. _panelsRoot reference is required.");
            DebugUtil.Require(_primaryActionConfirm != null, "[BattleHud] Not Initialized. PrimaryActionSelect reference is required.");
            DebugUtil.Require(_primaryActionSelect != null, "[BattleHud] Not Initialized. PrimaryActionSelect reference is required.");
            
            HidePrimaryActionConfirm();
            HidePrimaryActionSelectMenu();
            
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

            _battleController.InputStateChanged += OnBattleControllerInputStateChanged;

            _cursor.GridCursorFocusChanged += OnHoveredCellChanged;
            
            _primaryActionSelect.ActionFocused += OnPrimaryActionFocused;
            _primaryActionSelect.ActionSelected += OnPrimaryActionSelected;
            
            _selectionController.HoveredTerrainChanged += OnHoveredTerrainChanged;
            _selectionController.SelectedUnitChanged += OnSelectedUnitChanged;

            
        }

        private void _UnsubscribeFromEvents()
        {
            _logger.Log("DisconnectSignals", LogSeverity.Info, LogCategory.Exit);
            
            _battleController.InputStateChanged -= OnBattleControllerInputStateChanged;
            
            _cursor.GridCursorFocusChanged -= OnHoveredCellChanged;
            
            _primaryActionSelect.ActionFocused -= OnPrimaryActionFocused;
            _primaryActionSelect.ActionSelected -= OnPrimaryActionSelected;

            _selectionController.HoveredTerrainChanged -= OnHoveredTerrainChanged;
            _selectionController.SelectedUnitChanged -= OnSelectedUnitChanged;
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
        // Public Methods
        // ---------------------------------------------------------------------

        public void HidePrimaryActionConfirm()
        {
            _primaryActionConfirm.Visible = false;
        }
        
        public void HidePrimaryActionSelectMenu()
        {
            _primaryActionSelect.Visible = false;
            _primaryActionSelect.ReleaseFocus();
        }

        public void ShowPrimaryActionConfirm()
        {
            _primaryActionConfirm.Visible = true;
            _primaryActionConfirm.Message = _battleController.UnitActivation?.PrimaryAction.ToString();
        }
        
        public void ShowPrimaryActionSelectMenu()
        {
            _primaryActionSelect.Visible = true;
            
            // Pick a deterministic "top" action (don’t rely on enum order)
            var firstAction = PrimaryActionType.Attack; // choose your default
            _primaryActionSelect.TryFocus(firstAction);
        }
        
        // ---------------------------------------------------------------------
        // Signal / Event Callbacks
        // ---------------------------------------------------------------------

        private void OnHoveredCellChanged(Vector2I newCell, Vector2I oldCell)
        {
            _logger.Log($"[{nameof(OnHoveredCellChanged)}] newCell={newCell}, oldCell={oldCell}", LogSeverity.Trace, LogCategory.UiNavigation);
            foreach (var panel in _panels)
                panel.OnHoveredCellChanged(newCell, oldCell);
        }
        
        private void OnHoveredTerrainChanged(TerrainType? terrain)
        {
            _logger.Log("OnHoveredTerrainChanged", LogSeverity.Trace, LogCategory.UiNavigation);
            foreach (var panel in _panels)
                panel.OnHoveredTerrainChanged(terrain);
        }

        private void OnSelectedUnitChanged(Node? selectedUnit)
        {
            _logger.Log("OnSelectedUnitChanged", LogSeverity.Trace, LogCategory.UiNavigation);
            if (selectedUnit != null && selectedUnit is not BattleUnit)
                throw new InvalidCastException("Unit is wrong type, expect BattleUnit");

            foreach (var panel in _panels)
                panel.OnSelectedUnitChanged(selectedUnit as BattleUnit);
        }

        private void OnBattleControllerInputStateChanged(int s)
        {
            var state = (BattleInputState) s;
            var node = GetNode<Label>("BattleControllerInputState");
            _logger.Log($"OnBattleControllerInputStateChanged - state={state.ToString()}", LogSeverity.Info, LogCategory.UiNavigation);
            
            node.Text = state.ToString();
        }

        private void OnPrimaryActionFocused(int action)
        {
            _logger.Log("OnPrimaryActionFocused", LogSeverity.Trace, LogCategory.Signal);
            EmitSignal(SignalName.PrimaryActionFocused, action);
        }

        private void OnPrimaryActionSelected(int action)
        {
            _logger.Log("OnPrimaryActionSelected", LogSeverity.Trace, LogCategory.Signal);
            EmitSignal(SignalName.PrimaryActionSelected, action);
        }
    }
}
