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

## Current implementation

- The Unity 6 project contains a runtime-built BERNEOX launcher with an animated opening, home screen, featured game card, catalog, theme toggle, reduced-motion preference, profile placeholder, and settings information sheet.
- The launcher builds a stylized 3D night forest, violet signal beacon, zombie, and sample 3D objects for catalog entries. Motion can be reduced in settings.
- Catalog entries that are not playable are identified as planned. The Zombie Survival entry describes the prototype as unverified and not yet launched from the catalog.
- `BERNEOXHub.unity` is now the first enabled build scene. It contains the launcher controller and per-scene portrait orientation component; `SampleScene` is retained.
- Zombie Survival prototype generation now assigns landscape orientation to that scene while the launcher remains portrait.
- Zombie Survival movement uses camera-relative forward/right directions, and its follow camera stays behind the character as the character turns.
- Unity editor menu `BERNEOX > Crear inicio y catálogo` opens the existing hub scene, or creates it if missing.

## Checks and limitations

- `dotnet restore` and `dotnet build BERNEOXGame.slnx --no-restore`: passed, 0 warnings and 0 errors.
- `git diff --check`: passed.
- The Unity Editor Play preview has not yet been visually checked. Unity was blocked at its activation/licensing screen; accepting Unity's terms must be done by the user.
- This is still an early prototype. Online accounts, networking, currency/rewards, and the other catalog games are not implemented.

## Prioritized next tasks

1. Run `BERNEOXHub` in Unity and fix any visual or runtime issues found on the intended Android aspect ratios.
2. Verify and tune the Zombie Survival camera, touch joystick, movement, and landscape behavior; connect it to the launcher only when it is confirmed playable.
3. Add real art, sound, and accessibility polish after the first visual review.
4. Plan online services only after the offline prototype and product requirements are reviewed.
