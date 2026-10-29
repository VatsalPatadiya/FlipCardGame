namespace Eclipse.Core
{
    /// <summary>
    /// Applies one bot decision to a round, for whichever seat the round is waiting on.
    /// The presentation layer calls <see cref="Step"/> after a short "thinking" delay; headless
    /// simulations call it in a loop. <paramref name="brains"/> is indexed by seat; null = human seat.
    /// </summary>
    public static class BotDriver
    {
        /// <summary>Returns false if the round is over or waiting on a human.</summary>
        public static bool Step(TurnManager tm, BotBrain[] brains)
        {
            var s = tm.State;
            switch (s.Phase)
            {
                case Phase.AwaitTurn:
                {
                    var bot = brains[s.Current];
                    if (bot == null) return false;
                    if (s.Players[bot.Seat].Hand.Count == 2 && bot.WillCallIt()) tm.CallIt(bot.Seat);
                    var choice = bot.ChooseTurn(s);
                    return choice.IsDraw ? tm.Draw(bot.Seat) : tm.Play(bot.Seat, choice.Card, choice.Color);
                }
                case Phase.AwaitDrawnDecision:
                {
                    var bot = brains[s.Current];
                    if (bot == null) return false;
                    if (!bot.PlayDrawn(s)) return tm.Pass(bot.Seat);
                    if (s.Players[bot.Seat].Hand.Count == 2 && bot.WillCallIt()) tm.CallIt(bot.Seat);
                    var card = s.DrawnCard;
                    int color = card.Face(s.ActiveSide).IsWild ? bot.ChooseColor(s, card) : CardFace.NoColor;
                    return tm.Play(bot.Seat, card, color);
                }
                case Phase.AwaitColor:
                {
                    var bot = brains[s.ColorChooser];
                    return bot != null && tm.ChooseColor(bot.Seat, bot.ChooseColor(s));
                }
                case Phase.AwaitChallenge:
                {
                    var bot = brains[s.Pending.Victim];
                    if (bot == null) return false;
                    return bot.ShouldChallenge(s) ? tm.Challenge(bot.Seat) : tm.Accept(bot.Seat);
                }
                default:
                    return false;
            }
        }

        /// <summary>
        /// Give every bot a chance to catch a player who didn't call it. Call this once when
        /// <see cref="GameState.Vulnerable"/> becomes set, before the next player acts.
        /// Returns the catching seat, or -1.
        /// </summary>
        public static int TryCatch(TurnManager tm, BotBrain[] brains)
        {
            var s = tm.State;
            int target = s.Vulnerable;
            if (target < 0) return -1;
            for (int k = 1; k < s.PlayerCount; k++)
            {
                var bot = brains[s.NextSeat(target, k)];
                if (bot != null && bot.WillCatch() && tm.Catch(bot.Seat, target)) return bot.Seat;
            }
            return -1;
        }
    }
}
