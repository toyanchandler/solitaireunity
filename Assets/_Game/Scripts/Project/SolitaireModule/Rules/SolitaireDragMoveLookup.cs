using _Game.Scripts.Project.SolitaireModule.Data;

namespace _Game.Scripts.Project.SolitaireModule.Rules
{
    internal static class SolitaireDragMoveLookup
    {
        private static readonly SolitaireMoveType[,] Table = CreateTable();

        public static SolitaireMoveType Resolve(SolitairePileType target, SolitairePileType source)
        {
            return Table[(int)target, (int)source];
        }

        private static SolitaireMoveType[,] CreateTable()
        {
            int pileTypeCount = 4;
            var table = new SolitaireMoveType[pileTypeCount, pileTypeCount];

            table[(int)SolitairePileType.Tableau, (int)SolitairePileType.Waste] = SolitaireMoveType.WasteToTableau;
            table[(int)SolitairePileType.Tableau, (int)SolitairePileType.Tableau] = SolitaireMoveType.TableauToTableau;
            table[(int)SolitairePileType.Tableau, (int)SolitairePileType.Foundation] = SolitaireMoveType.FoundationToTableau;

            table[(int)SolitairePileType.Foundation, (int)SolitairePileType.Waste] = SolitaireMoveType.WasteToFoundation;
            table[(int)SolitairePileType.Foundation, (int)SolitairePileType.Tableau] = SolitaireMoveType.TableauToFoundation;

            return table;
        }
    }
}
