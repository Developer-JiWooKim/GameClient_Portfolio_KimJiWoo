# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

Unity 6 (`6000.4.9f1`, URP) portfolio project: a top-down maze exploration / chase-evasion game. The player is trapped
in a 20x20 maze, must collect 5 keys, then reach a Goal Point that spawns once all keys are collected. A monster
patrols the maze using an FSM (Idle/Chase/Attack) and NavMesh pathing; getting caught and attacked 3 times ends the
run. See `GameClient에 적용할 내용.txt` for the original Korean design notes (concept, planned hard-mode rules,
animation/intro plans) — some of it is aspirational/not yet implemented, so verify against actual code before relying
on it.

The core mechanic is a **dual maze layer**: the same space exists as two independently-generated mazes,
`Physical` and `Arcane`, with different wall layouts. The player toggles between them at will (input action
`SwitchLayer`); only one layer's walls are solid/rendered at a time. Hard mode changes monster detection rules while
in the Arcane layer (see below).

There is no CI, build script, linter, or automated test suite in this repo (the `com.unity.test-framework` package is
present but unused — no test files exist). "Running" this project means opening it in the Unity Editor.

## Working with this project

- Open with **Unity 6000.4.9f1** (URP render pipeline). Main scene: `Assets/MyAssets/Scenes/GameClientAssignment.unity`.
- There is no command-line build/test workflow — verify changes by pressing Play in the Editor. `GameManager.GameExit()`
  calls `EditorApplication.isPlaying = false` under `UNITY_EDITOR` (vs. `Application.Quit()` in a real build) so
  Play-mode exit doesn't close the editor.
- Compiled scripts assemble via the default `Assembly-CSharp` assembly (no asmdefs) — everything in `Assets/` compiles
  together.
- `Assets/MyAssets/Scripts/Obsolete/` (`AStarPathfinder.cs`, `FollowCamera.cs`) is dead code kept for reference only
  (marked `[Obsolete]`) — don't build on it; `MazeGenerator`/NavMesh and Cinemachine replaced it.
- External/imported assets (`AOSFogWar`, `SD Unity-Chan Haon Custom`, `Toon Shaders Pro`, `unity-chan!`, `TextMesh Pro`)
  are left at their original import paths and are not meant to be restructured. One deliberate exception:
  `AOSFogWar/csFogWar.cs`'s `ForceUpdateFog()` was changed from `private` to `public` (see the `[PROJECT
  CUSTOMIZATION]` comment at the top of that file) so replay can force an immediate fog refresh instead of waiting
  on the asset's internal lerp/update-rate gating — re-apply that one-line change if the asset is ever re-imported.

## Architecture

Code lives under `Assets/MyAssets/Scripts/`, split into `Player/`, `Monster/`, `UI/`, `Utility/` (with a `Manager/`
subfolder for singletons), `ScriptableObject/`, and `Obsolete/`.

### Game lifecycle and rules

- `GameManager` (singleton, survives via `Instance`) owns a `GameTimer` and a `GameRule`. `GameStart()` (called at the
  top of each playthrough, not just once at boot) creates a **fresh** `GameRule` and wires its `OnClear`/`OnGameOver`
  to stop the timer, so replaying doesn't leak state from the previous run. `PauseGame()`/`ResumeGame()`/`IsPaused`
  own `Time.timeScale`, and the static `CurrentGameMode` (`GameMode.Normal`/`Hard`) is selected from the
  difficulty-select UI. The static helper `GameManager.IsHardArcaneMode()` (Hard mode + `Arcane` layer active) is the
  single source of truth both `MonsterSight` and `MonsterMove` check for hard-mode behavior.
- `GameRule` is a **plain C# class**, not a MonoBehaviour — it's the pure rules/event hub for key collection and
  clear/game-over state (`OnKeyCollected`, `OnAllKeysCollected`, `OnClear`, `OnGameOver`). `GameManager` and
  `GameUIController` both subscribe to it rather than duplicating state.
- `GameUIController` is the panel/reference holder; actual UI flow is a **state machine**: `GameFlowFSM` drives
  `IGameFlowState` implementations (`TitleState`, `SelectState`, `PlayingState`, `PausedState`, `ResultState`) that
  each `Show()`/`Hide()` their panel on `Enter`/`Exit`. Pausing overlays `PausePanelUI` (which nests
  `OptionsPanelUI`) on top of the still-visible `InGamePanelUI` rather than hiding it. Restarting (from Select, from
  Pause's Replay, or from Result's Replay) all funnel through `GameUIController.StartNewGame()`, which regenerates
  the maze and re-spawns units **in place** — there is no scene reload for replay.

### Maze generation and the dual-layer system

- `MazeGenerator` builds one maze layer: DFS backtracker over a `Cell[,]` grid carves passages, then instantiates wall
  prefabs per remaining `Cell` wall flag (`northWall`/`eastWall`/etc.). `SetWallsActiveState(bool)` toggles a whole
  layer's walls between solid+rendered and invisible+trigger (used to make the inactive layer non-blocking but still
  detectable by overlap checks).
- `MazeLayerManager` (singleton) owns one `MazeGenerator` + one `NavMeshSurface` per layer (`Physical`/`Arcane`), each
  with an independently-baked NavMesh swapped via `AddData()`/`RemoveData()` on layer switch — monsters only path on
  whichever layer is currently active. Layer masks (`Wall_Physical`/`Wall_Arcane` Unity layers) gate raycasts/overlaps
  per layer.
- Layer switching (`SwitchLayer`, triggered by `PlayerInputHandler.OnLayerSwitchRequested`) first `Physics.CheckSphere`s
  the target layer's wall mask at the player's position to block switching into a wall, then runs
  `PlayLayerTransition`: pauses the game, disables player input, fades a screen-ripple shader in, swaps
  layer/NavMesh/fog-of-war state mid-ripple, fades out, and restores input/pause in a `finally` block. This uses
  Unity 6 `Awaitable` (not coroutines) throughout for these timed sequences.
- Fog of war is the external `AOSFogWar` asset (`FischlWorks_FogWar.csFogWar`); `MazeLayerManager` re-scans it whenever
  the maze is (re)generated or the layer changes, since wall geometry differs per layer.

### Spawning

- `UnitsSpawner` spawns player/monsters/keys/goal point after maze generation, using the active maze's `Cell` list
  (cell world positions are shared across both layers) as spawn candidates, shuffled via Fisher-Yates, excluding the
  player-start cell (0,0) and goal cell (bottom-right). The Goal Point only spawns once `GameRule.OnAllKeysCollected`
  fires. It also handles the intro→follow Cinemachine camera priority handoff (delayed via `Awaitable`) and registers
  the spawned player's `PlayerInputHandler` with `MazeLayerManager` and the fog-of-war revealer.
- Monsters and keys are **pooled** via `UnityEngine.Pool.ObjectPool<GameObject>` (`Get()`/`Release()`), not
  instantiated/destroyed per run — `MonsterController.ResetForReuse()`/`ClearTarget()` re-initialize a pooled
  monster's FSM/target/animation state. Follow this pooling pattern for new spawned/despawned unit types rather than
  raw `Instantiate`/`Destroy`.
- On layer switch, `UnitsSpawner` re-`Warp()`s every active monster's `NavMeshAgent` in place, since the two layers'
  wall layouts differ and a monster's old position/path can end up inside a wall on the newly active layer's NavMesh.

### Player

- Input flows through `PlayerInputHandler`, which subscribes to the new Input System's `PlayerInput.onActionTriggered`
  (event-driven, not per-frame polling) and exposes `InputVector` plus an `OnLayerSwitchRequested` event for the
  `Move`/`SwitchLayer` actions.
- `PlayerController` reads that input each `Update`, drives `PlayerMove`/`PlayerAnim`, tracks HP/invincibility-window
  on `TakeDamage()` (called by monsters), fires `OnHPChanged`/`OnDead`, and triggers a Cinemachine impulse (camera
  shake) on hit.

### Monster AI

- `MonsterController` composes `MonsterSight` (detection), `MonsterMove` (NavMeshAgent-based patrol/chase),
  `MonsterFSM`, `MonsterAnim`, and `MonsterAttackTrigger`/`MonsterFieldOfView` (children). `FixedUpdate` feeds sight
  results (`IsInRange`/`IsSensed`) into fields the FSM's states read; `Update` just delegates to `MonsterFSM.Tick()`.
- Monster state is a proper **State pattern**: `MonsterFSM` (Context) holds `IMonsterState` instances
  (`MonsterIdleState`/`MonsterChaseState`/`MonsterAttackState`) and only handles Enter/Exit transition plumbing +
  `Tick()` delegation (re-ticking once in the same frame if a transition happened, capped at 3 iterations as a
  safety net). Each state implementation owns its own transition logic and movement/animation calls — add new
  monster behaviors as new `IMonsterState` implementations, not as branches inside `MonsterFSM`.
- `MonsterSight.TargetSense`/`IsInRange` do line-of-sight: range check, then dot-product field-of-view check, then a
  `Physics.Raycast` against `MazeLayerManager.Instance.CurrentWallLayerMask` (so walls in the *currently active* layer
  block sight, and only that layer's walls). When `GameManager.IsHardArcaneMode()` is true (Hard mode + `Arcane`
  layer active), both checks are forced to always-true, **and** `MonsterMove.MoveToTarget` multiplies chase speed by
  `_hardArcaneChaseSpeedMultiplier` (1.3x, tuned to exceed player speed) — this is the difficulty-mode hook currently
  wired up. Other hard-mode ideas in `GameClient에 적용할 내용.txt` (time limit, sanity gauge, degrading Physical
  walls) are not yet implemented.
- Detection state is surfaced to the player: `MonsterController` fires `player.NotifyDetected(bool)` whenever a
  state's `IsAlertState` changes (true for Chase/Attack), which `PlayerFaceController` uses to swap expression.

### Cross-cutting

- Singletons (`GameManager`, `MazeLayerManager`, `SoundManager`) follow the same pattern: `Awake()` sets a static
  `Instance` and self-destroys duplicates.
- `SoundManager` plays SFX/BGM through a single `SoundLibrary` ScriptableObject asset; BGM swaps per maze layer via
  `PlayBGMForLayer`.
- Timed/sequenced behavior (layer transition ripple, camera intro delay) uses `Awaitable` + `destroyCancellationToken`,
  not `IEnumerator` coroutines — follow that convention for new async sequences.
