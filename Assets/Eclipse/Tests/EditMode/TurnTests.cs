using System.Linq;
using NUnit.Framework;
using static Eclipse.Core.Tests.TestTable;

namespace Eclipse.Core.Tests
{
    public class TurnTests
    {
        static TestTable Table(int players = 3, Side side = Side.Sun)
        {
            var t = new TestTable(players, side);
            t.FillPile(30);
            for (int seat = 0; seat < players; seat++)
                for (int i = 0; i < 4; i++) t.Give(seat, side == Side.Sun ? SunN(3, 9) : MoonN(3, 9));
            return t;
        }

        [Test]
        public void NumberPassesTheTurnLeft()
        {
            var t = Table();
            t.Top(SunN(0, 5));
            var card = t.Give(0, SunN(0, 2));
            Assert.IsTrue(t.Tm.Play(0, card));
            Assert.AreEqual(1, t.State.Current);
            Assert.AreEqual(card, t.State.Top);
        }

        [Test]
        public void CannotPlayOutOfTurnOrAnUnmatchedCard()
        {
            var t = Table();
            t.Top(SunN(0, 5));
            var wrong = t.Give(0, SunN(1, 2));
            var other = t.Give(1, SunN(0, 3));
            Assert.IsFalse(t.Tm.Play(0, wrong));
            Assert.IsFalse(t.Tm.Play(1, other));
            Assert.AreEqual(0, t.State.Current);
        }

        [Test]
        public void DrawOneMakesNextPlayerDrawAndLoseTheirTurn()
        {
            var t = Table();
            t.Top(SunN(0, 5));
            int before = t.HandCount(1);
            Assert.IsTrue(t.Tm.Play(0, t.Give(0, Sun(CardKind.DrawOne, 0))));
            Assert.AreEqual(before + 1, t.HandCount(1));
            Assert.AreEqual(2, t.State.Current);
        }

        [Test]
        public void SurgeFiveMakesNextPlayerDrawFive()
        {
            var t = Table(3, Side.Moon);
            t.Top(MoonN(0, 5));
            int before = t.HandCount(1);
            Assert.IsTrue(t.Tm.Play(0, t.Give(0, Moon(CardKind.DrawFive, 0))));
            Assert.AreEqual(before + 5, t.HandCount(1));
            Assert.AreEqual(2, t.State.Current);
        }

        [Test]
        public void NoStackingTheSkippedVictimCannotAnswerWithADrawCard()
        {
            var t = Table();
            t.Top(SunN(0, 5));
            var counter = t.Give(1, Sun(CardKind.DrawOne, 0));
            t.Tm.Play(0, t.Give(0, Sun(CardKind.DrawOne, 0)));
            Assert.IsFalse(t.Tm.Play(1, counter));
        }

        [Test]
        public void JumpSkipsTheNextPlayer()
        {
            var t = Table(4);
            t.Top(SunN(0, 5));
            t.Tm.Play(0, t.Give(0, Sun(CardKind.Jump, 0)));
            Assert.AreEqual(2, t.State.Current);
        }

        [Test]
        public void JumpAllLetsThePlayerGoAgain()
        {
            var t = Table(4, Side.Moon);
            t.Top(MoonN(0, 5));
            t.Tm.Play(0, t.Give(0, Moon(CardKind.JumpAll, 0)));
            Assert.AreEqual(0, t.State.Current);
            Assert.AreEqual(3, t.Log.Count(e => e.Type == GameEventType.PlayerSkipped));
        }

        [Test]
        public void TurnaboutReversesDirection()
        {
            var t = Table(4);
            t.Top(SunN(0, 5));
            t.State.Current = 1;
            t.Tm.Play(1, t.Give(1, Sun(CardKind.Turnabout, 0)));
            Assert.AreEqual(-1, t.State.Direction);
            Assert.AreEqual(0, t.State.Current);
        }

        [Test]
        public void TwoPlayerTurnaboutActsLikeJump()
        {
            var t = Table(2);
            t.Top(SunN(0, 5));
            t.Tm.Play(0, t.Give(0, Sun(CardKind.Turnabout, 0)));
            Assert.AreEqual(0, t.State.Current);
        }

        [Test]
        public void WildNeedsAColorAndSetsIt()
        {
            var t = Table();
            t.Top(SunN(0, 5));
            var wild = t.Give(0, Sun(CardKind.Wild));
            Assert.IsFalse(t.Tm.Play(0, wild));
            Assert.IsTrue(t.Tm.Play(0, wild, 2));
            Assert.AreEqual(2, t.State.ActiveColor);
            Assert.AreEqual(1, t.State.Current);
        }

        [Test]
        public void DrawingAnUnplayableCardEndsTheTurn()
        {
            var t = Table();
            t.Top(SunN(0, 5));
            t.Pile(SunN(1, 1));
            Assert.IsTrue(t.Tm.Draw(0));
            Assert.AreEqual(1, t.State.Current);
            Assert.AreEqual(Phase.AwaitTurn, t.State.Phase);
        }

        [Test]
        public void DrawingAPlayableCardLetsYouPlayOnlyThatCardOrPass()
        {
            var t = Table();
            t.Top(SunN(0, 5));
            var inHand = t.Give(0, SunN(0, 1));
            t.Pile(SunN(0, 2));
            t.Tm.Draw(0);
            Assert.AreEqual(Phase.AwaitDrawnDecision, t.State.Phase);
            Assert.IsFalse(t.Tm.Play(0, inHand));
            Assert.IsTrue(t.Tm.Play(0, t.State.DrawnCard));
            Assert.AreEqual(1, t.State.Current);

            var t2 = Table();
            t2.Top(SunN(0, 5));
            t2.Pile(SunN(0, 2));
            t2.Tm.Draw(0);
            Assert.IsTrue(t2.Tm.Pass(0));
            Assert.AreEqual(1, t2.State.Current);
        }

        [Test]
        public void FlipTurnsEverythingOver()
        {
            var t = Table();
            var bottom = t.Top(SunN(0, 5));
            var bottomMoonColor = bottom.Moon.Color;
            var firstDraw = t.State.DrawPile[t.State.DrawPile.Count - 1];
            var lastDraw = t.State.DrawPile[0];
            var flip = t.Give(0, Sun(CardKind.Flip, 0));

            Assert.IsTrue(t.Tm.Play(0, flip));
            Assert.AreEqual(Side.Moon, t.State.ActiveSide);
            Assert.AreEqual(bottom, t.State.Top, "discard pile turned over: the old bottom card is on top");
            Assert.AreEqual(flip, t.State.DiscardPile[0]);
            Assert.AreEqual(lastDraw, t.State.DrawPile[t.State.DrawPile.Count - 1], "draw pile turned over");
            Assert.AreEqual(firstDraw, t.State.DrawPile[0]);
            Assert.AreEqual(bottomMoonColor, t.State.ActiveColor);
            Assert.AreEqual(1, t.State.Current, "turn order carries over");
        }

        [Test]
        public void FlipWithoutTurningPilesKeepsTheFlipCardOnTop()
        {
            var t = Table();
            t.State.Rules.FlipTurnsPilesOver = false;
            t.Top(SunN(0, 5));
            var card = t.Card(Sun(CardKind.Flip, 0), MoonN(2, 4));
            t.State.Players[0].Hand.Add(card);
            t.Tm.Play(0, card);
            Assert.AreEqual(card, t.State.Top);
            Assert.AreEqual(2, t.State.ActiveColor);
        }

        [Test]
        public void FlipOntoAWildLetsTheFlipperChooseTheColor()
        {
            var t = Table();
            t.State.Rules.FlipTurnsPilesOver = false;
            t.Top(SunN(0, 5));
            var card = t.Card(Sun(CardKind.Flip, 0), Moon(CardKind.Wild));
            t.State.Players[0].Hand.Add(card);
            t.Tm.Play(0, card);
            Assert.AreEqual(Phase.AwaitColor, t.State.Phase);
            Assert.AreEqual(0, t.State.ColorChooser);
            Assert.IsFalse(t.Tm.ChooseColor(1, 2));
            Assert.IsTrue(t.Tm.ChooseColor(0, 3));
            Assert.AreEqual(3, t.State.ActiveColor);
            Assert.AreEqual(1, t.State.Current);
        }

        [Test]
        public void WildDrawTwoAccepted()
        {
            var t = Table();
            t.Top(SunN(0, 5));
            int before = t.HandCount(1);
            t.Tm.Play(0, t.Give(0, Sun(CardKind.WildDrawTwo)), 1);
            Assert.AreEqual(Phase.AwaitChallenge, t.State.Phase);
            Assert.IsFalse(t.Tm.Accept(2));
            Assert.IsTrue(t.Tm.Accept(1));
            Assert.AreEqual(before + 2, t.HandCount(1));
            Assert.AreEqual(2, t.State.Current);
            Assert.AreEqual(1, t.State.ActiveColor);
        }

        [Test]
        public void WildDrawTwoChallengeCatchesABluff()
        {
            var t = Table();
            t.Top(SunN(0, 5));
            t.Give(0, SunN(0, 1)); // held the active color: illegal
            int playerBefore = t.HandCount(0) + 1;
            int victimBefore = t.HandCount(1);
            t.Tm.Play(0, t.Give(0, Sun(CardKind.WildDrawTwo)), 1);
            Assert.IsTrue(t.Tm.Challenge(1));
            Assert.AreEqual(playerBefore - 1 + 2, t.HandCount(0));
            Assert.AreEqual(victimBefore, t.HandCount(1));
            Assert.AreEqual(1, t.State.Current, "challenger takes their turn");
            Assert.IsTrue(t.Log.Any(e => e.Type == GameEventType.ChallengeResolved && e.Success));
        }

        [Test]
        public void WildDrawTwoFailedChallengeCostsFour()
        {
            var t = Table();
            t.Top(SunN(0, 5));
            int victimBefore = t.HandCount(1);
            t.Tm.Play(0, t.Give(0, Sun(CardKind.WildDrawTwo)), 1);
            t.Tm.Challenge(1);
            Assert.AreEqual(victimBefore + 4, t.HandCount(1));
            Assert.AreEqual(2, t.State.Current);
        }

        [Test]
        public void WildChaseAcceptedDrawsUntilTheColor()
        {
            var t = new TestTable(3, Side.Moon);
            for (int s = 0; s < 3; s++) t.Give(s, MoonN(3, 9));
            t.Pile(MoonN(2, 1), MoonN(2, 2), MoonN(1, 3), MoonN(0, 4), Moon(CardKind.Wild)); // drawn right to left
            t.Top(MoonN(0, 5));
            t.Tm.Play(0, t.Give(0, Moon(CardKind.WildChase)), 2);
            t.Tm.Accept(1);
            Assert.AreEqual(1 + 4, t.HandCount(1), "wild, 4, 3, then the first color-2 card");
            Assert.AreEqual(1, t.State.DrawPile.Count);
            Assert.AreEqual(2, t.State.Current);
        }

        [Test]
        public void WildChaseFailedChallengeIsUntilColorPlusTwo()
        {
            var t = new TestTable(3, Side.Moon);
            for (int s = 0; s < 3; s++) t.Give(s, MoonN(3, 9));
            t.Pile(MoonN(1, 1), MoonN(1, 2), MoonN(2, 2), MoonN(1, 3));
            t.Top(MoonN(0, 5));
            t.Tm.Play(0, t.Give(0, Moon(CardKind.WildChase)), 2);
            t.Tm.Challenge(1);
            Assert.AreEqual(1 + 2 + 2, t.HandCount(1));
            Assert.AreEqual(2, t.State.Current);
        }

        [Test]
        public void WildChaseCaughtBluffMakesThePlayerChase()
        {
            var t = new TestTable(3, Side.Moon);
            for (int s = 0; s < 3; s++) t.Give(s, MoonN(3, 9));
            t.Give(0, MoonN(0, 1));
            t.Pile(MoonN(2, 1), MoonN(1, 3));
            t.Top(MoonN(0, 5));
            t.Tm.Play(0, t.Give(0, Moon(CardKind.WildChase)), 2);
            t.Tm.Challenge(1);
            Assert.AreEqual(2 + 2, t.HandCount(0));
            Assert.AreEqual(1, t.State.Current);
        }

        [Test]
        public void WildChaseStopsWhenTheCardsRunOut()
        {
            var t = new TestTable(2, Side.Moon);
            t.Give(0, MoonN(3, 9));
            t.Give(1, MoonN(3, 9));
            t.Pile(MoonN(1, 1), MoonN(1, 2));
            t.Top(MoonN(0, 5));
            t.Tm.Play(0, t.Give(0, Moon(CardKind.WildChase)), 2);
            t.Tm.Accept(1);
            Assert.AreEqual(1 + 3, t.HandCount(1), "both pile cards, then the reshuffled old top card");
            Assert.AreEqual(0, t.State.DrawPile.Count);
            Assert.AreEqual(1, t.State.DiscardPile.Count, "only the Wild Chase is left");
        }

        [Test]
        public void EmptyDrawPileIsRebuiltFromTheDiscardsUnderTheTopCard()
        {
            var t = new TestTable(2);
            t.Give(0, SunN(1, 1));
            t.Give(1, SunN(1, 1));
            t.Top(SunN(2, 2));
            t.Top(SunN(2, 3));
            var top = t.Top(SunN(0, 5));
            t.Tm.Draw(0);
            Assert.AreEqual(top, t.State.Top);
            Assert.AreEqual(1, t.State.DiscardPile.Count);
            Assert.AreEqual(1, t.State.DrawPile.Count);
            Assert.IsTrue(t.Log.Any(e => e.Type == GameEventType.Reshuffled));
            Assert.AreEqual(Side.Sun, t.State.ActiveSide);
        }

        [Test]
        public void ForgettingToCallItCanBeCaught()
        {
            var t = Table();
            t.Top(SunN(0, 5));
            t.State.Players[0].Hand.Clear();
            t.Give(0, SunN(1, 1));
            t.Tm.Play(0, t.Give(0, SunN(0, 2)));
            Assert.AreEqual(0, t.State.Vulnerable);
            Assert.IsFalse(t.Tm.Catch(0, 0));
            Assert.IsTrue(t.Tm.Catch(2, 0));
            Assert.AreEqual(3, t.HandCount(0));
            Assert.AreEqual(-1, t.State.Vulnerable);
        }

        [Test]
        public void CallingItProtectsYou()
        {
            var t = Table();
            t.Top(SunN(0, 5));
            t.State.Players[0].Hand.Clear();
            t.Give(0, SunN(1, 1));
            var card = t.Give(0, SunN(0, 2));
            Assert.IsTrue(t.Tm.CallIt(0));
            t.Tm.Play(0, card);
            Assert.AreEqual(-1, t.State.Vulnerable);
            Assert.IsFalse(t.Tm.Catch(1, 0));
        }

        [Test]
        public void CatchWindowClosesWhenTheNextPlayerActs()
        {
            var t = Table();
            t.Top(SunN(0, 5));
            t.State.Players[0].Hand.Clear();
            t.Give(0, SunN(1, 1));
            t.Tm.Play(0, t.Give(0, SunN(0, 2)));
            t.Tm.Draw(1);
            Assert.IsFalse(t.Tm.Catch(2, 0));
        }

        [Test]
        public void PlayingTheLastCardEndsTheRoundAndScoresTheActiveSide()
        {
            var t = new TestTable(3);
            t.FillPile(5);
            t.Top(SunN(0, 5));
            var last = t.Give(0, SunN(0, 2));
            t.State.Players[1].Hand.Add(t.Card(SunN(1, 7), Moon(CardKind.WildChase)));
            t.State.Players[2].Hand.Add(t.Card(Sun(CardKind.Wild), MoonN(1, 1)));
            t.Tm.Play(0, last);
            Assert.AreEqual(Phase.RoundOver, t.State.Phase);
            Assert.AreEqual(0, t.State.Winner);
            Assert.AreEqual(47, ScoreCalculator.RoundPoints(t.State, 0));
        }

        [Test]
        public void DrawPenaltyOnTheLastCardStillApplies()
        {
            var t = new TestTable(2);
            t.FillPile(5, color: 1);
            t.Top(SunN(0, 5));
            t.Give(1, SunN(1, 1));
            t.Tm.Play(0, t.Give(0, Sun(CardKind.DrawOne, 0)));
            Assert.AreEqual(Phase.RoundOver, t.State.Phase);
            Assert.AreEqual(2, t.HandCount(1));
            Assert.AreEqual(6, ScoreCalculator.RoundPoints(t.State, 0));
        }

        [Test]
        public void LastCardFlipScoresTheNewSide()
        {
            var t = new TestTable(2);
            t.FillPile(3);
            t.Top(SunN(0, 5));
            t.State.Players[1].Hand.Add(t.Card(SunN(1, 1), Moon(CardKind.JumpAll, 1)));
            t.Tm.Play(0, t.Give(0, Sun(CardKind.Flip, 0)));
            Assert.AreEqual(Phase.RoundOver, t.State.Phase);
            Assert.AreEqual(Side.Moon, t.State.ActiveSide);
            Assert.AreEqual(30, ScoreCalculator.RoundPoints(t.State, 0));
        }

        [Test]
        public void LastCardWildDrawTwoWaitsForTheChallenge()
        {
            var t = new TestTable(2);
            t.FillPile(6);
            t.Top(SunN(0, 5));
            t.Give(1, SunN(1, 1));
            t.Tm.Play(0, t.Give(0, Sun(CardKind.WildDrawTwo)), 2);
            Assert.AreEqual(Phase.AwaitChallenge, t.State.Phase);
            t.Tm.Accept(1);
            Assert.AreEqual(Phase.RoundOver, t.State.Phase);
            Assert.AreEqual(3, t.HandCount(1));
        }
    }
}
