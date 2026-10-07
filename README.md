# Color Sand — Sand Gameplay Replication

A high-fidelity, production-ready Unity replication of the mobile puzzle hit **Color Sand: Fill The Blocks**, faithfully modeled after reference gameplay (`gpay.mp4`).

Players navigate colored containers under a sand dispenser tank, docking them into position to drain matching sand grains. The simulation calculates cellular grain-by-grain physics with angle-of-repose slumping, multi-color filtering, obstacle avoidance, and celebratory launch sequences across **50 handcrafted progressive puzzle levels**.

---

## Key Highlights

- **50 Handcrafted Progressive Levels**: Fully replicated from level 1 to 50, featuring multi-color sands, multi-container docking stations, obstacle blockers, and spatial color puzzles.
- **Cellular Automaton Physics Engine**: Deterministic 120 Hz falling-sand simulation with exact grain conservation, natural 45° angle of repose, color-matching filters, and zero per-frame garbage collection.
- **Multi-Container Docking & Flow**: Dynamic dock/undock detection, spatial container snapping, color isolation, visual fill ratio scaling, container launch sequence, and praise popups.
- **Modern DOTween UI & Portrait Mobile UX**: Responsive portrait layout (1080×2400 / 9:16) with safe-area support, punch/fade transitions on all UI elements, 25% scale increase for readability, Main Menu, paged 50-level Level Select grid, In-Game HUD, Pause Modal, and Settings Modal.
- **Tactile Audio Suite & Cozy BGM**: Procedural audio for box picking, dropping, continuous dragging loop, sand pour stream, button clicks, launch pops, fanfare, and cozy looping background music with persistent volume and mute controls.
- **Production-Grade Engineering**: 69 automated EditMode unit and integration tests passing at 100%, strict SOLID architecture, explicit typing, K&R braces, and zero runtime GC allocations in hot simulation paths.

---

## Technical Specifications & Requirements

| Specification | Details |
|---|---|
| **Unity Version** | **6000.3.9f1** (Unity 6.3 LTS) with Android Build Support (SDK, NDK, OpenJDK) |
| **Render Pipeline** | Universal Render Pipeline (URP) 17.3.0 (`Mobile_RPAsset`, Mobile quality tier) |
| **Target Platform** | Android (ARM64, IL2CPP, Min API 25, Target API 34+) & Unity Editor |
| **Target Resolution** | Portrait **1080×2400** (20:9 / Galaxy S21) with `SafeAreaFitter` support |
| **Package ID** | `com.wrdeadman10.sand.gameplay` |
| **App Icon** | 512×512 icon assigned for Default, Standalone, and Android (Adaptive, Round, Legacy) |
| **Key Packages** | Input System 1.18.0, uGUI 2.0.0 (TextMeshPro), URP 17.3.0, DOTween (Demigiant), Test Framework 1.6.0 |

---

## How to Play & Controls

| Action | Control (Touch / Mouse) | Description |
|---|---|---|
| **Move Container** | Drag & drop | Touch and drag any colored container across the board grid. Containers snap to valid grid slots upon release. |
| **Dock Container** | Drag to top row | Slide the container directly underneath the tank opening. Once docked, sand matching the container's color pours in automatically. |
| **Steer Sand Flow** | Slide while docked | Shift the container horizontally along the tank outlet to drain specific piles. Pulling the container away immediately seals the flow without losing collected sand. |
| **Auto-Launch** | Fill to 100% | When a container is filled, it pops with celebratory confetti, plays a praise popup ("SWEET!", "PERFECT!"), and launches into the tank. |
| **Level Progression** | Clear all sand | Fill all matching containers to clear the tank and advance to the next level. |
| **Game Navigation** | HUD buttons | Access the Pause menu (Resume, Restart, Settings, Main Menu) or trigger instant Level Restart at any time. |

---

## How the Sand Simulation Works

1. **Cellular Automaton**: Each grain is represented by a 16-bit bit-packed `SandCell` (`ushort` containing occupancy, color index, and tone variance). Simulation scans bottom-to-top with alternating left/right scan passes each tick, ensuring stable 45° pile slumping without directional bias.
2. **Dual-Grid Architecture**:
   - **Tank Grid**: 144×48 simulation cells representing the main sand reservoir.
   - **Container Interior**: 40×40 cells plus 3 rim transfer rows per container, simulated in local coordinates so dragging never disturbs resting sand.
3. **Docking & Outlet Transfer**: When a container docks flush against the tank base, `DockResolver` maps the overlapping columns. `SandOutlet` extracts accepted grains from the bottom row each tick, queuing them into the container's rim buffer. Mismatched colors block flow naturally.
4. **Fill Accounting & Scaling**: Tank capacity equals the total matching grains in the reservoir. Because container grids are smaller than the reservoir, visual grains spawn proportionally while preserving color and tone. Stranded grains are vacuumed at ≥97% capacity, and containers top off cleanly at 100%.
5. **Direct Texture Rendering**: Changes are blitted directly into a point-filtered `Texture2D` using `GetPixelData<Color32>()` with color-tone LUTs. Settled grids enter sleep state to eliminate redundant GPU uploads.
6. **Performance & GC**: Fixed 120 ticks/s simulation (~0.03 ms/frame, ~0.04 ms texture upload). 0 bytes GC allocated per simulation step.

---

## Project Structure

```
Assets/_Project/
  Scenes/
    Game.unity                  # Unified game scene (Main Menu, Gameplay, UI Modals)
  Scripts/                      # SandGameplay.Runtime assembly
    Sand/                       # Grid cellular automaton, SandCell, RNG, Palette, TextureView
    Source/                     # SandSource (tank) and procedural/texture fill patterns
    Transfer/                   # DockResolver, SandOutlet, TransferQueue, ColorRule
    Container/                  # SandContainer, InteriorSpawner, ContainerPiece, ContainerPool
    Board/                      # BoardGrid, BoardSlot, ObstacleBlocker
    Interaction/                # PointerDragController, ContainerDragController (Input System)
    Simulation/                 # FixedTickClock, FlowPipeline, SandSimulationRunner
    Presentation/               # CameraFit, CompletionSequence, PraisePopup, PourSprayVfx
    UI/                         # MainMenuView, LevelSelectView, HudView, PauseView, SettingsView, Juice
    Audio/                      # AudioManager, AudioSettings, procedural sound effects & BGM
    Core/                       # LevelController (state machine & progression), LevelProgress
    Data/                       # LevelConfig ScriptableObjects, PaletteData, SimulationSettings
  Resources/
    Levels/                     # Level_01 to Level_50 ScriptableObjects (runtime loaded)
  Art/
    Icons/                      # 512×512 app_icon.png and Android adaptive assets
    Sprites/                    # Generated crisp 9-sliced UI frames, containers, blockers
    Patterns/                   # pattern_lvl01.png to pattern_lvl50.png tank textures
    Fonts/                      # Lilita One font asset and TextMeshPro SDF presets
    Materials/                  # URP unlit sprite materials
  Audio/
    bgm_cute_loop.wav           # Cozy looping background music (36s seamless loop)
    sfx_*.wav                   # Tactile procedural audio clips (pick, drop, drag, pour, pop, UI)
  Prefabs/                      # Container, Blocker, Spray, HUD, Modals, Praise
  Tests/EditMode/               # SandGameplay.Tests.EditMode assembly (69 tests)
docs/                           # project-overview.md, architecture.md, roadmap.md, tasks.md, decisions.md, ai_handoff.md
```

---

## Automated Test Suite

The project includes **69 automated EditMode tests** across 12 test suites verifying physics, transfer rules, docking geometry, and puzzle level integrity:

- **Automaton Physics** (`SandGridTests`): Gravity fall, diagonal slide, conservation of grains, deterministic seeds, sleep/wake cycles, 0-byte GC allocation.
- **Transfer & Rules** (`DockResolverTests`, `SandOutletTests`, `TransferQueueTests`): Overlap column math, color acceptance, queue push/pop.
- **Fill Accounting** (`FillAccountingTests`, `ContainerInteriorSpawnerTests`): Ratio scaling, capacity thresholds, topping-off logic.
- **Board & Timing** (`BoardGridTests`, `FixedTickClockTests`, `EndToEndFlowTests`): Slot snapping, fixed clock stepping, end-to-end tank-to-box drain.
- **Level Integrity** (`LevelIntegrityTests`): Verifies all 50 levels have valid bounds, sand counts ≥ 50, 100% container color coverage, zero blocker overlaps, and zero container overlaps.

### Running Tests in Unity

1. In the Editor, open **Window → General → Test Runner**.
2. Select the **EditMode** tab.
3. Click **Run All** (all 69 tests pass green).

### Running Tests via CLI

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Unity.exe" `
  -batchmode -runTests `
  -testPlatform EditMode `
  -projectPath "D:\Unity\sand gameplay" `
  -testResults "TestResults.xml" `
  -logFile "TestLog.txt"
```

### Code Style & Quality Verification

```powershell
dotnet format "SandGameplay.Runtime.csproj" --verify-no-changes --no-restore
dotnet format "SandGameplay.Tests.EditMode.csproj" --verify-no-changes --no-restore
```

---

## Building the Android APK

1. In Unity, open **File → Build Profiles**.
2. Ensure **Android** is the active build profile.
3. Verify settings:
   - **Target Architecture**: ARM64
   - **Scripting Backend**: IL2CPP
   - **Orientation**: Portrait
   - **Package ID**: `com.wrdeadman10.sand.gameplay`
   - **Icon**: App icon assigned across all density tiers
4. Click **Build** and choose output destination `ColorSand.apk`.

---

## Third-Party Assets & Attribution

| Asset / Package | Author | License / Usage |
|---|---|---|
| **Lilita One Font** | Juan Montoreano | SIL Open Font License 1.1 (`Assets/_Project/Art/Fonts/LilitaOne-OFL.txt`) |
| **DOTween** | Demigiant | Free edition included under `Assets/Plugins/Demigiant/DOTween/` |
| **Sprites & Audio** | Original / Procedural | Synthesized in-repo for this project |

---

## Architecture & Design Standards

This codebase strictly implements:
- **SOLID & Clean Architecture**: Composition over inheritance, thin MonoBehaviours, separated simulation data and view presentations.
- **Coding Standards (`AGENTS.md`)**: No `var`, explicit types only, no shorthand `new()`, K&R brace formatting, zero allocations in simulation tick loops, and comprehensive architectural documentation maintained under `docs/`.
