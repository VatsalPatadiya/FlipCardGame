using System;

namespace Eclipse.Core
{
    /// <summary>Which face of the cards is in play. Every round starts on Sun.</summary>
    public enum Side
    {
        Sun = 0,
        Moon = 1,
    }

    /// <summary>
    /// What a face does. Sun faces use Number, DrawOne, Jump, Turnabout, Flip, Wild, WildDrawTwo.
    /// Moon faces use Number, DrawFive, JumpAll, Turnabout, Flip, Wild, WildChase.
    /// </summary>
    public enum CardKind
    {
        Number,
        DrawOne,
        Jump,
        Turnabout,
        Flip,
        Wild,
        WildDrawTwo,
        DrawFive,
        JumpAll,
        WildChase,
    }

    /// <summary>One face of a double-sided card. Colors are indices 0-3 into the side's palette (see the theme).</summary>
    public readonly struct CardFace : IEquatable<CardFace>
    {
        public const int NoColor = -1;
        public const int ColorCount = 4;

        public readonly Side Side;
        public readonly CardKind Kind;
        /// <summary>0-3, or <see cref="NoColor"/> for wilds.</summary>
        public readonly int Color;
        /// <summary>1-9 for number cards, 0 otherwise.</summary>
        public readonly int Number;

        public CardFace(Side side, CardKind kind, int color, int number = 0)
        {
            Side = side;
            Kind = kind;
            Color = IsWildKind(kind) ? NoColor : color;
            Number = kind == CardKind.Number ? number : 0;
        }

        public static bool IsWildKind(CardKind kind) =>
            kind == CardKind.Wild || kind == CardKind.WildDrawTwo || kind == CardKind.WildChase;

        public bool IsWild => IsWildKind(Kind);

        /// <summary>Wild +2 and Wild Chase: only legal when you hold no card of the current color.</summary>
        public bool IsChallengeable => Kind == CardKind.WildDrawTwo || Kind == CardKind.WildChase;

        /// <summary>Score value when left in a hand at the end of a round (§3.8).</summary>
        public int Points
        {
            get
            {
                switch (Kind)
                {
                    case CardKind.Number: return Number;
                    case CardKind.DrawOne: return 10;
                    case CardKind.DrawFive:
                    case CardKind.Turnabout:
                    case CardKind.Jump:
                    case CardKind.Flip: return 20;
                    case CardKind.JumpAll: return 30;
                    case CardKind.Wild: return 40;
                    case CardKind.WildDrawTwo: return 50;
                    case CardKind.WildChase: return 60;
                    default: return 0;
                }
            }
        }

        /// <summary>Short text for logs and debug UI, e.g. "7", "+1", "Jump", "Wild +2".</summary>
        public string Label
        {
            get
            {
                switch (Kind)
                {
                    case CardKind.Number: return Number.ToString();
                    case CardKind.DrawOne: return "+1";
                    case CardKind.DrawFive: return "Surge +5";
                    case CardKind.Jump: return "Jump";
                    case CardKind.JumpAll: return "Jump All";
                    case CardKind.Turnabout: return "Turnabout";
                    case CardKind.Flip: return "Flip";
                    case CardKind.Wild: return "Wild";
                    case CardKind.WildDrawTwo: return "Wild +2";
                    case CardKind.WildChase: return "Wild Chase";
                    default: return Kind.ToString();
                }
            }
        }

        public bool Equals(CardFace other) =>
            Side == other.Side && Kind == other.Kind && Color == other.Color && Number == other.Number;

        public override bool Equals(object obj) => obj is CardFace other && Equals(other);

        public override int GetHashCode() => ((int)Side * 31 + (int)Kind) * 31 * 31 + (Color + 1) * 31 + Number;

        public override string ToString() => IsWild ? $"{Side} {Label}" : $"{Side} c{Color} {Label}";
    }

    /// <summary>A physical card: one Sun face and one Moon face. Ids are unique within a deck (0-111).</summary>
    public sealed class DoubleCard
    {
        public readonly int Id;
        public readonly CardFace Sun;
        public readonly CardFace Moon;

        public DoubleCard(int id, CardFace sun, CardFace moon)
        {
            Id = id;
            Sun = sun;
            Moon = moon;
        }

        public CardFace Face(Side side) => side == Side.Sun ? Sun : Moon;

        public override string ToString() => $"#{Id} [{Sun} | {Moon}]";
    }
}
