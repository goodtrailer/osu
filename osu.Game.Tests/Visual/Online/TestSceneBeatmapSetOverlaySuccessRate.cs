// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Screens.Select.Details;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneBeatmapSetOverlaySuccessRate : OsuTestScene
    {
        private GraphExposingSuccessRate successRate;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(275, 220),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Gray,
                    },
                    successRate = new GraphExposingSuccessRate
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(275, 220),
                        Padding = new MarginPadding(20)
                    }
                }
            };
        });

        [Test]
        public void TestMetrics()
        {
            var firstBeatmap = createBeatmap();
            var secondBeatmap = createBeatmap();

            AddStep("set first set", () => successRate.BeatmapInfo = firstBeatmap);
            AddAssert("ratings set", () => successRate.Graph.Metrics == firstBeatmap.Metrics);

            AddStep("set second set", () => successRate.BeatmapInfo = secondBeatmap);
            AddAssert("ratings set", () => successRate.Graph.Metrics == secondBeatmap.Metrics);

            static BeatmapInfo createBeatmap() => new BeatmapInfo
            {
                Metrics = new BeatmapMetrics
                {
                    Fails = Enumerable.Range(1, 100).Select(_ => RNG.Next(10)).ToArray(),
                    Retries = Enumerable.Range(-2, 100).Select(_ => RNG.Next(10)).ToArray(),
                }
            };
        }

        [Test]
        public void TestOnlyFailMetrics()
        {
            AddStep("set beatmap", () => successRate.BeatmapInfo = new BeatmapInfo
            {
                Metrics = new BeatmapMetrics
                {
                    Fails = Enumerable.Range(1, 100).ToArray(),
                }
            });
            AddAssert("graph max values correct",
                () => successRate.ChildrenOfType<BarGraph>().All(graph => graph.MaxValue == 100));
        }

        [Test]
        public void TestEmptyMetrics()
        {
            AddStep("set beatmap", () => successRate.BeatmapInfo = new BeatmapInfo
            {
                Metrics = new BeatmapMetrics()
            });

            AddAssert("graph max values correct",
                () => successRate.ChildrenOfType<BarGraph>().All(graph => graph.MaxValue == 0));
        }

        private class GraphExposingSuccessRate : SuccessRate
        {
            public new FailRetryGraph Graph => base.Graph;
        }
    }
}
