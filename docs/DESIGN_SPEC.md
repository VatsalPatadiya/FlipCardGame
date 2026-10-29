# Eclipse — Card Game Design Handoff

> **Purpose of this file:** a complete brief for building a premium Unity mobile card game. It covers the game rules and the three confirmed card UI directions, with exact colors, fonts and measurements. Hand it to any developer or AI assistant to continue the work.

**Working title:** Eclipse (a placeholder; the final name is still to be decided)
**Engine:** Unity, mobile first (iOS and Android)
**Goal:** a double-sided "flip" card game with a premium, polished UI and a clean experience: no intrusive ads and no dark patterns.

---

## 0. Hard rules (read first)

1. **This is an original game.** It's inspired by the general "flip" card-game mechanic, but it must **not** use the name "UNO", "UNO Flip" or any Mattel branding, logos, card layouts, the tilted white oval, the red/yellow/green/blue look, or anything else that makes it look like UNO. Mattel actively enforces its trademarks. That applies to the store listing, keywords and screenshots too.
2. All card names, symbols, colors and art in this document are original. Keep them that way.
3. Only **three UI directions** are confirmed: **Premium Set**, **Glass Aurora** and **Prism Holo**. Other explored directions (Royal Velvet, Candy Pop, Classic Party, the first Eclipse study) are **dropped**.
4. Get a short review from an IP lawyer before publishing.

---

## 1. Game concept

Every card has two faces:

- **Sun side (light):** the warm, friendly side the game starts on. It has milder action cards.
- **Moon side (dark):** the harsh, high-stakes side. It has brutal action cards.

When a **Flip** card is played, *everything* turns over at once: the draw pile, the discard pile and every player's hand. Play continues on the other side until the next Flip. The flip is the game's signature moment, so animate it lavishly.

Objective: be the first to empty your hand. Points come from the cards left in opponents' hands, and the first player to **500 points** wins the match.

---

## 2. Card set (112 double-sided cards)

Each physical card has one Sun face and one Moon face. Pairing is **random** (shuffle the Sun-face list and the Moon-face list independently when building a deck, then zip them together).

### 2.1 Sun side (112 faces)

| Card | Per color | Total | Effect |
|---|---|---|---|
| Number 1–9 | 2 of each (18) | 72 | Match by color or number |
| **+1** (Draw One) | 2 | 8 | Next player draws 1 and loses their turn |
| **Jump** (Skip) | 2 | 8 | Next player loses their turn |
| **Turnabout** (Reverse) | 2 | 8 | Reverses the direction of play |
| **Flip** | 2 | 8 | Flips everything to the Moon side |
| **Wild** | — | 4 | Choose the next color |
| **Wild +2** | — | 4 | Choose the color; next player draws 2 and loses their turn (challengeable) |

Four Sun colors (names differ per theme; see §5).

### 2.2 Moon side (112 faces)

| Card | Per color | Total | Effect |
|---|---|---|---|
| Number 1–9 | 2 of each (18) | 72 | Match by color or number |
| **Surge +5** (Draw Five) | 2 | 8 | Next player draws 5 and loses their turn |
| **Jump All** (Skip Everyone) | 2 | 8 | Every other player is skipped; the player who played it goes again |
| **Turnabout** (Reverse) | 2 | 8 | Reverses the direction of play |
| **Flip** | 2 | 8 | Flips everything back to the Sun side |
| **Wild** | — | 4 | Choose the next color |
| **Wild Chase** (Draw Color) | — | 4 | Choose a color; next player draws until they get that color, then loses their turn (challengeable) |

Four Moon colors, different from the Sun colors.

> **Note:** there are **no 0 cards**. Numbers run 1–9.

---

## 3. Rules

### 3.1 Setup
- 2–10 players. 7 cards each.
- The game always starts on the **Sun side**. Players see the Sun faces of their own hands; opponents' cards show their Moon faces (backs) to others. The draw pile is Sun-face down (Moon face visible on top).
- Choose the dealer by drawing; the highest Sun number deals (action cards count as 0).
- Turn the top draw-pile card over to start the discard pile.

### 3.2 Starting card
| Starting card | What happens |
|---|---|
| Number | Normal start |
| +1 | First player draws 1 and is skipped |
| Jump | First player is skipped |
| Turnabout | Dealer plays first and play goes counter-clockwise |
| Wild | First player chooses the color, then plays |
| **Wild +2** | Return it to the deck, reshuffle and turn up a new card |
| **Flip** | Flip everything immediately. The first player plays on the **Moon card now showing** (not on the Flip card) |

### 3.3 Turns
- Play passes left (clockwise) unless reversed.
- On your turn, play one card that matches the top discard by **color**, **number** or **symbol**, always using the faces of the side currently active.
- A plain **Wild** may be played at any time, even if you hold a matching card.
- If you can't play (or choose not to), draw 1. If the drawn card is playable, you may play it right away; otherwise your turn ends.
- **No stacking.** You can't answer a +1/+2/+5 with another draw card to pass the penalty on.

### 3.4 Challengeable wilds
**Wild +2** and **Wild Chase** may only be played legally when you hold **no card matching the current color** (number or action matches don't count). The next player may challenge:

| Card | Challenge succeeds (player cheated) | Challenge fails (play was legal) |
|---|---|---|
| Wild +2 | The player who played it draws 2; the challenger draws nothing and takes their turn | The challenger draws **4** and loses their turn |
| Wild Chase | The player who played it draws until they get the chosen color | The challenger draws until they get the chosen color **plus 2 more**, and loses their turn |

In the digital game, the challenge is a quick on-screen prompt ("Challenge?" with a 5-second timer) and the game checks the hand automatically.

### 3.5 Two-player rules
- **Turnabout** works like **Jump** (you play again).
- **Jump All** works like **Jump**.

### 3.6 Last card ("Call it!")
- When you play your second-to-last card, you must tap **Call it!**. In the digital game it's a button that appears when you're down to 2 cards.
- If you don't call it before the next player begins their turn and someone catches you, you draw **2**. In the digital game, bots catch you with a difficulty-based chance; online, any opponent can tap "Catch!".

### 3.7 Ending a round
- The round ends when a player plays their last card. A draw penalty on that last card still applies to the next player before scoring.
- If the draw pile runs out, shuffle the discard pile (except the top card) into a new draw pile on the **current side**.

### 3.8 Scoring
Score **only the faces of the side the round ended on.**

| Card | Points |
|---|---|
| Number 1–9 | Face value |
| +1 | 10 |
| Surge +5, Turnabout, Jump, Flip | 20 |
| Jump All | 30 |
| Wild | 40 |
| Wild +2 | 50 |
| Wild Chase | 60 |

The winner of the round scores the total of all opponents' cards. The first to **500** wins. (Alternative mode: keep a running total of each player's own leftover cards; when someone reaches 500, the lowest total wins.)

### 3.9 Edge cases to implement
- Flipping mid-turn: the turn order and direction carry over; only the active side changes.
- The active color after a flip is the color of the **new top face** of the discard pile. If that face is a Wild, the player who played the Flip chooses the color.
- Wild Chase when the draw pile runs dry: reshuffle and keep drawing. If the chosen color doesn't exist in the remaining cards, stop when the pile is exhausted.
- Jump All in multiplayer: the player who played it immediately takes another turn.
- Disconnects (online): a bot takes over after 15 seconds.

---

## 4. Shared card UI spec (all three themes)

These measurements are identical across themes. Only the "skin" changes.

### 4.1 Card geometry
- **Card size:** 220 × 330 px at reference scale (2:3 ratio). Scale for devices.
- **Outer frame:** theme-specific border (§5) around an inner panel.
- **Center emblem:** the theme's signature shape (starburst, circle or hexagon) about 182–186 px wide, centered.
- **Center number:** large, centered on the emblem, in the theme's number font (sizes below).
- **Corner indices:** a chip in the top-left and a matching one in the bottom-right **rotated 180°**. Chip: min 36 × 34 px, padding 0 8px, 10 px from the edges. It shows the number or symbol (`7`, `+1`, `»`, `»»`, `W`).
- **Gloss layer:** a semi-transparent highlight over everything (theme-specific).
- **Accessibility:** every color also has a distinct corner symbol in colorblind mode, and colors must differ in lightness, not just hue.

### 4.2 Center number sizes
- Single digit: about 112 px × the theme's scale (Premium uses Bungee at about 100 px).
- Two characters (`+1`, `+5`): about 84 px × the theme's scale.
- If the card has a sub-label (SURGE, JUMP ALL), shift the number up about 22 px, and place the sub-label below the emblem (13–14 px, weight 700, letter-spacing 0.3em, uppercase).

### 4.3 Number rendering (layered)
Each center number is drawn twice, stacked:
1. **Back layer:** the stroke/outline color, with a thick outline (CSS `-webkit-text-stroke`; only half its width shows outside the glyph) plus a drop shadow or glow.
2. **Front layer:** the fill, either solid or a vertical/diagonal gradient.

**Unity:** use TextMeshPro with SDF fonts. Use the material's **Outline** for the stroke and **Underlay** for the drop shadow or glow, and a gradient preset (or vertex color gradient) for gradient fills.

### 4.4 Action symbols
- **Jump (Sun):** a double chevron `» »`. Path (200×200 box): `M60 62 L98 100 L60 138 M104 62 L142 100 L104 138`. Dark outer stroke about 30 px, inner colored stroke about 14–16 px, round caps and joins.
- **Jump All (Moon):** a triple chevron. Path: `M44 66 L76 100 L44 134 M80 66 L112 100 L80 134 M116 66 L148 100 L116 134`, plus the "JUMP ALL" sub-label.
- **Wild / Wild Chase:** four nested copies of the emblem shape, one per color, at scales 1.0 / 0.78 / 0.58 / 0.40 (alternate layers rotated 13° for the starburst, 30° for the hexagon), with "WILD" or "CHASE" text in the center (about 24–30 px, heavy, with a dark outline).
- **Turnabout, Flip, Wild +2:** ⚠️ **not designed yet.** Design them in the same language: a bold symbol with a dark outline and colored inner stroke on the emblem.

### 4.5 Hand layout (fan)
- 5 cards shown at reference size. Horizontal spacing **150 px** (cards overlap).
- Rotations: **−14°, −7°, 0°, 7°, 14°**. Pivot point: 50% horizontally, **140%** vertically (below the card), for a natural arc.
- Vertical offsets: 20, 6, −40 (raised), 6, 20 px.
- **Playable card:** raised 40 px with a glowing ring (theme-specific).
- **Hover or touch:** lift −16 px over 220 ms, easing `cubic-bezier(0.2, 0.8, 0.2, 1)`.
- Production behavior: dim unplayable cards (about 55% brightness), offer one-tap auto-sort by color or number, and support both tap-to-play and drag-to-play.

### 4.6 Flip animation (signature moment)
- Each card does a 3D Y-axis rotation over 180°, **about 800 ms**, easing `cubic-bezier(0.2, 0.8, 0.2, 1)`, with a 30–40 ms stagger across the hand.
- The table background cross-fades from the Sun table to the Moon table, the lighting dims, and the music switches to its darker mix. Add a light haptic pulse at the midpoint.
- It must be skippable or fast-forwardable after the first time.

### 4.7 Color helpers used in the specs
- `lighten(c, t)` = mix the color with white by fraction t.
- `darken(c, t)` = mix the color with black by fraction t.

---

## 5. The three confirmed themes

Each theme is a full skin. In Unity, make each one a **ScriptableObject `CardTheme`** so the theme can be switched (and sold or unlocked as a cosmetic).

### 5.1 Theme A — Premium Set (warm and glossy, neon at night)

**Typography:** numbers and corner chips in **Bungee** (Regular 400); UI headings in **Bricolage Grotesque** 800; body text in **Manrope** 400/600/700.

#### Sun side
- **Table:** radial gradient `#22503F` (center) → `#163629` (55%) → `#0E241B` (edge), like green felt.
- **Colors:** Tangerine `#FF7A1A`, Berry `#E8327C`, Aqua `#13B5C4`, Lime `#8CC63F`.
- **Frame:** 9 px padding, outer radius 22 px, inner radius 15 px. Ink gradient at 160°: `#2E2554` → `#120D24`.
- **Panel:** radial gradient at 30% 18%: `lighten(c, .35)` → `c` (52%) → `darken(c, .35)`.
- **Background pattern:** two concentric starburst outlines in white at 16% opacity, 400 px, rotated 8°.
- **Emblem:** a 14-point starburst (outer radius 88, inner radius 72, in a 200×200 box), cream `#FFF6E4` with rounded joins, plus an inner outline in the card color at 45% opacity, scaled 0.86. Drop shadow `0 8px 8px rgba(0,0,0,.3)`.
- **Number:** ink `#1B1530` outline (16 px stroke) with drop shadow `0 8px 0 rgba(27,21,48,.35)`. Fill is a vertical gradient: `lighten(c, .42)` → `c` (55%) → `darken(c, .15)`.
- **Corner chips:** cream `#FFF6E4`, radius 10, ink text, shadow `0 3px 0 rgba(27,21,48,.4)`, Bungee 17 px.
- **Gloss:** top 46% of the card, white 32% → transparent.
- **Card shadow:** `0 24px 40px rgba(0,0,0,.45)`.
- **Playable glow:** `0 0 0 3px #FFE9A8, 0 0 44px rgba(255,220,140,.65)`.
- **Wild:** **gold foil frame** with a 150° gradient `#FBE3A0` → `#D9A441` (35%) → `#A8741F` (60%) → `#F6D98B`. Panel radial `#3A2E66` → `#120D24`. Four nested starbursts in the Sun colors; "WILD" in cream `#FFF6E4` with an ink outline.

#### Moon side
- **Table:** radial gradient `#2A2160` → `#151036` → `#08061A` (midnight).
- **Colors:** Violet `#8F6BFF`, Rose `#FF4F86`, Ice `#3FD6E8`, Ember `#FF8A3D`.
- **Frame:** ink gradient `#241E4A` → `#07051A`.
- **Panel:** radial gradient `darken(c, .45)` → `darken(c, .72)` (55%) → `#07051A`, plus a scattered white starfield at 55% opacity.
- **Emblem:** the same starburst as a **neon outline**: 5 px stroke in the card color, fill white at 6%, glow `0 0 10px c`.
- **Number:** Bungee. Back layer `darken(c, .6)` with a 14 px stroke and glow `0 0 24px c, 0 0 8px c`. Front fill white `#FFFFFF` (0–40%) → `lighten(c, .45)`.
- **Corner chips:** `#0E0B22` background with a 1.5 px border in the card color and white text.
- **Gloss:** top 40%, white 14% → transparent.
- **Playable glow:** the card's own color: `0 0 0 3px c, 0 0 48px c`.
- **Wild Chase:** **silver foil frame** with a gradient `#F2F4FF` → `#9AA3C7` → `#5D6488` → `#E3E7FF`. Four nested neon-outline starbursts in the Moon colors; "CHASE" in white with a dark outline.

### 5.2 Theme B — Glass Aurora (airy frosted glass, northern lights at night)

**Typography:** numbers and corner chips in **Righteous** (400), at 1.04× scale; UI headings in **Unbounded** 800; body text in **Manrope**.

#### Sun side
- **Table:** a soft aurora of three radial blobs over `#F4F0FF`: pink `#FFB8CC` (top left), sky `#A6DCFF` (top right), lavender `#CDBDFF` (bottom).
- **Colors:** Coral `#FF5E6C`, Violet `#6C5CE7`, Jade `#00A884`, Azure `#0984E3`.
- **Frame:** 2 px padding, outer radius 24 px, inner radius 22 px. Frame gradient at 160°: white 95% → white 40%.
- **Panel:** a color glow (radial at 50% 52%, `c` at 75% fading to 0 by 72%) over frosted glass (160° gradient: white 80% → white 35%). In Unity, add a **background blur** behind the cards for the real glass effect.
- **Emblem:** a **circle** (radius 86 in a 200 box), fill white 40%, 3 px stroke in white 95%, shadow `0 10px 18px rgba(30,22,64,.18)`.
- **Number:** Righteous. Fill is the solid card color `c`, with a **white** outline (about 14 px stroke) and drop shadow `0 6px 10px rgba(30,22,64,.28)`.
- **Corner chips:** white 75%, 1 px border in white 95%, radius 12, text `darken(c, .35)`, shadow `0 4px 10px rgba(30,22,64,.12)`.
- **Gloss:** diagonal at 135°, white 60% fading to 0 by 42%.
- **Card shadow:** `0 30px 50px rgba(60,40,140,.25)`.
- **Playable glow:** a white halo: `0 0 0 2px #FFF, 0 0 60px rgba(255,255,255,.95)`.
- **Wild:** a **rainbow frame** (135°: Coral → Violet → Jade → Azure). The panel has four soft color blobs on white. Four nested circle outlines in white; "WILD" in white with an outline in `#1E1640`.
- **UI text:** heading `#1E1640`, muted `#4A3F6E`, eyebrow `#5B3FD0`.

#### Moon side
- **Table:** a night aurora over `#070A1F` with radial blobs of violet `rgba(130,70,255,.55)`, teal `rgba(0,220,200,.35)` and magenta `rgba(255,60,140,.35)`.
- **Colors:** Amethyst `#A27BFF`, Magenta `#FF4FA3`, Cyan `#2FF3E0`, Volt `#E7FF5A`.
- **Frame:** white 45% → white 8%.
- **Panel:** a color glow (`c` at 50%) over smoked glass (160°: `rgba(40,44,90,.85)` → `rgba(10,12,32,.92)`), plus a starfield.
- **Emblem:** a circle outline, 3 px stroke in the card color, with glow `0 0 10px c`.
- **Number:** Righteous. White fill, outline `darken(c, .6)`, glow `0 0 14px c`.
- **Corner chips:** `rgba(10,12,32,.7)` background, 1 px border in the card color, white text.
- **Playable glow:** `0 0 0 2px c, 0 0 60px c`.
- **Wild Chase:** a rainbow frame in the Moon colors, with nested neon circles and "CHASE".

### 5.3 Theme C — Prism Holo (collector's foil, black chrome at night)

**Typography:** numbers and corner chips in **Orbitron** Black (900), at 0.84× scale (it's a wide font); UI headings in **Syne** 800; body text in **Manrope**.

#### Sun side
- **Table:** radial gradient `#FAFAFD` → `#E6E7F0` (60%) → `#C9CBDA` (pearl studio).
- **Colors:** Ruby `#F23E6B`, Cobalt `#4C6BFF`, Emerald `#12A884`, Amber `#F29A0C`.
- **Frame:** 7 px padding, outer radius 22 px, inner radius 16 px. **Iridescent conic gradient** (from 210°): `#FFC9E3` → `#C3D4FF` → `#BFFBEA` → `#FFF1B8` → `#E3C9FF` → `#FFC9E3`. In Unity, animate the hue or angle slowly with device tilt (gyroscope) for a real holographic effect.
- **Panel:** pearl, 160°: `#FFFFFF` → `#EFF0F7`, with a hairline inner border `rgba(20,21,31,.08)`.
- **Emblem:** a **hexagon** (pointy-top, radius 88), fill in the card color `c`, 6 px white stroke, shadow `0 10px 14px rgba(20,21,31,.22)`.
- **Number:** Orbitron 900. Fill is a 160° gradient `#FFFFFF` → `#FFE3F1` → `#DCEBFF` (pearl iridescent), outline `darken(c, .45)` (about 11 px stroke), drop shadow `0 3px 0 rgba(0,0,0,.18)`.
- **Corner chips:** solid card color `c`, white text, radius 8, shadow `0 2px 6px rgba(20,21,31,.2)`.
- **Gloss (holo sheen):** a diagonal band at 115°, transparent to 28%, white 70% at 44%, pale blue `rgba(200,220,255,.35)` at 50%, transparent at 62%. **Animate this band sweeping across the card** on idle and when a card is selected.
- **Card shadow:** `0 24px 40px rgba(20,21,31,.25)`.
- **Playable glow:** `0 0 0 3px #FFF, 0 0 0 6px c, 0 0 50px c (50%)`.
- **Wild:** a stronger conic frame (`#FF7AB8` → `#7A9BFF` → `#5FFFD2` → `#FFE36F` → `#C98BFF`) around a **black chrome** panel (160°: `#1D1F2E` → `#0B0C12`). Four nested hexagons in the Sun colors with white strokes (alternate layers rotated 30°); "WILD" in white.
- **UI text:** heading `#14151F`, muted `#45475A`, eyebrow `#5A49D6`.

#### Moon side
- **Table:** radial gradient `#22243A` → `#0F1019` → `#050509`.
- **Colors:** Hot Pink `#FF4D8D`, Periwinkle `#6E7BFF`, Mint `#2DE3B4`, Gold `#FFC23D`.
- **Frame:** 3 px padding, conic foil `#FF6FB5` → `#6F8BFF` → `#4FFFD2` → `#FFE16F` → `#C77BFF` (a thin rainbow rim).
- **Panel:** black chrome, 160°: `#1C1D2A` → `#0B0C12`, plus a starfield.
- **Emblem:** a hexagon with fill `darken(c, .62)`, a 4 px stroke in the card color and glow `0 0 10px c`.
- **Number:** Orbitron 900. Fill is a gradient `#FFB8DC` → `#B8C8FF` → `#B8FFE9` (iridescent pastel), outline `#0B0C12`, glow `0 0 10px c` (80%).
- **Corner chips:** `#0B0C12` background, 1 px border in the card color, text in the card color.
- **Gloss:** the same sheen band, dimmer (white 20%).
- **Playable glow:** `0 0 0 2px c, 0 0 54px c`.
- **Wild Chase:** a conic foil frame around a black panel, with four nested neon hexagon outlines and "CHASE".

---

## 6. Fonts (all free on Google Fonts, under the SIL Open Font License, so they're OK to bundle in a commercial game)

| Role | Premium Set | Glass Aurora | Prism Holo |
|---|---|---|---|
| Card numbers and chips | Bungee 400 | Righteous 400 | Orbitron 900 |
| UI headings | Bricolage Grotesque 800 | Unbounded 800 | Syne 800 |
| Body and UI text | Manrope | Manrope | Manrope |

Generate TextMeshPro SDF atlases for digits 0–9, `+`, `»`, and A–Z (for WILD, CHASE, SURGE, JUMP ALL).

---

## 7. Unity architecture (suggested)

- **Data:** `CardFaceData` ScriptableObject (side, color index, type, value) and a `DoubleCard` runtime object (sunFace, moonFace). Use a `DeckBuilder` that randomizes the pairing.
- **Themes:** a `CardTheme` ScriptableObject holding the colors, frame materials, emblem sprite, fonts, glow and shadow settings, and table background for both sides. There's one asset per theme (Premium, Glass, Holo).
- **Logic and visuals separated:** keep the pure C# rules engine (`GameState`, `TurnManager`, `RuleValidator`, `ScoreCalculator`) free of any MonoBehaviour, so bots and online play reuse it and it can be unit-tested.
- **State machine:** Deal → TurnStart → AwaitAction → ResolveCard → (Challenge?) → (Flip?) → CheckWin → NextTurn → RoundEnd → Scoring.
- **Presentation:** use a card prefab built in layers (frame → panel → pattern → emblem → number back/front → sub-label → gloss → chips). Use DOTween for the fan, lift, play-to-pile and flip animations.
- **Bots:** Easy, Medium and Hard difficulty. Hard tracks the colors players have played, holds back Flip cards strategically, and challenges based on probability.
- **Roadmap:** (1) offline against bots with all three themes; (2) pass-and-play plus house-rule toggles; (3) online multiplayer (Photon or Netcode) only after the core game proves itself.
- **Monetization stance:** optional rewarded ads only, never mid-match. Cosmetics (themes, card backs, tables) are the main revenue. No pay-to-win.

---

## 8. Still to design (next steps)

- [ ] Turnabout, Flip and Wild +2 card faces (Sun), and Turnabout and Flip (Moon), for all three themes
- [ ] A card back for each theme
- [ ] The table screen: discard pile, draw pile, opponents' seats, turn indicator, direction arrow, "Call it!" button
- [ ] The full flip animation sequence
- [ ] Color picker modal for Wilds, the challenge prompt, round-end scoring screen, and lobby
- [ ] Final game name and logo (must be clearly original)
