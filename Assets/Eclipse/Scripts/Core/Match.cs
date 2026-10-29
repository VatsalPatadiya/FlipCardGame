using System;
using System.Collections.Generic;

namespace Eclipse.Core
{
    /// <summary>A match: rounds until someone reaches the target score (§3.8). The deal passes left each round.</summary>
    public sealed class Match
    {
        public readonly int PlayerCount;
        public readonly RuleSettings Rules;
        public readonly int[] Scores;
        public int Dealer { get; private set; }
        public int RoundNumber { get; private set; }
        public TurnManager Round { get; private set; }
        /// <summary>Seat that won the match, or -1 while it's still going.</summary>
        public int MatchWinner { get; private set; } = -1;

        readonly Random rng;

        public Match(int playerCount, Random rng, RuleSettings rules = null)
        {
            PlayerCount = playerCount;
            Rules = rules ?? new RuleSettings();
            Scores = new int[playerCount];
            this.rng = rng;
            Dealer = ChooseDealerByDraw(playerCount, rng);
        }

        /// <summary>Deal the next round. <paramref name="listener"/> is subscribed before the deal, so it sees every event; <see cref="Round"/> is already set when it's called.</summary>
        public TurnManager StartRound(Action<GameEvent> listener = null)
        {
            if (MatchWinner >= 0) throw new InvalidOperationException("The match is over.");
            if (RoundNumber > 0) Dealer = (Dealer + 1) % PlayerCount;
            RoundNumber++;
            Round = TurnManager.NewRound(PlayerCount, Dealer, rng, Rules);
            if (listener != null) Round.Events += listener;
            Round.Deal();
            return Round;
        }

        /// <summary>Add the finished round to the totals. Returns each seat's points for the round.</summary>
        public int[] FinishRound()
        {
            if (Round == null || Round.State.Phase != Phase.RoundOver) throw new InvalidOperationException("The round isn't over.");
            var deltas = ScoreCalculator.RoundDeltas(Round.State, Rules.Scoring);
            for (int i = 0; i < PlayerCount; i++) Scores[i] += deltas[i];
            MatchWinner = FindMatchWinner();
            return deltas;
        }

        int FindMatchWinner()
        {
            int best = -1;
            if (Rules.Scoring == ScoringMode.WinnerTakesPoints)
            {
                for (int i = 0; i < PlayerCount; i++)
                    if (Scores[i] >= Rules.TargetScore && (best < 0 || Scores[i] > Scores[best])) best = i;
                return best;
            }
            bool someoneReached = false;
            for (int i = 0; i < PlayerCount; i++)
            {
                if (Scores[i] >= Rules.TargetScore) someoneReached = true;
                if (best < 0 || Scores[i] < Scores[best]) best = i;
            }
            return someoneReached ? best : -1;
        }

        /// <summary>
        /// Everyone draws a card; the highest Sun number deals (action cards count as 0).
        /// Players tied for highest draw again.
        /// </summary>
        public static int ChooseDealerByDraw(int playerCount, Random rng)
        {
            var contenders = new List<int>();
            for (int i = 0; i < playerCount; i++) contenders.Add(i);
            while (true)
            {
                var deck = DeckBuilder.Build(rng);
                int best = -1;
                var tied = new List<int>();
                for (int i = 0; i < contenders.Count; i++)
                {
                    var face = deck[i].Sun;
                    int value = face.Kind == CardKind.Number ? face.Number : 0;
                    if (value > best) { best = value; tied.Clear(); }
                    if (value == best) tied.Add(contenders[i]);
                }
                if (tied.Count == 1) return tied[0];
                contenders = tied;
            }
        }
    }
}
