using System.Linq;
using Eclipse.Core;
using Eclipse.Themes;
using NUnit.Framework;
using UnityEngine;

namespace Eclipse.Rendering.Tests
{
    public class CardViewTests
    {
        [Test]
        public void NumberCardShowsItsDigitAtFullSize()
        {
            var go = new GameObject("card", typeof(RectTransform));
            var view = go.AddComponent<CardView>();
            view.theme = ThemeLibrary.CreatePremiumSet();
            view.side = Side.Sun; view.kind = CardKind.Number; view.color = 0; view.number = 7;
            view.Configure();
            var numberText = go.GetComponentsInChildren<TMPro.TMP_Text>().First(t => t.text == "7");
            Assert.AreEqual(CardLayoutSpec.NumberSingleDigitPremium, numberText.fontSize, 0.5f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DrawOneCardShowsTwoCharacterSize()
        {
            var go = new GameObject("card", typeof(RectTransform));
            var view = go.AddComponent<CardView>();
            view.theme = ThemeLibrary.CreatePremiumSet();
            view.side = Side.Sun; view.kind = CardKind.DrawOne; view.color = 1;
            view.Configure();
            var numberText = go.GetComponentsInChildren<TMPro.TMP_Text>().First(t => t.text == "+1");
            Assert.AreEqual(CardLayoutSpec.NumberTwoChars, numberText.fontSize, 0.5f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void WildCardDoesNotThrowDespiteHavingNoCardColor()
        {
            var go = new GameObject("card", typeof(RectTransform));
            var view = go.AddComponent<CardView>();
            view.theme = ThemeLibrary.CreatePremiumSet();
            view.side = Side.Sun; view.kind = CardKind.Wild; view.color = CardFace.NoColor;
            Assert.DoesNotThrow(() => view.Configure());
            Object.DestroyImmediate(go);
        }

        [Test]
        public void BottomRightChipIsRotated180Degrees()
        {
            var go = new GameObject("card", typeof(RectTransform));
            var view = go.AddComponent<CardView>();
            view.theme = ThemeLibrary.CreatePremiumSet();
            view.side = Side.Sun; view.kind = CardKind.Number; view.color = 0; view.number = 3;
            view.Configure();
            var chips = go.GetComponentsInChildren<RectTransform>().Where(r => r.name.Contains("Chip")).ToList();
            var bottomRight = chips.First(c => c.anchoredPosition.x > 0 && c.anchoredPosition.y < 0);
            Assert.AreEqual(180f, bottomRight.localEulerAngles.z, 0.5f);
            Object.DestroyImmediate(go);
        }
    }
}
