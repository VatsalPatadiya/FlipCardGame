using System;
using System.Linq;
using NUnit.Framework;
using static Eclipse.Core.Tests.TestTable;

namespace Eclipse.Core.Tests
{
    public class DeckAndRuleTests
    {
        [Test]
        public void DeckHas112UniqueDoubleSidedCards()
        {
            var deck = DeckBuilder.Build(new Random(7));
            Assert.AreEqual(112, deck.Count);
            Assert.AreEqual(112, deck.Select(c => c.Id).Distinct().Count());
            Assert.IsTrue(deck.All(c => c.Sun.Side == Side.Sun && c.Moon.Side == Side.Moon));
        }

        [TestCase(Side.Sun, CardKind.DrawOne, CardKind.Jump, CardKind.WildDrawTwo)]
        [TestCase(Side.Moon, CardKind.DrawFive, CardKind.JumpAll, CardKind.WildChase)]
        public void SideHasTheSpecCardCounts(Side side, CardKind draw, CardKind jump, CardKind strongWild)
        {
            var faces = side == Side.Sun ? DeckBuilder.SunFaces() : DeckBuilder.MoonFaces();
            Assert.AreEqual(112, faces.Count);
            Assert.AreEqual(72, faces.Count(f => f.Kind == CardKind.Number));
            Assert.IsFalse(faces.Any(f => f.Kind == CardKind.Number && (f.Number < 1 || f.Number > 9)), "numbers run 1-9, no zeros");
            foreach (var kind in new[] { draw, jump, CardKind.Turnabout, CardKind.Flip })
            {
                Assert.AreEqual(8, faces.Count(f => f.Kind == kind), kind.ToString());
                for (int c = 0; c < 4; c++) Assert.AreEqual(2, faces.Count(f => f.Kind == kind && f.Color == c));
            }
            Assert.AreEqual(4, faces.Count(f => f.Kind == CardKind.Wild));
            Assert.AreEqual(4, faces.Count(f => f.Kind == strongWild));
            for (int c = 0; c < 4; c++)
                for (int n = 1; n <= 9; n++)
                    Assert.AreEqual(2, faces.Count(f => f.Kind == CardKind.Number && f.Color == c && f.Number == n));
        }

        [Test]
        public void PairingIsRandomButSeedable()
        {
            var a = DeckBuilder.Build(new Random(1));
            var b = DeckBuilder.Build(new Random(1));
            var c = DeckBuilder.Build(new Random(2));
            Assert.IsTrue(a.Select(x => x.ToString()).SequenceEqual(b.Select(x => x.ToString())));
            Assert.IsFalse(a.Select(x => x.ToString()).SequenceEqual(c.Select(x => x.ToString())));
        }

        [Test]
        public void PointValuesMatchTheSpec()
        {
            Assert.AreEqual(7, SunN(0, 7).Points);
            Assert.AreEqual(10, Sun(CardKind.DrawOne).Points);
            Assert.AreEqual(20, Moon(CardKind.DrawFive).Points);
            Assert.AreEqual(20, Sun(CardKind.Turnabout).Points);
            Assert.AreEqual(20, Sun(CardKind.Jump).Points);
            Assert.AreEqual(20, Moon(CardKind.Flip).Points);
            Assert.AreEqual(30, Moon(CardKind.JumpAll).Points);
            Assert.AreEqual(40, Sun(CardKind.Wild).Points);
            Assert.AreEqual(50, Sun(CardKind.WildDrawTwo).Points);
            Assert.AreEqual(60, Moon(CardKind.WildChase).Points);
        }

        [Test]
        public void MatchingByColorNumberOrSymbol()
        {
            var top = SunN(1, 5);
            Assert.IsTrue(RuleValidator.CanPlay(SunN(1, 2), top, 1), "color");
            Assert.IsTrue(RuleValidator.CanPlay(SunN(2, 5), top, 1), "number");
            Assert.IsFalse(RuleValidator.CanPlay(SunN(2, 6), top, 1));
            Assert.IsFalse(RuleValidator.CanPlay(Sun(CardKind.Jump, 2), top, 1));
            Assert.IsTrue(RuleValidator.CanPlay(Sun(CardKind.Jump, 2), Sun(CardKind.Jump, 0), 0), "symbol");
            Assert.IsTrue(RuleValidator.CanPlay(Sun(CardKind.Wild), top, 1));
            Assert.IsTrue(RuleValidator.CanPlay(Sun(CardKind.WildDrawTwo), top, 1), "may bluff; challengeable");
        }

        [Test]
        public void OnAWildOnlyTheChosenColorMatches()
        {
            var top = Sun(CardKind.Wild);
            Assert.IsTrue(RuleValidator.CanPlay(SunN(2, 4), top, 2));
            Assert.IsFalse(RuleValidator.CanPlay(SunN(1, 4), top, 2));
            Assert.IsFalse(RuleValidator.CanPlay(Sun(CardKind.Jump, 1), Sun(CardKind.WildDrawTwo), 2));
            Assert.IsTrue(RuleValidator.CanPlay(Sun(CardKind.Wild), top, 2));
        }

        [Test]
        public void ChallengeableWildIsLegalOnlyWithoutTheCurrentColor()
        {
            var t = new TestTable(2);
            t.Top(SunN(1, 5));
            var wild = t.Give(0, Sun(CardKind.WildDrawTwo));
            t.Give(0, SunN(2, 5)); // number match doesn't count
            t.Give(0, Sun(CardKind.Wild));
            Assert.IsTrue(RuleValidator.IsLegalChallengeable(t.State, t.State.Players[0].Hand, wild));
            t.Give(0, SunN(1, 9));
            Assert.IsFalse(RuleValidator.IsLegalChallengeable(t.State, t.State.Players[0].Hand, wild));
        }
    }
}
