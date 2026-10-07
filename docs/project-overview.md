# Project Overview — Color Sand Prototype

> A Unity practical implementation replicating the sand puzzle gameplay from **Color Sand: Fill The Blocks** and the `gpay.mp4` reference video.
> **Status (2026-10-07):** **Full 50-level puzzle game playable in Unity Editor.** Extended beyond the initial 1-box prototype into a multi-container, multi-color puzzle experience with 50 audited levels, funky Main Menu, 1–50 scrollable Level Select with progress badges, in-game Pause modal with simulation freeze, universal audio Settings modal, and cute looping background music. See [ai_handoff.md](ai_handoff.md) for the current state.

## Doc map

| Doc | What it answers |
|---|---|
| [project-overview.md](project-overview.md) | What the test asks for, what the reference game does, what we will build, and how we know we are done (this file). |
| [architecture.md](architecture.md) | How it is built: sand simulation, rendering, interaction, fill/completion, UI, data, performance, Android build. |
| [roadmap.md](roadmap.md) | Milestones M0–M9 with scope, acceptance criteria, estimates, the 2-day schedule, the cut list, and risks. |
| [tasks.md](tasks.md) | Task checklist. Task IDs map to milestones. |
| [decisions.md](decisions.md) | Decision log, including decisions still waiting on the user. |
| [ai_handoff.md](ai_handoff.md) | Session handoff: current state, files touched, next task, open questions. |

Source material in the repo root:

| File | Content |
|---|---|
| `scopdoc.pdf` | The test brief: 2 pages covering requirements, deliverables, evaluation criteria, and the checklist. |
| `gamaeplay.mp4` / `gpay.mp4` | Reference gameplay recordings demonstrating multi-container and multi-color puzzle gameplay. |
| `AGENTS.md` | Agent behaviour, C# style, Unity rules, docs workflow. |
| `CODE_FORMAT_STANDARDS.md` | C# formatting and naming rules, plus the `dotnet format` validation steps. |

---

## 1. Context

- **What this is:** a practical project replicating the puzzle mechanics of *Color Sand: Fill The Blocks* (Google Play id `puzzle.color.sand.blocks`) and the `gpay.mp4` gameplay video.
- **Objective:** Build a complete, polished, and performant Unity game demonstrating falling sand simulation, multi-container interaction, audio/music, level progression, and mobile UX.
- **Scope evolution:** Initially prototyped as a 1-container vertical slice (M0–M6), the project was subsequently scaled up to full production parity: 50 handcrafted puzzle levels, multi-container docking, wall blockers, White sand containers, funky Main Menu, Level Select grid, in-game pause menu, and audio settings.

## 2. Scope digest (from `scopdoc.pdf` + user requirements)

### 2.1 Requirements matrix

| Area | Requirement | How the project meets it |
|---|---|---|
| Platform | Unity project + Android APK | Unity 6000.3.9f1, URP, IL2CPP/ARM64 APK (see [architecture.md §19](architecture.md#19-android-build)) |
| Core gameplay | Sand visibly falls/flows from source toward containers | Grains fall and slump inside tank, cross junction, and rain into docked matching containers |
| Sand | Colored grains behave and look like reference | CPU cellular-automaton grid, one cell per grain, per-grain tone noise, rendered to point-filtered texture |
| Interaction | Touch/mouse input controlling flow and container positioning | Drag containers on board grid. Docking under matching tank columns opens flow; undocking pauses flow |
| Multi-container | Support multiple containers per level with obstacles | Dynamic container spawning per `ContainerDef`, color-partitioned capacity, multi-box board collision |
| Levels | 50 progressive puzzle levels matching `gpay.mp4` | 50 validated levels in `Resources/Levels/` with 0 obstacle blocker collisions and 100% color coverage |
| UI & Navigation | Main Menu, Level Select, Pause, Settings | Funky Main Menu, 1-50 scrollable grid with cleared badges, Pause modal with physics freeze, audio Settings modal |
| Audio & Music | Cute background music and SFX | Looping cozy BGM (`bgm_cute_loop.wav`), full SFX suite, volume sliders and mute toggles with PlayerPrefs persistence |
| Performance | Smooth 60 FPS on mobile | Zero per-grain GameObjects, fixed-tick sim sleeping when settled, zero steady-state GC |

### 2.2 What the candidate should build (brief items 1–6)

1. **Sand Source:** a top/side source similar in concept to the reference; sand released continuously or in controlled batches.
2. **Sand Simulation:** convincing falling/flowing sand. Particles, a custom grid, ECS/DOTS, and compute shaders are all acceptable. The brief cares about the result and reasonable performance.
3. **Player Interaction:** simple touch/mouse input for the key action that controls where/how sand reaches the container.
4. **One Bottom Container:** receives sand and progressively fills.
5. **Fill/Complete Animation:** once sufficiently filled, a satisfying completion (scale, particles, bounce, color change, or similar).
6. **Restart:** a simple restart/replay option for repeated demos.

### 2.3 Suggested split (brief)

| Day | Focus |
|---|---|
| Day 1 | Project + mobile scene, sand simulation/flow, source, collision/settling, player interaction. **Core loop playable before polish.** |
| Day 2 | Container filling, completion animation, restart, visual polish, performance cleanup, Android testing, final APK + video. |

### 2.4 Deliverables

| Deliverable | Detail |
|---|---|
| APK | Working Android APK that installs and runs. |
| Video | 30–90 s screen recording: **start → interaction → sand flow → container filling → completion animation → restart**. |
| Unity project | Zipped source project with all required scripts/assets. |
| README | Unity version, how to run the scene, controls, brief explanation of the sand implementation, plus any third-party packages. |

### 2.5 Evaluation criteria

| Category | What they look for |
|---|---|
| Core Gameplay | Convincing sand flow/settling and a reliable main interaction |
| Unity Skills | C# quality, components, prefabs, input, physics/simulation approach, general Unity knowledge |
| Animation & Polish | Smooth motion, fill animation, completion feedback, transitions, overall feel |
| Performance | Stable runtime performance and sensible handling of grain counts |
| Code Quality | Readable, maintainable code and sensible architecture |
| Android Build | APK installs, launches, and plays correctly |
| Problem Solving | Effective prioritisation of required features within 2 days |

### 2.6 Rules

- 2 days. Do not recreate the production game.
- The multi-block puzzle/level system is explicitly out of scope.
- Third-party packages are allowed if legally usable **and listed in the README**.
- AI assistance is allowed, but the candidate must understand and be able to explain the code. That is one reason [architecture.md](architecture.md) documents every system in plain terms.
- Prioritise the working core loop over menus, level systems, or extras.

### 2.7 Submission checklist (brief)

- [ ] APK installs and launches on Android
- [ ] Sand source works
- [ ] Sand visibly falls/flows and settles
- [ ] Player interaction works
- [ ] Single bottom container receives sand
- [ ] Container visibly fills
- [ ] Completion animation works
- [ ] Restart/replay works
- [ ] Video demonstrates the full loop

---

## 3. Reference game analysis (from `gamaeplay.mp4`)

The recording covers Levels 1–12. The orange circle with a running clock in the top-left corner belongs to the **screen recorder**, not the game UI. The overview times below are approximate (±1 s). Times marked *precise* come from dense 2–4 fps frame extraction.

### 3.1 Genre and core fantasy

The game is a **color-sorting puzzle** that combines a falling-sand simulation with a sliding-block board. Tanks at the top hold colored sand. The player drags open-top boxes on a grid until they sit directly under the tank, and the sand of the box's color pours in. The satisfaction comes from the continuous pour, the rising percentage, and the pop-and-praise when a box fills.

### 3.2 Screen layout (portrait)

```
┌──────────────────────────────────────┐
│ (coins 400 +)   [04:00]          ⚙   │  HUD: coins with "+", countdown timer pill,
│                 Level 1              │       level label, settings gear
│                                      │
│   ┌──────────────────────────────┐   │  Sand tank(s): glass frame with a thick
│   │ ▓▓▓▓ colored sand ▓▓▓▓▓▓▓▓▓▓ │   │  bevelled bottom lip, attached to the
│   └──────────────────────────────┘   │  top edge of the board
│   ┌──┬──┬──┬──┬──┬──┐               │
│   │  │▉▉│  │  │  │  │  ← dock row   │  Board: dark rounded panel with a grid
│   ├──┼──┼──┼──┼──┼──┤               │  of darker rounded cells
│   │  │  │  │▉▉│  │  │               │
│   ├──┼──┼──┼──┼──┼──┤               │  Boxes: chunky 3D open-top pieces with
│   │  │  │  │  │  │  │               │  a "0%" pill, in polyomino shapes
│   └──┴──┴──┴──┴──┴──┘               │
│                                      │
│      (🔒)      (🔒)      (🔒)         │  Booster bar: 3 slots (locked until earned)
└──────────────────────────────────────┘
  Background: blurred desert/Egypt theme (pyramid, cat statue, rocks) in purple tones
```

### 3.3 Core loop, step by step (Level 1 tutorial, *precise* 0:00–0:16)

| Time | What happens |
|---|---|
| 0:00–0:06 | Tutorial hint: "**Move the Box to the Container**". A hand pointer swipes up on the white box, and the target white tank has a white outline. The game calls the tank the "Container" and the movable piece the "Box". |
| 0:06.5–0:07.0 | The player swipes up. The box slides cell by cell (~0.5 s, eased) and stops flush under the white tank. |
| 0:07.5 | Box at **2 %**: the pour starts the moment the box touches the tank. |
| 0:08.0 | **24 %**: the tank's sand surface drops with a ragged top, and grains rain down inside the box as a sparse cloud. |
| 0:08.5 | **78 %**: the tank is nearly empty and the box is mostly full. |
| 0:09.0 | **100 %**: about 2 s from dock to full. |
| 0:09.5 | The full box **shrinks to ~50 % and moves up into the emptied tank**. |
| 0:10.0–0:10.5 | It disappears in a **"NICE!" praise burst** with orange/white stars, and the next tank (red) brightens as the next hint. |
| 0:10.5–0:11.5 | The red box is selected (white outline), dragged up following the finger, and docks: **29 %**. |
| 0:12.0 → 0:13.0 | 62 % → 99 % → **100 %**. The box lifts and pops toward the tank. |
| 0:13.5–0:15.5 | "**AWESOME!**", then "**Level Completed!**": dimmed screen, bouncy title, coin stack (40) with light rays, unlock progress bar 0 % → 17 %, and coins flying to the counter. |

### 3.4 Sand behaviour

- **Grains are pixel-sized cells** with per-grain tone noise, which gives the glittery look. A 6-column tank is roughly 150–200 grains wide, i.e. about 25–33 grains per board cell.
- **Gravity rules:** grains fall straight down, slide diagonally off edges, and form piles at roughly 45° (angle of repose). When grains leave, the layers above slump into the gap and flow.
- **Patterns deform as they drain.** Tanks start as solid colors, layers, diagonal bands, or pixel art: a heart (Level 6, ~2:02), a pyramid (Level 7, ~2:37), a sun (Level 8, ~3:02), checkerboards (Level 12, ~6:07). The art visibly collapses as sand drains, which is a big part of the visual appeal.
- **The drain point is local.** Sand leaves the tank only through the part of the tank's bottom edge that sits directly above a docked box's opening. Everywhere else the bottom edge is a floor.
- **Color filtering** (Level 3, *precise* 0:39–0:54). Only grains of the box's color pass through. A grain of another color sitting over the opening **blocks** the flow. Example: at 0:49–0:50 the red box sits under green sand and stays stuck at 51 %. Moved under red sand at 0:51, it climbs to 100 %. This is the puzzle: clear the covering color with another box first.
- **Flow is fast.** A 1×1 tank empties into a 1×1 box in about 1.5–2 s. A T-shaped box went 24 % → 84 % in 0.75 s (Level 2, *precise* 0:30.0–0:30.75).
- **Moving a box changes where it drains, and moving it away stops the flow.** A partially filled box keeps its sand and its % and can re-dock elsewhere. At 0:39–0:40 the yellow box was slid from the right column to the middle while filling. At 0:29.5 a yellow box was moved away at 86 % and the flow paused. **This "open/close the path" behaviour is exactly the interaction the brief asks for.**
- **Remaining sand stays put.** Once the sand under the opening is gone, side piles remain at the angle of repose. The player has to slide the box under each pile to drain it (Level 7, ~2:47: two red side piles remain after the middle drained).
- **Stranded grains get mopped up.** At 99 % a few unreachable grains were absorbed and the box completed (Level 3, 0:42–0:43).

### 3.5 Boxes and the board

- **Shapes:** polyominoes: 1×1, 1×2, 2×1, 1×3, 2×2, L, T, and U-like pieces.
- **Look:** chunky bevelled 3D plastic in the sand color, with a darker interior and a small white `n%` pill in the interior's bottom-left corner. The selected box gets a white outline.
- **Movement:** free drag that follows the finger, blocked by other pieces, walls, and board edges, with a snap to cells on release. It pours while being slid along the dock row.
- **Completion of a box:** scale pop, shrink, fly into the tank, star burst, and a praise word (**NICE! / GREAT! / AWESOME! / PERFECT!**). The vacated cells free up for other pieces.

### 3.6 Systems outside our scope (observed, not built)

| System | Observed |
|---|---|
| Multiple tanks / stacked tanks | From Level 4 (~1:02). Level 12 (~6:07) stacks two tanks with ice between them. |
| Static walls | Light-blue slab cells (Level 8, ~3:02). |
| Ice blocks | Numbered blocks that count down by one per completed box and melt at 0 (Levels 7, 9, 10, 12; 10 → 1 in Level 12). |
| Boosters | **Extra Time** (hourglass, pause the timer for 10 s, unlocked ~1:57), **Break box** (star wand, split a box into smaller ones, ~2:57), **Fill box** (bucket, instantly fill a box, ~5:17). |
| Timer | Per-level countdown: 4:00 early, then 3:00, then 2:00. The HUD turns red at Level 12. |
| Economy and meta | Coins (+40 per level), feature-unlock progress bar, Level Completed with **Continue / x2 Coins** (rewarded ad), world-map island transition (~4:32). |
| Settings | Popup with Music / Sound / Vibration toggles and **Home / Restart** buttons (~0:22). |

### 3.7 Look and feel

- **Palette:** saturated candy colors (red, blue, cyan, green, yellow, orange, pink, white, grey) on a dark desaturated purple board, against a blurred, low-contrast themed background.
- **Motion:** snappy and springy. Drags ease in and out, pops overshoot, text bounces, and praise words burst with stars.
- **Feedback stack per box:** continuous pour, rising %, pop, praise word, particles. Per level: dim, bouncing title, reward burst.

### 3.8 Video timeline (approximate)

| ≈ Time | Level | Event |
|---|---|---|
| 0:00–0:17 | 1 | Tutorial with white and red 1×1 tanks and boxes; first Level Completed (coins 400 → 440). |
| 0:22 | 2 | Settings popup opened, then closed. |
| 0:27–0:33 | 2 | Diagonal blue/yellow tank, yellow 2×2 box and blue T-box; yellow paused at 86 % while the blue drains; GREAT!. |
| 0:37–0:57 | 3 | 4-layer tank (green/red/blue/yellow) with four boxes; full color-filter puzzle; PERFECT!; Level Completed. |
| 1:02–1:22 | 4 | Two tanks (red, blue) plus yellow side tanks; walls of other pieces; Level Completed. |
| 1:27–1:52 | 5 | L-shaped tanks, L-shaped boxes; "Continue / x2 Coins" screen. |
| 1:57 | — | **EXTRA TIME** booster unlocked. |
| 2:02–2:32 | 6 | Heart pixel-art tank; time-freeze ice vignette; PERFECT!/AWESOME!. |
| 2:37–2:52 | 7 | Red/white pyramid tank; ice blocks 1–4; side piles drained one by one. |
| 2:57 | — | **Break box** booster unlocked. |
| 3:02–3:32 | 8 | Sun pixel art; static wall; box broken into 1×1 pieces. |
| 3:37–4:27 | 9 | Two multi-color pixel tanks; many pieces; ice. |
| 4:32 | — | World-map island transition. |
| 4:37–5:12 | 10 | Wide puzzle-piece pattern tank; ice; Level Completed. |
| 5:17 | 11 | **Fill box** (bucket) booster tutorial: "Instantly fill a box!". |
| 6:07–7:00 | 12 | Stacked tanks with ice 10 → 1 in between; 2:00 timer; red HUD. Recording ends mid-level. |

---

## 4. What we will build

### 4.1 Pitch

> One tank full of colored sand sits on top of a small board. The player drags a single open-top box across the board and docks it under the tank. Sand pours in from directly above the box while the rest of the tank visibly slumps toward the hole. The player slides the box along the tank to drain each side pile. When the box is full it pops, praises the player, flies into the tank, and the Level Completed screen offers Play Again.

### 4.2 Reference element → prototype

| Reference element | Prototype | Notes |
|---|---|---|
| Sand tank pre-filled with colored sand | **Included**, 1 tank | 6×2 cells across the top of a 6-column board (data-driven) |
| Falling-sand behaviour (fall, slide, pile, slump, pattern deformation) | **Included** | CPU cellular automaton ([architecture.md §4](architecture.md#4-sand-simulation-core)) |
| Drag a box under the tank to start pouring | **Included** | Core interaction |
| Local drain: sand leaves only above the box's opening | **Included** | Sliding the box along the dock row drains different areas |
| Moving the box away stops the flow; the box keeps its sand | **Included** | "Open/close the path" |
| Color rule (box accepts only its color) | **Included**, default pending **D-005** | Options A/B/C in [decisions.md](decisions.md#d-005) |
| `n%` label and sand piling inside the box | **Included** | Sand inside the box is simulated, not a fill bar |
| Box completion: pop, praise word, star burst, fly into tank | **Included** | |
| Level Completed screen | **Included**, simplified | Dim, bouncing title, burst, **Play Again** (no coins/economy) |
| Restart | **Included** | HUD button at any time, plus Play Again on the end screen |
| Tutorial hint | **Included**, light | Hint text plus a tank outline pulse at start |
| Multiple boxes / sliding-block puzzle | Excluded (brief) | |
| Static walls | Optional | Supported by the board data; none in the default level |
| Ice blocks, stacked tanks, boosters, timer | Excluded | |
| Coins, unlock progress, map, settings, ads | Excluded | |
| Audio / haptics | Stretch | Only if time remains after M7 |

### 4.3 Player experience (this is also the demo-video script)

1. **Start:** the board fades in. The tank is full of sand, the box sits near the bottom of the board, and a hint reads "Drag the box under the sand!" while the tank outline pulses.
2. **Interaction:** the player presses the box (white outline, slight lift) and drags it up. It follows the finger, stays inside the board, and slides along edges.
3. **Sand flow:** when the box's top meets the tank's bottom edge, the path opens. Sand directly above the opening pours in, the column above drops, the neighbours slump diagonally into the crater, and the pattern deforms.
4. **Container filling:** grains rain into the box and pile up; the `%` label climbs. The player slides the box along the dock row under the remaining side piles. Pulling the box away pauses the flow and the box keeps its sand.
5. **Completion:** at 100 % the box squash-pops with a flash, a praise word bursts with stars, and the box flies into the tank and pops. Then the "Level Completed!" panel bounces in with a celebration burst and the **Play Again** button.
6. **Restart:** Play Again (or the HUD restart button at any time) instantly resets to step 1, with no scene reload.

### 4.4 Controls

| Platform | Action |
|---|---|
| Android | One finger: touch the box, drag, release. Tap buttons. |
| Editor / PC | Left mouse: press on the box, drag, release. Click buttons. |
| Any | Restart: HUD button (top-right) or **Play Again** on the completion screen. |

### 4.5 Non-goals

No level progression, timer, economy, boosters, obstacles beyond optional walls, menus, settings, saving, ads, analytics, or localisation.

---

## 5. Definition of done

**Scope checklist:** all nine items in §2.7, each with evidence (device run + video).

**Quality bars:**

| Bar | Target |
|---|---|
| Frame rate | 60 FPS target on the test device (≥ 55 FPS average, no frame > 33 ms during a pour). Device TBD, see open questions. |
| GC | 0 B managed allocation per frame in steady-state play (Profiler, development build). |
| Stability | No errors or exceptions in the Console or `adb logcat -s Unity` across 10 consecutive play → complete → restart cycles. |
| Build | Release APK installs and launches on Android 7.1+ (minSdk 25, the current setting), portrait-locked, respects the safe area on tall/notched screens. |
| Video | 30–90 s showing the full loop in the order of §4.3. |
| README | Unity version, how to run, controls, sand implementation summary, third-party list. |
| Code | Passes `dotnet format --verify-no-changes` on our scope; follows AGENTS.md and CODE_FORMAT_STANDARDS.md; EditMode tests pass. |

---

## 6. Project baseline (as found on 2026-10-07)

| Item | Value | Action |
|---|---|---|
| Unity | **6000.3.9f1** | — |
| Template | URP "blank" (Universal 3D): `SampleScene` with a perspective camera, directional light, and global volume (Bloom, Vignette, Tonemapping) | New `Game` scene with an orthographic camera (M0) |
| Render pipeline | URP 17.3.0, quality levels **Mobile** (Android default) and **PC** | Mobile asset: Render Scale **0.8 → 1.0** (crisp pixel sand); shadows and post-processing not needed |
| Input | Input System 1.18.0; *Active Input Handling* = Input System only; `Assets/InputSystem_Actions.inputactions` (Player + UI maps) | Add a `Gameplay` map (D-011) |
| UI | com.unity.ugui 2.0.0, which **includes TextMeshPro** | Import TMP Essential Resources (M0) |
| Tests | Test Framework 1.6.0 | EditMode test assembly (M1) |
| Other template packages | Visual Scripting, Multiplayer Center, AI Navigation, Collab Proxy, Timeline | Removal pending user approval (D-017) |
| Editor tooling | `com.coplaydev.unity-mcp` (Unity MCP bridge, git URL on the **unpinned `#main` branch**), plus its dependency `com.unity.nuget.newtonsoft-json` 3.2.2. Added to the manifest during the analysis session, outside the agent's actions. | Keep for development; pin or remove before submission (D-020) |
| Orientation | `defaultScreenOrientation` = Auto Rotation (all four allowed) | **Portrait** lock (M0) |
| Identity | Company `DefaultCompany`, product `sand gameplay`, Android id `com.UnityTechnologies.com.unity.template.urpblank` (template id) | Real values pending user (D-016) |
| Android | IL2CPP ✔, ARM64 ✔, minSdk 25, target SDK = highest installed, Graphics APIs Vulkan + OpenGLES3, Linear color space | Keep |
| Android toolchain | Unity Android module with SDK, NDK, and OpenJDK present; `adb` and `scrcpy` on PATH; no device attached during analysis | Connect a device for M4/M7 |
| .NET tooling | `dotnet` 9.0.304 with `dotnet format` | Verify against Unity's generated csproj (M0) |
| Git | Branch `main`, everything staged, **no commits**; media is LFS-tracked per `.gitattributes`; `Builds/`, `*.apk`, `*.aab` ignored | **We never commit** (D-001) |
| Missing | **`.editorconfig`** (referenced by AGENTS.md and CODE_FORMAT_STANDARDS.md), `README.md`, `docs/` (now created) | `.editorconfig` pending user (D-013) |

---

## 7. Working agreements

- **AGENTS.md** governs behaviour: senior Unity/C# engineer, production-ready code only, match project patterns, and the docs workflow (read architecture, roadmap, and handoff before coding; update tasks and decisions during; update the handoff after).
- **CODE_FORMAT_STANDARDS.md** governs code: no `var`, no target-typed `new()`, no expression-bodied members, K&R braces, explicit access modifiers, 4-space indent, CRLF, UTF-8 BOM for `.cs`.
- **Never commit to git , Only Antigravity is allowed to commit and push** (user instruction, D-001). The format/verify steps that the standards place "before commit" are run, and then the change is handed back to the user.
- **Inconsistencies found in the standards** (tracked in [decisions.md](decisions.md)):
  1. `.editorconfig` is referenced as the primary rule source but is not in the repo (D-013).
  2. Private-field naming differs: AGENTS.md says `camelCase`, CODE_FORMAT_STANDARDS.md says `_camelCase` for non-serialized fields (D-012, provisional `_camelCase`).
  3. CODE_FORMAT_STANDARDS.md's validation commands point at another project (`indoor-navigation-unity`). Adapted commands are in [architecture.md §20](architecture.md#20-coding-standards-applied).
  4. AGENTS.md says "no unnecessary comments", while the organisation guidance asks for comments that aid clarity. Resolution: XML doc comments on public APIs, and inline comments only where the logic is non-obvious (e.g. simulation rules).

## 8. Open questions

All are tracked in [decisions.md](decisions.md) with a recommended default:

1. **D-005:** what is in the tank, and which sand does the single box accept? (Recommended: **A**, a single-hue tank in multiple tones.)
2. **D-013:** will the user provide the original `.editorconfig`, or should one be generated from AGENTS.md + CODE_FORMAT_STANDARDS.md?
3. **D-012:** confirm `_camelCase` for private non-serialized fields.
4. **D-016:** company name and Android package id (e.g. `com.<company>.colorsand`).
5. **D-017:** may the unused template packages and the `TutorialInfo/` + `Readme.asset` template files be removed?
6. **Test device:** which Android phone (model / Android version) defines the performance target?
7. **D-018:** OK to use self-made sprites and an OFL-licensed Google Font? Downloading the font needs approval.
8. **D-020:** should the `com.coplaydev.unity-mcp` editor package be removed from the submitted project, or pinned and listed in the README?
