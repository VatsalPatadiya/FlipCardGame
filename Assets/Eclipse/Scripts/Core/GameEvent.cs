namespace Eclipse.Core
{
    public enum GameEventType
    {
        RoundStarted,
        /// <summary>A Wild +2 was turned up as the starting card and went back into the deck.</summary>
        StartCardReturned,
        StartCardTurned,
        TurnStarted,
        CardPlayed,
        CardsDrawn,
        Flipped,
        ColorChosen,
        DirectionChanged,
        PlayerSkipped,
        ChallengeResolved,
        CalledIt,
        Caught,
        Reshuffled,
        RoundEnded,
    }

    /// <summary>
    /// What just happened, for the presentation layer (animations, sounds), bots and the network.
    /// Fields that don't apply to a type are left at their defaults.
    /// </summary>
    public sealed class GameEvent
    {
        public GameEventType Type;
        public int Seat = -1;
        /// <summary>Second seat involved: the challenger, or the catcher.</summary>
        public int OtherSeat = -1;
        public DoubleCard Card;
        public int Count;
        public int Color = CardFace.NoColor;
        public Side Side;
        /// <summary>ChallengeResolved: true if the challenge caught a cheat.</summary>
        public bool Success;
        /// <summary>RoundEnded: points the round is worth under the match's scoring mode.</summary>
        public int Points;

        public override string ToString()
        {
            switch (Type)
            {
                case GameEventType.CardPlayed: return $"P{Seat} plays {Card.Face(Side)}";
                case GameEventType.CardsDrawn: return $"P{Seat} draws {Count}";
                case GameEventType.Flipped: return $"Flip to {Side}";
                case GameEventType.ColorChosen: return $"P{Seat} chooses color {Color}";
                case GameEventType.ChallengeResolved: return $"P{OtherSeat} challenges P{Seat}: {(Success ? "caught" : "failed")}";
                case GameEventType.Caught: return $"P{OtherSeat} catches P{Seat}";
                case GameEventType.RoundEnded: return $"P{Seat} wins the round ({Points} pts)";
                default: return Seat >= 0 ? $"{Type} P{Seat}" : Type.ToString();
            }
        }
    }
}
