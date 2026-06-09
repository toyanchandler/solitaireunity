using System;
using _Game.Scripts.Project.SolitaireModule.Data;
using _Game.Scripts.Project.SolitaireModule.Rules;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    public sealed class FixedCardPileState
    {
        private readonly int[] _cardIds;
        private int _count;

        public SolitairePileType PileType { get; }
        public int PileIndex { get; }
        public int Count => _count;

        public FixedCardPileState(SolitairePileType pileType, int pileIndex, int capacity = SolitaireCardUtility.CardCount)
        {
            PileType = pileType;
            PileIndex = pileIndex;
            _cardIds = new int[capacity];
            Clear();
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
                throw new InvalidOperationException($"{PileType} {PileIndex} capacity exceeded.");

            _cardIds[_count] = cardId;
            _count++;
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

        public void CopyRangeTo(int startIndex, int[] targetBuffer, out int copiedCount)
        {
            copiedCount = _count - startIndex;

            for (int i = 0; i < copiedCount; i++)
                targetBuffer[i] = _cardIds[startIndex + i];
        }

        public void CopyAllTo(int[] targetBuffer, out int copiedCount)
        {
            if (targetBuffer.Length < _count)
                throw new ArgumentException("Target buffer is smaller than the pile count.", nameof(targetBuffer));

            copiedCount = _count;

            for (int i = 0; i < _count; i++)
                targetBuffer[i] = _cardIds[i];
        }

        public void RestoreFrom(int[] sourceCardIds, int sourceCount)
        {
            if ((uint)sourceCount > (uint)_cardIds.Length || sourceCount > sourceCardIds.Length)
                throw new ArgumentOutOfRangeException(nameof(sourceCount));

            Clear();

            for (int i = 0; i < sourceCount; i++)
                Add(sourceCardIds[i]);
        }

        public void Clear()
        {
            for (int i = 0; i < _cardIds.Length; i++)
                _cardIds[i] = -1;

            _count = 0;
        }
    }
}
