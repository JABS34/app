# BERNEOX project progress

Updated: 2026-10-09

## Confirmed product decisions

- Android-first platform for multiple games.
- Zombie Survival is the first game: 3D and landscape while playing, with touch joystick controls.
- COINS remain internal virtual currency. No cash-out or gift-card redemption.
- Critical rewards require server-side validation before they are implemented.
- The launcher uses a mainly purple palette, with light and dark themes.
- The launcher opens with an animated BERNEOX wordmark, then shows the game catalog.
- Zombie Survival uses a third-person camera behind the character. Camera distance, height, and smoothing remain provisional until tested.
- Zombie Survival is a night-forest survival game structured around recurring hordes. Round one is easy for learning and gathering; later rounds grow gradually harder. Between hordes, players explore, collect ground resources, chop trees, craft, and prepare or repair shelter. The confirmed interval is 3 minutes between the end of one horde and the start of the next. The initial wait before round one remains provisional.

## Current implementation

- The Unity 6 project contains a runtime-built BERNEOX launcher with an animated opening, home screen, featured game card, catalog, theme toggle, reduced-motion preference, profile placeholder, and settings information sheet.
- The launcher builds a stylized 3D night forest, violet signal beacon, zombie, and sample 3D objects for catalog entries. Motion can be reduced in settings.
- Catalog entries that are not playable are identified as planned. The Zombie Survival entry describes the prototype as unverified and not yet launched from the catalog.
- `BERNEOXHub.unity` is now the first enabled build scene. It contains the launcher controller and per-scene portrait orientation component; `SampleScene` is retained.
- Zombie Survival prototype generation now assigns landscape orientation to that scene while the launcher remains portrait.
- Zombie Survival movement uses camera-relative forward/right directions, and its follow camera stays behind the character as the character turns.
- The Zombie Survival scene builder creates a provisional horde loop: 60 seconds of initial preparation, an easy first wave of three zombies, then a three-minute break after each cleared wave. Later waves add enemies and raise health and speed gradually. The HUD shows the round state/countdown. The player can attack using the touch button or Space in the Editor.
- Ground resources cover the five concept materials: wood, stone, fiber, scrap, and food. Trees can be chopped in three interactions and drop wood. One contextual button switches between `RECOGER`, `TALAR`, `FABRICAR`, and `ATACAR`; E performs the same interaction in the Editor.
- A 12-slot inventory supports resource stacks up to 30. Items that do not fit remain in the world. A temporary workbench enables the document's provisional bat and axe recipes during preparation; crafted weapons equip automatically and change combat values, and the axe doubles wood from trees. Inventory and crafting screens pause the game. Inventory is not persisted between sessions.
- The scene builder preserves `BERNEOXHub` and `SampleScene`, and opens the existing ZombiePrototype rather than replacing it.
- Unity editor menu `BERNEOX > Crear inicio y catálogo` opens the existing hub scene, or creates it if missing.

## Checks and limitations

- Script compilation and Unity Play verification for the current horde changes are pending. `dotnet` is not available in the current environment, and Unity was previously blocked at its licensing screen.
- A prior `git diff --check` passed before these horde changes.
- The Unity Editor Play preview has not yet been visually checked. Unity was blocked at its activation/licensing screen; accepting Unity's terms must be done by the user.
- This is still an early prototype. Online accounts, networking, currency/rewards, and the other catalog games are not implemented.

## Prioritized next tasks

1. Add a shelter players can build, reinforce, and repair between hordes.
2. Run `ZombiePrototype` in Unity to verify gathering, inventory limits, crafting, horde timing, enemy spawning, camera, and touch controls.
3. Tune round difficulty and connect Zombie Survival to the launcher after the mode has been visually checked and feels playable.
