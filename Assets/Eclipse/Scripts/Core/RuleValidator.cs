using System.Collections.Generic;

namespace Eclipse.Core
{
    /// <summary>Stateless matching and legality checks (§3.3, §3.4).</summary>
    public static class RuleValidator
    {
        /// <summary>
        /// Can <paramref name="face"/> go on <paramref name="top"/>? Match by color, number or symbol.
        /// Wilds can always be played; Wild +2 and Wild Chase may be a bluff, which the next player can challenge.
        /// </summary>
        public static bool CanPlay(CardFace face, CardFace top, int activeColor)
        {
            if (face.IsWild) return true;
            if (face.Color == activeColor) return true;
            if (top.IsWild) return false;
            if (face.Kind == CardKind.Number) return top.Kind == CardKind.Number && top.Number == face.Number;
            return face.Kind == top.Kind;
        }

        public static bool CanPlay(GameState s, DoubleCard card) =>
            CanPlay(card.Face(s.ActiveSide), s.TopFace, s.ActiveColor);

        /// <summary>Does the hand hold a card of <paramref name="color"/> on <paramref name="side"/>? Number and symbol matches don't count.</summary>
        public static bool HasColor(IEnumerable<DoubleCard> hand, Side side, int color, DoubleCard except = null)
        {
            foreach (var card in hand)
            {
                if (card == except) continue;
                if (card.Face(side).Color == color) return true;
            }
            return false;
        }

        /// <summary>Would playing this challengeable wild from <paramref name="hand"/> be legal right now?</summary>
        public static bool IsLegalChallengeable(GameState s, List<DoubleCard> hand, DoubleCard card) =>
            !HasColor(hand, s.ActiveSide, s.ActiveColor, card);

        public static List<DoubleCard> PlayableCards(GameState s, int seat)
        {
            var result = new List<DoubleCard>();
            foreach (var card in s.Players[seat].Hand)
                if (CanPlay(s, card)) result.Add(card);
            return result;
        }
    }
}
