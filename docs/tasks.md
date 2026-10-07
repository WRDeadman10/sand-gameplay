# Tasks — Color Sand Prototype

> Update this file **during** coding (AGENTS.md §16). IDs map to milestones in [roadmap.md](roadmap.md).
> Legend: ☐ Todo · ◐ Partly done · ☑ Done · ⏸ Deferred/blocked (reason given)

## Summary (2026-10-07, end of session 3)

| Milestone | Tasks | Done | Status |
|---|---|---|---|
| Planning | 2 | 2 | ☑ |
| M0 Setup & standards | 14 | 14 | ☑ |
| M1 Sand simulation core | 6 | 6 | ☑ |
| M2 Tank + rendering | 7 | 7 | ☑ |
| M3 Board, box & drag | 6 | 6 | ☑ |
| M4 Path & fill | 10 | 9 | ☑ in the Editor; device APK deferred (D-021) |
| M5 Completion & restart | 7 | 7 | ☑ |
| M6 Visual polish & feel | 8 | 8 | ☑ Full SFX suite completed |
| M7 Performance & Android | 6 | 1.0 | ◐ Pause state implemented; device profiling pending |
| M8 Deliverables | 6 | 1.5 | ◐ README written; APK, video, zip pending |
| M9 50 Levels, UI & Audio | 8 | 8 | ☑ 50 levels, Main Menu, Level Select, Pause, Settings, BGM |
| M10 UI Modernization, DOTween & SFX Suite | 6 | 6 | ☑ SFX suite, DOTween everywhere, 25% scale, portrait layout fixes |
| M11 Level Integrity Hardening & Regression Tests | 5 | 5 | ☑ Blocker (L50) & phantom box (L35/15/22/30) fixes + 5 automated integrity tests |

## Planning

| ID | Task | Notes | Status |
|---|---|---|---|
| TP.1 | Analyse brief, video, standards; audit project | project-overview.md §2–§6 | ☑ |
| TP.2 | Create the docs set | `docs/` | ☑ |

## M0 — Setup & standards

| ID | Task | Result | Status |
|---|---|---|---|
| T0.1 | Resolve open decisions | D-005 A, D-012 `_camelCase`, D-013 provided, D-016 identity, D-017 cleanup, D-018 font + sprites, D-021 S21 | ☑ |
| T0.2 | Root `.editorconfig` | Provided by the user | ☑ |
| T0.3 | `Assets/_Project/` tree | architecture.md §15 | ☑ |
| T0.4 | asmdefs | `SandGameplay.Runtime`, `SandGameplay.Tests.EditMode` | ☑ |
| T0.5 | `Game.unity`, the only build scene | Orthographic camera, EventSystem + `InputSystemUIInputModule` | ☑ |
| T0.6 | Player settings | Portrait; `WRDeadman10`; `com.wrdeadman10.sand.gameplay`; 0.1.0 / 1 | ☑ |
| T0.7 | Mobile URP asset | Render Scale 1.0, main-light shadows off | ☑ |
| T0.8 | Sorting layers | `Gameplay`, `UI`, `UIFX` | ☑ |
| T0.9 | `Gameplay` action map | `PointerPosition`, `PointerPress` (D-027 format) | ☑ |
| T0.10 | TMP Essential Resources | Imported | ☑ |
| T0.11 | Android platform | Active; Editor quality level = Mobile (D-028) | ☑ |
| T0.12 | Sprite material check | `Sprite-Unlit-Default` renders (`SRPDefaultUnlit` pass) | ☑ |
| T0.13 | `dotnet format` check | Both csproj files pass `--verify-no-changes --no-restore`; detection proven | ☑ |
| T0.14 | Template cleanup | Removed `visualscripting`, `multiplayer.center`, `ai.navigation`, `collab-proxy`, `timeline`; deleted `TutorialInfo/` and `Readme.asset`; `SampleScene` kept but out of the build | ☑ |

## M1 — Sand simulation core

| ID | Task | Result | Status |
|---|---|---|---|
| T1.1 | `SandCell` | Occupied bit + 10-bit LUT index | ☑ |
| T1.2 | `XorShift32` | Exact style reference | ☑ |
| T1.3 | `SandGrid` | Step, sleep/wake, spawn/remove/set, counts, `Version`, `Cells` span | ☑ |
| T1.4 | `SimulationSettings` + `DefaultSimulation.asset` | Plus assist and spread settings | ☑ |
| T1.5 | Runner | `FixedTickClock` + `SandSimulationRunner` (`Sand.Tick` marker) | ☑ |
| T1.6 | `SandGridTests` | 14 tests, including conservation, determinism, 0-alloc `Step` | ☑ |

## M2 — Tank + rendering

| ID | Task | Result | Status |
|---|---|---|---|
| T2.1 | `SandPalette` + LUT + `DefaultPalette.asset` | 6 colors; tone ramp + sparkle | ☑ |
| T2.2 | `SandGridView` | Point texture, PPU = 24, dirty upload (`Sand.Upload` marker) | ☑ |
| T2.3 | `TankFillPattern` + tests | Solid + PixelArt; Bands dropped (D-024) | ☑ |
| T2.4 | `SandSource` | Initialize, reset, hint pulse. The debug drain was not needed because the real outlet came early | ☑ |
| T2.5 | `CameraFitter` | Fits any aspect; background cover | ☑ |
| T2.6 | Tank and board art | Final generated art (`Tools/ArtGen/generate_sprites.py`) | ☑ |
| T2.7 | Tuning | Defaults kept (G 24, 120 tps, slide 1.0); they match the reference look | ☑ |

## M3 — Board, box & drag

| ID | Task | Result | Status |
|---|---|---|---|
| T3.1 | `BoardGrid` + tests | 6 tests | ☑ |
| T3.2 | `BoardView` | Panel + one tiled cell renderer + wall clones | ☑ |
| T3.3 | `ContainerPiece` movement | Sub-stepped X/Y, smoothing, 0.12 s snap | ☑ |
| T3.4 | `ContainerVisuals` | Tint, selection lift + outline, dock bump, flash | ☑ |
| T3.5 | `ContainerDragController` | Polling, hit-test + padding, grab offset, UI rejection | ☑ |
| T3.6 | Pointer check | Verified with Input System mouse events in Play mode (press, drag, release); touch uses the same `<Pointer>` bindings | ☑ |

## M4 — Path & fill (core loop playable)

| ID | Task | Result | Status |
|---|---|---|---|
| T4.1 | `OutletSpan` + `DockResolver` + tests | 5 tests | ☑ |
| T4.2 | `ContainerColorRule` | + `Accepts` / `CountAcceptable` | ☑ |
| T4.3 | `GrainTransferQueue` + tests | 4 tests | ☑ |
| T4.4 | `SandOutlet` + tests | 7 tests | ☑ |
| T4.5 | Box interior | 40×40 + 3 rim rows; ratio spawn with spread; top-up (D-023) | ☑ |
| T4.6 | `SandContainer` + tests | 6 tests | ☑ |
| T4.7 | `ContainerLabelView` | Cached strings, pulse per 10 % | ☑ |
| T4.8 | Tick pipeline + assist | `SandFlowSimulation` + 6 tests | ☑ |
| T4.9 | Path events | `PathOpened` / `PathClosed` | ☑ |
| T4.10 | First development APK on device | Deferred: the user asked for Editor-first (D-021) | ⏸ |

## M5 — Completion & restart

| ID | Task | Result | Status |
|---|---|---|---|
| T5.1 | `LevelController` | State machine, validation, `targetFrameRate = 60`, in-place restart | ☑ |
| T5.2 | `Easing` | OutQuad, InBack, OutBack, InOutSine | ☑ |
| T5.3 | `CompletionSequencer` | Pop + flash, praise + stars, fly into tank, puff | ☑ |
| T5.4 | Praise popup, bursts | Pooled particle systems, tinted `Emit` | ☑ |
| T5.5 | `LevelCompleteView` | Dim, rays, bouncing title, confetti, pulsing Play Again | ☑ |
| T5.6 | `HudView` | Restart button, bobbing hint | ☑ |
| T5.7 | 10-cycle restart test | 40 automated cycles: fresh state every time, exactly 1 completion per cycle, no managed-memory growth after warm-up | ☑ |

## M6 — Visual polish & feel

| ID | Task | Result | Status |
|---|---|---|---|
| T6.1 | Sprites | 16 generated sprites + tank pattern | ☑ |
| T6.2 | Font | Lilita One (OFL) + SDF asset + Title/Outline/Label presets | ☑ |
| T6.3 | Palette & background | Candy palette; purple gradient background | ☑ |
| T6.4 | `PourSprayVfx` | Subtle rim spray, proportional to flow | ☑ |
| T6.5 | Dock bump, hint pulse, label pulse | Done | ☑ |
| T6.6 | Rays + confetti | Done | ☑ |
| T6.7 | `SafeAreaFitter` | Done | ☑ |
| T6.8 | SFX suite | UI clicks, dock, sand pour loop, box pop, praise, level complete in `AudioManager` | ☑ |

## M7 — Performance & Android hardening (deferred, D-021)

| ID | Task | Notes | Status |
|---|---|---|---|
| T7.1 | Profiling | Editor done: `Sand.Tick` 0.033 ms avg, `Sand.Upload` 0.042 ms avg. Device (S21) pending | ◐ |
| T7.2 | Budget tuning | Only if the device misses the budget | ☐ |
| T7.3 | Pause/resume and Android back button | In-game pause modal freezes sim/input; back button hook pending | ◐ |
| T7.4 | Aspect-ratio checks | Editor at 1080×2400 done; on-device pending | ☐ |
| T7.5 | Release APK | — | ☐ |
| T7.6 | 10-minute soak on device | — | ☐ |

## M8 — Deliverables

| ID | Task | Notes | Status |
|---|---|---|---|
| T8.1 | `README.md` | Written; add APK and video links when they exist | ◐ |
| T8.2 | Demo video, 30–90 s | Needs the device or Editor recording | ☐ |
| T8.3 | Project zip + verify it opens | Exclusions in roadmap.md M8 | ☐ |
| T8.4 | Submission checklist | — | ☐ |
| T8.5 | Final `ai_handoff.md` | Updated each session | ◐ |
| T8.6 | Editor-tooling packages for submission | D-020 / D-025 | ⏸ |

## M9 — Extended Puzzle Features (50 Levels, Full UI & Audio)

| ID | Task | Result | Status |
|---|---|---|---|
| T9.1 | Multi-container & multi-color pipeline | Multi-container pooling, color partitioning, simulation runner binding | ☑ |
| T9.2 | 50 Levels creation matching `gpay.mp4` | 50 LevelConfig ScriptableObjects created in `Resources/Levels/` | ☑ |
| T9.3 | Level validation & obstacle collision fix | Automated audit (0 blocker overlaps, 0 box overlaps, 100% color coverage); Level 50 pink box fixed | ☑ |
| T9.4 | Funky Main Menu & Level Select | `MainMenuView` with floating title, funky background, scrollable 1-50 level buttons with cleared badges | ☑ |
| T9.5 | Sequential progression & progress storage | `LevelProgress` persistence in PlayerPrefs; auto-advance to next level on completion | ☑ |
| T9.6 | Cute background music synthesis & manager | 36s seamless looping cozy BGM (`bgm_cute_loop.wav`); looping AudioSource in `AudioManager` | ☑ |
| T9.7 | In-game pause button & pause modal | `HudView` pause button, `PauseView` modal (Resume, Restart, Settings, Main Menu), simulation/drag freeze | ☑ |
| T9.8 | Audio settings modal | `SettingsView` modal with Music & SFX volume sliders + mute toggles, PlayerPrefs persistence | ☑ |

## M10 — UI Modernization, DOTween & Interactive Audio Suite

| ID | Task | Result | Status |
|---|---|---|---|
| T10.1 | Pick, drop, drag, button SFX synthesis | `sfx_pick_box.wav`, `sfx_drop_box.wav`, `sfx_drag_loop.wav`, `sfx_button_press.wav` synthesized | ☑ |
| T10.2 | Audio integration in controller & manager | `AudioManager` and `ContainerDragController` play pick, drop, drag loop, button click | ☑ |
| T10.3 | Header text & hint overlap fixes | LevelLabel 640px wide with auto-sizing (22-44pt); Hint moved to y=75 below boxes | ☑ |
| T10.4 | 25% Scale increase for portrait devices | PauseModal card (675x775), SettingsModal card (700x775), MainMenu (650x150, 450x320), LevelSelectPanel (675x920, cells 106x106) | ☑ |
| T10.5 | Universal DOTween animations | `UIButtonJuice` punch/scale, `PauseView` fade/scale, `SettingsView` fade/scale, `MainMenuView` punch, `LevelCompleteView` fade/scale | ☑ |
| T10.6 | Fresh cute BGM | Sunny G-major cozy melody (`bgm_cute_loop.wav`) with glockenspiel, ukulele, upright bass, and shakers | ☑ |

## M11 — Level Integrity Hardening & Regression Tests

| ID | Task | Result | Status |
|---|---|---|---|
| T11.1 | Stray pixel audit & texture repair | Fixed stray (0,0) pixels in `pattern_lvl15.png`, `pattern_lvl22.png`, `pattern_lvl35.png` | ☑ |
| T11.2 | Level 35 phantom white box removal | Removed White container from `Level_35.asset`; centered Pink & Green containers at x=1, x=4 | ☑ |
| T11.3 | Levels 15, 22, 30 phantom box removal | Removed phantom containers from `Level_15.asset`, `Level_22.asset`, `Level_30.asset`; balanced layouts | ☑ |
| T11.4 | Level 50 blocker collision resolution | Verified pink container relocated to {x:0, y:0} clear of all wall blockers | ☑ |
| T11.5 | Automated level integrity test suite | Created `LevelIntegrityTests.cs` (5 tests asserting matching sand >= 50, all colors accepted, 0 blocker overlaps, 0 box overlaps, bounds validity); 69/69 tests green | ☑ |


