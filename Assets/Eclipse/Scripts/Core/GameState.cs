using System.Collections.Generic;

namespace Eclipse.Core
{
    /// <summary>What the round is waiting for. Only <see cref="TurnManager"/> changes it.</summary>
    public enum Phase
    {
        /// <summary><see cref="GameState.Current"/> plays a card or draws.</summary>
        AwaitTurn,
        /// <summary><see cref="GameState.Current"/> drew a playable card: play it or pass.</summary>
        AwaitDrawnDecision,
        /// <summary><see cref="GameState.ColorChooser"/> picks the color (wild start card, or a flip that turned up a wild).</summary>
        AwaitColor,
        /// <summary><see cref="GameState.Current"/> (the next player) accepts or challenges a Wild +2 / Wild Chase.</summary>
        AwaitChallenge,
        RoundOver,
    }

    public enum ScoringMode
    {
        /// <summary>The round winner scores everyone else's cards. First to the target wins.</summary>
        WinnerTakesPoints,
        /// <summary>Everyone keeps a running total of their own leftover cards. When someone reaches the target, the lowest total wins.</summary>
        LowestTotal,
    }

    /// <summary>Rules that might become house-rule toggles later. Defaults follow the design spec.</summary>
    public sealed class RuleSettings
    {
        public int HandSize = 7;
        public int TargetScore = 500;
        public ScoringMode Scoring = ScoringMode.WinnerTakesPoints;
        /// <summary>
        /// A Flip turns the draw pile and the discard pile over like real stacks, so their order reverses
        /// and the old bottom card of the discard pile ends up on top. Off: piles keep their order and only
        /// the side changes (the Flip card's other face stays on top).
        /// </summary>
        public bool FlipTurnsPilesOver = true;
    }

    public sealed class PlayerState
    {
        public readonly int Seat;
        public readonly List<DoubleCard> Hand = new List<DoubleCard>();
        public bool CalledIt;

        public PlayerState(int seat) { Seat = seat; }
    }

    /// <summary>A Wild +2 or Wild Chase waiting for the next player to accept or challenge it.</summary>
    public sealed class PendingWild
    {
        public int Player;
        public int Victim;
        public CardKind Kind;
        public int ChosenColor;
        /// <summary>The color that was active before the wild: the one the player must not have held.</summary>
        public int PreviousColor;
        /// <summary>True if the player held no card of the color that was active when they played it.</summary>
        public bool WasLegal;
    }

    /// <summary>Everything about one round. Pure data; read it freely, change it only through <see cref="TurnManager"/>.</summary>
    public sealed class GameState
    {
        public readonly RuleSettings Rules;
        public readonly List<PlayerState> Players = new List<PlayerState>();
        /// <summary>Top of the pile is the last element.</summary>
        public readonly List<DoubleCard> DrawPile = new List<DoubleCard>();
        /// <summary>Top of the pile is the last element.</summary>
        public readonly List<DoubleCard> DiscardPile = new List<DoubleCard>();

        public Side ActiveSide = Side.Sun;
        public int ActiveColor;
        public int Dealer;
        public int Current;
        /// <summary>+1 = clockwise (left), -1 = counter-clockwise.</summary>
        public int Direction = 1;
        public Phase Phase;

        public DoubleCard DrawnCard;
        public int ColorChooser = -1;
        /// <summary>After <see cref="Phase.AwaitColor"/>: true to pass the turn on, false to let the chooser play.</summary>
        public bool AdvanceAfterColor;
        public PendingWild Pending;
        /// <summary>Seat that is down to one card without calling it and can be caught, or -1.</summary>
        public int Vulnerable = -1;
        public int Winner = -1;

        public GameState(int playerCount, RuleSettings rules)
        {
            Rules = rules ?? new RuleSettings();
            for (int i = 0; i < playerCount; i++) Players.Add(new PlayerState(i));
        }

        public int PlayerCount => Players.Count;
        public DoubleCard Top => DiscardPile[DiscardPile.Count - 1];
        public CardFace TopFace => Top.Face(ActiveSide);

        public int NextSeat(int from, int steps = 1)
        {
            int n = PlayerCount;
            return ((from + Direction * steps) % n + n) % n;
        }

        public int TotalCards()
        {
            int total = DrawPile.Count + DiscardPile.Count;
            foreach (var p in Players) total += p.Hand.Count;
            return total;
        }
    }
}
