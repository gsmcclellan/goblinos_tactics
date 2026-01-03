# Goblinos Godot C# Style Guide

This document defines the coding style and architectural conventions for this project.
When writing or editing code, follow these rules unless explicitly told otherwise.

---

## 1) Language & Tooling

- **Godot 4.x**, **C#**
- Prefer **signals for gameplay/UI events** across nodes.
- Use **C# events** for *intra-object / local* eventing when signals aren’t a good fit.

---

## 2) File & Type Organization
- Prefer ClassName.cs for a primary type.
- Use partial classes to split large systems into concerns:
    - BattleController.cs
    - BattleController.Actions.cs
    - BattleController.Input.cs

### 2.1 One type per file (generally)
- Prefer `ClassName.cs` for a primary type.
- Use partial classes to split large systems into concerns:
    - `BattleController.Actions.cs`
    - `BattleController.State.cs`
    - `BattleController.Input.cs`

### 2.2 Namespaces
- Use project namespaces consistently:
    - `Goblinos.Scripts.Battle`
      - `Goblinos.Scripts.Terrain`
    - `Goblinos.Scripts.Core`
    - `Goblinos.Scripts.Data`
    - `Goblinos.Scirpt.Input`
    - `Goblinos.Scirpt.Overworld`
    - `Goblinos.Scripts.UI`
      - `Goblinos.Scripts.UI.Battle`
    - `Goblinos.Scirpt.Units`
    - `Goblinos.Scripts.Util`
- Keep folder structure aligned with namespaces.

---

## 3) Naming Conventions

### 3.1 Types & Members
- Classes/Structs/Enums: `PascalCase`
- Methods/Properties: `PascalCase`
- Local variables: `camelCase`
- Constants: `SCREAMING_SNAKE_CASE`

### 3.2 Fields
- Private fields: `_camelCase` (leading underscore)
    - Example: `_cursor`, `_stateMachine`
- `readonly` fields: still `_camelCase`
- Avoid public fields. Prefer properties.

### 3.3 Events & Signals
- C# events: `PascalCase` with a past-tense or descriptive name:
    - `public event Action<GridCursorFocus> GridCursorFocusChanged;`
- Godot signals: `PascalCase` signal name in C#:
    - `[Signal] public delegate void GridCursorFocusChangedEventHandler(GridCursorFocus focus);`

### 3.4 Naming Clarity
- Prefer descriptive, explicit names over abbreviations or initialisms
- Avoid unclear shorthand such as:
    - `ctx`, `mgr`, `fsm`, `tmp`, `sel`
- Use abbreviations only when meaning is unambiguous in context
- Good:
```csharp
gridCursorFocus
selectedTargetCell
battleStateController
```

---

## 4) Events & Signals

### 4.1 Godot Signals

- Use signals for **node-to-node communication** and public-facing events.
- Signals must use Variant-friendly types:
    - primitives
    - `Vector2I`, `Vector2`
    - `Node`, `Resource`, `RefCounted`

Example:

```
[Signal]
public delegate void GridCursorFocusChangedEventHandler(GridCursorFocus focus);
```

### 4.2 C# Events

- Use C# events for **internal subsystem communication**.
- Prefer explicit, descriptive event names.

Example:

```
public event Action<GridCursorFocus> GridCursorFocusChanged;

public partial class GridCursorFocus: RefCounted
{
    public Vector2I Cell { get; init; }
    public TerrainType Terrain { get; init; }
    public Unit? Unit { get; init; }
    public Node? TopNode { get; init; }
    public Godot.Collections.Array<Node> Nodes { get; init; } = new();
    public bool HasUnit => Unit != null;
}
```

### 4.3 Facade Pattern

- Controllers (e.g. `BattleController`) may forward internal signals/events.
- External systems should depend on the controller, not its internals.

---

## 5) Formatting & Control Flow

### 5.1 Indentation & Style

- Indentation: **4 spaces**
- Brace style: **Allman**

### 5.2 One-liners

- **Do not use braces for single-line statements**:

```
if (cell == lastCellFocused)
    return;
```

- Use braces for multi-line blocks:

```
if (shouldUpdate)
{
    UpdateFocus();
    EmitChanges();
}
```

- If a one-liner grows, convert it to a braced block immediately.

---

## 6) Documentation (Docstrings)

### 6.1 Required documentation

- All **public classes**, **public methods**, and **non-trivial private methods** must have XML doc comments.
- Docstrings should describe **intent**, not implementation details.

Example:

```
/// <summary>
/// Updates the cursor focus based on its current world position
/// and emits a focus-changed signal if the focused cell changed.
/// </summary>
private void UpdateFocus()
{
}
```

---

## 7) Logging

### 7.1 Logging rule

- Every method should include at least one call to `DebugUtil.Log`.
    - Entry logs for public methods
    - State-change logs for private helpers
    - Ready callbacks log `"[NodeName] Ready"` at end of ready function
- When logging in hot paths (`_Process`, `_PhysicsProcess`) use LoggingSeverity.Extra severity.

Example:

```
DebugUtil.Log("[GridCursor] Move " + direction, 0, DebugLogCategory.UiNavigation);
```

## 7.2 DebugUtil Configuration & Usage Rules

The project uses a centralized logging utility (`DebugUtil`) to control log verbosity and signal-to-noise during development.

---

### 7.2.1 Global Logging Controls

`DebugUtil` exposes two global configuration fields:

- `LoggingEnabled` - Acts as a *master kill-switch* for all logging.
- `LoggingSeverity` - Defines the *minimum severity* that will be emitted.

- Severity ordering is defined by `DebugLogSeverity`
- All logs **should** specify an appropriate `DebugLogCategory`, but None can be used if no suitable category is available

```csharp
public enum DebugLogCategory
{
    // Core & Engine-Level
    None,               // Default / uncategorized
    Initialization,     // Node setup, _Ready, dependency wiring
    Exit,               // Shutdown, cleanup, scene exit
    Error,              // Non-fatal errors, recoverable failures
    Warning,            // Suspicious but allowed states
    Signal              // Godot Signals

    // Input & Cursor
    Input,              // Raw input events, actions
    UiNavigation,       // Menu focus, UI selection, UI cursor
    
    // Battle & Gameplay flow
    BattleState,        // State machine transitions, turn start/end, phase changes
    CombatResolution,  //  Attacks, abilities, items, damage, hit/miss, crits, status effects
    
    // Units & AI
    UnitLifecycle,      // Spawn, death, removal
    UnitStats,          // HP, buffs, debuffs, stat changes
    AiDecision,         // AI evaluation & choice
    AiMovement,         // Pathing decisions, movement intent
    
    // Data & Resources
    DataLoading,        // Loading resources, JSON, configs
    Serialization,     // Save/load
    Validation,        // Data sanity checks
    
    // Performance / Diagnostics
    Performance,       // Timing, frame-sensitive diagnostics
    DebugOnly           // Temporary or experimental logs
}

public enum DebugLogSeverity
{
    Extra = -1,    // Extremely spammy logs that will dominate the console
    Trace = 0,     // Minor info
    Info = 1,      // Basic info level
    Warning = 2,   // Potential issues, non-gamebreaking
    Error = 3,     // Major issues that cause unintended side effects
    Critical = 4   // Severe game-breaking bugs
}
```

---

## 8) Assertions & Guards

### 8.1 Debug.Assert usage

- Use `Debug.Assert` to enforce required invariants:
    - Required node references
    - Assumptions that must always hold in a valid state

Example:

```
Debug.Assert(_cursor != null, "GridCursor not initialized.");
```

### 8.2 Guard clauses

- Use early returns for invalid or unchanged state:

```
if (cell == lastCellFocused)
    return;
```

- Prefer guard clauses over deep nesting.

