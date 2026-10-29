using Eclipse.EditorTools;
using Eclipse.Themes;
using NUnit.Framework;
using UnityEngine;

namespace Eclipse.Rendering.Tests
{
    public class TableMockupSceneBuilderTests
    {
        [Test]
        public void HandFanUsesSpecPivotRotationsAndOffsets()
        {
            var parent = new GameObject("hand", typeof(RectTransform)).GetComponent<RectTransform>();
            var cards = TableMockupSceneBuilder.BuildHandFan(parent, ThemeLibrary.CreatePremiumSet());
            Assert.AreEqual(5, cards.Count);
            for (int i = 0; i < 5; i++)
            {
                var rect = cards[i].GetComponent<RectTransform>();
                Assert.AreEqual(CardLayoutSpec.FanPivot, rect.pivot);
                // Transform.localEulerAngles always normalizes to [0, 360) (verified empirically: setting
                // z = -14 and reading it straight back gives 346, not -14), so compare via the shortest
                // angular distance rather than a literal AreEqual against the signed spec values.
                Assert.AreEqual(0f, Mathf.DeltaAngle(CardLayoutSpec.FanRotations[i], rect.localEulerAngles.z), 0.1f);
                float expectedY = CardLayoutSpec.FanOffsetsY[i] - (cards[i].playable ? CardLayoutSpec.PlayableRaise : 0f);
                Assert.AreEqual(expectedY, rect.anchoredPosition.y, 0.5f);
            }
            Object.DestroyImmediate(parent.gameObject);
        }
    }
}
