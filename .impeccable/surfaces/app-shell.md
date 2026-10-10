---
version: 1
slug: "app-shell"
primary_target: "app-shell"
related_targets: ["BERNEOXGame/Assets/Scripts/Hub/BerneoxHubController.cs"]
---

# BERNEOX App Shell

## Scope and visitor mode
First-run splash and the Android game-platform home/catalog. Operate surface in Unity 6 UGUI, portrait hub; Zombie Survival keeps its separate landscape gameplay requirement.

## Audience, job, action, proof, constraints
For the confirmed 6–30 audience opening BERNEOX to choose a game. The primary action is to explore the featured Zombie Survival prototype; other named games are visibly marked as planned. Product evidence is the project itself, not online services: do not invent profile data, COINS balance, rewards, login, or playable content. Keep 48 dp touch targets, safe areas, strong contrast, dark/light themes, and an in-app reduced-motion option. No accounts or network systems in this shell.

## Chosen direction and memorable moment
Chosen direction: **La baliza nocturna**, assigned by direction seed `6902625e` and explicitly selected as `assigned`. A violet emergency beacon links a restrained survival forest to BERNEOX’s shared game destinations. Named raises: variable-font legibility (letters assemble as one readable word), Labanotation timing (one authored rhythm), botanical scale (catalog objects share framing), Crouwel grid (stable alignment). Keep the zombie setting in the featured game artwork; other planned games retain distinct forms and names. Memorable moment: seven BERNEOX letters enter from alternating screen edges and meet before a beacon pulse, then resolve into the home surface.

## Direction contract

### THESIS
A violet night beacon turns the first app opening into a clear signal toward a growing multi-game library; the category-default static launcher is refused.

### OWN-WORLD
Night-fog surfaces, satin dark metal, a committed signal-violet palette (`#100A20`, `#6E35DD`, `#BDA2FF`, `#D9F2CC`) and a quiet modular grid. The light theme maps the same named roles to pale lavender surfaces and deep purple text. Avoid decorative neon halos and noisy arcade effects.

### STORY
The player understands that BERNEOX contains several different games, sees Zombie Survival first, and can inspect its prototype status. Future games are labeled as planned. COINS and online/profile data stay absent until real services provide them.

### FIRST VIEWPORT
A portrait screen: compact brand/profile header, large featured Zombie Survival region with an authored low-poly forest, a moving zombie silhouette and a violet beacon, then a horizontally browsable catalog of distinct game destinations. Bottom navigation stays visible. The featured action sits below its title, with game status visible before the action.

### FORM
Build path is code-led. Use Unity Canvas/UGUI with safe-area-aware anchors, consistent 48 dp minimum controls, and distinct layouts for the opening, featured game and catalog. The splash letters enter from alternating sides, meet legibly, then the beacon opens into the home; no forced extra spectacle on every control. This is the assigned direction, seed `6902625e`; the user chose `assigned`.

### FINISH
unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance

## Unresolved decisions
Final camera angle for Zombie Survival, exact prototype scope, authentication, offline behavior, age classification and minor-safety rules remain open. The first build may use code-authored geometry only; no generated or sourced raster is required.
