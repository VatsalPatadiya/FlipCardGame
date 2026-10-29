using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Eclipse.Core.Tests
{
    public class RoundAndMatchTests
    {
        /// <summary>Finds a seed whose starting card (Sun face) is <paramref name="kind"/>.</summary>
        static TurnManager RoundStartingWith(CardKind kind, int players = 4, int dealer = 0)
        {
            for (int seed = 0; seed < 5000; seed++)
            {
                var tm = TurnManager.StartRound(players, dealer, new Random(seed));
                if (tm.State.DiscardPile[0].Sun.Kind == kind) return tm;
            }
            Assert.Fail("no seed found for " + kind);
            return null;
        }

        [Test]
        public void DealGivesSevenEachAndKeepsAll112Cards()
        {
            var tm = TurnManager.StartRound(5, 2, new Random(3));
            Assert.IsTrue(tm.State.Players.All(p => p.Hand.Count >= 7));
            Assert.AreEqual(112, tm.State.TotalCards());
            Assert.AreEqual(1, tm.State.DiscardPile.Count);
        }

        [Test]
        public void NumberStartIsNormal()
        {
            var tm = RoundStartingWith(CardKind.Number, dealer: 1);
            Assert.AreEqual(2, tm.State.Current);
            Assert.AreEqual(tm.State.DiscardPile[0].Sun.Color, tm.State.ActiveColor);
        }

        [Test]
        public void DrawOneStartSkipsTheFirstPlayerAfterDrawing()
        {
            var tm = RoundStartingWith(CardKind.DrawOne, dealer: 1);
            Assert.AreEqual(8, tm.State.Players[2].Hand.Count);
            Assert.AreEqual(3, tm.State.Current);
        }

        [Test]
        public void JumpStartSkipsTheFirstPlayer()
        {
            var tm = RoundStartingWith(CardKind.Jump, dealer: 1);
            Assert.AreEqual(3, tm.State.Current);
        }

        [Test]
        public void TurnaboutStartDealerPlaysFirstCounterClockwise()
        {
            var tm = RoundStartingWith(CardKind.Turnabout, dealer: 1);
            Assert.AreEqual(1, tm.State.Current);
            Assert.AreEqual(-1, tm.State.Direction);
            Assert.AreEqual(0, tm.State.NextSeat(1));
        }

        [Test]
        public void WildStartFirstPlayerChoosesThenPlays()
        {
            var tm = RoundStartingWith(CardKind.Wild, dealer: 1);
            Assert.AreEqual(Phase.AwaitColor, tm.State.Phase);
            Assert.AreEqual(2, tm.State.ColorChooser);
            Assert.IsTrue(tm.ChooseColor(2, 3));
            Assert.AreEqual(2, tm.State.Current);
            Assert.AreEqual(Phase.AwaitTurn, tm.State.Phase);
        }

        [Test]
        public void WildDrawTwoNeverStaysAsTheStartCard()
        {
            for (int seed = 0; seed < 2000; seed++)
            {
                var tm = TurnManager.StartRound(3, 0, new Random(seed));
                Assert.AreNotEqual(CardKind.WildDrawTwo, tm.State.DiscardPile[0].Sun.Kind);
            }
        }

        [Test]
        public void FlipStartFlipsAndPlaysOnTheMoonFace()
        {
            var tm = RoundStartingWith(CardKind.Flip, dealer: 1);
            var s = tm.State;
            Assert.AreEqual(Side.Moon, s.ActiveSide);
            Assert.AreEqual(2, s.Current);
            if (s.TopFace.IsWild) Assert.AreEqual(Phase.AwaitColor, s.Phase);
            else
            {
                Assert.AreEqual(Phase.AwaitTurn, s.Phase);
                Assert.AreEqual(s.TopFace.Color, s.ActiveColor);
            }
        }

        [Test]
        public void DealerIsChosenAndRotatesLeft()
        {
            var m = new Match(4, new Random(11));
            int first = m.Dealer;
            Assert.That(first, Is.InRange(0, 3));
            m.StartRound();
            Assert.AreEqual(first, m.Dealer);
            ForceRoundOver(m.Round, 0);
            m.FinishRound();
            m.StartRound();
            Assert.AreEqual((first + 1) % 4, m.Dealer);
        }

        [Test]
        public void WinnerTakesOpponentsPointsUntil500()
        {
            var m = new Match(3, new Random(5), new RuleSettings { TargetScore = 100 });
            m.StartRound();
            ForceRoundOver(m.Round, 1);
            int expected = ScoreCalculator.RoundPoints(m.Round.State, 1);
            var deltas = m.FinishRound();
            Assert.AreEqual(expected, deltas[1]);
            Assert.AreEqual(0, deltas[0] + deltas[2]);
            Assert.AreEqual(expected >= 100 ? 1 : -1, m.MatchWinner);
        }

        [Test]
        public void LowestTotalModeAddsOwnLeftoversAndLowestWins()
        {
            var m = new Match(3, new Random(5), new RuleSettings { Scoring = ScoringMode.LowestTotal, TargetScore = 50 });
            m.StartRound();
            ForceRoundOver(m.Round, 2);
            var s = m.Round.State;
            var deltas = m.FinishRound();
            for (int i = 0; i < 3; i++) Assert.AreEqual(ScoreCalculator.HandPoints(s.Players[i].Hand, s.ActiveSide), deltas[i]);
            Assert.AreEqual(0, deltas[2]);
            Assert.AreEqual(2, m.MatchWinner, "someone passed 50, and the round winner has the lowest total");
        }

        static void ForceRoundOver(TurnManager tm, int winner)
        {
            var s = tm.State;
            s.DrawPile.AddRange(s.Players[winner].Hand);
            s.Players[winner].Hand.Clear();
            s.Winner = winner;
            s.Phase = Phase.RoundOver;
        }

        [TestCase(2, BotDifficulty.Easy)]
        [TestCase(2, BotDifficulty.Hard)]
        [TestCase(3, BotDifficulty.Medium)]
        [TestCase(4, BotDifficulty.Hard)]
        [TestCase(6, BotDifficulty.Medium)]
        [TestCase(10, BotDifficulty.Easy)]
        public void BotsPlayWholeMatchesWithoutLosingCards(int players, BotDifficulty difficulty)
        {
            for (int seed = 0; seed < 8; seed++)
            {
                var rng = new Random(seed * 97 + players);
                var match = new Match(players, rng);
                var brains = Enumerable.Range(0, players).Select(i => new BotBrain(i, difficulty, rng)).ToArray();
                int rounds = 0;
                while (match.MatchWinner < 0)
                {
                    PlayRound(match, brains);
                    match.FinishRound();
                    Assert.Less(++rounds, 200, "match should finish");
                }
                Assert.GreaterOrEqual(match.Scores[match.MatchWinner], 500);
            }
        }

        static void PlayRound(Match match, BotBrain[] brains)
        {
            var tm = match.StartRound(e =>
            {
                foreach (var b in brains) b.Observe(match.Round.State, e);
            });
            int steps = 0;
            while (tm.State.Phase != Phase.RoundOver)
            {
                Assert.IsTrue(BotDriver.Step(tm, brains), "bot must always have a legal action");
                BotDriver.TryCatch(tm, brains);
                Assert.AreEqual(112, tm.State.TotalCards());
                Assert.IsTrue(AllUnique(tm.State));
                Assert.Less(++steps, 20000, "round should finish");
            }
            Assert.AreEqual(0, tm.State.Players[tm.State.Winner].Hand.Count);
        }

        static bool AllUnique(GameState s)
        {
            var ids = new HashSet<int>();
            foreach (var c in s.DrawPile) if (!ids.Add(c.Id)) return false;
            foreach (var c in s.DiscardPile) if (!ids.Add(c.Id)) return false;
            foreach (var p in s.Players)
                foreach (var c in p.Hand) if (!ids.Add(c.Id)) return false;
            return true;
        }

        [Test]
        public void HardBotsBeatEasyBotsMoreOftenThanNot()
        {
            int hardWins = 0, rounds = 0;
            var rng = new Random(42);
            for (int i = 0; i < 300; i++)
            {
                var brains = new[]
                {
                    new BotBrain(0, BotDifficulty.Hard, rng),
                    new BotBrain(1, BotDifficulty.Easy, rng),
                };
                var tm = TurnManager.NewRound(2, i % 2, rng);
                tm.Events += e => { foreach (var b in brains) b.Observe(tm.State, e); };
                tm.Deal();
                int steps = 0;
                while (tm.State.Phase != Phase.RoundOver && steps++ < 20000)
                {
                    BotDriver.Step(tm, brains);
                    BotDriver.TryCatch(tm, brains);
                }
                rounds++;
                if (tm.State.Winner == 0) hardWins++;
            }
            Assert.Greater(hardWins, rounds / 2, $"hard won {hardWins}/{rounds}");
        }
    }
}
