#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle;
using Goblinos.Scripts.Battle.Preview;
using Goblinos.Scripts.Battle.Terrain;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.UI.Battle;
using Godot;
using Goblinos.Scripts.Util;
using BattleUnit = Goblinos.Scripts.Battle.Units.BattleUnit;
using SelectionController = Goblinos.Scripts.Battle.Controllers.SelectionController;
using TurnController = Goblinos.Scripts.Battle.Controllers.TurnController;

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
        [Export] private NodePath _combatPreviewPath;
        [Export] private NodePath _primaryActionSelectPath;
        [Export] private NodePath _primaryActionConfirmPath;
        
        [Export] private Label _turnNumberLabel;
        [Export] private Button _endTurnButton;

        private BattleController _battleController;
        private Scripts.Battle.Core.GridCursor _cursor;
        private Node _panelsRoot;
        private PrimaryActionConfirm _primaryActionConfirm;
        private PrimaryActionSelect _primaryActionSelect;
        private SelectionController _selectionController;
        private TurnController _turnController;
        
        private Logger _logger = LogManager.For<BattleHud>();
        
        /** Fields */
        private readonly List<IBattleHudPanel> _panels = new();

        /** Properties */
        public bool IsPrimaryActionSelectMenuActive => _primaryActionSelect.Visible;

        // ---------------------------------------------------------------------
        // Lifecycle / Setup Methods
        // ---------------------------------------------------------------------
        public void Bind(BattleController battleController, Scripts.Battle.Core.GridCursor cursor, SelectionController selectionController, TurnController turnController)
        {
            _logger.Log("Bind", LogSeverity.Info, LogCategory.Initialization);
            
            _battleController = battleController;
            _cursor = cursor;
            _selectionController = selectionController;
            _turnController = turnController;
            
            if (!DebugUtil.Require(_battleController != null, "[BattleHud] requires BattleController binding") ||
                !DebugUtil.Require(_cursor != null, "[BattleHud] requires GridCursor binding") ||
                !DebugUtil.Require(_selectionController != null, "[BattleHud] requires SelectionController binding") ||
                !DebugUtil.Require(_turnController != null, "[BattleHud] requires TurnController binding")
               )
                return;
            
            Debug.Assert(_turnNumberLabel != null, "[BattleHud] Missing Turn Number Label");
            Debug.Assert(_endTurnButton != null, "[BattleHud] Missing End Turn Button");
            
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
            _logger.Log("SubscribeToEvents", LogSeverity.Info, LogCategory.Initialization);
            if (_primaryActionSelect == null)
                throw new InvalidOperationException("[BattleHud] Bind called before _Ready. _primaryActionSelect is not initialized.");

            _battleController.InputStateChanged += OnBattleControllerInputStateChanged;
            _battleController.CombatPreviewUpdated += OnCombatPreviewUpdated;
            
            _cursor.GridCursorFocusChanged += OnHoveredCellChanged;

            _endTurnButton.Pressed += OnEndTurnButtonPressed;
            
            _primaryActionSelect.ActionFocused += OnPrimaryActionFocused;
            _primaryActionSelect.ActionSelected += OnPrimaryActionSelected;
            
            _selectionController.HoveredTerrainChanged += OnHoveredTerrainChanged;
            _selectionController.HoveredUnitChanged += OnHoveredUnitChanged;
            _selectionController.SelectedUnitChanged += OnSelectedUnitChanged;

            _turnController.TurnStarted += OnTurnStarted;
        }

        private void _UnsubscribeFromEvents()
        {
            _logger.Log("UnsubscribeFromEvents", LogSeverity.Info, LogCategory.Exit);
            
            _battleController.InputStateChanged -= OnBattleControllerInputStateChanged;
            _battleController.CombatPreviewUpdated -= OnCombatPreviewUpdated;
            
            _cursor.GridCursorFocusChanged -= OnHoveredCellChanged;
            
            _endTurnButton.Pressed -= OnEndTurnButtonPressed;
            
            _primaryActionSelect.ActionFocused -= OnPrimaryActionFocused;
            _primaryActionSelect.ActionSelected -= OnPrimaryActionSelected;

            _selectionController.HoveredTerrainChanged -= OnHoveredTerrainChanged;
            _selectionController.HoveredUnitChanged -= OnHoveredUnitChanged;
            _selectionController.SelectedUnitChanged -= OnSelectedUnitChanged;
            
            _turnController.TurnStarted -= OnTurnStarted;
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
        
        /// <summary>
        /// Shows the primary action menu, disables actions with no valid targets, and focuses the first enabled action.
        /// </summary>
        public void ShowPrimaryActionSelectMenu(PrimaryActionValidTargetsPreview? previews)
        {
            _primaryActionSelect.Visible = true;
            
            // Disable actions that cannot currently target anything.
            foreach (var actionType in PrimaryActionInfo.PrimaryActionOrder)
            {
                if (actionType == PrimaryActionType.None)
                    continue;

                var requiresTarget = PrimaryActionInfo.RequiresTarget(actionType);
                var hasTargets = previews != null && previews.HasTargets(actionType);

                // For now, allow Wait even if it has no targets (it is always valid).
                if (actionType == PrimaryActionType.Wait)
                    hasTargets = true;

                _primaryActionSelect.SetActionEnabled(actionType, !requiresTarget || hasTargets);
            }
            
            // Pick a deterministic "top" action (don’t rely on enum order)
            if (!_primaryActionSelect.TryFocusFirstEnabled(PrimaryActionInfo.PrimaryActionOrder))
                _primaryActionSelect.ReleaseFocus();
        }
        
        // ---------------------------------------------------------------------
        // Signal / Event Callbacks
        // ---------------------------------------------------------------------

        private void OnBattleControllerInputStateChanged(int s)
        {
            var state = (BattleInputState) s;
            _logger.Log($"OnBattleControllerInputStateChanged - state={state.ToString()}", LogSeverity.Info, LogCategory.UiNavigation);
            
            var node = GetNode<Label>("BattleControllerInputState"); // TODO - make this a IBattleHudPanel (or add to terrain info)
            node.Text = state.ToString();
            
            _panels.ForEach(panel => panel.OnBattleInputStateChanged(s));
        }

        private void OnCombatPreviewUpdated(CombatPreview? combatPreview)
        {
            _logger.Log($"OnCombatPreviewUpdated", LogSeverity.Info, LogCategory.UiNavigation);
            _panels.ForEach(panel => panel.OnCombatPreviewUpdated(combatPreview));
        }

        private void OnEndTurnButtonPressed()
        {
            _logger.Log("OnEndTurnButtonPressed", LogSeverity.Info, LogCategory.UiNavigation);
            _battleController.RequestEndTurn();
        }
        
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
        
        private void OnHoveredUnitChanged(Node? hoveredUnit)
        {
            _logger.Log("OnHoveredUnitChanged", LogSeverity.Trace, LogCategory.UiNavigation);
            if (hoveredUnit != null && hoveredUnit is not BattleUnit)
                throw new InvalidCastException("Unit is wrong type, expect BattleUnit");
            
            foreach (var panel in _panels)
                panel.OnHoveredUnitChanged(hoveredUnit as BattleUnit);
        }

        private void OnSelectedUnitChanged(Node? selectedUnit)
        {
            _logger.Log("OnSelectedUnitChanged", LogSeverity.Trace, LogCategory.UiNavigation);
            if (selectedUnit != null && selectedUnit is not BattleUnit)
                throw new InvalidCastException("Unit is wrong type, expect BattleUnit");

            foreach (var panel in _panels)
                panel.OnSelectedUnitChanged(selectedUnit as BattleUnit);
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
        
        private void OnTurnStarted(BattleSide activeSide, int turnNumber)
        {
            _turnNumberLabel.Text = turnNumber.ToString();
        }
    }
}
