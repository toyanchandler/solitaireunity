# Game Design Document: Klondike Solitaire Mobile

## Document Metadata

| Field | Value |
| --- | --- |
| Project | Klondike Solitaire Mobile |
| Internal Module Name | SolitaireModule |
| Document Type | Game Design Document |
| Version | 1.0.0 Case Ready |
| Target Platform | iOS and Android |
| Orientation | Portrait only |
| Gameplay Space | 2D world-space card board |
| UI Scope | HUD and menus are base UI layer, board layout is covered here |
| Primary Input | Drag and drop |
| Shortcut Input | Double tap auto-move to Foundation |
| Rule Variant | Klondike, Draw 1 for MVP |

---

## 1. Design Intent

Klondike Solitaire Mobile is a clean, fast, portrait-first implementation of classic Klondike Solitaire. The design goal is to preserve the recognizable Solitaire rules while adapting the board, input, readability, and motion feel for one-handed mobile play.

The project is implemented as a modular gameplay package on top of an existing Unity base architecture. It does not own global game lifecycle, level flow, user profile, ads, purchases, or shared UI systems. The Solitaire module owns only card board state, card presentation, board layout, move validation, move execution, and card interaction.

### 1.1 Assumptions

- The existing base already owns game boot, level session lifecycle, scene loading, global UI, audio routing, analytics routing, and save/profile access.
- The Solitaire board is implemented in Unity 2D world-space.
- The scene starts with 52 card prefab instances under `DeckParent`.
- The scene contains physical slot anchors for Stock, Waste, 4 Foundations, and 7 Tableau columns.
- Runtime game state is not stored inside ScriptableObjects.
- ScriptableObjects are used only for shared configuration, visual assets, input thresholds, layout offsets, and rule variant settings.
- Draw 1 Klondike is MVP. Draw 3 is an optional rule variant.
- Double tap auto-move attempts Foundation only in MVP. Tableau auto-targeting is not included in MVP because it can create ambiguous player intent.

---

## 2. Product Pillars

### 2.1 Classic Rule Integrity

The player should feel that this is standard Klondike Solitaire. Helpers can reduce friction, but they must not modify legal move rules or introduce wild cards into the classic mode.

### 2.2 Mobile Readability

Cards must remain readable on portrait phones. Rank, suit, active card ownership, valid targets, and face-up states must be clear at small sizes.

### 2.3 Low Friction Input

The game must support reliable drag and drop, plus double tap to auto-move eligible cards to Foundation. Input should be deterministic and should never surprise the player by choosing an ambiguous Tableau destination.

### 2.4 Modular Unity Integration

The feature must be easy to plug into an existing base. The module should use scene references, ScriptableObject config, runtime state classes, and local controllers without singleton dependency.

### 2.5 Fast Iteration

Board layout, card offsets, animation duration, drag thresholds, sprites, and rule variant values must be editable from configuration without rewriting logic.

---

## 3. Target Audience

| Audience | Need |
| --- | --- |
| Casual mobile players | Familiar rules, fast sessions, low cognitive load |
| Older players | High contrast card faces, readable ranks, stable touch targets |
| Returning Solitaire players | Classic behavior, no rule-breaking monetized mechanics |
| Case reviewers | Clear module boundaries, maintainable data and controller design |

---

## 4. Game Overview

### 4.1 Objective

Move all 52 cards into the four Foundation piles. Each Foundation is built by suit from Ace to King.

### 4.2 Core Loop

```text
Deal board
-> Inspect visible Tableau and Waste
-> Drag valid cards or sequences
-> Draw from Stock when blocked
-> Reveal face-down Tableau cards
-> Build Foundations from Ace to King
-> Complete all Foundations to win
```

### 4.3 Session Length

Expected session length for MVP is 3 to 8 minutes depending on player skill, random seed, and optional undo usage.

### 4.4 Win Condition

The game is won when all four Foundation piles contain 13 cards each, total 52 Foundation cards.

### 4.5 Fail State

Classic Solitaire does not need a forced loss state. A session can become blocked. The player may continue searching, use undo, restart, or request a hint if available.

---

## 5. Terminology

| Term | Turkish Label | Description |
| --- | --- | --- |
| Stock | Deste | Face-down draw pile containing undealt reserve cards |
| Waste | Atık | Face-up pile that receives cards drawn from Stock |
| Foundation | Seri Yuvası | Four completion piles, one per suit, built Ace to King |
| Tableau | Oyun Sütunları | Seven main play columns where descending alternating color sequences are built |
| Face-up | Açık Kart | Visible card that can be evaluated for interaction |
| Face-down | Kapalı Kart | Hidden card that cannot be moved until revealed |
| Sequence | Seri | One or more face-up Tableau cards moved together |
| Legal Move | Geçerli Hamle | Move accepted by the active Klondike ruleset |

---

## 6. Gameplay Rules

### 6.1 Deck

- 52 standard playing cards.
- 4 suits: Hearts, Diamonds, Clubs, Spades.
- 13 ranks per suit: Ace, 2 through 10, Jack, Queen, King.
- Hearts and Diamonds are red.
- Clubs and Spades are black.

### 6.2 Initial Deal

Seven Tableau columns are dealt from left to right.

| Tableau | Total Cards | Face-down Cards | Face-up Cards |
| --- | ---: | ---: | ---: |
| T0 | 1 | 0 | 1 |
| T1 | 2 | 1 | 1 |
| T2 | 3 | 2 | 1 |
| T3 | 4 | 3 | 1 |
| T4 | 5 | 4 | 1 |
| T5 | 6 | 5 | 1 |
| T6 | 7 | 6 | 1 |

After dealing, all remaining cards go to Stock face-down.

### 6.3 Tableau Rules

A card or face-up sequence can be moved onto a Tableau column if:

- Target column is empty and the moving card is King.
- Or target top card is face-up.
- Moving top card rank is exactly one lower than target top card rank.
- Moving top card color is opposite of target top card color.

Example valid Tableau stack:

```text
Black 9
Red 8
Black 7
Red 6
```

### 6.4 Foundation Rules

A card can move to Foundation if:

- It is a single face-up card.
- The target Foundation is empty and the card is Ace.
- Or the target Foundation top card has the same suit and rank exactly one lower.

A Foundation pile must contain only one suit.

### 6.5 Stock and Waste Rules

MVP uses Draw 1.

- Tapping Stock draws one card to Waste.
- Drawn card becomes face-up.
- Only the top Waste card is interactable.
- Waste top card can move to Tableau or Foundation if legal.
- When Stock is empty, tapping the empty Stock slot recycles Waste back into Stock if the selected rule configuration allows recycle.
- Recycled Stock cards become face-down and keep Waste order reversed according to Klondike recycle behavior.

### 6.6 Face-down Reveal Rule

When the top card of a Tableau column is face-down and no card is above it, tapping it flips it face-up. It can also auto-flip after a successful move if config enables automatic reveal.

Recommended MVP setting:

```text
AutoRevealTableauTopCard = true
```

### 6.7 Undo Rule

Undo reverses the last executed command. The system must support at least:

- Move card or sequence between piles.
- Draw from Stock to Waste.
- Recycle Waste into Stock.
- Flip top Tableau card.
- Auto-move to Foundation.

Undo is local to the session and does not need cloud persistence in MVP.

### 6.8 Hint Rule

Hint is optional for MVP. If included, it should suggest legal moves without executing them.

Hint priority:

1. Move a card to Foundation if safe.
2. Reveal a hidden Tableau card by moving a blocking sequence.
3. Move Waste card to Tableau.
4. Move Tableau card to another Tableau if it reveals a hidden card.
5. Draw from Stock.

---

## 7. Player Actions

| Action | Input | Result |
| --- | --- | --- |
| Draw from Stock | Tap Stock | Draw one card to Waste |
| Recycle Waste | Tap empty Stock | Move Waste back to Stock if allowed |
| Reveal Tableau top card | Tap top face-down Tableau card | Flip card face-up if no card above it |
| Move card or sequence | Drag and drop | Execute if legal, otherwise return to source |
| Auto-move to Foundation | Double tap face-up card | Move to matching Foundation if legal |
| Open pause or menu | Base UI | Handled outside board module |
| Undo | Base UI button calls module API | Undo last command |
| Hint | Base UI button calls module API | Highlight recommended legal move |

---

## 8. Input Design

### 8.1 Input Priority

```text
1. Ignore board input when base modal UI is open
2. Ignore board input while board animation lock is active
3. Resolve active drag if pointer is moving beyond drag threshold
4. Resolve double tap if second tap arrives inside threshold
5. Resolve single tap on Stock, Waste, Tableau top card, or face-up card
```

### 8.2 Drag and Drop

Drag is the primary move input.

Drag start is valid if:

- Card is face-up.
- Card belongs to Waste and is top card.
- Or card belongs to Tableau and every card from selected card to top is face-up and forms a valid internal descending alternating sequence.
- Card does not belong to Foundation unless `AllowFoundationDragBack` is enabled in config.

During drag:

- Dragged card or sequence is re-parented under `DragParent`.
- Sorting order is raised above every board card.
- Potential target slot can be highlighted.
- Cards preserve relative vertical offsets.

On drop:

- Controller resolves target pile using slot colliders or nearest valid anchor.
- MoveResolver validates rules against BoardState.
- If valid, MoveExecutor mutates BoardState and LayoutController animates final positions.
- If invalid, cards return to their source positions and invalid feedback plays.

### 8.3 Double Tap Auto-move

Double tap is a shortcut for Foundation only in MVP.

Double tap is valid if:

- Card is face-up.
- Card is a single movable card.
- Card is top card of Waste, top card of Tableau, or optionally top card of Foundation if returning is supported.
- Matching Foundation move is legal.

If multiple Foundations are possible, target is determined by suit. There is no ambiguity because each suit has one Foundation slot.

If no Foundation move is legal:

- Card performs invalid micro-feedback.
- Board state does not mutate.

### 8.4 Single Tap on Cards

Single tap on a face-up card does not auto-move to Tableau in MVP. This avoids unexpected moves when multiple Tableau destinations exist.

Allowed single tap behaviors:

- Face-down top Tableau card flips if legal.
- Face-up card may become selected if selection mode is enabled.
- Selected card can show a short hint pulse for valid drop zones if config enables tap selection.

Recommended MVP setting:

```text
EnableTapSelection = false
EnableDoubleTapFoundationAutoMove = true
```

---

## 9. Portrait Board Layout

### 9.1 High Level Layout

The board uses a portrait-safe 2D world layout with slot anchors.

```text
+------------------------------------------------+
| Base HUD, optional and owned by base UI layer   |
+------------------------------------------------+
| StockSlot  WasteSlot      Foundation 0 1 2 3    |
+------------------------------------------------+
| T0   T1   T2   T3   T4   T5   T6               |
| |    |    |    |    |    |    |                |
| v    v    v    v    v    v    v                |
| Dynamic vertical card stacks                    |
+------------------------------------------------+
| Base footer actions, optional                   |
+------------------------------------------------+
```

### 9.2 Slot Anchors

The scene contains these anchors:

```text
SlotRoot
|-- StockSlot
|-- WasteSlot
|-- FoundationSlot_Hearts
|-- FoundationSlot_Diamonds
|-- FoundationSlot_Clubs
|-- FoundationSlot_Spades
|-- TableauSlot_00
|-- TableauSlot_01
|-- TableauSlot_02
|-- TableauSlot_03
|-- TableauSlot_04
|-- TableauSlot_05
|-- TableauSlot_06
```

Each anchor stores:

- Pile type.
- Pile index.
- Optional suit for Foundation.
- World transform position.
- Drop collider.

### 9.3 Card Position Calculation

Stock, Waste, and Foundation cards align to their slot anchor with optional stack offset.

Tableau cards use the X position of their Tableau slot and calculate Y by card order and configurable offsets.

Recommended formula:

```text
TableauCardWorldPosition = TableauSlotPosition + Vector3.down * SumOffsetBeforeCard
```

Where `SumOffsetBeforeCard` is accumulated using:

```text
FaceDownTableauYOffset for hidden cards
FaceUpTableauYOffset for visible cards
CompressedFaceUpYOffset when the stack would exceed allowed vertical area
```

### 9.4 Dynamic Offset Compression

The Tableau layout must keep long columns inside the playable board area.

Inputs:

- Slot top Y.
- Bottom playable Y.
- Card height.
- Number of cards in column.
- Face-down offset.
- Desired face-up offset.
- Minimum readable face-up offset.

Calculation requirement:

```text
availableStackHeight = abs(slotTopY - bottomPlayableY) - cardHeight
requiredHeight = sum(faceDownOffsets) + faceUpCount * desiredFaceUpOffset
if requiredHeight <= availableStackHeight:
    use desiredFaceUpOffset
else:
    compress face-up offset, clamped to minimum
```

This gives more stable results than a purely count-based formula because it uses actual board space.

### 9.5 Card Size Rule

Card width is derived from available board width and seven Tableau columns.

```text
cardWidth = (availableBoardWidth - totalHorizontalSpacing) / 7
cardHeight = cardWidth * cardAspectRatio
```

Recommended card aspect ratio:

```text
cardAspectRatio = 1.4
```

The exact value should come from `SolitaireDeckConfigSO`.

---

## 10. Art Style Direction

### 10.1 Visual Style

Modern minimal premium card table.

Characteristics:

- Clean card silhouettes.
- Soft shadows.
- Matte table background.
- High contrast ranks and suits.
- Clear face-down card backs.
- Low visual noise.
- No heavy skeuomorphic decoration in MVP.

### 10.2 Palette Direction

| Element | Direction |
| --- | --- |
| Background | Deep green, dark navy, or warm matte charcoal |
| Card front | Warm white or soft ivory |
| Red suits | Accessible red with strong contrast |
| Black suits | Charcoal, not pure black if softer look is preferred |
| Slot placeholders | Low opacity outlines or ghost cards |
| Valid highlight | Soft green or blue glow, configurable |
| Invalid feedback | Short red pulse or shake, configurable |

### 10.3 Card Readability

Card front must prioritize readable corner rank and suit marks.

Requirements:

- Rank and suit must be readable at smallest supported card size.
- Corner information must remain visible in compressed Tableau stacks.
- Color contrast must remain clear under dark and light themes.
- Face-down cards must be visually distinct from face-up cards.

### 10.4 Accessibility

- Minimum touch target should be controlled by collider padding, not only visual size.
- Invalid feedback should not rely only on color. Use motion or haptic as secondary cues.
- Haptic feedback should be optional and configurable.
- Animations should be short and avoid blocking rapid gameplay.

---

## 11. Feedback and Game Feel

| State | Visual Feedback | Haptic | State Owner |
| --- | --- | --- | --- |
| Idle | Normal sprite and sorting | None | CardView |
| Pressed | Slight scale or shadow | Light, optional | CardStateMachine |
| Dragging | Higher sorting, lifted shadow, optional alpha | None | CardDragBehaviour |
| Valid target | Slot highlight | None | SlotAnchor or LayoutController |
| Invalid drop | Return animation and small shake | Optional warning | CardStateMachine |
| Successful move | Smooth position animation | Optional light impact | MoveExecutor plus CardView |
| Foundation move | Short slot pulse | Optional medium impact | CardView plus FoundationSlot |
| Win | Celebration animation | Optional | DeckController event |

Recommended move animation duration:

```text
StandardMoveDuration = 0.15 to 0.25 seconds
InvalidReturnDuration = 0.12 to 0.2 seconds
FlipDuration = 0.12 to 0.18 seconds
```

---

## 12. Feature Scope

### 12.1 MVP

| Feature | Included |
| --- | --- |
| 52-card Klondike setup | Yes |
| Draw 1 Stock | Yes |
| Waste pile | Yes |
| 7 Tableau columns | Yes |
| 4 Foundation piles | Yes |
| Drag and drop cards | Yes |
| Drag and drop sequences | Yes |
| Double tap Foundation auto-move | Yes |
| Invalid move return animation | Yes |
| Auto reveal Tableau top card | Yes |
| Win detection | Yes |
| Restart session | Through base lifecycle |
| Undo | Recommended MVP |
| Hint | Optional MVP |
| Score | Optional MVP |
| Timer and move counter | Through base UI, optional |

### 12.2 Out of MVP

| Feature | Reason |
| --- | --- |
| Daily challenges | Requires seed service and profile reward flow |
| Ads and IAP | Not needed for game case core implementation |
| Magic Move wild card | Breaks classic rule integrity |
| Server validated solvable boards | Requires backend or solver pipeline |
| Full cosmetic store | Requires profile, inventory, purchase, and theme systems |
| Draw 3 | Simple config extension after Draw 1 is stable |
| Multi-language UI | Base UI concern |

### 12.3 Optional Backlog

- Draw 3 rule variant.
- Safe auto-complete when all remaining moves to Foundation are deterministic.
- Hint quality levels.
- Daily seed mode.
- Cosmetic card backs and table themes.
- Classic waterfall win animation.
- Local statistics: wins, best time, fewest moves.

---

## 13. Unity Module Requirements

### 13.1 Scene Hierarchy

The board scene should contain this gameplay hierarchy excluding global UI.

```text
SolitaireRoot
|
|-- DeckParent
|   |-- Card_00
|   |-- Card_01
|   |-- ...
|   |-- Card_51
|
|-- SlotRoot
|   |-- StockSlot
|   |-- WasteSlot
|   |-- FoundationSlot_Hearts
|   |-- FoundationSlot_Diamonds
|   |-- FoundationSlot_Clubs
|   |-- FoundationSlot_Spades
|   |-- TableauSlot_00
|   |-- TableauSlot_01
|   |-- TableauSlot_02
|   |-- TableauSlot_03
|   |-- TableauSlot_04
|   |-- TableauSlot_05
|   |-- TableauSlot_06
|
|-- DragParent
|
|-- Controllers
|   |-- SolitaireModuleInstaller
|   |-- SolitaireDeckController
|   |-- SolitaireInputController
|   |-- SolitaireLayoutController
|
|-- Debug
    |-- SolitaireDebugGizmos
```

### 13.2 Prefabs

#### Card.prefab

Required components:

- `CardView`
- `CardRuntimeIdentity`
- `CardInputReceiver`
- `CardDragBehaviour`
- `CardStateMachine`
- `SpriteRenderer`
- `BoxCollider2D`
- `SortingGroup`, if card uses multiple renderers

Responsibilities:

- Render front and back sprites.
- Expose pointer events.
- Run local visual/input state machine.
- Never decide game legality.
- Never mutate BoardState directly.

#### Slot prefab or scene anchor

Required components:

- `SolitaireSlotAnchor`
- `BoxCollider2D`
- Optional debug renderer.

Responsibilities:

- Provide world position.
- Provide pile type and pile index.
- Provide Foundation suit where applicable.
- Provide drop hit area.

#### SolitaireRoot.prefab

Required components:

- `SolitaireModuleInstaller`
- Serialized references to config, deck parent, slot anchors, drag parent, and controllers.

Responsibilities:

- Create runtime context.
- Register 52 card views.
- Initialize controllers.
- Start deal flow when base architecture calls module start.

### 13.3 ScriptableObject Config

`SolitaireDeckConfigSO` stores shared editable config only.

Recommended fields:

```text
Card visual assets:
- CardBackSprite
- FrontSprites or Rank/Suit sprite mapping
- PlaceholderSprites

Layout:
- CardAspectRatio
- HorizontalColumnSpacing
- FaceUpTableauYOffset
- FaceDownTableauYOffset
- MinCompressedFaceUpYOffset
- WasteStackOffset
- FoundationStackOffset

Input:
- DragStartThreshold
- DoubleTapThreshold
- DropSnapDistance
- ColliderPadding

Animation:
- MoveDuration
- InvalidReturnDuration
- FlipDuration
- DragLiftSortingOrder

Rules:
- DrawMode
- AllowWasteRecycle
- AutoRevealTableauTopCard
- AllowFoundationDragBack
- EnableDoubleTapFoundationAutoMove
- EnableTapSelection

Debug:
- ShowSlotGizmos
- LogMoveValidation
- ForceSeed
```

ScriptableObject must not store:

- Current Stock cards.
- Current Waste cards.
- Current Tableau arrays.
- Current Foundation cards.
- Current selected card.
- Current dragged card sequence.
- Move history.
- Score, timer, or session statistics.

### 13.4 Runtime Data Ownership

Runtime data belongs to `SolitaireRuntimeContext`.

```text
SolitaireRuntimeContext
|-- SolitaireBoardState
|   |-- Cards[52]
|   |-- Stock
|   |-- Waste
|   |-- Foundations[4]
|   |-- Tableaus[7]
|
|-- SolitaireViewRegistry
|   |-- CardViews[52]
|
|-- SolitaireMoveHistory
|
|-- SolitaireSelectionState
```

Rules:

- `MoveExecutor` is the only class that mutates BoardState during gameplay moves.
- `MoveResolver` validates and produces a candidate move.
- `SolitaireDeckController` orchestrates requests but does not hard-code rules.
- `CardStateMachine` controls visual/input state only.
- Static classes are allowed only for pure utility methods.
- Singleton is not used by this module.

---

## 14. Controller Responsibilities

### 14.1 SolitaireDeckController

Owns gameplay orchestration.

Responsibilities:

- Initialize deck from scene card instances.
- Build and shuffle card data.
- Deal initial Tableau and Stock state.
- Receive public board requests from input and base UI.
- Ask MoveResolver whether a move is legal.
- Call MoveExecutor for accepted moves.
- Trigger LayoutController after state changes.
- Emit module events such as move completed, invalid move, game won.

Does not:

- Decide Klondike legality directly.
- Store authoritative pile arrays outside BoardState.
- Own global game session lifecycle.

### 14.2 SolitaireInputController

Responsibilities:

- Translate pointer/touch events into board requests.
- Detect drag start, drag update, drag end.
- Detect double tap within configured threshold.
- Resolve drop target candidate.
- Respect animation lock and base modal lock.

### 14.3 SolitaireLayoutController

Responsibilities:

- Calculate world positions for all cards based on BoardState and slot anchors.
- Apply dynamic Tableau offset compression.
- Apply sorting order.
- Handle snap and return positions.
- Provide preview position for dragged sequences.

### 14.4 SolitaireMoveResolver

Responsibilities:

- Validate whether selected card or sequence is movable.
- Validate target pile rules.
- Build `SolitaireMove` candidate.
- Resolve double tap Foundation target.
- Never mutate BoardState.

### 14.5 SolitaireMoveExecutor

Responsibilities:

- Move card ids between pile arrays.
- Update `CardState.CurrentPileType`, `CurrentPileIndex`, and `IndexInPile`.
- Flip top Tableau card when configured.
- Push reversible commands into MoveHistory.
- Return mutation results to controller.

---

## 15. Data Model Requirements

### 15.1 CardState

CardState should be a lightweight struct.

```text
CardState
- Id
- Suit
- Rank
- Color
- IsFaceUp
- CurrentPileType
- CurrentPileIndex
- IndexInPile
```

### 15.2 CardPileState

CardPileState should store card ids, not card GameObjects.

Recommended implementation:

- Fixed int array for optimized runtime, because the deck has exactly 52 cards.
- Count field for active length.
- Methods for add, remove top, remove from index, index lookup, and sequence copy.

Readable MVP implementation can use `List<int>` if case delivery speed matters. The architecture should still keep the pile API isolated so implementation can later switch to fixed arrays without changing controllers.

### 15.3 SolitaireBoardState

BoardState is authoritative gameplay state.

```text
SolitaireBoardState
- CardState[] Cards = new CardState[52]
- CardPileState Stock
- CardPileState Waste
- CardPileState[] Foundations = new CardPileState[4]
- CardPileState[] Tableaus = new CardPileState[7]
```

### 15.4 ViewRegistry

ViewRegistry maps card ids to `CardView` references.

```text
CardViews[52]
```

The registry avoids repeated scene searches and keeps data separate from presentation.

---

## 16. Event Hooks for Base Integration

The module should expose simple public events or base event-channel calls.

Recommended events:

```text
OnSolitaireReady
OnDealStarted
OnDealCompleted
OnMoveRequested
OnMoveCompleted
OnInvalidMove
OnCardFlipped
OnStockDrawn
OnWasteRecycled
OnFoundationProgressChanged
OnUndoChanged
OnHintAvailable
OnGameWon
```

Base UI can listen to:

- Move count changed.
- Timer start and stop.
- Undo availability.
- Hint availability.
- Win event.

The Solitaire module should not directly open global menus or navigate scenes.

---

## 17. Scoring and Session Metrics

MVP can ship without score. If score is included, keep it isolated from move validation.

Recommended simple metrics:

| Metric | Owner | Notes |
| --- | --- | --- |
| Move count | Solitaire module, surfaced to base UI | Increments on successful player move |
| Timer | Base session or module adapter | Starts after first interaction |
| Undo count | MoveHistory | Optional display |
| Win flag | BoardState or session result | True when all Foundations complete |

Optional score model:

```text
+10 Move card to Foundation
+5 Reveal Tableau card
-1 Each undo
0 Draw Stock card
```

Score should be optional and configurable.

---

## 18. Acceptance Criteria

### 18.1 Gameplay

| ID | Requirement |
| --- | --- |
| AC-GAME-01 | Initial deal creates 7 Tableau columns with counts 1 through 7. |
| AC-GAME-02 | Only the top card of each initial Tableau column is face-up. |
| AC-GAME-03 | Remaining cards are placed face-down in Stock. |
| AC-GAME-04 | Tableau moves allow descending alternating colors only. |
| AC-GAME-05 | Empty Tableau accepts King or a sequence starting with King only. |
| AC-GAME-06 | Foundation accepts Ace on empty pile and same-suit ascending ranks after that. |
| AC-GAME-07 | Waste top card can move if legal. Non-top Waste cards cannot move. |
| AC-GAME-08 | Dragged Tableau sequence must be fully face-up and internally legal. |
| AC-GAME-09 | Invalid moves do not mutate BoardState. |
| AC-GAME-10 | Win triggers when all four Foundations contain 13 cards. |

### 18.2 Input

| ID | Requirement |
| --- | --- |
| AC-IN-01 | Drag start ignores face-down cards. |
| AC-IN-02 | Drag start ignores covered Waste cards. |
| AC-IN-03 | Dragged cards move under DragParent and render above board cards. |
| AC-IN-04 | Dropping on legal target executes move and animates to target positions. |
| AC-IN-05 | Dropping on illegal target returns cards to source positions. |
| AC-IN-06 | Double tap on eligible card attempts Foundation move only. |
| AC-IN-07 | Double tap does not auto-route to Tableau in MVP. |
| AC-IN-08 | Tapping Stock draws one card to Waste in Draw 1 mode. |
| AC-IN-09 | Board input is locked while critical move animation is executing. |

### 18.3 Layout

| ID | Requirement |
| --- | --- |
| AC-LAY-01 | Board fits portrait aspect ratios from 16:9 to 21:9. |
| AC-LAY-02 | All slot anchors remain inside safe playable area. |
| AC-LAY-03 | Seven Tableau columns fit horizontally without card overlap. |
| AC-LAY-04 | Tableau vertical offset compresses when required. |
| AC-LAY-05 | Minimum readable face-up offset is respected. |
| AC-LAY-06 | Card positions are recalculated from BoardState after every accepted move. |

### 18.4 Architecture

| ID | Requirement |
| --- | --- |
| AC-ARCH-01 | Runtime state is not stored in ScriptableObjects. |
| AC-ARCH-02 | BoardState stores card ids, not GameObject references. |
| AC-ARCH-03 | MoveResolver validates without mutating state. |
| AC-ARCH-04 | MoveExecutor is the only gameplay move mutator. |
| AC-ARCH-05 | Card prefab state machine does not know Klondike rules. |
| AC-ARCH-06 | The module does not require singleton access. |
| AC-ARCH-07 | Static methods are pure utilities only. |
| AC-ARCH-08 | Game restart reuses the existing 52 scene card instances. |

### 18.5 Performance

| ID | Requirement |
| --- | --- |
| AC-PERF-01 | Move validation must complete without frame-visible delay. |
| AC-PERF-02 | No instantiate or destroy during normal restart. |
| AC-PERF-03 | Card lookup uses a 52-slot array registry. |
| AC-PERF-04 | Drag update avoids per-frame allocations. |
| AC-PERF-05 | Layout recalculation is bounded by 52 cards. |

### 18.6 Art and Feedback

| ID | Requirement |
| --- | --- |
| AC-ART-01 | Card rank and suit are readable at smallest supported card size. |
| AC-ART-02 | Face-down and face-up states are visually distinct. |
| AC-ART-03 | Valid drop target highlight is visible above slot placeholder. |
| AC-ART-04 | Invalid feedback includes motion, not only color. |
| AC-ART-05 | Haptics can be disabled through config or platform settings. |

---

## 19. QA Test Scenarios

### 19.1 Deal Validation

- Start a session with a fixed seed.
- Verify all 52 cards exist in exactly one pile.
- Verify Tableau counts 1 through 7.
- Verify Stock contains 24 cards in Draw 1 Klondike after deal.
- Verify only top Tableau cards are face-up.

### 19.2 Tableau Move Validation

- Drag red 7 onto black 8. Expected: accepted.
- Drag red 7 onto red 8. Expected: rejected.
- Drag black Queen onto empty Tableau. Expected: rejected.
- Drag King sequence onto empty Tableau. Expected: accepted.

### 19.3 Foundation Move Validation

- Double tap Ace. Expected: moves to matching Foundation.
- Double tap 2 before Ace is present. Expected: rejected.
- Double tap 2 of Hearts after Ace of Hearts is present. Expected: accepted.
- Drag a sequence to Foundation. Expected: rejected, because Foundation accepts single cards only.

### 19.4 Stock and Waste

- Tap Stock once. Expected: one card moves to Waste face-up.
- Try dragging covered Waste card. Expected: impossible.
- Empty Stock, tap Stock if recycle enabled. Expected: Waste returns to Stock face-down.

### 19.5 Undo

- Execute Tableau move, then undo. Expected: source and target piles return to previous state.
- Draw Stock, then undo. Expected: card returns from Waste to Stock face-down.
- Auto-flip after move, then undo. Expected: flip state returns correctly.

### 19.6 Layout

- Create a long Tableau column with 13 plus cards.
- Verify cards remain inside playable area.
- Verify minimum face-up offset remains readable.
- Rotate device is not supported. Verify orientation lock remains portrait.

---

## 20. Risks and Design Decisions

| Risk | Decision |
| --- | --- |
| Smart tap to Tableau can select unexpected target | MVP excludes Tableau auto-targeting. Use drag for Tableau. |
| Monetized Magic Move can break classic rules | Excluded from classic MVP. Only optional non-classic mode later. |
| SO runtime state can leak across sessions | SO stores config only. Runtime context owns state. |
| Singleton can conflict with base architecture | Module uses dependency injection from installer. |
| Long Tableau columns can overflow phone screen | Layout uses space-based dynamic compression. |
| Dragging many cards can cause sorting bugs | DragParent owns temporary high sorting order. |

---

## 21. Definition of Done

The feature is done when:

- The board starts with 52 preplaced card prefab instances under `DeckParent`.
- `SolitaireDeckConfigSO` controls visuals, offsets, input thresholds, animation timings, and rule settings.
- `SolitaireRuntimeContext` is created at runtime and owns BoardState, ViewRegistry, MoveHistory, and SelectionState.
- Stock, Waste, 4 Foundation slots, and 7 Tableau slots exist as scene anchors.
- Initial Klondike deal works deterministically from seed.
- Drag and drop works for Waste, Tableau cards, Tableau sequences, and Foundation targets where legal.
- Double tap moves eligible cards to Foundation only.
- Invalid moves produce feedback and do not mutate BoardState.
- Win condition is detected.
- Restart reuses existing card GameObjects without creating or destroying card objects.
- The module does not require singleton state.

---

## 22. Case Presentation Summary

This implementation is not only a Klondike clone. It is a modular card-board system that separates rules, state, input, layout, and rendering. The same architecture can later support Draw 3, custom themes, improved hints, daily seeds, and additional Solitaire variants without rewriting the core card view layer.
