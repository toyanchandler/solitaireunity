# SoftwareArchitectureData.md

## Klondike Solitaire Module Architecture

Bu doküman, mevcut Level ve Game Architecture üzerine modüler olarak eklenecek 2D Klondike Solitaire gameplay paketinin data, controller, struct, class ve ScriptableObject mimarisini tanımlar.

Kapsam UI hariçtir. Scene içinde 52 adet hazır Card prefab instance bulunur. Gameplay world-space 2D çalışır. HUD, menu, skor, timer ve diğer UI sistemleri daha sonra base UI layer üzerinden bağlanır.

---

## 1. Ana Mimari Kararı

Solitaire modülü mevcut base mimariyi ele geçirmez. Kendi scene root'u altında çalışan izole bir gameplay module olarak davranır.

Temel kararlar:

- Runtime kart state'i ScriptableObject içinde tutulmaz.
- ScriptableObject sadece config, prefab referansı, sprite referansı, layout offsetleri, input eşikleri ve rule varyantları için kullanılır.
- Runtime state normal C# class ve struct yapılarında tutulur.
- Controller, input sonrası BoardState ve MoveResolver üzerinden hamlenin legal olup olmadığını sorar.
- MoveExecutor dışında hiçbir sınıf BoardState'i mutate etmez.
- Singleton kullanılmaz.
- Static sadece pure utility fonksiyonları için kullanılır.
- Card prefab üzerindeki state machine yalnızca input ve visual state yönetir, oyun kurallarını bilmez.
- 52 kart scene içinde hazır olduğu için game restart sırasında instantiate veya destroy yapılmaz.

---

## 2. Scene Hierarchy

UI hariç önerilen 2D scene hiyerarşisi:

```text
SolitaireRoot
|
|-- DeckParent
|   |-- Card_00
|   |-- Card_01
|   |-- Card_02
|   |-- ...
|   |-- Card_51
|
|-- SlotRoot
|   |
|   |-- StockSlot
|   |-- WasteSlot
|   |
|   |-- FoundationSlots
|   |   |-- FoundationSlot_Hearts
|   |   |-- FoundationSlot_Diamonds
|   |   |-- FoundationSlot_Clubs
|   |   |-- FoundationSlot_Spades
|   |
|   |-- TableauSlots
|       |-- TableauSlot_00
|       |-- TableauSlot_01
|       |-- TableauSlot_02
|       |-- TableauSlot_03
|       |-- TableauSlot_04
|       |-- TableauSlot_05
|       |-- TableauSlot_06
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

### 2.1 DeckParent

`DeckParent` altında 52 adet `Card.prefab` instance bulunur.

Önerilen yaklaşım:

- Tek bir `Card.prefab` kullanılır.
- Bu prefab scene içinde 52 kez instance edilir.
- Runtime initialization sırasında her instance'a deterministic `CardId`, `Suit` ve `Rank` atanır.
- Kart front sprite'ları ScriptableObject config üzerinden resolve edilir.
- Kartlar oyun başında `Stock`, `Waste`, `Foundation`, `Tableau` pile'larına data üzerinden dağıtılır.

Bu sayede kart identity prefab dosyasında hardcoded olmaz. Aynı prefab farklı skin, farklı sprite seti veya farklı rule config ile tekrar kullanılabilir.

### 2.2 SlotRoot

Slotlar gerçek pile data'sını tutmaz. Sadece world-space pozisyon ve drop target bilgisidir.

Slotların görevi:

- Stock başlangıç pozisyonunu tutmak.
- Waste başlangıç pozisyonunu tutmak.
- 4 foundation slotunun pozisyonunu tutmak.
- 7 tableau kolonunun X başlangıç pozisyonunu tutmak.
- Drop hit test için `BoxCollider2D` sağlamak.
- LayoutController'a anchor pozisyonu vermek.

### 2.3 DragParent

Drag sırasında kart veya kart serisi geçici olarak `DragParent` altına alınır.

Amaç:

- Drag edilen kartların sorting order değerini yükseltmek.
- Kaynak tableau altındaki kartlardan bağımsız hareket ettirmek.
- Invalid drop sonrası kartları eski pile layout'una döndürmek.

---

## 3. Prefab Component Yapısı

### 3.1 Card.prefab

```text
Card.prefab
|
|-- CardView
|-- CardRuntimeIdentity
|-- CardInputReceiver
|-- CardDragBehaviour
|-- CardVisualStateMachine
|-- SpriteRenderer
|-- BoxCollider2D
|-- SortingGroup veya SpriteRenderer sorting yönetimi
```

Component sorumlulukları:

| Component | Sorumluluk |
|---|---|
| CardView | Sprite, face state, sorting, position animation, visual refresh |
| CardRuntimeIdentity | RuntimeId, Suit ve Rank bilgisini view tarafında tutar |
| CardInputReceiver | Pointer down, tap, double tap, drag start eventlerini toplar |
| CardDragBehaviour | Drag hareketini yönetir, world position hesaplar |
| CardVisualStateMachine | Visual ve interaction state yönetir |
| SpriteRenderer | Kart yüzünü render eder |
| BoxCollider2D | 2D hit detection sağlar |
| SortingGroup | Kart üzerindeki child renderer'lar varsa bütün kartı tek sorting unit gibi taşır |

### 3.2 Slot prefabları

```text
StockSlot.prefab
|-- SolitaireSlotAnchor
|-- BoxCollider2D

WasteSlot.prefab
|-- SolitaireSlotAnchor
|-- BoxCollider2D

FoundationSlot.prefab
|-- SolitaireSlotAnchor
|-- BoxCollider2D

TableauSlot.prefab
|-- SolitaireSlotAnchor
|-- BoxCollider2D
```

`SolitaireSlotAnchor` data array'i tutmaz. Sadece target metadata ve Transform pozisyonu tutar.

---

## 4. Runtime Ownership

Runtime state ownership şu şekilde olmalı:

```text
SolitaireModuleInstaller
|
|-- Creates SolitaireRuntimeContext
|   |
|   |-- SolitaireBoardState
|   |-- SolitaireViewRegistry
|   |-- SolitaireSelectionState
|   |-- SolitaireMoveHistory
|
|-- Initializes controllers
    |
    |-- SolitaireDeckController
    |-- SolitaireInputController
    |-- SolitaireLayoutController
```

### 4.1 Neden singleton yok?

Solitaire modülü level bazlı çalışacak. Singleton kullanılırsa şu problemler oluşur:

- Restart sonrası stale data riski.
- Aynı test scene içinde birden fazla board kuramama.
- Unit test setup maliyeti.
- Base architecture ile lifecycle çakışması.
- Runtime state'in asset ya da global memory üzerinden kontrolsüz paylaşılması.

### 4.2 Static nerede kullanılabilir?

Static sadece stateless helper için kullanılmalı.

Uygun static örnekleri:

```text
SolitaireCardUtility
SolitaireShuffleUtility
SolitaireRuleUtility
```

Uygun olmayan static örnekleri:

```text
CurrentBoardState
CurrentDeckController
CurrentSelectedCard
CurrentDraggedCards
CurrentMoveHistory
```

### 4.3 Controller data'ya nereden erişir?

`SolitaireDeckController`, `Initialize` sırasında `SolitaireRuntimeContext` alır.

```csharp
public void Initialize(
    SolitaireDeckConfigSO config,
    SolitaireRuntimeContext context,
    SolitaireMoveResolver moveResolver,
    SolitaireMoveExecutor moveExecutor,
    SolitaireLayoutController layoutController)
{
    _config = config;
    _context = context;
    _moveResolver = moveResolver;
    _moveExecutor = moveExecutor;
    _layoutController = layoutController;
}
```

Bu modelde data erişimi explicit dependency olarak akar. Singleton veya static lookup gerekmez.

---

## 5. ScriptableObject Layer

ScriptableObject, runtime board state değil, paylaşılan immutable gameplay configuration kaynağıdır.

### 5.1 SolitaireDeckConfigSO

```csharp
using UnityEngine;

namespace Handler.GameModules.Solitaire
{
    [CreateAssetMenu(
        menuName = "GameModules/Solitaire/Deck Config",
        fileName = "SolitaireDeckConfig")]
    public sealed class SolitaireDeckConfigSO : ScriptableObject
    {
        [Header("Prefab References")]
        [SerializeField] private CardView cardPrefab;

        [Header("Card Sprites")]
        [SerializeField] private Sprite cardBackSprite;
        [SerializeField] private Sprite[] hearts = new Sprite[13];
        [SerializeField] private Sprite[] diamonds = new Sprite[13];
        [SerializeField] private Sprite[] clubs = new Sprite[13];
        [SerializeField] private Sprite[] spades = new Sprite[13];

        [Header("Layout")]
        [SerializeField] private float faceUpTableauYOffset = 0.36f;
        [SerializeField] private float faceDownTableauYOffset = 0.14f;
        [SerializeField] private float wasteStackXOffset = 0.18f;
        [SerializeField] private float stockZStep = -0.005f;
        [SerializeField] private float cardZStep = -0.01f;

        [Header("Input")]
        [SerializeField] private float doubleTapThreshold = 0.25f;
        [SerializeField] private float dragStartThresholdWorld = 0.08f;

        [Header("Animation")]
        [SerializeField] private float moveAnimationDuration = 0.16f;
        [SerializeField] private float invalidMoveReturnDuration = 0.12f;
        [SerializeField] private int dragSortingOrder = 5000;

        [Header("Rules")]
        [SerializeField] private SolitaireDrawMode drawMode = SolitaireDrawMode.DrawOne;
        [SerializeField] private bool allowFoundationToTableau = false;
        [SerializeField] private bool autoFlipTableauTopCard = true;
        [SerializeField] private bool doubleTapMovesToFoundationOnly = true;

        public CardView CardPrefab => cardPrefab;
        public Sprite CardBackSprite => cardBackSprite;
        public float FaceUpTableauYOffset => faceUpTableauYOffset;
        public float FaceDownTableauYOffset => faceDownTableauYOffset;
        public float WasteStackXOffset => wasteStackXOffset;
        public float StockZStep => stockZStep;
        public float CardZStep => cardZStep;
        public float DoubleTapThreshold => doubleTapThreshold;
        public float DragStartThresholdWorld => dragStartThresholdWorld;
        public float MoveAnimationDuration => moveAnimationDuration;
        public float InvalidMoveReturnDuration => invalidMoveReturnDuration;
        public int DragSortingOrder => dragSortingOrder;
        public SolitaireDrawMode DrawMode => drawMode;
        public bool AllowFoundationToTableau => allowFoundationToTableau;
        public bool AutoFlipTableauTopCard => autoFlipTableauTopCard;
        public bool DoubleTapMovesToFoundationOnly => doubleTapMovesToFoundationOnly;

        public Sprite GetFrontSprite(CardSuit suit, CardRank rank)
        {
            int rankIndex = (int)rank - 1;

            return suit switch
            {
                CardSuit.Hearts => hearts[rankIndex],
                CardSuit.Diamonds => diamonds[rankIndex],
                CardSuit.Clubs => clubs[rankIndex],
                CardSuit.Spades => spades[rankIndex],
                _ => null
            };
        }
    }
}
```

### 5.2 SO içinde tutulacak şeyler

```text
Card prefab reference
Card back sprite
Card front sprite arrays
Layout offsets
Sorting constants
Animation durations
Input thresholds
Rule toggles
Draw 1 veya Draw 3 setting
Debug flags
```

### 5.3 SO içinde tutulmayacak şeyler

```text
Current stock card ids
Current waste card ids
Current tableau arrays
Current foundation arrays
Currently dragged card ids
Selected card id
Move history
Score
Timer
Session seed
```

Bunlar runtime session state olduğu için `SolitaireRuntimeContext` altında tutulur.

### 5.4 Runtime share için SO kullanımı

SO asset instance project ve scene tarafından paylaşılan bir data container olduğu için runtime mutable state için kullanılmamalı.

Doğru kullanım:

```text
SolitaireDeckConfigSO shared immutable config olarak kullanılır.
```

Yanlış kullanım:

```text
SolitaireDeckConfigSO.CurrentWasteCards
SolitaireDeckConfigSO.CurrentTableauCards
SolitaireDeckConfigSO.CurrentSelectedCard
```

Runtime paylaşım gerekiyorsa:

```text
SolitaireRuntimeContext per session oluşturulur.
Controller ve view registry bu context üzerinden konuşur.
Base architecture gerekiyorsa adapter üzerinden snapshot alır.
```

---

## 6. Enum ve Primitive Data Types

```csharp
namespace Handler.GameModules.Solitaire
{
    public enum CardSuit : byte
    {
        Hearts = 0,
        Diamonds = 1,
        Clubs = 2,
        Spades = 3
    }

    public enum CardColor : byte
    {
        Red = 0,
        Black = 1
    }

    public enum CardRank : byte
    {
        Ace = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13
    }

    public enum SolitairePileType : byte
    {
        Stock = 0,
        Waste = 1,
        Foundation = 2,
        Tableau = 3
    }

    public enum SolitaireDrawMode : byte
    {
        DrawOne = 1,
        DrawThree = 3
    }

    public enum SolitaireMoveType : byte
    {
        None = 0,
        StockToWaste = 1,
        WasteRecycleToStock = 2,
        WasteToTableau = 3,
        WasteToFoundation = 4,
        TableauToTableau = 5,
        TableauToFoundation = 6,
        FoundationToTableau = 7,
        FlipTableauTop = 8,
        AutoMoveToFoundation = 9
    }

    public enum CardVisualState : byte
    {
        Inactive = 0,
        InStock = 1,
        FaceDown = 2,
        FaceUpIdle = 3,
        Selected = 4,
        Dragging = 5,
        Moving = 6,
        Returning = 7,
        Locked = 8
    }
}
```

---

## 7. Struct ve Class Karar Matrisi

| Type | Struct veya Class | Neden |
|---|---|---|
| CardState | struct | 52 fixed kart, küçük data, array içinde cache friendly |
| PileRef | readonly struct | Source ve target referansını value object olarak taşır |
| SolitaireMove | readonly struct | Move request immutable olmalı |
| SolitaireMoveResult | readonly struct | Execution sonrası post-effect bilgisi taşır |
| FixedCardPileState | class | Mutable container ve reference semantics gerekli |
| SolitaireBoardState | class | Aggregate root, mutable runtime state sahibi |
| SolitaireRuntimeContext | class | Runtime dependency container |
| SolitaireDeckConfigSO | ScriptableObject | Shared immutable config ve asset referansları |
| CardView | MonoBehaviour class | Scene component |
| SolitaireDeckController | MonoBehaviour class | Scene controller |
| SolitaireMoveResolver | class | Stateless veya context-light rule decision object |
| SolitaireMoveExecutor | class | Board mutation merkezi |

### 7.1 Struct mutation notu

`CardState` struct olduğu için array dışına value copy olarak alınırsa yapılan mutation kaybolur.

Riskli kullanım:

```csharp
var card = board.Cards[cardId];
card.IsFaceUp = true;
```

Doğru kullanım:

```csharp
ref CardState card = ref board.GetCardRef(cardId);
card.IsFaceUp = true;
```

Bu yüzden `SolitaireBoardState` ref-return API sağlamalıdır.

---

## 8. CardState

Kartın authoritative runtime data'sıdır.

```csharp
namespace Handler.GameModules.Solitaire
{
    public struct CardState
    {
        public int Id;
        public CardSuit Suit;
        public CardRank Rank;
        public bool IsFaceUp;
        public SolitairePileType CurrentPileType;
        public int CurrentPileIndex;
        public int IndexInPile;

        public CardColor Color
        {
            get
            {
                return Suit == CardSuit.Hearts || Suit == CardSuit.Diamonds
                    ? CardColor.Red
                    : CardColor.Black;
            }
        }
    }
}
```

### 8.1 Card id mapping

Önerilen deterministic id mapping:

```text
0  - 12  Hearts A - K
13 - 25  Diamonds A - K
26 - 38  Clubs A - K
39 - 51  Spades A - K
```

Utility:

```csharp
public static class SolitaireCardUtility
{
    public const int CardCount = 52;
    public const int SuitCount = 4;
    public const int RankCount = 13;

    public static int GetCardId(CardSuit suit, CardRank rank)
    {
        return ((int)suit * RankCount) + ((int)rank - 1);
    }

    public static CardSuit GetSuitFromId(int cardId)
    {
        return (CardSuit)(cardId / RankCount);
    }

    public static CardRank GetRankFromId(int cardId)
    {
        return (CardRank)((cardId % RankCount) + 1);
    }

    public static int GetFoundationIndex(CardSuit suit)
    {
        return (int)suit;
    }

    public static bool HasOppositeColor(CardState a, CardState b)
    {
        return a.Color != b.Color;
    }
}
```

---

## 9. PileRef

PileRef, source ve target pile'ları lightweight value olarak taşır.

```csharp
namespace Handler.GameModules.Solitaire
{
    public readonly struct PileRef
    {
        public readonly SolitairePileType Type;
        public readonly int Index;

        public PileRef(SolitairePileType type, int index)
        {
            Type = type;
            Index = index;
        }

        public bool IsValid => Index >= 0;

        public static PileRef Invalid => new PileRef(SolitairePileType.Stock, -1);
    }
}
```

---

## 10. FixedCardPileState

Solitaire'de toplam 52 kart olduğu için pile container allocation yapmadan fixed array ile tutulabilir.

```csharp
using System;

namespace Handler.GameModules.Solitaire
{
    public sealed class FixedCardPileState
    {
        private readonly int[] _cardIds;
        private int _count;

        public SolitairePileType PileType { get; }
        public int PileIndex { get; }
        public int Count => _count;

        public FixedCardPileState(SolitairePileType pileType, int pileIndex, int capacity = 52)
        {
            PileType = pileType;
            PileIndex = pileIndex;
            _cardIds = new int[capacity];
            _count = 0;
        }

        public int this[int index]
        {
            get
            {
                if ((uint)index >= (uint)_count)
                    throw new IndexOutOfRangeException(nameof(index));

                return _cardIds[index];
            }
        }

        public void Add(int cardId)
        {
            if (_count >= _cardIds.Length)
                throw new InvalidOperationException("Pile capacity exceeded.");

            _cardIds[_count] = cardId;
            _count++;
        }

        public void AddRange(int[] source, int startIndex, int count)
        {
            for (int i = 0; i < count; i++)
                Add(source[startIndex + i]);
        }

        public int RemoveTop()
        {
            if (_count == 0)
                return -1;

            _count--;
            int cardId = _cardIds[_count];
            _cardIds[_count] = -1;
            return cardId;
        }

        public void RemoveFromIndex(int startIndex)
        {
            if ((uint)startIndex > (uint)_count)
                throw new IndexOutOfRangeException(nameof(startIndex));

            for (int i = startIndex; i < _count; i++)
                _cardIds[i] = -1;

            _count = startIndex;
        }

        public int PeekTop()
        {
            return _count > 0 ? _cardIds[_count - 1] : -1;
        }

        public bool IsTopCard(int cardId)
        {
            return _count > 0 && _cardIds[_count - 1] == cardId;
        }

        public int IndexOf(int cardId)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_cardIds[i] == cardId)
                    return i;
            }

            return -1;
        }

        public void CopyRangeTo(int startIndex, int[] targetBuffer)
        {
            int length = _count - startIndex;

            for (int i = 0; i < length; i++)
                targetBuffer[i] = _cardIds[startIndex + i];
        }

        public void CopyRangeTo(int startIndex, int[] targetBuffer, out int copiedCount)
        {
            copiedCount = _count - startIndex;

            for (int i = 0; i < copiedCount; i++)
                targetBuffer[i] = _cardIds[startIndex + i];
        }

        public void Clear()
        {
            for (int i = 0; i < _count; i++)
                _cardIds[i] = -1;

            _count = 0;
        }
    }
}
```

### 10.1 Neden List yerine fixed array?

Solitaire kart sayısı sabittir. Fixed array tercihinin avantajları:

- Runtime allocation daha azdır.
- Capacity büyümesi olmaz.
- Pile başına maksimum kapasite bellidir.
- Oyun sırasında `GetRange`, LINQ ve iterator allocation ihtiyacı ortadan kalkar.
- Unit testlerde deterministik davranır.

Case sırasında readability önceliklendirilirse `List<int>` ile başlanabilir. Production path için fixed array daha iyi tercihtir.

---

## 11. SolitaireBoardState

BoardState tüm card ve pile runtime data'sının aggregate root'udur.

```csharp
using System;

namespace Handler.GameModules.Solitaire
{
    public sealed class SolitaireBoardState
    {
        private readonly CardState[] _cards = new CardState[SolitaireCardUtility.CardCount];

        public CardState[] Cards => _cards;

        public FixedCardPileState Stock { get; }
        public FixedCardPileState Waste { get; }
        public FixedCardPileState[] Foundations { get; }
        public FixedCardPileState[] Tableaus { get; }

        public SolitaireBoardState()
        {
            Stock = new FixedCardPileState(SolitairePileType.Stock, 0);
            Waste = new FixedCardPileState(SolitairePileType.Waste, 0);

            Foundations = new FixedCardPileState[4];
            for (int i = 0; i < Foundations.Length; i++)
                Foundations[i] = new FixedCardPileState(SolitairePileType.Foundation, i);

            Tableaus = new FixedCardPileState[7];
            for (int i = 0; i < Tableaus.Length; i++)
                Tableaus[i] = new FixedCardPileState(SolitairePileType.Tableau, i);
        }

        public ref CardState GetCardRef(int cardId)
        {
            if ((uint)cardId >= (uint)_cards.Length)
                throw new IndexOutOfRangeException(nameof(cardId));

            return ref _cards[cardId];
        }

        public CardState GetCard(int cardId)
        {
            if ((uint)cardId >= (uint)_cards.Length)
                throw new IndexOutOfRangeException(nameof(cardId));

            return _cards[cardId];
        }

        public FixedCardPileState GetPile(PileRef pileRef)
        {
            return GetPile(pileRef.Type, pileRef.Index);
        }

        public FixedCardPileState GetPile(SolitairePileType pileType, int pileIndex)
        {
            return pileType switch
            {
                SolitairePileType.Stock => Stock,
                SolitairePileType.Waste => Waste,
                SolitairePileType.Foundation => Foundations[pileIndex],
                SolitairePileType.Tableau => Tableaus[pileIndex],
                _ => throw new ArgumentOutOfRangeException(nameof(pileType), pileType, null)
            };
        }

        public void InitializeCards()
        {
            for (int suitIndex = 0; suitIndex < 4; suitIndex++)
            {
                for (int rankIndex = 1; rankIndex <= 13; rankIndex++)
                {
                    var suit = (CardSuit)suitIndex;
                    var rank = (CardRank)rankIndex;
                    int cardId = SolitaireCardUtility.GetCardId(suit, rank);

                    _cards[cardId] = new CardState
                    {
                        Id = cardId,
                        Suit = suit,
                        Rank = rank,
                        IsFaceUp = false,
                        CurrentPileType = SolitairePileType.Stock,
                        CurrentPileIndex = 0,
                        IndexInPile = -1
                    };
                }
            }
        }

        public void ClearPiles()
        {
            Stock.Clear();
            Waste.Clear();

            for (int i = 0; i < Foundations.Length; i++)
                Foundations[i].Clear();

            for (int i = 0; i < Tableaus.Length; i++)
                Tableaus[i].Clear();
        }

        public void ReindexPile(FixedCardPileState pile)
        {
            for (int i = 0; i < pile.Count; i++)
            {
                int cardId = pile[i];
                ref CardState card = ref GetCardRef(cardId);
                card.CurrentPileType = pile.PileType;
                card.CurrentPileIndex = pile.PileIndex;
                card.IndexInPile = i;
            }
        }

        public void ReindexAllPiles()
        {
            ReindexPile(Stock);
            ReindexPile(Waste);

            for (int i = 0; i < Foundations.Length; i++)
                ReindexPile(Foundations[i]);

            for (int i = 0; i < Tableaus.Length; i++)
                ReindexPile(Tableaus[i]);
        }
    }
}
```

### 11.1 BoardState invariants

BoardState her mutation sonrası şu invariantları korumalıdır:

```text
Her card id tam olarak bir pile içinde bulunur.
CardState.CurrentPileType gerçek pile ile eşleşir.
CardState.CurrentPileIndex gerçek pile index ile eşleşir.
CardState.IndexInPile gerçek array index ile eşleşir.
Stock kartları face down kalır.
Waste kartları face up kalır.
Foundation kartları face up kalır.
Tableau içinde face down kartlar açık kartların altında kalır.
```

Development build için integrity validation eklenebilir.

---

## 12. Runtime Context

RuntimeContext, session bazlı data ve registry container'dır.

```csharp
namespace Handler.GameModules.Solitaire
{
    public sealed class SolitaireRuntimeContext
    {
        public SolitaireBoardState BoardState { get; }
        public SolitaireViewRegistry ViewRegistry { get; }
        public SolitaireSelectionState SelectionState { get; }
        public SolitaireMoveHistory MoveHistory { get; }

        public SolitaireRuntimeContext(
            SolitaireBoardState boardState,
            SolitaireViewRegistry viewRegistry,
            SolitaireSelectionState selectionState,
            SolitaireMoveHistory moveHistory)
        {
            BoardState = boardState;
            ViewRegistry = viewRegistry;
            SelectionState = selectionState;
            MoveHistory = moveHistory;
        }
    }
}
```

Bu context her new game session için yeniden oluşturulabilir veya resetlenebilir. SO asset içine yazılmaz.

---

## 13. View Registry

Scene içinde 52 kart hazır olduğu için runtime lookup array ile yapılmalıdır.

```csharp
using System;

namespace Handler.GameModules.Solitaire
{
    public sealed class SolitaireViewRegistry
    {
        private readonly CardView[] _cardViews = new CardView[SolitaireCardUtility.CardCount];

        public void Register(int cardId, CardView cardView)
        {
            if ((uint)cardId >= (uint)_cardViews.Length)
                throw new IndexOutOfRangeException(nameof(cardId));

            _cardViews[cardId] = cardView;
        }

        public CardView GetCardView(int cardId)
        {
            if ((uint)cardId >= (uint)_cardViews.Length)
                throw new IndexOutOfRangeException(nameof(cardId));

            return _cardViews[cardId];
        }

        public bool IsComplete()
        {
            for (int i = 0; i < _cardViews.Length; i++)
            {
                if (_cardViews[i] == null)
                    return false;
            }

            return true;
        }
    }
}
```

Dictionary yerine array tercih edilir çünkü card id aralığı 0 ile 51 arasında sabittir.

---

## 14. Move Model

### 14.1 SolitaireMove

```csharp
namespace Handler.GameModules.Solitaire
{
    public readonly struct SolitaireMove
    {
        public readonly SolitaireMoveType MoveType;
        public readonly PileRef Source;
        public readonly PileRef Target;
        public readonly int StartCardId;
        public readonly int SourceStartIndex;
        public readonly int CardCount;

        public SolitaireMove(
            SolitaireMoveType moveType,
            PileRef source,
            PileRef target,
            int startCardId,
            int sourceStartIndex,
            int cardCount)
        {
            MoveType = moveType;
            Source = source;
            Target = target;
            StartCardId = startCardId;
            SourceStartIndex = sourceStartIndex;
            CardCount = cardCount;
        }

        public bool IsValid => MoveType != SolitaireMoveType.None && CardCount > 0;
    }
}
```

### 14.2 SolitaireMoveResult

```csharp
namespace Handler.GameModules.Solitaire
{
    public readonly struct SolitaireMoveResult
    {
        public readonly bool Success;
        public readonly SolitaireMove Move;
        public readonly int AutoFlippedCardId;

        public SolitaireMoveResult(bool success, SolitaireMove move, int autoFlippedCardId)
        {
            Success = success;
            Move = move;
            AutoFlippedCardId = autoFlippedCardId;
        }

        public bool HasAutoFlip => AutoFlippedCardId >= 0;

        public static SolitaireMoveResult Failed =>
            new SolitaireMoveResult(false, default, -1);
    }
}
```

---

## 15. MoveResolver

MoveResolver oyun kuralı decision layer'dır. Controller input sonrası buraya sorar.

Controller'ın sorusu:

```text
Bu card id, bu target pile'a gidebilir mi?
```

Teknik karşılığı:

```csharp
bool canMove = _moveResolver.TryCreateMove(
    _context.BoardState,
    startCardId,
    targetPile,
    out SolitaireMove move);
```

### 15.1 Sorumluluklar

MoveResolver şunları yapar:

- Card id'nin source pile bilgisini BoardState'ten okur.
- Kartın face up olup olmadığını kontrol eder.
- Waste için sadece top card kuralını kontrol eder.
- Tableau için açık seri valid mi kontrol eder.
- Foundation için sadece tek kart ve suit sırası valid mi kontrol eder.
- Target tableau boşsa King kuralını kontrol eder.
- Target tableau doluysa alternating color ve descending rank kuralını kontrol eder.
- Legal ise immutable `SolitaireMove` üretir.

MoveResolver şunları yapmaz:

- BoardState mutate etmez.
- CardView hareket ettirmez.
- Animation başlatmaz.
- Input state tutmaz.

### 15.2 Örnek MoveResolver

```csharp
namespace Handler.GameModules.Solitaire
{
    public sealed class SolitaireMoveResolver
    {
        private readonly SolitaireDeckConfigSO _config;

        public SolitaireMoveResolver(SolitaireDeckConfigSO config)
        {
            _config = config;
        }

        public bool TryCreateMove(
            SolitaireBoardState board,
            int startCardId,
            PileRef target,
            out SolitaireMove move)
        {
            move = default;

            CardState startCard = board.GetCard(startCardId);

            if (!startCard.IsFaceUp)
                return false;

            var source = new PileRef(startCard.CurrentPileType, startCard.CurrentPileIndex);

            if (!CanMoveFromSource(board, startCardId, source, out int sourceStartIndex, out int cardCount))
                return false;

            if (!CanMoveToTarget(board, startCardId, cardCount, target))
                return false;

            var moveType = ResolveMoveType(source, target, cardCount);

            move = new SolitaireMove(
                moveType,
                source,
                target,
                startCardId,
                sourceStartIndex,
                cardCount);

            return true;
        }

        private bool CanMoveFromSource(
            SolitaireBoardState board,
            int startCardId,
            PileRef source,
            out int sourceStartIndex,
            out int cardCount)
        {
            sourceStartIndex = -1;
            cardCount = 0;

            FixedCardPileState sourcePile = board.GetPile(source);
            sourceStartIndex = sourcePile.IndexOf(startCardId);

            if (sourceStartIndex < 0)
                return false;

            switch (source.Type)
            {
                case SolitairePileType.Waste:
                    if (!sourcePile.IsTopCard(startCardId))
                        return false;
                    cardCount = 1;
                    return true;

                case SolitairePileType.Foundation:
                    if (!_config.AllowFoundationToTableau)
                        return false;
                    if (!sourcePile.IsTopCard(startCardId))
                        return false;
                    cardCount = 1;
                    return true;

                case SolitairePileType.Tableau:
                    if (!IsValidFaceUpTableauSequence(board, sourcePile, sourceStartIndex))
                        return false;
                    cardCount = sourcePile.Count - sourceStartIndex;
                    return true;

                default:
                    return false;
            }
        }

        private bool CanMoveToTarget(
            SolitaireBoardState board,
            int startCardId,
            int cardCount,
            PileRef target)
        {
            return target.Type switch
            {
                SolitairePileType.Tableau => CanMoveToTableau(board, startCardId, target),
                SolitairePileType.Foundation => CanMoveToFoundation(board, startCardId, cardCount, target),
                _ => false
            };
        }

        private bool CanMoveToTableau(SolitaireBoardState board, int startCardId, PileRef target)
        {
            CardState movingCard = board.GetCard(startCardId);
            FixedCardPileState targetPile = board.GetPile(target);

            if (targetPile.Count == 0)
                return movingCard.Rank == CardRank.King;

            int topTargetCardId = targetPile.PeekTop();
            CardState topTargetCard = board.GetCard(topTargetCardId);

            if (!topTargetCard.IsFaceUp)
                return false;

            bool isDescending = (int)movingCard.Rank == (int)topTargetCard.Rank - 1;
            bool isAlternatingColor = movingCard.Color != topTargetCard.Color;

            return isDescending && isAlternatingColor;
        }

        private bool CanMoveToFoundation(
            SolitaireBoardState board,
            int startCardId,
            int cardCount,
            PileRef target)
        {
            if (cardCount != 1)
                return false;

            CardState movingCard = board.GetCard(startCardId);
            int expectedFoundationIndex = SolitaireCardUtility.GetFoundationIndex(movingCard.Suit);

            if (target.Index != expectedFoundationIndex)
                return false;

            FixedCardPileState targetPile = board.GetPile(target);

            if (targetPile.Count == 0)
                return movingCard.Rank == CardRank.Ace;

            int topTargetCardId = targetPile.PeekTop();
            CardState topTargetCard = board.GetCard(topTargetCardId);

            bool sameSuit = movingCard.Suit == topTargetCard.Suit;
            bool ascending = (int)movingCard.Rank == (int)topTargetCard.Rank + 1;

            return sameSuit && ascending;
        }

        private bool IsValidFaceUpTableauSequence(
            SolitaireBoardState board,
            FixedCardPileState pile,
            int startIndex)
        {
            for (int i = startIndex; i < pile.Count; i++)
            {
                CardState current = board.GetCard(pile[i]);

                if (!current.IsFaceUp)
                    return false;

                if (i == startIndex)
                    continue;

                CardState previous = board.GetCard(pile[i - 1]);

                bool descending = (int)current.Rank == (int)previous.Rank - 1;
                bool alternatingColor = current.Color != previous.Color;

                if (!descending || !alternatingColor)
                    return false;
            }

            return true;
        }

        private static SolitaireMoveType ResolveMoveType(PileRef source, PileRef target, int cardCount)
        {
            if (source.Type == SolitairePileType.Waste && target.Type == SolitairePileType.Tableau)
                return SolitaireMoveType.WasteToTableau;

            if (source.Type == SolitairePileType.Waste && target.Type == SolitairePileType.Foundation)
                return SolitaireMoveType.WasteToFoundation;

            if (source.Type == SolitairePileType.Tableau && target.Type == SolitairePileType.Tableau)
                return SolitaireMoveType.TableauToTableau;

            if (source.Type == SolitairePileType.Tableau && target.Type == SolitairePileType.Foundation)
                return SolitaireMoveType.TableauToFoundation;

            if (source.Type == SolitairePileType.Foundation && target.Type == SolitairePileType.Tableau)
                return SolitaireMoveType.FoundationToTableau;

            return SolitaireMoveType.None;
        }
    }
}
```

---

## 16. AutoMove Resolver

Double tap davranışı için ayrı resolver tutulur.

Case için önerilen karar:

```text
Double tap sadece foundation auto move yapar.
Drag ise tableau ve foundation targetlarını destekler.
```

Bu mobil Solitaire UX için sade, anlaşılır ve hataya kapalıdır.

```csharp
namespace Handler.GameModules.Solitaire
{
    public sealed class SolitaireAutoMoveResolver
    {
        private readonly SolitaireMoveResolver _moveResolver;

        public SolitaireAutoMoveResolver(SolitaireMoveResolver moveResolver)
        {
            _moveResolver = moveResolver;
        }

        public bool TryCreateFoundationAutoMove(
            SolitaireBoardState board,
            int cardId,
            out SolitaireMove move)
        {
            CardState card = board.GetCard(cardId);
            int foundationIndex = SolitaireCardUtility.GetFoundationIndex(card.Suit);
            var target = new PileRef(SolitairePileType.Foundation, foundationIndex);

            return _moveResolver.TryCreateMove(board, cardId, target, out move);
        }
    }
}
```

---

## 17. MoveExecutor

MoveExecutor BoardState'i mutate eden tek merkezdir.

### 17.1 Sorumluluklar

MoveExecutor şunları yapar:

- Source pile'dan kartları çıkarır.
- Target pile'a kartları ekler.
- Moved kartların `CurrentPileType`, `CurrentPileIndex`, `IndexInPile` değerlerini günceller.
- Gerekirse tableau source top card'ını auto flip yapar.
- MoveHistory için undo record üretir.

MoveExecutor şunları yapmaz:

- Input dinlemez.
- Hamle legal mi diye karar vermez.
- Slot pozisyonu hesaplamaz.
- Animation oynatmaz.

### 17.2 Örnek MoveExecutor

```csharp
namespace Handler.GameModules.Solitaire
{
    public sealed class SolitaireMoveExecutor
    {
        private readonly SolitaireDeckConfigSO _config;
        private readonly int[] _moveBuffer = new int[SolitaireCardUtility.CardCount];

        public SolitaireMoveExecutor(SolitaireDeckConfigSO config)
        {
            _config = config;
        }

        public SolitaireMoveResult Execute(SolitaireBoardState board, SolitaireMove move)
        {
            if (!move.IsValid)
                return SolitaireMoveResult.Failed;

            FixedCardPileState sourcePile = board.GetPile(move.Source);
            FixedCardPileState targetPile = board.GetPile(move.Target);

            sourcePile.CopyRangeTo(move.SourceStartIndex, _moveBuffer, out int movedCount);
            sourcePile.RemoveFromIndex(move.SourceStartIndex);

            for (int i = 0; i < movedCount; i++)
                targetPile.Add(_moveBuffer[i]);

            board.ReindexPile(sourcePile);
            board.ReindexPile(targetPile);

            int autoFlippedCardId = TryAutoFlipSourceTableauTop(board, move.Source);

            return new SolitaireMoveResult(true, move, autoFlippedCardId);
        }

        private int TryAutoFlipSourceTableauTop(SolitaireBoardState board, PileRef source)
        {
            if (!_config.AutoFlipTableauTopCard)
                return -1;

            if (source.Type != SolitairePileType.Tableau)
                return -1;

            FixedCardPileState pile = board.GetPile(source);

            if (pile.Count == 0)
                return -1;

            int topCardId = pile.PeekTop();
            ref CardState topCard = ref board.GetCardRef(topCardId);

            if (topCard.IsFaceUp)
                return -1;

            topCard.IsFaceUp = true;
            return topCardId;
        }
    }
}
```

---

## 18. Deck Initialization ve Deal Flow

### 18.1 DeckController New Game Flow

```text
1. BoardState.InitializeCards()
2. BoardState.ClearPiles()
3. 0 - 51 arası card id listesi oluşturulur.
4. Seed ile shuffle yapılır.
5. Tableau dağıtılır.
6. Kalan kartlar Stock'a eklenir.
7. BoardState.ReindexAllPiles()
8. CardView identity ve sprite refresh edilir.
9. LayoutController tüm kartların final pozisyonlarını hesaplar.
10. CardView'lar animasyonsuz initial pozisyona set edilir.
```

### 18.2 Shuffle Utility

```csharp
using System;

namespace Handler.GameModules.Solitaire
{
    public static class SolitaireShuffleUtility
    {
        public static void FillOrderedDeck(int[] cardIds)
        {
            for (int i = 0; i < SolitaireCardUtility.CardCount; i++)
                cardIds[i] = i;
        }

        public static void Shuffle(int[] cardIds, int seed)
        {
            var random = new Random(seed);

            for (int i = cardIds.Length - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                int temp = cardIds[i];
                cardIds[i] = cardIds[swapIndex];
                cardIds[swapIndex] = temp;
            }
        }
    }
}
```

### 18.3 Deal algoritması

```csharp
private void DealNewGame(SolitaireBoardState board, int[] shuffledDeck)
{
    int deckCursor = 0;

    for (int tableauIndex = 0; tableauIndex < 7; tableauIndex++)
    {
        FixedCardPileState tableau = board.Tableaus[tableauIndex];

        for (int row = 0; row <= tableauIndex; row++)
        {
            int cardId = shuffledDeck[deckCursor];
            deckCursor++;

            ref CardState card = ref board.GetCardRef(cardId);
            card.IsFaceUp = row == tableauIndex;

            tableau.Add(cardId);
        }
    }

    while (deckCursor < shuffledDeck.Length)
    {
        int cardId = shuffledDeck[deckCursor];
        deckCursor++;

        ref CardState card = ref board.GetCardRef(cardId);
        card.IsFaceUp = false;

        board.Stock.Add(cardId);
    }

    board.ReindexAllPiles();
}
```

---

## 19. Stock ve Waste Data Flow

### 19.1 Stock tap

Stock slotuna tap gelince:

```text
InputController
-> DeckController.RequestDrawFromStock()
-> DeckController stock count kontrol eder
-> Stock doluysa StockToWaste execute edilir
-> Stock boş ve Waste doluysa WasteRecycleToStock execute edilir
-> Layout refresh edilir
```

### 19.2 Draw One

Data davranışı:

```text
Stock top card çıkarılır.
Card face up yapılır.
Waste top olarak eklenir.
```

### 19.3 Waste recycle

Data davranışı:

```text
Waste kartları ters sırayla Stock'a geri alınır.
Kartlar face down yapılır.
Waste clear edilir.
Stock tekrar draw edilebilir hale gelir.
```

### 19.4 Waste playable rule

Default karar:

```text
Waste pile içinde sadece top card oynanabilir.
```

Draw 3 ileride açılırsa presentation layer son 3 kartı gösterebilir, ama legal move yine top card üzerinden yapılır.

---

## 20. LayoutController

LayoutController slot anchorlarını ve BoardState'i kullanarak kart pozisyonu hesaplar.

### 20.1 Slot X pozisyonu

Tableau X pozisyonları scene'deki `TableauSlot_00 ... TableauSlot_06` transformlarından gelir.

```text
TableauSlot_00 transform.position.x
TableauSlot_01 transform.position.x
...
TableauSlot_06 transform.position.x
```

### 20.2 Tableau Y pozisyonu

Kartın Y pozisyonu pile içindeki önceki kartların sayısı ve face state'ine göre hesaplanır.

```csharp
using UnityEngine;

namespace Handler.GameModules.Solitaire
{
    public sealed class SolitaireLayoutController : MonoBehaviour
    {
        [SerializeField] private SolitaireSlotAnchor stockSlot;
        [SerializeField] private SolitaireSlotAnchor wasteSlot;
        [SerializeField] private SolitaireSlotAnchor[] foundationSlots = new SolitaireSlotAnchor[4];
        [SerializeField] private SolitaireSlotAnchor[] tableauSlots = new SolitaireSlotAnchor[7];
        [SerializeField] private Transform dragParent;

        private SolitaireDeckConfigSO _config;
        private SolitaireRuntimeContext _context;

        public Transform DragParent => dragParent;

        public void Initialize(SolitaireDeckConfigSO config, SolitaireRuntimeContext context)
        {
            _config = config;
            _context = context;
        }

        public Vector3 GetCardWorldPosition(PileRef pileRef, int cardIndex)
        {
            return pileRef.Type switch
            {
                SolitairePileType.Stock => GetStockPosition(cardIndex),
                SolitairePileType.Waste => GetWastePosition(cardIndex),
                SolitairePileType.Foundation => GetFoundationPosition(pileRef.Index, cardIndex),
                SolitairePileType.Tableau => GetTableauPosition(pileRef.Index, cardIndex),
                _ => Vector3.zero
            };
        }

        public Vector3 GetTableauPosition(int tableauIndex, int cardIndex)
        {
            FixedCardPileState pile = _context.BoardState.Tableaus[tableauIndex];
            Vector3 position = tableauSlots[tableauIndex].transform.position;
            float yOffset = 0f;

            for (int i = 0; i < cardIndex; i++)
            {
                int previousCardId = pile[i];
                CardState previousCard = _context.BoardState.GetCard(previousCardId);

                yOffset += previousCard.IsFaceUp
                    ? _config.FaceUpTableauYOffset
                    : _config.FaceDownTableauYOffset;
            }

            position += Vector3.down * yOffset;
            position += Vector3.forward * (_config.CardZStep * cardIndex);
            return position;
        }

        private Vector3 GetStockPosition(int cardIndex)
        {
            Vector3 position = stockSlot.transform.position;
            position += Vector3.forward * (_config.StockZStep * cardIndex);
            return position;
        }

        private Vector3 GetWastePosition(int cardIndex)
        {
            Vector3 position = wasteSlot.transform.position;

            if (_config.DrawMode == SolitaireDrawMode.DrawThree)
            {
                int visibleIndex = Mathf.Clamp(cardIndex, 0, 2);
                position += Vector3.right * (_config.WasteStackXOffset * visibleIndex);
            }

            position += Vector3.forward * (_config.CardZStep * cardIndex);
            return position;
        }

        private Vector3 GetFoundationPosition(int foundationIndex, int cardIndex)
        {
            Vector3 position = foundationSlots[foundationIndex].transform.position;
            position += Vector3.forward * (_config.CardZStep * cardIndex);
            return position;
        }
    }
}
```

### 20.3 Layout refresh akışı

```text
Move executed
-> BoardState updated
-> LayoutController iterates piles
-> Her cardId için target world position hesaplanır
-> CardView animate veya snap edilir
-> CardView sorting order güncellenir
```

---

## 21. SolitaireSlotAnchor

```csharp
using UnityEngine;

namespace Handler.GameModules.Solitaire
{
    public sealed class SolitaireSlotAnchor : MonoBehaviour
    {
        [SerializeField] private SolitairePileType pileType;
        [SerializeField] private int pileIndex;
        [SerializeField] private CardSuit foundationSuit;

        public SolitairePileType PileType => pileType;
        public int PileIndex => pileIndex;
        public CardSuit FoundationSuit => foundationSuit;
        public Vector3 Position => transform.position;

        public PileRef ToPileRef()
        {
            if (pileType == SolitairePileType.Foundation)
                return new PileRef(pileType, SolitaireCardUtility.GetFoundationIndex(foundationSuit));

            return new PileRef(pileType, pileIndex);
        }
    }
}
```

---

## 22. CardRuntimeIdentity

```csharp
using UnityEngine;

namespace Handler.GameModules.Solitaire
{
    public sealed class CardRuntimeIdentity : MonoBehaviour
    {
        public int RuntimeId { get; private set; } = -1;
        public CardSuit Suit { get; private set; }
        public CardRank Rank { get; private set; }

        public bool IsInitialized => RuntimeId >= 0;

        public void Initialize(int runtimeId, CardSuit suit, CardRank rank)
        {
            RuntimeId = runtimeId;
            Suit = suit;
            Rank = rank;
        }
    }
}
```

---

## 23. CardView

CardView görsel layer'dır. BoardState mutate etmez.

```csharp
using UnityEngine;

namespace Handler.GameModules.Solitaire
{
    public sealed class CardView : MonoBehaviour
    {
        [SerializeField] private CardRuntimeIdentity identity;
        [SerializeField] private CardVisualStateMachine stateMachine;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BoxCollider2D boxCollider;

        private SolitaireDeckConfigSO _config;

        public int CardId => identity.RuntimeId;
        public CardVisualState VisualState => stateMachine.CurrentState;
        public Transform CachedTransform { get; private set; }

        private void Awake()
        {
            CachedTransform = transform;
        }

        public void Initialize(SolitaireDeckConfigSO config, int cardId, CardSuit suit, CardRank rank)
        {
            _config = config;
            identity.Initialize(cardId, suit, rank);
            RefreshFace(false);
        }

        public void RefreshFace(bool isFaceUp)
        {
            spriteRenderer.sprite = isFaceUp
                ? _config.GetFrontSprite(identity.Suit, identity.Rank)
                : _config.CardBackSprite;

            stateMachine.SetFaceState(isFaceUp);
        }

        public void SetInputEnabled(bool isEnabled)
        {
            boxCollider.enabled = isEnabled;
        }

        public void SetSortingOrder(int sortingOrder)
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }

        public void SnapTo(Vector3 worldPosition)
        {
            CachedTransform.position = worldPosition;
        }

        public void MoveTo(Vector3 worldPosition, float duration)
        {
            // Tween veya coroutine implementation burada bağlanır.
            CachedTransform.position = worldPosition;
        }
    }
}
```

---

## 24. CardVisualStateMachine

Kart state machine'i sadece visual ve input state yönetir.

### 24.1 State machine sorumlulukları

Doğru:

```text
Kart input alabilir mi?
Kart drag sırasında mı?
Kart animation sırasında mı?
Kart selected mı?
Kart face down mı?
Invalid move sonrası returning state'e geçmeli mi?
```

Yanlış:

```text
Kart foundation'a gidebilir mi?
Kart tableau'ya gidebilir mi?
Kart hangi pile'a ait?
Oyun kazanıldı mı?
```

Bu yanlış sorular BoardState, MoveResolver ve MoveExecutor tarafındadır.

### 24.2 Örnek state machine

```csharp
using UnityEngine;

namespace Handler.GameModules.Solitaire
{
    public sealed class CardVisualStateMachine : MonoBehaviour
    {
        public CardVisualState CurrentState { get; private set; } = CardVisualState.Inactive;

        public bool CanReceivePointer =>
            CurrentState == CardVisualState.FaceUpIdle ||
            CurrentState == CardVisualState.Selected ||
            CurrentState == CardVisualState.InStock;

        public bool IsBusy =>
            CurrentState == CardVisualState.Dragging ||
            CurrentState == CardVisualState.Moving ||
            CurrentState == CardVisualState.Returning ||
            CurrentState == CardVisualState.Locked;

        public void SetFaceState(bool isFaceUp)
        {
            CurrentState = isFaceUp
                ? CardVisualState.FaceUpIdle
                : CardVisualState.FaceDown;
        }

        public void SetInStock()
        {
            CurrentState = CardVisualState.InStock;
        }

        public void SetSelected()
        {
            CurrentState = CardVisualState.Selected;
        }

        public void SetDragging()
        {
            CurrentState = CardVisualState.Dragging;
        }

        public void SetMoving()
        {
            CurrentState = CardVisualState.Moving;
        }

        public void SetReturning()
        {
            CurrentState = CardVisualState.Returning;
        }

        public void SetLocked()
        {
            CurrentState = CardVisualState.Locked;
        }
    }
}
```

---

## 25. Input Flow

### 25.1 Drag flow

```text
CardInputReceiver.OnPointerDown(cardId)
-> CardDragBehaviour drag threshold bekler
-> SolitaireInputController.OnDragStarted(cardId)
-> DeckController.CanStartDrag(cardId)
-> MoveResolver source drag valid mi kontrol eder
-> Valid ise kartlar DragParent'a alınır
-> Drag sırasında world position update edilir
-> Drop anında target slot raycast edilir
-> DeckController.RequestDrop(cardId, targetPile)
-> MoveResolver.TryCreateMove(board, cardId, targetPile, out move)
-> MoveExecutor.Execute(board, move)
-> LayoutController.RefreshBoard()
```

### 25.2 Double tap flow

```text
CardInputReceiver.OnDoubleTap(cardId)
-> SolitaireInputController.OnDoubleTap(cardId)
-> DeckController.RequestAutoMoveToFoundation(cardId)
-> AutoMoveResolver.TryCreateFoundationAutoMove(board, cardId, out move)
-> MoveExecutor.Execute(board, move)
-> LayoutController.RefreshBoard()
```

### 25.3 Stock tap flow

```text
StockSlot input detected
-> SolitaireInputController.OnStockTapped()
-> DeckController.RequestDrawFromStock()
-> StockToWaste veya WasteRecycleToStock execute edilir
-> LayoutController.RefreshBoard()
```

---

## 26. SolitaireDeckController

DeckController gameplay action orchestrator'dır. Kural çözmez, state mutation'ı doğrudan yapmaz.

### 26.1 Sorumluluklar

DeckController şunları yapar:

- New game başlatır.
- Scene'deki 52 kartı context'e register eder.
- BoardState initialization ve deal flow'u tetikler.
- Input requestlerini alır.
- MoveResolver'a legal move sorar.
- MoveExecutor ile legal move'u uygular.
- Layout refresh ve card animation tetikler.
- Auto move requestlerini yönetir.
- Stock draw requestlerini yönetir.

DeckController şunları yapmaz:

- Global game lifecycle yönetmez.
- UI yönetmez.
- Rule logic'i içinde tutmaz.
- BoardState arraylerini dışarıdan elle mutate etmez.
- Singleton olmaz.

### 26.2 Örnek DeckController iskeleti

```csharp
using UnityEngine;

namespace Handler.GameModules.Solitaire
{
    public sealed class SolitaireDeckController : MonoBehaviour
    {
        private SolitaireDeckConfigSO _config;
        private SolitaireRuntimeContext _context;
        private SolitaireMoveResolver _moveResolver;
        private SolitaireAutoMoveResolver _autoMoveResolver;
        private SolitaireMoveExecutor _moveExecutor;
        private SolitaireLayoutController _layoutController;

        private readonly int[] _deckBuffer = new int[SolitaireCardUtility.CardCount];

        public void Initialize(
            SolitaireDeckConfigSO config,
            SolitaireRuntimeContext context,
            SolitaireMoveResolver moveResolver,
            SolitaireAutoMoveResolver autoMoveResolver,
            SolitaireMoveExecutor moveExecutor,
            SolitaireLayoutController layoutController)
        {
            _config = config;
            _context = context;
            _moveResolver = moveResolver;
            _autoMoveResolver = autoMoveResolver;
            _moveExecutor = moveExecutor;
            _layoutController = layoutController;
        }

        public void StartNewGame(int seed)
        {
            SolitaireBoardState board = _context.BoardState;

            board.InitializeCards();
            board.ClearPiles();

            SolitaireShuffleUtility.FillOrderedDeck(_deckBuffer);
            SolitaireShuffleUtility.Shuffle(_deckBuffer, seed);

            DealNewGame(board, _deckBuffer);
            RefreshAllCardViews(immediate: true);
        }

        public bool RequestDrop(int startCardId, PileRef targetPile)
        {
            SolitaireBoardState board = _context.BoardState;

            if (!_moveResolver.TryCreateMove(board, startCardId, targetPile, out SolitaireMove move))
            {
                ReturnDraggedCardsToSource(startCardId);
                return false;
            }

            SolitaireMoveResult result = _moveExecutor.Execute(board, move);

            if (!result.Success)
            {
                ReturnDraggedCardsToSource(startCardId);
                return false;
            }

            RefreshAllCardViews(immediate: false);
            return true;
        }

        public bool RequestAutoMoveToFoundation(int cardId)
        {
            SolitaireBoardState board = _context.BoardState;

            if (!_autoMoveResolver.TryCreateFoundationAutoMove(board, cardId, out SolitaireMove move))
            {
                PlayInvalidFeedback(cardId);
                return false;
            }

            SolitaireMoveResult result = _moveExecutor.Execute(board, move);

            if (!result.Success)
            {
                PlayInvalidFeedback(cardId);
                return false;
            }

            RefreshAllCardViews(immediate: false);
            return true;
        }

        public void RequestDrawFromStock()
        {
            SolitaireBoardState board = _context.BoardState;

            if (board.Stock.Count > 0)
            {
                DrawFromStockToWaste(board);
                RefreshAllCardViews(immediate: false);
                return;
            }

            if (board.Waste.Count > 0)
            {
                RecycleWasteToStock(board);
                RefreshAllCardViews(immediate: false);
            }
        }

        private void DrawFromStockToWaste(SolitaireBoardState board)
        {
            int drawCount = (int)_config.DrawMode;

            for (int i = 0; i < drawCount; i++)
            {
                if (board.Stock.Count == 0)
                    break;

                int cardId = board.Stock.RemoveTop();
                ref CardState card = ref board.GetCardRef(cardId);
                card.IsFaceUp = true;
                board.Waste.Add(cardId);
            }

            board.ReindexPile(board.Stock);
            board.ReindexPile(board.Waste);
        }

        private void RecycleWasteToStock(SolitaireBoardState board)
        {
            while (board.Waste.Count > 0)
            {
                int cardId = board.Waste.RemoveTop();
                ref CardState card = ref board.GetCardRef(cardId);
                card.IsFaceUp = false;
                board.Stock.Add(cardId);
            }

            board.ReindexPile(board.Waste);
            board.ReindexPile(board.Stock);
        }

        private void DealNewGame(SolitaireBoardState board, int[] shuffledDeck)
        {
            int deckCursor = 0;

            for (int tableauIndex = 0; tableauIndex < 7; tableauIndex++)
            {
                FixedCardPileState tableau = board.Tableaus[tableauIndex];

                for (int row = 0; row <= tableauIndex; row++)
                {
                    int cardId = shuffledDeck[deckCursor];
                    deckCursor++;

                    ref CardState card = ref board.GetCardRef(cardId);
                    card.IsFaceUp = row == tableauIndex;
                    tableau.Add(cardId);
                }
            }

            while (deckCursor < shuffledDeck.Length)
            {
                int cardId = shuffledDeck[deckCursor];
                deckCursor++;

                ref CardState card = ref board.GetCardRef(cardId);
                card.IsFaceUp = false;
                board.Stock.Add(cardId);
            }

            board.ReindexAllPiles();
        }

        private void RefreshAllCardViews(bool immediate)
        {
            SolitaireBoardState board = _context.BoardState;

            RefreshPile(board.Stock, immediate);
            RefreshPile(board.Waste, immediate);

            for (int i = 0; i < board.Foundations.Length; i++)
                RefreshPile(board.Foundations[i], immediate);

            for (int i = 0; i < board.Tableaus.Length; i++)
                RefreshPile(board.Tableaus[i], immediate);
        }

        private void RefreshPile(FixedCardPileState pile, bool immediate)
        {
            var pileRef = new PileRef(pile.PileType, pile.PileIndex);

            for (int i = 0; i < pile.Count; i++)
            {
                int cardId = pile[i];
                CardState card = _context.BoardState.GetCard(cardId);
                CardView view = _context.ViewRegistry.GetCardView(cardId);
                Vector3 targetPosition = _layoutController.GetCardWorldPosition(pileRef, i);

                view.RefreshFace(card.IsFaceUp);
                view.SetSortingOrder(CalculateSortingOrder(pile, i));

                if (immediate)
                    view.SnapTo(targetPosition);
                else
                    view.MoveTo(targetPosition, _config.MoveAnimationDuration);
            }
        }

        private int CalculateSortingOrder(FixedCardPileState pile, int index)
        {
            int baseOrder = pile.PileType switch
            {
                SolitairePileType.Stock => 100,
                SolitairePileType.Waste => 300,
                SolitairePileType.Foundation => 500,
                SolitairePileType.Tableau => 1000 + pile.PileIndex * 100,
                _ => 0
            };

            return baseOrder + index;
        }

        private void ReturnDraggedCardsToSource(int startCardId)
        {
            RefreshAllCardViews(immediate: false);
        }

        private void PlayInvalidFeedback(int cardId)
        {
            CardView view = _context.ViewRegistry.GetCardView(cardId);
            view.MoveTo(view.CachedTransform.position, _config.InvalidMoveReturnDuration);
        }
    }
}
```

---

## 27. SolitaireModuleInstaller

Installer scene reference'larını toplar ve runtime graph'i kurar.

```csharp
using UnityEngine;

namespace Handler.GameModules.Solitaire
{
    public sealed class SolitaireModuleInstaller : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private SolitaireDeckConfigSO config;

        [Header("Scene References")]
        [SerializeField] private Transform deckParent;
        [SerializeField] private SolitaireDeckController deckController;
        [SerializeField] private SolitaireInputController inputController;
        [SerializeField] private SolitaireLayoutController layoutController;

        private SolitaireRuntimeContext _context;

        private void Awake()
        {
            BuildRuntimeContext();
            BindSceneCards();
            InitializeControllers();
        }

        private void Start()
        {
            int seed = GenerateSessionSeed();
            deckController.StartNewGame(seed);
        }

        private void BuildRuntimeContext()
        {
            var boardState = new SolitaireBoardState();
            var viewRegistry = new SolitaireViewRegistry();
            var selectionState = new SolitaireSelectionState();
            var moveHistory = new SolitaireMoveHistory();

            _context = new SolitaireRuntimeContext(
                boardState,
                viewRegistry,
                selectionState,
                moveHistory);
        }

        private void BindSceneCards()
        {
            CardView[] cards = deckParent.GetComponentsInChildren<CardView>(includeInactive: true);

            if (cards.Length != SolitaireCardUtility.CardCount)
            {
                Debug.LogError($"DeckParent must contain exactly 52 CardView instances. Found: {cards.Length}", this);
                return;
            }

            for (int cardId = 0; cardId < cards.Length; cardId++)
            {
                CardSuit suit = SolitaireCardUtility.GetSuitFromId(cardId);
                CardRank rank = SolitaireCardUtility.GetRankFromId(cardId);

                cards[cardId].Initialize(config, cardId, suit, rank);
                _context.ViewRegistry.Register(cardId, cards[cardId]);
            }
        }

        private void InitializeControllers()
        {
            var moveResolver = new SolitaireMoveResolver(config);
            var autoMoveResolver = new SolitaireAutoMoveResolver(moveResolver);
            var moveExecutor = new SolitaireMoveExecutor(config);

            layoutController.Initialize(config, _context);

            deckController.Initialize(
                config,
                _context,
                moveResolver,
                autoMoveResolver,
                moveExecutor,
                layoutController);

            inputController.Initialize(config, _context, deckController, layoutController);
        }

        private int GenerateSessionSeed()
        {
            return System.Environment.TickCount;
        }
    }
}
```

Base architecture level seed sağlıyorsa `GenerateSessionSeed` yerine base level context seed'i kullanılmalı.

---

## 28. SolitaireInputController

InputController inputları DeckController'a taşır. Rule bilmez.

```csharp
using UnityEngine;

namespace Handler.GameModules.Solitaire
{
    public sealed class SolitaireInputController : MonoBehaviour
    {
        private SolitaireDeckConfigSO _config;
        private SolitaireRuntimeContext _context;
        private SolitaireDeckController _deckController;
        private SolitaireLayoutController _layoutController;

        public void Initialize(
            SolitaireDeckConfigSO config,
            SolitaireRuntimeContext context,
            SolitaireDeckController deckController,
            SolitaireLayoutController layoutController)
        {
            _config = config;
            _context = context;
            _deckController = deckController;
            _layoutController = layoutController;
        }

        public void OnCardDoubleTapped(int cardId)
        {
            _deckController.RequestAutoMoveToFoundation(cardId);
        }

        public void OnCardDropped(int cardId, SolitaireSlotAnchor targetSlot)
        {
            if (targetSlot == null)
            {
                _deckController.RequestDrop(cardId, PileRef.Invalid);
                return;
            }

            _deckController.RequestDrop(cardId, targetSlot.ToPileRef());
        }

        public void OnStockTapped()
        {
            _deckController.RequestDrawFromStock();
        }
    }
}
```

---

## 29. Selection State

Tap selection ileride eklenirse runtime context altında tutulmalıdır.

```csharp
namespace Handler.GameModules.Solitaire
{
    public sealed class SolitaireSelectionState
    {
        public int SelectedCardId { get; private set; } = -1;
        public bool HasSelection => SelectedCardId >= 0;

        public void Select(int cardId)
        {
            SelectedCardId = cardId;
        }

        public void Clear()
        {
            SelectedCardId = -1;
        }
    }
}
```

Bu dokümanda MVP input kararımız drag ve double tap olduğundan selection optional kalır.

---

## 30. Move History

Undo ilk MVP içinde varsa data layer'a eklenir. Undo record runtime state olduğu için SO içinde tutulmaz.

```csharp
using System.Collections.Generic;

namespace Handler.GameModules.Solitaire
{
    public sealed class SolitaireMoveHistory
    {
        private readonly Stack<SolitaireUndoRecord> _records = new Stack<SolitaireUndoRecord>(128);

        public int Count => _records.Count;

        public void Push(SolitaireUndoRecord record)
        {
            _records.Push(record);
        }

        public bool TryPop(out SolitaireUndoRecord record)
        {
            if (_records.Count == 0)
            {
                record = default;
                return false;
            }

            record = _records.Pop();
            return true;
        }

        public void Clear()
        {
            _records.Clear();
        }
    }

    public readonly struct SolitaireUndoRecord
    {
        public readonly SolitaireMove Move;
        public readonly int AutoFlippedCardId;

        public SolitaireUndoRecord(SolitaireMove move, int autoFlippedCardId)
        {
            Move = move;
            AutoFlippedCardId = autoFlippedCardId;
        }
    }
}
```

---

## 31. Data Mutation Boundaries

BoardState mutation sadece şu class üzerinden yapılır:

```text
SolitaireMoveExecutor
```

Allowed mutation methods:

```text
Execute legal move
Draw from stock
Recycle waste
Flip tableau top card
New game deal
Undo move
```

Bunların dışında hiçbir yerde şunlar yapılmamalı:

```csharp
board.Tableaus[0].Add(cardId);
board.Cards[cardId].IsFaceUp = true;
board.Waste.RemoveTop();
```

Controller içinde bile direct mutation minimumda tutulmalıdır. Üstteki DeckController örneğinde stock draw ve recycle inline yazıldı. Daha temiz final implementation'da bunlar da `SolitaireMoveExecutor` içine taşınabilir.

Önerilen final boundary:

```text
DeckController
-> MoveResolver
-> MoveExecutor
-> BoardState
```

---

## 32. Legal Move Rules

### 32.1 Tableau to Tableau

Legal koşullar:

```text
Source card face up olmalı.
Source tableau içindeki cardId'den pile top'a kadar olan seri valid olmalı.
Target tableau boşsa moving card King olmalı.
Target tableau doluysa target top face up olmalı.
Moving card rank, target top rank değerinden 1 düşük olmalı.
Moving card color, target top color değerinden farklı olmalı.
```

### 32.2 Waste to Tableau

Legal koşullar:

```text
Waste card top card olmalı.
Target tableau rule aynı uygulanır.
```

### 32.3 Tableau to Foundation

Legal koşullar:

```text
Sadece tek kart taşınır.
Source card tableau top card olmalı.
Target foundation suit ile card suit eşleşmeli.
Foundation boşsa card Ace olmalı.
Foundation doluysa moving card rank, foundation top rank değerinden 1 yüksek olmalı.
```

### 32.4 Waste to Foundation

Legal koşullar:

```text
Waste card top card olmalı.
Foundation rule aynı uygulanır.
```

### 32.5 Foundation to Tableau

Default config:

```text
allowFoundationToTableau = false
```

Bu rule kapalı tutulursa case UX sadeleşir. Açılırsa target tableau rule uygulanır.

---

## 33. Card State Machine Kararı

Kartlarda state machine olmalı, ama bu state machine oyun kurallarından izole kalmalı.

### 33.1 Card state machine neden gerekli?

- Drag sırasında aynı karta tekrar input gelmesini engeller.
- Animation sırasında kartın tıklanmasını engeller.
- Invalid move sonrası returning state gösterir.
- Selection, highlight ve locked state yönetimi sadeleşir.
- CardView kodu if bloklarıyla büyümez.

### 33.2 Card state machine neden rule bilmemeli?

Rule bilgisi karta konursa şu problemler çıkar:

- Kart başka pile'ların state'ini bilmek zorunda kalır.
- View layer data layer'a bağımlı olur.
- Undo ve replay zorlaşır.
- Unit testler MonoBehaviour'a bağımlı hale gelir.
- Base architecture üstüne modüler ekleme zayıflar.

Doğru sınır:

```text
CardVisualStateMachine = input ve visual state
SolitaireMoveResolver = legal move decision
SolitaireMoveExecutor = runtime data mutation
```

---

## 34. Runtime Lifecycle

### 34.1 Awake

```text
Installer runtime context oluşturur.
Scene referansları validate edilir.
52 CardView register edilir.
Controller dependency'leri bağlanır.
```

### 34.2 Start

```text
Seed alınır.
New game deal yapılır.
Initial layout snap edilir.
```

### 34.3 Input sırasında

```text
InputController event alır.
DeckController request methodunu çağırır.
MoveResolver legal move üretir.
MoveExecutor data mutate eder.
LayoutController view targetlarını hesaplar.
CardView animation oynar.
```

### 34.4 Restart

```text
BoardState.ClearPiles()
BoardState.InitializeCards()
MoveHistory.Clear()
SelectionState.Clear()
DeckParent altındaki aynı 52 CardView reuse edilir.
New shuffle ve deal yapılır.
```

---

## 35. Folder Structure

```text
Assets/GameModules/Solitaire
|
|-- Runtime
|   |
|   |-- Config
|   |   |-- SolitaireDeckConfigSO.cs
|   |
|   |-- Data
|   |   |-- CardState.cs
|   |   |-- FixedCardPileState.cs
|   |   |-- PileRef.cs
|   |   |-- SolitaireBoardState.cs
|   |   |-- SolitaireMove.cs
|   |   |-- SolitaireMoveResult.cs
|   |   |-- SolitaireRuntimeContext.cs
|   |   |-- SolitaireSelectionState.cs
|   |   |-- SolitaireMoveHistory.cs
|   |
|   |-- Controllers
|   |   |-- SolitaireModuleInstaller.cs
|   |   |-- SolitaireDeckController.cs
|   |   |-- SolitaireInputController.cs
|   |   |-- SolitaireLayoutController.cs
|   |
|   |-- Rules
|   |   |-- SolitaireMoveResolver.cs
|   |   |-- SolitaireAutoMoveResolver.cs
|   |
|   |-- Views
|   |   |-- CardView.cs
|   |   |-- CardRuntimeIdentity.cs
|   |   |-- CardInputReceiver.cs
|   |   |-- CardDragBehaviour.cs
|   |   |-- CardVisualStateMachine.cs
|   |   |-- SolitaireSlotAnchor.cs
|   |   |-- SolitaireViewRegistry.cs
|   |
|   |-- Utilities
|       |-- SolitaireCardUtility.cs
|       |-- SolitaireShuffleUtility.cs
|
|-- Prefabs
|   |-- SolitaireRoot.prefab
|   |-- Card.prefab
|   |-- StockSlot.prefab
|   |-- WasteSlot.prefab
|   |-- FoundationSlot.prefab
|   |-- TableauSlot.prefab
|
|-- ScriptableObjects
    |-- SolitaireDeckConfig.asset
```

---

## 36. Integration With Existing Base Architecture

Bu modül base Level ve Game Architecture içine şu şekilde eklenir:

```text
Existing Level System
-> Loads level scene veya level prefab
-> SolitaireRoot aktif edilir
-> SolitaireModuleInstaller kendi runtime graph'ini kurar
-> Base sadece lifecycle event gönderir
```

Base architecture ile minimum temas noktaları:

```text
OnLevelStarted -> deckController.StartNewGame(seed)
OnLevelRestarted -> deckController.StartNewGame(newSeed)
OnLevelPaused -> inputController.SetEnabled(false)
OnLevelResumed -> inputController.SetEnabled(true)
OnLevelCompleted -> foundation complete event
```

Solitaire modülü global manager'a dönüşmez. Base tarafına gerekirse adapter interface açılır.

```csharp
public interface ISolitaireLevelModule
{
    void StartModule(int seed);
    void RestartModule(int seed);
    void SetInputEnabled(bool isEnabled);
    bool IsCompleted { get; }
}
```

---

## 37. Performance Guidelines

C# ve Unity runtime için öneriler:

```text
LINQ kullanma.
GetRange kullanma.
Card lookup için Dictionary yerine CardView[52] array kullan.
Pile data için fixed int[] kullan.
Runtime sırasında card instantiate veya destroy yapma.
GetComponent çağrılarını Awake sırasında cachele.
Board mutation tek yerden yapılsın.
Move request için struct kullan.
Sprite lookup SO üzerinden tek methodla yapılsın.
Input hit detection için Collider2D ve cached camera kullan.
Sorting order hesaplarını deterministic yap.
```

### 37.1 Allocation kontrolü

Riskli noktalar:

```text
List.GetRange
LINQ Where, Select, ToList
foreach over interface typed collections
Per-frame new Vector/List allocation
Per-frame GetComponent
Per-frame Camera.main lookup
Coroutine allocation yoğun animasyon sistemi
```

Önerilen yaklaşım:

```text
Fixed buffers
Cached arrays
Explicit for loops
Cached Transform
Cached Camera
Reusable move buffer
Reusable drag buffer
```

---

## 38. Editor Validation

`SolitaireModuleInstaller.OnValidate` veya custom editor ile kontrol edilmesi gerekenler:

```text
Config assigned mı?
DeckParent assigned mı?
DeckParent altında 52 CardView var mı?
StockSlot assigned mı?
WasteSlot assigned mı?
Foundation slot count 4 mü?
Tableau slot count 7 mi?
FoundationSlot suit mapping doğru mu?
Card sprite arrays length 13 mü?
CardBackSprite assigned mı?
Card collider size doğru mu?
```

Example:

```csharp
private void OnValidate()
{
    if (deckParent == null)
        return;

    int cardCount = deckParent.GetComponentsInChildren<CardView>(true).Length;

    if (cardCount != SolitaireCardUtility.CardCount)
        Debug.LogWarning($"DeckParent should contain 52 cards. Current: {cardCount}", this);
}
```

---

## 39. Final Responsibility Map

| Layer | Owns | Does Not Own |
|---|---|---|
| ScriptableObject | Config, sprites, offsets, rule flags | Runtime session state |
| BoardState | Authoritative card and pile data | Input, animation, scene references |
| FixedCardPileState | Card id order for one pile | Card visual state |
| MoveResolver | Legal move decision | Board mutation, animation |
| MoveExecutor | Board mutation | Input, target detection, animation |
| DeckController | Gameplay action orchestration | Global game lifecycle, UI, raw rule logic |
| InputController | Pointer events and target reporting | Klondike rules |
| LayoutController | World position calculation | Rule validation |
| CardView | Sprite, position, sorting | Board mutation |
| CardVisualStateMachine | Visual and interaction state | Legal move rules |
| ViewRegistry | cardId to CardView lookup | Card rules |

---

## 40. Recommended Implementation Order

```text
1. Enums ve CardState
2. FixedCardPileState
3. SolitaireBoardState
4. SolitaireDeckConfigSO
5. CardView ve CardRuntimeIdentity
6. SolitaireViewRegistry
7. SolitaireModuleInstaller ile 52 card register
8. Shuffle ve deal flow
9. LayoutController ile initial placement
10. MoveResolver tableau ve foundation rules
11. MoveExecutor
12. Drag input
13. Double tap foundation auto move
14. Stock to waste ve recycle
15. Auto flip tableau top card
16. Debug gizmos ve validation
17. Undo support
```

---

## 41. Net Mimari Özeti

Bu mimaride authoritative source of truth `SolitaireBoardState` olur. Scene'deki kartlar sadece view ve input receiver olarak davranır. Slotlar data değil, world-space anchor ve drop target metadata'sıdır. Controller inputtan gelen requestleri alır, MoveResolver'a legal move sorar, MoveExecutor ile data'yı günceller ve LayoutController ile view'ları taşır. ScriptableObject yalnızca shared immutable config olarak kullanılır. Singleton kullanılmaz. Static sadece pure helper fonksiyonları için tutulur.
