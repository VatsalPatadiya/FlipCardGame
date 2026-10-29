using System;
using System.Collections.Generic;

namespace Eclipse.Core.Tests
{
    /// <summary>Builds hand-made rounds for rule tests: exact hands, top card and draw pile.</summary>
    sealed class TestTable
    {
        public readonly GameState State;
        public readonly TurnManager Tm;
        public readonly List<GameEvent> Log = new List<GameEvent>();
        int nextId = 1000;

        public TestTable(int players, Side side = Side.Sun, int seed = 1)
        {
            State = new GameState(players, new RuleSettings()) { ActiveSide = side, Phase = Phase.AwaitTurn, Current = 0 };
            Tm = TurnManager.FromState(State, new Random(seed));
            Tm.Events += Log.Add;
        }

        public static CardFace Sun(CardKind kind, int color = 0, int number = 0) => new CardFace(Side.Sun, kind, color, number);
        public static CardFace Moon(CardKind kind, int color = 0, int number = 0) => new CardFace(Side.Moon, kind, color, number);
        public static CardFace SunN(int color, int number) => Sun(CardKind.Number, color, number);
        public static CardFace MoonN(int color, int number) => Moon(CardKind.Number, color, number);

        public DoubleCard Card(CardFace sun, CardFace moon) => new DoubleCard(nextId++, sun, moon);

        /// <summary>A card whose active-side face is <paramref name="face"/>; the other face is a plain number.</summary>
        public DoubleCard Card(CardFace face) =>
            face.Side == Side.Sun ? Card(face, MoonN(3, 9)) : Card(SunN(3, 9), face);

        public DoubleCard Give(int seat, CardFace face)
        {
            var c = Card(face);
            State.Players[seat].Hand.Add(c);
            return c;
        }

        public DoubleCard Top(CardFace face, int? activeColor = null)
        {
            var c = Card(face);
            State.DiscardPile.Add(c);
            State.ActiveColor = activeColor ?? face.Color;
            return c;
        }

        /// <summary>Adds cards to the draw pile; the last one given is drawn first.</summary>
        public void Pile(params CardFace[] faces)
        {
            foreach (var f in faces) State.DrawPile.Add(Card(f));
        }

        /// <summary>Fill the draw pile with plain numbers of <paramref name="color"/>.</summary>
        public void FillPile(int count, int color = 3)
        {
            for (int i = 0; i < count; i++) State.DrawPile.Add(Card(SunN(color, 5), MoonN(color, 5)));
        }

        public int HandCount(int seat) => State.Players[seat].Hand.Count;
    }
}
