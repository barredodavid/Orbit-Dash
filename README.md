# Orbit Dash

A tiny hypercasual mobile game built in Unity 6 (URP 2D) as a portfolio piece: dodge asteroids by
switching between concentric orbit rings around a central sun, with one input — tap.

![gameplay GIF placeholder](docs/gameplay.gif)
*(insert a 10-15s gameplay GIF here before publishing the portfolio page)*

**Play it:** [itch.io link — insert after hosting the WebGL build]

---

## What this project demonstrates

Orbit Dash is deliberately small in scope so the code stays readable end to end, but it's built to
show a specific set of engineering habits recruiters look for in a Unity codebase:

- **Object pooling**, written from scratch (`Utils/ObjectPool.cs`) instead of relying on
  `Instantiate`/`Destroy` in a loop — obstacles are recycled every run.
- **ScriptableObject-driven configuration** (`Data/`): ring layout, obstacle behaviour and the
  difficulty ramp are all tunable assets, not hardcoded constants — a designer could balance the
  whole game without touching a script.
- **A small, explicit state machine** (`Core/GameManager.cs`) driving Menu / Playing / Paused /
  GameOver, instead of scattered booleans.
- **Decoupled communication via C# events** (`Core/GameEvents.cs`): gameplay systems (obstacles,
  score, audio, camera shake, particles, haptics) never hold references to each other or to the UI —
  they all react to the same small set of static events. Adding a new reaction to "player got hit"
  never requires touching the class that raises it.
- **The new Input System**, used both as a raw polled action (`Core/TapInputReader.cs`, works with
  touch/mouse/pen through the generic `Pointer` device) and through `InputSystemUIInputModule` for
  UI navigation.
- A UI layer built entirely from code (`UI/`, see [UI architecture](#ui-architecture-note) below) —
  no hand-tweaked Canvas prefab to keep in sync with the scripts.

## Gameplay

- A single tap/click switches the player to the next orbit ring (inner → middle → outer → inner…).
- Asteroids spawn at increasing frequency and speed the longer you survive.
- Score = 1 point per second survived. Top 5 scores are saved locally (PlayerPrefs, JSON).
- Menu → Play → Pause anytime → Game Over → Replay/Menu, all with a short fade transition.

## Project structure

```
Assets/_Project/
  Scripts/
    Core/        GameManager (state machine), GameEvents, GameState, TapInputReader, AudioPreferences
    Gameplay/     OrbitController, ObstacleSpawner, Obstacle, ScoreManager, CameraShake,
                  ImpactParticles, HapticsFeedback, PlayerMarker
    UI/           UIManager, MenuScreen, PauseScreen, GameOverScreen, HudView, SafeAreaFitter
    Data/         OrbitLayoutConfig, ObstacleConfig, DifficultyCurveConfig (ScriptableObjects)
    Audio/        AudioManager
    Save/         SaveSystem
    Utils/        ObjectPool<T>, UIFactory, RuntimeCircleSpriteFactory, PlaceholderCircleVisual
    Editor/       BuildScript (command-line WebGL/Android build entry points)
  Data/Configs/   The actual ScriptableObject asset instances (tune these in the Inspector)
  Prefabs/        Obstacle.prefab (pooled)
  Scenes/         Game.unity (single scene; UI/state fully code-driven, no scene reload needed)
```

## UI architecture note

The Canvas, EventSystem and every screen (Menu/Pause/GameOver/HUD) are built at runtime in code
(`UIFactory` + `UIManager`), not authored as a Canvas prefab in the Editor. This was a deliberate
trade-off for this project: it kept the whole UI reliably reproducible and scriptable end to end.
The cost is that it's less drag-and-drop than a typical Unity UI workflow — real art (see below) is
wired in by `Resources.Load` path convention rather than dragging assets onto Inspector fields.
Worth knowing if you extend this project.

## Art & audio

Player, sun, obstacles, buttons and icons use real art (Kenney, CC0); SFX use real clips too. None
of it is hand-tweaked per file in the Editor — a `AssetPostprocessor`
(`Editor/SpriteImportPostprocessor.cs`) auto-configures every texture under `Art/Resources/Sprites/`
as a Sprite (button sprites additionally get a 9-slice border so they don't distort at different
button sizes), and gameplay/UI/audio scripts resolve their asset by a `Resources.Load` path with a
procedural fallback if nothing's there. Concretely:

- `PlaceholderCircleVisual` (Player/Sun/Obstacle): tries `overrideSprite` (Inspector reference),
  then `resourceSpritePath` (a `Resources.Load<Sprite>` path), then falls back to a generated flat
  circle. Real art is wired today via `resourceSpritePath`.
- `AudioManager`: same pattern per clip (`tapClip`/`tapClipResourcePath`, etc.) - `musicClip` still
  has no resource art behind it (see below), so it silently stays silent.
- UI buttons/icons (`UIFactory`, `UITheme`): `CreateButton`/`CreateIconButton` load a background
  sprite (and, for icon buttons, an icon sprite) from a `Resources` path and fall back to a flat
  color if the sprite isn't found — so the UI never breaks even with art missing.

This means adding/replacing art going forward is just dropping a file at the right
`Assets/_Project/Art/Resources/...` (or `Audio/Resources/...`) path - no Inspector dragging, no
prefab/scene edits.

**What's in the project right now:**

| Use | File | Source | License |
|---|---|---|---|
| Sun | `Art/Resources/Sprites/Gameplay/sun.png` | [Planets](https://kenney.nl/assets/planets) (`planet08`, downscaled to 256px) | CC0 |
| Player | `.../Gameplay/player.png` | [Simple Space](https://kenney.nl/assets/simple-space) (`ship_E`) | CC0 |
| Obstacle | `.../Gameplay/obstacle.png` | Simple Space (`meteor_squareDetailedSmall`) | CC0 |
| Primary/secondary buttons | `.../UI/Buttons/button_*.png` | [UI Pack](https://kenney.nl/assets/ui-pack) (`button_rectangle_depth_gloss`, Blue/Grey) | CC0 |
| Pause / sound / home icons | `.../UI/Icons/icon_*.png` | [Game Icons](https://kenney.nl/assets/game-icons) | CC0 |
| Tap SFX | `Audio/Resources/SFX/tap.ogg` | UI Pack (`tap-a`) | CC0 |
| Dodge SFX | `.../SFX/dodge.ogg` | [Digital Audio](https://kenney.nl/assets/digital-audio) (`phaserUp1`) | CC0 |
| Collision SFX | `.../SFX/collision.ogg` | [Impact Sounds](https://kenney.nl/assets/impact-sounds) (`impactMetal_heavy_001`) | CC0 |
| Background music | `Audio/Resources/Music/theme.mp3` | [incompetech.com](https://incompetech.com/) - "Level Up!" by Kevin MacLeod | **CC-BY 4.0 - credit required, see below** |
| UI font | `Art/Fonts/Orbitron-VariableFont_wght.ttf` -> generated SDF asset | [Orbitron](https://fonts.google.com/specimen/Orbitron) (Google Fonts) | OFL |

All from kenney.nl were direct CC0 downloads (Sept 2026); itch.io asset pages (e.g. Tallbeard
Studios' CC0 loop bundle, considered for the music slot) block scripted downloads behind Cloudflare,
which is why the music came from incompetech.com instead.

**Required credit** (CC-BY, unlike everything else above which needs none) - keep this visible in
the portfolio page / itch.io description / in-game credits:
> "Level Up" Kevin MacLeod (incompetech.com)
> Licensed under Creative Commons: By Attribution 4.0 License
> http://creativecommons.org/licenses/by/4.0/

**Still not wired up:**
- **Impact particle texture** — `Gameplay/ImpactParticles.cs` still renders a flat white dot; assign
  a nicer texture/material to its `ParticleSystemRenderer` for a more convincing burst. The Unity
  Technologies [Particle Pack](https://assetstore.unity.com/packages/vfx/particles/particle-pack-127325)
  (free, Asset Store) works, but predates URP - re-assign Built-in-RP materials to a
  `Universal Render Pipeline/Particles/Unlit` shader after import.
- **Impact particle texture** — `Gameplay/ImpactParticles.cs` still renders a flat white dot; assign
  a nicer texture/material to its `ParticleSystemRenderer` for a more convincing burst. The Unity
  Technologies [Particle Pack](https://assetstore.unity.com/packages/vfx/particles/particle-pack-127325)
  (free, Asset Store) works, but predates URP - re-assign Built-in-RP materials to a
  `Universal Render Pipeline/Particles/Unlit` shader after import.

## Setup

1. Unity **6000.5.10f1** (Unity 6 LTS) or newer, with the **2D URP** template.
2. Open the project, let Unity import; if prompted, run
   `Window > TextMeshPro > Import TMP Essential Resources` once.
3. Open `Assets/_Project/Scenes/Game.unity` and press Play — mouse click doubles as tap in the Editor.

## Building

WebGL Build Support is installed; Android Build Support (SDK/NDK/OpenJDK) needs to be added via
Unity Hub before an Android build can run. `Assets/_Project/Editor/BuildScript.cs` exposes two
command-line entry points:

```
Unity.exe -batchmode -quit -projectPath . -executeMethod OrbitDash.EditorTools.BuildScript.BuildWebGL
Unity.exe -batchmode -quit -projectPath . -executeMethod OrbitDash.EditorTools.BuildScript.BuildAndroid
```

(Close the Editor first — Unity refuses a second instance on a project that's already open.) Output
goes to `Builds/WebGL/` and `Builds/Android/OrbitDash.apk`, both git-ignored.

For the portfolio: host the WebGL build on [itch.io](https://itch.io) (set it as an embedded HTML5
game) so it's playable directly from the portfolio page without installing anything, and keep the
APK as a downloadable link for a mobile demo.

## Design notes / known simplifications

- **"Dodge" SFX**: fires whenever an obstacle completes its lap without hitting the player, rather
  than a strict close-proximity check — see the comment in `ObstacleSpawner.HandleLapCompleted`.
- **UI raycast vs. gameplay taps**: `TapInputReader` checks `EventSystem.IsPointerOverGameObject()`
  before firing, so tapping the in-game Pause button doesn't also switch rings underneath it.
- Single-scene architecture: Menu/Playing/Paused/GameOver are UI overlays + enabled/disabled gameplay
  systems, not separate Unity scenes — avoids scene-load hitches for a game this small.
