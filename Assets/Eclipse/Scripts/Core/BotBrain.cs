using System;
using System.Collections.Generic;

namespace Eclipse.Core
{
    public enum BotDifficulty
    {
        Easy,
        Medium,
        Hard,
    }

    /// <summary>What a bot wants to do on its turn.</summary>
    public struct TurnChoice
    {
        /// <summary>The card to play, or null to draw.</summary>
        public DoubleCard Card;
        public int Color;

        public bool IsDraw => Card == null;
    }

    /// <summary>
    /// Decisions for one bot seat. It only reads the state; the caller applies the choice through
    /// <see cref="TurnManager"/> (with a short delay so it feels human). Feed it every event with
    /// <see cref="Observe"/> so Hard can track which colors players seem to be missing.
    /// Bots never bluff: they only play Wild +2 / Wild Chase legally.
    /// </summary>
    public sealed class BotBrain
    {
        public readonly int Seat;
        public readonly BotDifficulty Difficulty;
        readonly Random rng;

        /// <summary>Per seat: colors that player drew on instead of playing (probably holds none).</summary>
        readonly Dictionary<int, HashSet<int>> likelyMissing = new Dictionary<int, HashSet<int>>();
        Side missingSide;

        public BotBrain(int seat, BotDifficulty difficulty, Random rng)
        {
            Seat = seat;
            Difficulty = difficulty;
            this.rng = rng;
        }

        /// <summary>Chance of remembering to call "Call it!" on the second-to-last card.</summary>
        public double CallItChance => Difficulty == BotDifficulty.Easy ? 0.7 : Difficulty == BotDifficulty.Medium ? 0.92 : 1.0;

        /// <summary>Chance of catching a player who forgot to call it.</summary>
        public double CatchChance => Difficulty == BotDifficulty.Easy ? 0.25 : Difficulty == BotDifficulty.Medium ? 0.5 : 0.85;

        public bool WillCallIt() => rng.NextDouble() < CallItChance;
        public bool WillCatch() => rng.NextDouble() < CatchChance;

        // ---------- Memory ----------

        public void Observe(GameState s, GameEvent e)
        {
            if (Difficulty != BotDifficulty.Hard) return;
            if (s.ActiveSide != missingSide)
            {
                // Everyone's hand just turned over: what we knew is about the other faces now.
                likelyMissing.Clear();
                missingSide = s.ActiveSide;
            }
            // A voluntary draw (one card, on your own turn) hints you had nothing of the active color.
            if (e.Type == GameEventType.CardsDrawn && e.Seat == s.Current && s.Phase == Phase.AwaitTurn && e.Count == 1)
                Missing(e.Seat).Add(s.ActiveColor);
            else if (e.Type == GameEventType.CardPlayed && e.Seat >= 0)
            {
                var face = e.Card.Face(e.Side);
                if (!face.IsWild) Missing(e.Seat).Remove(face.Color);
            }
        }

        HashSet<int> Missing(int seat)
        {
            if (!likelyMissing.TryGetValue(seat, out var set)) likelyMissing[seat] = set = new HashSet<int>();
            return set;
        }

        // ---------- Decisions ----------

        public TurnChoice ChooseTurn(GameState s)
        {
            var hand = s.Players[Seat].Hand;
            var options = new List<DoubleCard>();
            foreach (var card in hand)
            {
                if (!RuleValidator.CanPlay(s, card)) continue;
                var face = card.Face(s.ActiveSide);
                if (face.IsChallengeable && !RuleValidator.IsLegalChallengeable(s, hand, card)) continue;
                options.Add(card);
            }
            if (options.Count == 0) return new TurnChoice { Card = null };

            DoubleCard pick;
            if (Difficulty == BotDifficulty.Easy) pick = options[rng.Next(options.Count)];
            else
            {
                pick = null;
                double bestScore = double.MinValue;
                foreach (var card in options)
                {
                    double score = Score(s, card, options.Count) + rng.NextDouble() * 0.5;
                    if (score > bestScore) { bestScore = score; pick = card; }
                }
            }
            return new TurnChoice { Card = pick, Color = pick.Face(s.ActiveSide).IsWild ? ChooseColor(s, pick) : CardFace.NoColor };
        }

        double Score(GameState s, DoubleCard card, int optionCount)
        {
            var face = card.Face(s.ActiveSide);
            var hand = s.Players[Seat].Hand;
            int next = s.NextSeat(Seat);
            int nextCards = s.Players[next].Hand.Count;
            int leaderCards = FewestOpponentCards(s);
            double score = 0;

            // Shed points, and keep the color we hold most of so we keep having plays.
            score += face.Points * 0.04;
            if (!face.IsWild) score += CountColor(hand, s.ActiveSide, face.Color, card) * 0.6;

            switch (face.Kind)
            {
                case CardKind.DrawOne:
                case CardKind.DrawFive:
                case CardKind.Jump:
                case CardKind.JumpAll:
                    score += nextCards <= 3 ? 4 : 1;
                    break;
                case CardKind.Wild:
                case CardKind.WildDrawTwo:
                case CardKind.WildChase:
                    // Save wilds as an escape, unless someone is about to go out.
                    score += leaderCards <= 2 ? 5 : -4;
                    break;
                case CardKind.Flip:
                    if (Difficulty == BotDifficulty.Hard && optionCount > 1)
                        score += leaderCards <= 2 ? 3 : -6; // Hold flips back to break up an opponent's last cards.
                    break;
            }
            return score;
        }

        /// <summary>Pick a color for a wild: the one we hold most of. Hard breaks ties with a color the next player seems to lack.</summary>
        public int ChooseColor(GameState s, DoubleCard playing = null)
        {
            if (Difficulty == BotDifficulty.Easy) return rng.Next(CardFace.ColorCount);
            var hand = s.Players[Seat].Hand;
            int next = s.NextSeat(Seat);
            int best = 0;
            double bestScore = double.MinValue;
            for (int c = 0; c < CardFace.ColorCount; c++)
            {
                double score = CountColor(hand, s.ActiveSide, c, playing) + rng.NextDouble() * 0.1;
                if (Difficulty == BotDifficulty.Hard && likelyMissing.TryGetValue(next, out var missing) && missing.Contains(c))
                    score += 0.75;
                if (score > bestScore) { bestScore = score; best = c; }
            }
            return best;
        }

        /// <summary>Drew a playable card: play it now?</summary>
        public bool PlayDrawn(GameState s)
        {
            var card = s.DrawnCard;
            if (card == null) return false;
            var face = card.Face(s.ActiveSide);
            if (face.IsChallengeable && !RuleValidator.IsLegalChallengeable(s, s.Players[Seat].Hand, card)) return false;
            if (Difficulty == BotDifficulty.Hard && face.Kind == CardKind.Flip && FewestOpponentCards(s) > 2) return false;
            return true;
        }

        /// <summary>Challenge the pending Wild +2 / Wild Chase?</summary>
        public bool ShouldChallenge(GameState s)
        {
            var p = s.Pending;
            if (p == null) return false;
            if (Difficulty == BotDifficulty.Easy) return rng.NextDouble() < 0.1;

            // Probability they held the old color (so the play was a bluff), from their hand size
            // and how common that color still is among the cards we can't see.
            int oldColor = p.PreviousColor;
            int theirCards = s.Players[p.Player].Hand.Count;
            double share = UnseenColorShare(s, oldColor);
            double pBluff = 1 - Math.Pow(1 - share, theirCards);
            if (Difficulty == BotDifficulty.Hard && likelyMissing.TryGetValue(p.Player, out var missing) && missing.Contains(oldColor))
                pBluff *= 0.3;
            // Wrong guess costs 2 extra cards; right guess saves 2 and hands them 2, so challenge above ~1/3.
            double threshold = Difficulty == BotDifficulty.Hard ? 0.4 : 0.6;
            return pBluff > threshold;
        }

        double UnseenColorShare(GameState s, int color)
        {
            int total = 0, ofColor = 0;
            var seen = new HashSet<int>();
            foreach (var c in s.Players[Seat].Hand) seen.Add(c.Id);
            foreach (var c in s.DiscardPile) seen.Add(c.Id);
            foreach (var c in AllCards(s))
            {
                if (seen.Contains(c.Id)) continue;
                total++;
                if (c.Face(s.ActiveSide).Color == color) ofColor++;
            }
            return total == 0 ? 0 : (double)ofColor / total;
        }

        static IEnumerable<DoubleCard> AllCards(GameState s)
        {
            // The bot knows the deck's composition, not where unseen cards are; iterating every pile
            // only counts which faces exist, it never looks at whose they are.
            foreach (var c in s.DrawPile) yield return c;
            foreach (var p in s.Players)
                foreach (var c in p.Hand) yield return c;
        }

        int FewestOpponentCards(GameState s)
        {
            int fewest = int.MaxValue;
            foreach (var p in s.Players)
                if (p.Seat != Seat && p.Hand.Count < fewest) fewest = p.Hand.Count;
            return fewest;
        }

        static int CountColor(List<DoubleCard> hand, Side side, int color, DoubleCard except)
        {
            int n = 0;
            foreach (var c in hand)
                if (c != except && c.Face(side).Color == color) n++;
            return n;
        }
    }
}
