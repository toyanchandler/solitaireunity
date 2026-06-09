namespace _Game.Scripts.Project.SolitaireModule.Data
{
    public enum SolitaireDebugScenarioId
    {
        None = 0,

        /// <summary>T3: ♣9♦8♠7♥6 → T0 ♥10. 4 kart birlikte taşınmalı.</summary>
        ValidFourCardMerge = 1,

        /// <summary>T3: ♦9♣8♠7 → T0 ♥10. Bağlantı kırmızı-kırmızı, reddedilmeli.</summary>
        RejectSameColorJunction = 2,

        /// <summary>T5: ♠K♥Q♦J♣10 → boş T4. King serisi boş sütuna.</summary>
        ValidKingSequenceToEmpty = 3,

        /// <summary>T2: ♣9♠8 (siyah-siyah). Seri tutulamaz.</summary>
        RejectInvalidInternalSequence = 4,

        /// <summary>T1: ♣9♦8♠7♥6 — ♠7'den tut. Sadece ♠7+♥6 taşınır, T0 ♠K üstüne reddedilmeli.</summary>
        PartialSequenceTwoCards = 5,

        /// <summary>T3: ♣9♦8 → T0 ♥10. 2 kartlık kısa seri, geçerli.</summary>
        ValidTwoCardMerge = 6,

        /// <summary>Success endgame UI akışını tetikler.</summary>
        EndGameSuccess = 7,

        /// <summary>Fail endgame UI akışını tetikler.</summary>
        EndGameFail = 8,

        /// <summary>Level'ı yeniden yükler (restart).</summary>
        Restart = 9
    }
}
