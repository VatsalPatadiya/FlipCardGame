# Eclipse (working title)

A double-sided "flip" card game for phones, built in Unity 6. Every card has a warm **Sun** face and a harsh **Moon** face. Playing a **Flip** card turns everything over at once: the draw pile, the discard pile and every hand. The first player to empty their hand wins the round, and the first to 500 points wins the match.

The full design brief (rules, card set, three themes, measurements) is in [`docs/DESIGN_SPEC.md`](docs/DESIGN_SPEC.md). It's an original game: no UNO/Mattel names, layouts or colors (see §0 of the spec).

## Status

This is roadmap step 1, the foundation:

| Part | State |
|---|---|
| Rules engine (pure C#, no Unity types) | Done: every rule in §3 of the spec, including challenges, "Call it!", flips, start cards and reshuffles |
| Scoring and matches | Done: winner-takes-points to 500, plus the "lowest total" alternative mode |
| Bots | Done: Easy, Medium and Hard |
| Themes as data | Done: Premium Set, Glass Aurora and Prism Holo, both sides, built from the spec values |
| Tests | 60 edit-mode tests, plus a 20,000-round bot simulation used during development |
| Card prefab, table screen, animations | Not started. Several card faces and screens are still undesigned (spec §8) |

## Open the project

1. In **Unity Hub**, choose **Add → Add project from disk** and pick this folder. Open it with **Unity 6000.5.9f1**. Unity creates the missing project settings on first import.
2. Run **Eclipse → Create Theme Assets** to write the three themes to `Assets/Eclipse/Themes/`. Existing assets are never overwritten, so hand-tuned changes are safe.
3. Run the tests from **Window → General → Test Runner → EditMode**.

## Project layout

```
Assets/Eclipse/
  Scripts/Core/     Eclipse.Core: the rules engine. No UnityEngine reference, so it runs on a server or in plain .NET too.
    Cards.cs          CardFace (one face), DoubleCard (Sun + Moon), point values
    DeckBuilder.cs    The 112-card deck; Sun and Moon faces shuffled separately, then paired
    GameState.cs      Round data, Phase, RuleSettings (house-rule toggles)
    RuleValidator.cs  Matching, and whether a Wild +2 / Wild Chase was legal
    TurnManager.cs    Every player action, each returning false if it's not allowed right now
    GameEvent.cs      What happened, in order, for animations, sounds, bots and networking
    ScoreCalculator.cs, Match.cs   Scoring, dealer choice and rotation, rounds to 500
    BotBrain.cs, BotDriver.cs      Bot decisions, and a helper that applies them
  Scripts/Themes/   Eclipse.Themes: CardTheme ScriptableObject, the three spec themes (ThemeLibrary), shared measurements (CardLayoutSpec)
  Editor/           Eclipse → Create Theme Assets
  Tests/            Edit-mode tests for the engine and the themes
docs/DESIGN_SPEC.md
```

## Using the engine

```csharp
var match = new Match(playerCount: 4, rng: new System.Random());
var round = match.StartRound(e => Debug.Log(e));   // events: CardPlayed, Flipped, CardsDrawn, ...
var s = round.State;                               // read-only for the UI: hands, piles, side, color, phase

// Human input:
round.Play(seat, card, chosenColor);   // chosenColor only for wilds
round.Draw(seat);                      // then Play(seat, s.DrawnCard) or Pass(seat)
round.CallIt(seat);  round.Catch(catcher, target);
round.Accept(seat);  round.Challenge(seat);
round.ChooseColor(seat, color);        // when s.Phase == Phase.AwaitColor

// Bots, after a short "thinking" delay:
BotDriver.Step(round, brains);         // brains[seat] = null for humans

// When s.Phase == Phase.RoundOver:
match.FinishRound();                   // adds the scores; match.MatchWinner >= 0 when it's over
```

`s.Phase` tells the UI what to show: `AwaitTurn`, `AwaitDrawnDecision` (play the drawn card or keep it), `AwaitColor` (color picker), `AwaitChallenge` (the 5-second "Challenge?" prompt), `RoundOver`.

## Rule decisions the spec left open

These are the calls the engine makes where the brief didn't say. Each is easy to change.

- **Flipping the piles.** A Flip turns the draw pile and the discard pile over like real stacks, so the old *bottom* card of the discard pile ends up on top, showing its other face. Turn off `RuleSettings.FlipTurnsPilesOver` to keep the Flip card itself on top instead.
- **Flip as the starting card.** The Moon face that's now showing sets the color, but its action doesn't apply. If it's a wild, the first player picks the color.
- **"Call it!"** can be tapped while holding 1 or 2 cards. Drawing any card cancels a call. You can be caught from the moment you're down to one card until another player takes an action.
- **A Wild +2 / Wild Chase as your last card** still gets the challenge prompt, but it's always legal (there's no other card in your hand), so a challenge always fails.
- **The deal** passes one seat to the left each round.
- **Bots never bluff.** They only play Wild +2 and Wild Chase legally, but Medium and Hard still challenge you based on the odds.

## Notes for the next steps

- **Color accessibility.** The spec asks for colors that differ in lightness, not just hue. None of the six palettes do that yet. Each side has a pair less than 5 L* apart: Premium Sun Tangerine and Aqua (66 and 67), Glass Sun Jade and Coral (61 and 62), Glass Moon Amethyst and Magenta (both 61), Holo Moon Mint and Gold (81 and 82), Premium Moon Violet and Rose, and Holo Sun Cobalt and Ruby. Adjust the colors, or rely on the colorblind corner symbols.
- **Hard bots** currently play about as well as Medium. Both beat Easy in about 69% of two-player rounds. Hard's card tracking and flip-holding need tuning once real play-testing starts.
- Theme values that the spec doesn't give (for example the Moon card shadows and the Wild Chase panels) are marked `// not in spec` in `ThemeLibrary.cs`.
- Fonts (Bungee, Righteous, Orbitron, Bricolage Grotesque, Unbounded, Syne, Manrope) aren't in the repo yet. Download them from Google Fonts (SIL OFL), generate the TextMeshPro SDF atlases (digits, `+`, `»`, A–Z), and assign them to each theme.
- The spec's animation stack uses DOTween (Asset Store), which still needs to be imported.
