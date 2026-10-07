# AI Handoff — Color Sand Prototype

> Read this first, then [architecture.md](architecture.md) (as-built design) and [roadmap.md](roadmap.md). Before coding, **summarize your understanding to the user** (AGENTS.md §16).

## Current state — 2026-10-07, end of session 6

| Item | State |
|---|---|
| Phase | **M0–M6, M9, M10, and M11 done and verified in the Unity Editor.** M7 (device) and M8 (deliverables) are next, when the user asks (D-021). |
| Game | `Assets/_Project/Scenes/Game.unity` (the only build scene). Full 50-level puzzle loop, multi-container docking & color flow, funky Main Menu, 1–50 scrollable Level Select with clearance tracking, in-game Pause modal, universal audio Settings modal, rich DOTween transitions, interactive SFX suite, and cozy looping BGM. |
| Tests | **69/69 EditMode tests pass** (`SandGameplay.Tests.EditMode`, ~1.8 s), including 5 new automated level integrity assertions. |
| Levels | **50 handcrafted levels** in `Resources/Levels/`. Verified with 0 wall-blocker overlaps, 0 container overlaps, 0 phantom containers, and 100% color-to-container coverage. |
| Level Bug Fixes | **Bug 1 (Level 50 Blocker Overlap)** resolved: pink box free of blockers at (0,0). **Bug 2 (Level 35 Phantom White Box)** resolved: phantom box removed, texture stray pixel repaired, balanced layout at x=1, x=4. Levels 15, 22, 30 also repaired. |
| Automated Guardrail | `LevelIntegrityTests.cs` runs 5 comprehensive checks on every level to permanently prevent future phantom boxes or blocker collisions. |
| Tooling & Build | 512×512 app icon configured across all platforms (D-038). Roslyn DLLs in `Assets/Plugins/Roslyn/` configured Editor-only (D-029) to prevent build bloat. |
| Audio & UI | Looping BGM (`bgm_cute_loop.wav`), full SFX suite, 25% scale increase for portrait mobile devices (1080×2400), DOTween animations everywhere, zero abrupt `SetActive`. |
| Code Standards | 100% strictly compliant with `AGENTS.md`: 0 `var` keywords, 0 `new()` shorthand expressions, explicit types, K&R braces, full method bodies. |
| Git | **Committed and pushed to GitHub `origin/main`**. |

## What was done in session 5

1. **Root Cause Analysis & Fix for Level 35 Phantom White Box (D-037):**
   - Investigated user screenshot showing a White box on Level 35 ("Sakura Blossom") with only Green and Pink sand in the tank.
   - Identified root cause: bottom-left pixel `(0, 0)` in `pattern_lvl35.png` was accidentally colored White (`#EBEFFB`) instead of Green meadow (`#38D161`). The level generator script had detected this 1 pixel and automatically created a White container definition (`colorId: 5`).
   - Repaired `pattern_lvl35.png` using PIL to set pixel `(0, 0)` to Green meadow.
   - Removed phantom White container from `Level_35.asset` and centered the 2 legitimate containers (Pink and Green) at `{x: 1, y: 0}` and `{x: 4, y: 0}` on the 7-wide board.

2. **Full-Matrix Audit of All 50 Levels (D-037):**
   - Scanned all 50 textures and LevelConfig assets.
   - Discovered and repaired 3 other affected levels:
     - `Level 15 (Island Lagoon)`: 1 stray Blue pixel at (0, 0) fixed to Yellow; removed phantom Blue container.
     - `Level 22 (Desert Oasis)`: 1 stray Yellow pixel at (0, 0) fixed to Blue; removed phantom Yellow container.
     - `Level 30 (Target Ring)`: defined phantom White container with 0 white sand; removed phantom container.
   - Re-verified Level 50: confirmed pink container starts at `{x: 0, y: 0}`, completely free of wall blockers.

3. **Automated Regression Prevention Suite (`LevelIntegrityTests.cs`):**
   - Implemented 5 automated EditMode unit tests in `Assets/_Project/Tests/EditMode/LevelIntegrityTests.cs`:
     1. `AllLevels_EachContainerHasMatchingSandInTank`: verifies every container has `>= 50` grains in tank.
     2. `AllLevels_EachSandColorHasAcceptingContainer`: verifies every tank color has an accepting container.
     3. `AllLevels_NoContainerOverlapsWallBlocker`: verifies no container spawns on a wall obstacle.
     4. `AllLevels_NoContainerOverlapsAnotherContainer`: verifies no containers overlap each other.
     5. `AllLevels_AllContainersWithinBoardBounds`: verifies all containers fit inside board boundaries.
   - Total test count expanded from 64 to 69 tests. All 69 tests pass green.

4. **Play Mode Integration Verification:**
   - Loaded Level 35 in Unity Play Mode: confirmed exactly 2 containers (Pink at (1,0), Green at (4,0)), 0 white sand grains, 0 white containers, clean board presentation.

## Files created or changed

| Area | Files |
|---|---|
| Tests | `Assets/_Project/Tests/EditMode/LevelIntegrityTests.cs` (5 automated integrity tests) |
| Levels | `Assets/_Project/Resources/Levels/Level_15.asset`, `Level_22.asset`, `Level_30.asset`, `Level_35.asset` |
| Patterns | `Assets/_Project/Art/Patterns/pattern_lvl15.png`, `pattern_lvl22.png`, `pattern_lvl35.png` |
| Icons & Build | `Assets/_Project/Art/Icons/app_icon.png`, `ProjectSettings/ProjectSettings.asset` |
| Documentation | `docs/tasks.md`, `docs/decisions.md`, `docs/ai_handoff.md` |

## What was done in session 6

1. **App Icon Creation (D-038):**
   - Synthesized a 512×512 3D casual game icon (`Assets/_Project/Art/Icons/app_icon.png`) featuring a tilted glass hourglass pouring glowing multi-colored sand into a vibrant candy-colored toy box on a deep violet gradient tile.
   - Configured texture importer with Clamp wrap mode, bilinear filtering, and alpha transparency.
2. **Build Settings & Multi-Platform Assignment (D-038):**
   - Assigned icon to Default Application Icon (`NamedBuildTarget.Unknown`, `IconKind.Application`).
   - Assigned icon to Android Adaptive (API 26), Round (API 25), and Legacy tiers across all 6 density resolutions (432px down to 36px).
   - Assigned icon to Standalone build target.
3. **Roslyn Compiler DLLs Configured as Editor-Only (D-029):**
   - Configured `PluginImporter` for all 5 DLLs in `Assets/Plugins/Roslyn/` (`Microsoft.CodeAnalysis*.dll`, `System.Collections.Immutable.dll`, `System.Reflection.Metadata.dll`, `System.Runtime.CompilerServices.Unsafe.dll`) to `Editor-only` (excluded from Android and Standalone).
   - Prevents 12.7 MB of compiler assemblies from bloating the Android APK or causing IL2CPP stripping issues, while preserving full Roslyn compilation for the Unity MCP bridge in the Editor.
   - All 69 EditMode tests pass.

## Next recommended tasks

1. **Ask the user:**
   - Ready to test on physical Android device (Galaxy S21 via `adb devices`) or continue in Editor?
   - Prepare deliverables (Release APK, demo video recording, project zip)?
2. **M7 (Android Device Testing):**
   - Connect the Galaxy S21.
   - Create a development build with Autoconnect Profiler.
   - Check performance budgets (60 FPS, ≤ 2.0 ms sim, 0 B/frame GC in steady state).
   - Verify touch response and haptics on physical screen.
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
  - Use `run_tests` mode `EditMode` to verify test suite (69 tests).
- **Scene structure:**
  - `Bootstrap`: contains `SandSimulationRunner` and `LevelController`.
  - `UI`: contains `Canvas`, `SafeArea`, `MainMenu`, `PauseModal`, `SettingsModal`.
  - `UI/SafeArea/Hud`: contains `LevelLabel`, `RestartButton`, `PauseButton`, `Hint`.
  - `Audio`: contains `AudioManager` with BGM, SFX sources, and drag source.
