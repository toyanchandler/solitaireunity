using System;
using _Game.Scripts.Project.SolitaireModule.Data;

namespace _Game.Scripts.Project.SolitaireModule.Runtime
{
    internal sealed class SolitairePileTypeTable<T>
    {
        private readonly Func<int, T>[] _resolveByIndex;

        public SolitairePileTypeTable(T stock, T waste, T[] foundations, T[] tableaus)
        {
            _resolveByIndex = new Func<int, T>[]
            {
                _ => stock,
                _ => waste,
                index => foundations[index],
                index => tableaus[index],
            };
        }

        public T Resolve(PileRef pileRef) => _resolveByIndex[(int)pileRef.Type](pileRef.Index);

        public T Resolve(SolitairePileType pileType, int index) => _resolveByIndex[(int)pileType](index);
    }
}
