# AI Handoff — Color Sand Prototype

> Read this first, then [architecture.md](architecture.md) (as-built design) and [roadmap.md](roadmap.md). Before coding, **summarize your understanding to the user** (AGENTS.md §16).

## Current state — 2026-10-07, end of session 3

| Item | State |
|---|---|
| Phase | **M0–M6 and M9 done and verified in the Unity Editor.** M7 (device) and M8 (deliverables) are next, when the user asks (D-021). |
| Game | `Assets/_Project/Scenes/Game.unity` (the only build scene). Full 50-level puzzle loop, multi-container docking & color flow, funky Main Menu, 1–50 scrollable Level Select with clearance tracking, in-game Pause modal with simulation freeze, universal audio Settings modal, and cute looping BGM. |
| Tests | **64/64 EditMode tests pass** (`SandGameplay.Tests.EditMode`, ~1.6 s). |
| Levels | **50 handcrafted levels** in `Resources/Levels/`. Verified with 0 wall-blocker overlaps, 0 container overlaps, and 100% color-to-container coverage. |
| Audio | Procedural cute looping BGM (`bgm_cute_loop.wav`) + 6 SFX clips managed by `AudioManager` with volume and mute persistence in `PlayerPrefs`. |
| Git | **Committed and pushed to GitHub `origin/main`** (`c1846bc`). |

## What was done in session 3

1. **Multi-Container Architecture (D-030):**
   - Upgraded `LevelConfig` to support an array of `ContainerDef` structures specifying color ID, footprint, and start position.
   - Dynamic container cloning in `LevelController`, partitioning total grains of each color evenly among containers matching that color.
   - `SandFlowSimulation` and `SandSimulationRunner` bind to `IReadOnlyList<ContainerPiece>`, updating flow, docking, and sequential completions across multiple containers.
2. **50 Handcrafted Levels Replicating `gpay.mp4` (D-031):**
   - Generated 50 levels (`Level_01` to `Level_50`) with progressive board sizes, wall blocker obstacles, and multi-color patterns.
   - Added White container support across all levels with white sand.
   - Automated audit verifying 0 blocker collisions and 100% color coverage. Fixed the Level 50 pink box blocker overlap bug.
3. **Funky Main Menu & Level Select (D-032):**
   - `MainMenuView` featuring floating title animation, funky background, Play button, Settings button, and Level Select button.
   - Scrollable `ScrollRect` grid displaying levels 1 to 50 with emerald green tiles and checkmark badges for cleared levels, and vibrant gold highlights for current level.
   - `LevelProgress.cs` storing cleared status in `PlayerPrefs` (`LevelCleared_{index}`).
   - Sequential auto-advancement to the next level on victory via `NextLevel()`.
4. **Cute Background Music & Audio Manager (D-033):**
   - Procedurally synthesized a 36-second seamless looping stereo WAV (`Assets/_Project/Audio/bgm_cute_loop.wav`) with music box/glockenspiel leads, woody marimba counter-melodies, ukulele chord arpeggios, soft acoustic bass, and gentle percussion.
   - Centralized `AudioManager.cs` with looping BGM AudioSource, SFX pool, pour loop handling, and `PlayerPrefs` volume and mute state persistence (`SG_MusicVolume`, `SG_MusicMuted`, `SG_SfxVolume`, `SG_SfxMuted`).
5. **In-Game Pause Modal & HUD Pause Button (D-034):**
   - Top-left HUD pause button (⏸) in `HudView.cs` mirrored symmetrically with the restart button.
   - Added `LevelState.Paused` to `LevelState.cs`. Pausing halts sand simulation, disables container dragging, and pauses pour audio.
   - `PauseView.cs` modal offering Resume, Restart, Settings, and Main Menu actions.
6. **Universal Audio Settings Modal (D-034):**
   - `SettingsView.cs` modal accessible from both Main Menu and Pause modal.
   - Sliders and ON/OFF mute buttons for Music and Sound Effects with immediate live response and persistence.
7. **Verification & Git:**
   - Automated Play Mode integration tests passed: verified BGM playback, settings adjustments, pause freeze/resume flow, restart flow, and menu navigation.
   - 64/64 EditMode tests passed.
   - Committed and pushed to `origin main` (`c1846bc`).

## Files created or changed

| Area | Files |
|---|---|
| Audio | `Assets/_Project/Audio/bgm_cute_loop.wav`, `Assets/_Project/Audio/AudioManager.cs`, `generate_cute_bgm.py` |
| UI | `Assets/_Project/Scripts/UI/HudView.cs`, `MainMenuView.cs`, `PauseView.cs`, `SettingsView.cs` |
| Core & Simulation | `Assets/_Project/Scripts/Core/LevelController.cs`, `LevelProgress.cs`, `LevelState.cs`, `Assets/_Project/Scripts/Simulation/SandFlowSimulation.cs`, `SandSimulationRunner.cs` |
| Levels | `Assets/_Project/Resources/Levels/Level_01.asset` … `Level_50.asset`, `Assets/_Project/Scripts/Data/LevelConfig.cs`, `ContainerDef.cs` |
| Scene | `Assets/_Project/Scenes/Game.unity` (configured PauseModal, SettingsModal, PauseButton, Btn_Settings) |
| Standards & Rules | `AGENTS.md`, `CODE_FORMAT_STANDARDS.md` |
| Documentation | `docs/project-overview.md`, `architecture.md`, `roadmap.md`, `tasks.md`, `decisions.md`, `ai_handoff.md` |

## Next recommended tasks

1. **Ask the user:**
   - Ready to test on physical device (Galaxy S21 via `adb devices`) or continue in Editor?
   - Prepare deliverables (Release APK, demo video recording, project zip)?
2. **M7 (Android Device Testing):**
   - Connect the Galaxy S21.
   - Create a development build with Autoconnect Profiler.
   - Check performance budgets (60 FPS, ≤ 2.0 ms sim, 0 B/frame GC in steady state).
   - Verify touch response on physical screen.
   - Produce release APK at `Builds/Android/ColorSand_v0.1.0.apk`.
3. **M8 (Deliverables):**
   - Record 30–90s gameplay video (`scrcpy --record=Recordings/colorsand_demo.mp4`).
   - Create clean project zip excluding Library, Temp, Builds, etc.
   - Complete submission checklist in README.

## Notes for the next agent

- **Git remote:** `https://x-access-token:<token>@github.com/WRDeadman10/sand-gameplay.git`.
- **Coding Style (STRICT from AGENTS.md):**
  - No `var`.
  - Explicit types everywhere.
  - No implicit `new()`.
  - K&R bracing (opening brace on same line).
  - No expression-bodied members.
  - `[SerializeField] private camelCase` for serialized fields; `_camelCase` for private fields.
- **Unity MCP (port 6400):**
  - Use `execute_code` with `roslyn` compiler for in-Editor inspection or test automation.
  - Use `refresh_unity` with `compile: request` after script modifications.
  - Use `run_tests` mode `EditMode` to verify test suite.
- **Scene structure:**
  - `Bootstrap`: contains `SandSimulationRunner` and `LevelController`.
  - `UI`: contains `Canvas`, `SafeArea`, `MainMenu`, `PauseModal`, `SettingsModal`.
  - `UI/SafeArea/Hud`: contains `LevelLabel`, `RestartButton`, `PauseButton`, `Hint`.
  - `Audio`: contains `AudioManager` with BGM and SFX sources.
