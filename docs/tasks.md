# Tasks — Color Sand Prototype

> Update this file **during** coding (AGENTS.md §16). IDs map to milestones in [roadmap.md](roadmap.md).
> Legend: ☐ Todo · ◐ Partly done · ☑ Done · ⏸ Deferred/blocked (reason given)

## Summary (2026-10-07, end of session 2)

| Milestone | Tasks | Done | Status |
|---|---|---|---|
| Planning | 2 | 2 | ☑ |
| M0 Setup & standards | 14 | 14 | ☑ |
| M1 Sand simulation core | 6 | 6 | ☑ |
| M2 Tank + rendering | 7 | 7 | ☑ |
| M3 Board, box & drag | 6 | 6 | ☑ |
| M4 Path & fill | 10 | 9 | ☑ in the Editor; device APK deferred (D-021) |
| M5 Completion & restart | 7 | 7 | ☑ |
| M6 Visual polish & feel | 8 | 7 | ☑ (audio/haptics stretch not done) |
| M7 Performance & Android | 6 | 0.5 | ⏸ Device work deferred by the user (D-021); Editor profiling done |
| M8 Deliverables | 6 | 1.5 | ◐ README written; APK, video, zip pending |

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
| T6.8 | (Stretch) SFX + haptics | Not started | ☐ |

## M7 — Performance & Android hardening (deferred, D-021)

| ID | Task | Notes | Status |
|---|---|---|---|
| T7.1 | Profiling | Editor done: `Sand.Tick` 0.033 ms avg, `Sand.Upload` 0.042 ms avg. Device (S21) pending | ◐ |
| T7.2 | Budget tuning | Only if the device misses the budget | ☐ |
| T7.3 | Pause/resume and Android back button | — | ☐ |
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
