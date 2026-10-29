using System.Collections.Generic;

namespace Eclipse.Core
{
    /// <summary>Scoring (§3.8). Only the faces of the side the round ended on count.</summary>
    public static class ScoreCalculator
    {
        public static int HandPoints(IEnumerable<DoubleCard> hand, Side side)
        {
            int total = 0;
            foreach (var card in hand) total += card.Face(side).Points;
            return total;
        }

        /// <summary>The total of every opponent's hand: what the winner scores.</summary>
        public static int RoundPoints(GameState s, int winner)
        {
            int total = 0;
            foreach (var p in s.Players)
                if (p.Seat != winner) total += HandPoints(p.Hand, s.ActiveSide);
            return total;
        }

        /// <summary>
        /// Points each seat adds to its match total for a finished round.
        /// WinnerTakesPoints: only the winner gains. LowestTotal: everyone adds their own leftovers.
        /// </summary>
        public static int[] RoundDeltas(GameState s, ScoringMode mode)
        {
            var deltas = new int[s.PlayerCount];
            if (mode == ScoringMode.WinnerTakesPoints)
            {
                deltas[s.Winner] = RoundPoints(s, s.Winner);
            }
            else
            {
                foreach (var p in s.Players) deltas[p.Seat] = HandPoints(p.Hand, s.ActiveSide);
            }
            return deltas;
        }
    }
}
