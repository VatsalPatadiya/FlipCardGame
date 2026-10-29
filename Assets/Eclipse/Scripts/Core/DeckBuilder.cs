using System;
using System.Collections.Generic;

namespace Eclipse.Core
{
    /// <summary>
    /// Builds the 112-card double-sided deck (§2). The Sun list and the Moon list are
    /// shuffled independently and zipped, so every deck pairs faces differently.
    /// </summary>
    public static class DeckBuilder
    {
        public const int DeckSize = 112;

        public static List<CardFace> SunFaces() =>
            Faces(Side.Sun, CardKind.DrawOne, CardKind.Jump, CardKind.WildDrawTwo);

        public static List<CardFace> MoonFaces() =>
            Faces(Side.Moon, CardKind.DrawFive, CardKind.JumpAll, CardKind.WildChase);

        static List<CardFace> Faces(Side side, CardKind draw, CardKind jump, CardKind strongWild)
        {
            var faces = new List<CardFace>(DeckSize);
            for (int color = 0; color < CardFace.ColorCount; color++)
            {
                for (int n = 1; n <= 9; n++)
                {
                    faces.Add(new CardFace(side, CardKind.Number, color, n));
                    faces.Add(new CardFace(side, CardKind.Number, color, n));
                }
                foreach (var kind in new[] { draw, jump, CardKind.Turnabout, CardKind.Flip })
                {
                    faces.Add(new CardFace(side, kind, color));
                    faces.Add(new CardFace(side, kind, color));
                }
            }
            for (int i = 0; i < 4; i++) faces.Add(new CardFace(side, CardKind.Wild, CardFace.NoColor));
            for (int i = 0; i < 4; i++) faces.Add(new CardFace(side, strongWild, CardFace.NoColor));
            return faces;
        }

        public static List<DoubleCard> Build(Random rng)
        {
            var sun = SunFaces();
            var moon = MoonFaces();
            Shuffle(sun, rng);
            Shuffle(moon, rng);
            var deck = new List<DoubleCard>(DeckSize);
            for (int i = 0; i < DeckSize; i++) deck.Add(new DoubleCard(i, sun[i], moon[i]));
            return deck;
        }

        /// <summary>Fisher-Yates shuffle.</summary>
        public static void Shuffle<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                T tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }
    }
}
