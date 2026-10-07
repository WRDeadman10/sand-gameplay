# Color Sand — Sand Gameplay Prototype

A playable Unity prototype of the core loop of **Color Sand: Fill The Blocks**, built for a 2-day practical test. The player drags a box under a tank of colored sand. The sand pours in from directly above the box, the tank slumps grain by grain, and the box fills with a live percentage. A full box pops, praises the player, and flies into the tank. Play Again restarts instantly.

## Requirements

| Item | Version |
|---|---|
| Unity | **6000.3.9f1** (Unity 6.3 LTS) with Android Build Support (SDK, NDK, OpenJDK) |
| Render pipeline | Universal RP 17.3.0 (Mobile quality level on Android) |
| Packages used by the game | Input System 1.18.0, uGUI 2.0.0 (includes TextMeshPro), Universal RP 17.3.0 |
| Packages used by tests | Test Framework 1.6.0 |

## How to run

1. Open the project folder in Unity 6000.3.9f1.
2. Open **`Assets/_Project/Scenes/Game.unity`**. It is the only scene in the build.
3. Press **Play**. For phone framing, set the Game view to a portrait size such as **1080×2400** (Galaxy S21).

Android build: *File → Build Profiles → Android → Build*. The platform is already Android, with package id `com.wrdeadman10.sand.gameplay`, IL2CPP, ARM64, portrait, and min API 25.

## Controls

| Input | Action |
|---|---|
| Touch (Android) / left mouse (Editor) | Press the box, drag, release. It snaps to the grid. |
| Dock | Drag the box up until its top touches the tank. **Sand pours in.** |
| Steer the flow | Slide the box along the tank to drain the side piles. Pull it away to stop the flow; the box keeps its sand. |
| Restart | ↻ button (top-right) at any time, or **Play Again** on the completion screen |

## How the sand works

- **Cellular automaton.** One cell per grain, stored as a `ushort` (occupied bit plus color and tone). Each tick, rows are processed bottom to top: a grain falls if the cell below is empty, otherwise it slides diagonally at random, otherwise it rests. Bottom-up order guarantees a grain moves at most once per tick, and the scan direction alternates every tick so piles don't lean. Piles settle at about 45°, like the reference.
- **Two grids.** The tank has its own grid (144×48 grains) and so does the box interior (40×40 grains plus 3 rim rows). The box's sand lives in box-local space, so dragging never disturbs it.
- **The path (outlet).** When the box's top is flush with the tank's bottom, `DockResolver` finds the tank columns above the box opening. Each tick, `SandOutlet` removes accepted grains from the tank's bottom row in that span and queues them, and they spawn in the box's rim row. Moving the box moves the drain point; leaving closes it. The color rule (`MatchColor`) is in place: a grain of another color would block its column, as in the reference.
- **Fill accounting.** Capacity is the number of acceptable grains in the tank. The box interior has fewer cells than the tank has grains, so visual grains spawn by ratio and keep their color and tone. From 97% full while docked, the last stranded grains are pulled in gradually. At 100% the interior is topped up so it reads as exactly full.
- **Rendering.** Each grid writes into a point-filtered `Texture2D` (`GetPixelData`, no allocation) through a color × tone lookup table, and only when the grid changed.
- **Timing and cost.** The simulation runs at a fixed 120 ticks/s (max 4 per frame), and settled grids sleep. In the editor it measured about **0.03 ms/frame** for the simulation and **0.04 ms/frame** for texture upload. There are no per-grain GameObjects, and `SandGrid.Step` allocates 0 bytes (covered by a unit test).

## Project layout

```
Assets/_Project/
  Scenes/Game.unity              the game scene
  Scripts/                       SandGameplay.Runtime assembly
    Sand/        grid, cell encoding, RNG, palette, settings, texture view
    Source/      tank (SandSource) and its fill patterns
    Transfer/    dock resolver, outlet, transfer queue, color rule
    Container/   box logic (SandContainer), interior spawner, box piece and visuals, % label
    Board/       board grid and view
    Interaction/ pointer drag (Input System "Gameplay" actions)
    Simulation/  fixed-tick clock, per-tick flow, runner
    Presentation/ camera fit, completion sequence, praise popup, pour spray
    UI/          HUD, Level Completed panel, safe area
    Core/        LevelController (composition root and state machine)
    Data/        LevelConfig (ScriptableObject)
  Tests/EditMode/                SandGameplay.Tests.EditMode (64 tests)
  Data/                          Level_Default, DefaultPalette, DefaultSimulation assets
  Art/                           generated sprites, tank pattern, font, materials
  Prefabs/                       Tank, Container
Tools/ArtGen/generate_sprites.py  regenerates every sprite (Python + Pillow + numpy)
docs/                            overview, architecture, roadmap, tasks, decisions, handoff
```

## Tests

Run *Window → General → Test Runner → EditMode → Run All*. There are 64 tests covering:
- the automaton: fall, slide, conservation, determinism, sleep/wake, zero allocation
- the outlet and color rule, the transfer queue, fill accounting, and the interior spawner
- docking, board constraints, fill patterns, the fixed-tick clock
- the end-to-end tank-to-box flow

Code formatting follows `.editorconfig` and is checked with:

```powershell
dotnet format "SandGameplay.Runtime.csproj" --verify-no-changes --no-restore
dotnet format "SandGameplay.Tests.EditMode.csproj" --verify-no-changes --no-restore
```

## Third-party assets

| Asset | License |
|---|---|
| **Lilita One** font (Juan Montoreano, from the Google Fonts repository) | SIL Open Font License 1.1, see `Assets/_Project/Art/Fonts/LilitaOne-OFL.txt` |
| All sprites and the tank pattern | Original; generated by `Tools/ArtGen/generate_sprites.py` |

No Asset Store content and no third-party runtime code. The manifest also contains editor tooling that the game does not use: `com.coplaydev.unity-mcp` (Unity MCP bridge) plus Cinemachine, glTFast, ProBuilder and VFX Graph.

## Known limitations

- One tank and one box, by design (test brief). Multi-box puzzles, obstacles and boosters are out of scope.
- Tested in the Unity Editor. The Android APK, on-device profiling (Samsung Galaxy S21) and the demo video are the next steps.
- No audio yet.
