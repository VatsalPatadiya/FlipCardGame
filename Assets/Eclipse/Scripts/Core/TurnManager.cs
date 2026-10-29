using System;
using System.Collections.Generic;

namespace Eclipse.Core
{
    /// <summary>
    /// The rules engine for one round. Every player action goes through a method here; each returns
    /// false (and changes nothing) if the action isn't allowed right now. What happened is reported
    /// through <see cref="Events"/> in order, so the presentation layer can animate it step by step.
    /// No Unity types: bots, tests and an online server all share this class.
    /// </summary>
    public sealed class TurnManager
    {
        public readonly GameState State;
        readonly Random rng;

        public event Action<GameEvent> Events;

        TurnManager(GameState state, Random rng)
        {
            State = state;
            this.rng = rng;
        }

        /// <summary>Shuffle a new deck, deal, and turn up the starting card (§3.1, §3.2).</summary>
        public static TurnManager StartRound(int playerCount, int dealer, Random rng, RuleSettings rules = null)
        {
            var tm = NewRound(playerCount, dealer, rng, rules);
            tm.Deal();
            return tm;
        }

        /// <summary>A shuffled round that hasn't been dealt yet: subscribe to <see cref="Events"/>, then call <see cref="Deal"/>.</summary>
        public static TurnManager NewRound(int playerCount, int dealer, Random rng, RuleSettings rules = null)
        {
            if (playerCount < 2 || playerCount > 10) throw new ArgumentOutOfRangeException(nameof(playerCount), "2-10 players");
            var state = new GameState(playerCount, rules) { Dealer = dealer };
            state.DrawPile.AddRange(DeckBuilder.Build(rng));
            return new TurnManager(state, rng);
        }

        /// <summary>Build a round from a prepared state (tests, saved games, network sync).</summary>
        public static TurnManager FromState(GameState state, Random rng) => new TurnManager(state, rng);

        // ---------- Setup ----------

        public void Deal()
        {
            var s = State;
            if (s.DiscardPile.Count > 0) throw new InvalidOperationException("Already dealt.");
            for (int i = 0; i < s.Rules.HandSize; i++)
                for (int k = 1; k <= s.PlayerCount; k++)
                    s.Players[s.NextSeat(s.Dealer, k)].Hand.Add(Pop(s.DrawPile));
            Emit(new GameEvent { Type = GameEventType.RoundStarted, Seat = s.Dealer });

            DoubleCard start;
            while (true)
            {
                start = Pop(s.DrawPile);
                if (start.Sun.Kind != CardKind.WildDrawTwo) break;
                s.DrawPile.Add(start);
                DeckBuilder.Shuffle(s.DrawPile, rng);
                Emit(new GameEvent { Type = GameEventType.StartCardReturned, Card = start });
            }
            s.DiscardPile.Add(start);
            Emit(new GameEvent { Type = GameEventType.StartCardTurned, Card = start, Side = s.ActiveSide });

            int first = s.NextSeat(s.Dealer);
            var face = start.Sun;
            s.ActiveColor = face.Color;
            switch (face.Kind)
            {
                case CardKind.DrawOne:
                    DrawCards(first, 1);
                    Skip(first);
                    BeginTurn(s.NextSeat(first));
                    break;
                case CardKind.Jump:
                    Skip(first);
                    BeginTurn(s.NextSeat(first));
                    break;
                case CardKind.Turnabout:
                    // Dealer plays first, play goes counter-clockwise. With 2 players this is the same as a Jump.
                    s.Direction = -1;
                    Emit(new GameEvent { Type = GameEventType.DirectionChanged, Count = s.Direction });
                    BeginTurn(s.Dealer);
                    break;
                case CardKind.Wild:
                    AskColor(first, advanceAfter: false);
                    break;
                case CardKind.Flip:
                    DoFlip();
                    // The first player plays on the Moon face now showing; its action doesn't apply.
                    if (s.TopFace.IsWild) AskColor(first, advanceAfter: false);
                    else
                    {
                        s.ActiveColor = s.TopFace.Color;
                        BeginTurn(first);
                    }
                    break;
                default:
                    BeginTurn(first);
                    break;
            }
        }

        // ---------- Actions ----------

        public bool CanPlay(int seat, DoubleCard card)
        {
            var s = State;
            if (seat != s.Current || card == null) return false;
            if (s.Phase == Phase.AwaitDrawnDecision) { if (card != s.DrawnCard) return false; }
            else if (s.Phase != Phase.AwaitTurn) return false;
            return s.Players[seat].Hand.Contains(card) && RuleValidator.CanPlay(s, card);
        }

        /// <summary>Play a card. Wilds need <paramref name="chosenColor"/> (0-3).</summary>
        public bool Play(int seat, DoubleCard card, int chosenColor = CardFace.NoColor)
        {
            if (!CanPlay(seat, card)) return false;
            var s = State;
            var face = card.Face(s.ActiveSide);
            if (face.IsWild && (chosenColor < 0 || chosenColor >= CardFace.ColorCount)) return false;

            CloseCatchWindow(seat);
            var player = s.Players[seat];
            bool wasLegal = !face.IsChallengeable || RuleValidator.IsLegalChallengeable(s, player.Hand, card);
            int previousColor = s.ActiveColor;

            player.Hand.Remove(card);
            s.DiscardPile.Add(card);
            s.DrawnCard = null;
            Emit(new GameEvent { Type = GameEventType.CardPlayed, Seat = seat, Card = card, Side = s.ActiveSide });

            if (player.Hand.Count == 1 && !player.CalledIt) s.Vulnerable = seat;

            if (face.IsWild)
            {
                s.ActiveColor = chosenColor;
                Emit(new GameEvent { Type = GameEventType.ColorChosen, Seat = seat, Color = chosenColor });
            }
            else s.ActiveColor = face.Color;

            int next = s.NextSeat(seat);
            if (player.Hand.Count == 0 && !face.IsChallengeable)
            {
                // Last card: a draw penalty still applies before scoring, and a Flip changes the side that's scored.
                if (face.Kind == CardKind.DrawOne) DrawCards(next, 1);
                else if (face.Kind == CardKind.DrawFive) DrawCards(next, 5);
                else if (face.Kind == CardKind.Flip) DoFlip();
                EndRound(seat);
                return true;
            }

            switch (face.Kind)
            {
                case CardKind.DrawOne:
                case CardKind.DrawFive:
                    DrawCards(next, face.Kind == CardKind.DrawOne ? 1 : 5);
                    Skip(next);
                    BeginTurn(s.NextSeat(next));
                    break;
                case CardKind.Jump:
                    Skip(next);
                    BeginTurn(s.NextSeat(next));
                    break;
                case CardKind.JumpAll:
                    for (int k = 1; k < s.PlayerCount; k++) Skip(s.NextSeat(seat, k));
                    BeginTurn(seat);
                    break;
                case CardKind.Turnabout:
                    if (s.PlayerCount == 2)
                    {
                        // Two players: works like a Jump, so you play again.
                        Skip(next);
                        BeginTurn(seat);
                    }
                    else
                    {
                        s.Direction = -s.Direction;
                        Emit(new GameEvent { Type = GameEventType.DirectionChanged, Seat = seat, Count = s.Direction });
                        BeginTurn(s.NextSeat(seat));
                    }
                    break;
                case CardKind.Flip:
                    DoFlip();
                    // The new top face sets the color. If it's a wild, the player who flipped chooses.
                    if (s.TopFace.IsWild) AskColor(seat, advanceAfter: true);
                    else
                    {
                        s.ActiveColor = s.TopFace.Color;
                        BeginTurn(s.NextSeat(seat));
                    }
                    break;
                case CardKind.WildDrawTwo:
                case CardKind.WildChase:
                    s.Pending = new PendingWild
                    {
                        Player = seat, Victim = next, Kind = face.Kind, ChosenColor = chosenColor,
                        PreviousColor = previousColor, WasLegal = wasLegal,
                    };
                    s.Current = next;
                    // On a last card the round still waits here so the next player gets the challenge prompt.
                    s.Phase = Phase.AwaitChallenge;
                    break;
                default:
                    BeginTurn(next);
                    break;
            }
            return true;
        }

        /// <summary>Draw one card instead of playing. If it's playable the player may play it right away.</summary>
        public bool Draw(int seat)
        {
            var s = State;
            if (s.Phase != Phase.AwaitTurn || seat != s.Current) return false;
            CloseCatchWindow(seat);
            var card = DrawCards(seat, 1);
            if (card != null && RuleValidator.CanPlay(s, card))
            {
                s.DrawnCard = card;
                s.Phase = Phase.AwaitDrawnDecision;
            }
            else BeginTurn(s.NextSeat(seat));
            return true;
        }

        /// <summary>Keep the playable card just drawn and end the turn.</summary>
        public bool Pass(int seat)
        {
            var s = State;
            if (s.Phase != Phase.AwaitDrawnDecision || seat != s.Current) return false;
            BeginTurn(s.NextSeat(seat));
            return true;
        }

        public bool ChooseColor(int seat, int color)
        {
            var s = State;
            if (s.Phase != Phase.AwaitColor || seat != s.ColorChooser) return false;
            if (color < 0 || color >= CardFace.ColorCount) return false;
            s.ActiveColor = color;
            s.ColorChooser = -1;
            Emit(new GameEvent { Type = GameEventType.ColorChosen, Seat = seat, Color = color });
            BeginTurn(s.AdvanceAfterColor ? s.NextSeat(seat) : seat);
            return true;
        }

        /// <summary>Take the Wild +2 / Wild Chase penalty without challenging.</summary>
        public bool Accept(int seat) => ResolveWild(seat, challenge: false);

        /// <summary>Challenge the Wild +2 / Wild Chase (§3.4). The engine checks the hand.</summary>
        public bool Challenge(int seat) => ResolveWild(seat, challenge: true);

        bool ResolveWild(int seat, bool challenge)
        {
            var s = State;
            if (s.Phase != Phase.AwaitChallenge || seat != s.Pending.Victim) return false;
            CloseCatchWindow(seat);
            var p = s.Pending;
            s.Pending = null;
            bool chase = p.Kind == CardKind.WildChase;
            bool victimTakesTurn = false;

            if (!challenge)
            {
                if (chase) DrawUntilColor(p.Victim, p.ChosenColor);
                else DrawCards(p.Victim, 2);
            }
            else if (!p.WasLegal)
            {
                // Caught a bluff: the player who played it takes the penalty, the challenger plays.
                Emit(new GameEvent { Type = GameEventType.ChallengeResolved, Seat = p.Player, OtherSeat = seat, Success = true });
                if (chase) DrawUntilColor(p.Player, p.ChosenColor);
                else DrawCards(p.Player, 2);
                victimTakesTurn = true;
            }
            else
            {
                Emit(new GameEvent { Type = GameEventType.ChallengeResolved, Seat = p.Player, OtherSeat = seat, Success = false });
                if (chase)
                {
                    DrawUntilColor(p.Victim, p.ChosenColor);
                    DrawCards(p.Victim, 2);
                }
                else DrawCards(p.Victim, 4);
            }

            if (s.Players[p.Player].Hand.Count == 0)
            {
                EndRound(p.Player);
                return true;
            }
            if (victimTakesTurn) BeginTurn(p.Victim);
            else
            {
                Skip(p.Victim);
                BeginTurn(s.NextSeat(p.Victim));
            }
            return true;
        }

        /// <summary>"Call it!": allowed while holding one or two cards.</summary>
        public bool CallIt(int seat)
        {
            var s = State;
            if (s.Phase == Phase.RoundOver) return false;
            var player = s.Players[seat];
            if (player.CalledIt || player.Hand.Count < 1 || player.Hand.Count > 2) return false;
            player.CalledIt = true;
            if (s.Vulnerable == seat) s.Vulnerable = -1;
            Emit(new GameEvent { Type = GameEventType.CalledIt, Seat = seat });
            return true;
        }

        /// <summary>
        /// Catch a player who is down to one card without calling it, before anyone else has acted.
        /// They draw 2.
        /// </summary>
        public bool Catch(int catcher, int target)
        {
            var s = State;
            if (s.Phase == Phase.RoundOver || catcher == target || s.Vulnerable != target) return false;
            s.Vulnerable = -1;
            Emit(new GameEvent { Type = GameEventType.Caught, Seat = target, OtherSeat = catcher });
            DrawCards(target, 2);
            return true;
        }

        // ---------- Internals ----------

        void BeginTurn(int seat)
        {
            var s = State;
            s.Current = seat;
            s.Phase = Phase.AwaitTurn;
            s.DrawnCard = null;
            Emit(new GameEvent { Type = GameEventType.TurnStarted, Seat = seat });
        }

        void AskColor(int seat, bool advanceAfter)
        {
            var s = State;
            s.Current = seat;
            s.ColorChooser = seat;
            s.AdvanceAfterColor = advanceAfter;
            s.Phase = Phase.AwaitColor;
        }

        void Skip(int seat) => Emit(new GameEvent { Type = GameEventType.PlayerSkipped, Seat = seat });

        /// <summary>The next player beginning their turn closes the window for catching an un-called last card.</summary>
        void CloseCatchWindow(int actingSeat)
        {
            if (State.Vulnerable != actingSeat) State.Vulnerable = -1;
        }

        void DoFlip()
        {
            var s = State;
            s.ActiveSide = s.ActiveSide == Side.Sun ? Side.Moon : Side.Sun;
            if (s.Rules.FlipTurnsPilesOver)
            {
                s.DrawPile.Reverse();
                s.DiscardPile.Reverse();
            }
            Emit(new GameEvent { Type = GameEventType.Flipped, Side = s.ActiveSide, Card = s.Top });
        }

        /// <summary>Draws up to <paramref name="count"/> cards. Returns the last card drawn, or null if none were left.</summary>
        DoubleCard DrawCards(int seat, int count)
        {
            DoubleCard last = null;
            int drawn = 0;
            for (int i = 0; i < count; i++)
            {
                var card = TakeFromDrawPile();
                if (card == null) break;
                State.Players[seat].Hand.Add(card);
                last = card;
                drawn++;
            }
            AfterDraw(seat, drawn);
            return last;
        }

        /// <summary>Draw until a card of <paramref name="color"/> turns up, or the cards run out (§3.9).</summary>
        void DrawUntilColor(int seat, int color)
        {
            var s = State;
            int drawn = 0;
            while (true)
            {
                var card = TakeFromDrawPile();
                if (card == null) break;
                s.Players[seat].Hand.Add(card);
                drawn++;
                if (card.Face(s.ActiveSide).Color == color) break;
            }
            AfterDraw(seat, drawn);
        }

        void AfterDraw(int seat, int drawn)
        {
            var s = State;
            var player = s.Players[seat];
            if (drawn > 0) player.CalledIt = false;
            if (s.Vulnerable == seat && player.Hand.Count > 1) s.Vulnerable = -1;
            Emit(new GameEvent { Type = GameEventType.CardsDrawn, Seat = seat, Count = drawn });
        }

        /// <summary>When the draw pile runs out, the discard pile (except its top card) is shuffled into a new one, on the current side.</summary>
        DoubleCard TakeFromDrawPile()
        {
            var s = State;
            if (s.DrawPile.Count == 0)
            {
                if (s.DiscardPile.Count <= 1) return null;
                var top = Pop(s.DiscardPile);
                s.DrawPile.AddRange(s.DiscardPile);
                s.DiscardPile.Clear();
                s.DiscardPile.Add(top);
                DeckBuilder.Shuffle(s.DrawPile, rng);
                Emit(new GameEvent { Type = GameEventType.Reshuffled, Count = s.DrawPile.Count });
            }
            return Pop(s.DrawPile);
        }

        void EndRound(int winner)
        {
            var s = State;
            s.Winner = winner;
            s.Phase = Phase.RoundOver;
            s.Vulnerable = -1;
            Emit(new GameEvent
            {
                Type = GameEventType.RoundEnded, Seat = winner, Side = s.ActiveSide,
                Points = ScoreCalculator.RoundPoints(s, winner),
            });
        }

        static DoubleCard Pop(List<DoubleCard> pile)
        {
            var card = pile[pile.Count - 1];
            pile.RemoveAt(pile.Count - 1);
            return card;
        }

        void Emit(GameEvent e) => Events?.Invoke(e);
    }
}
