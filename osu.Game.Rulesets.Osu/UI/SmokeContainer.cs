// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.UI
{
    /// <summary>
    /// Manages smoke trails generated from user input.
    /// </summary>
    public class SmokeContainer : Container, IRequireHighFrequencyMousePosition, IKeyBindingHandler<OsuAction>
    {
        private const double min_smoke_rate = 1000 / 60.0;

        private SmokeSkinnableDrawable? currentSegmentSkinnable;
        private SmokeSegment? currentSegment => currentSegmentSkinnable?.Drawable as SmokeSegment;

        private Vector2 lastMousePosition;
        private double lastTimeAdded;

        public override bool ReceivePositionalInputAt(Vector2 _) => true;

        public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
        {
            if (e.Action != OsuAction.Smoke)
                return false;

            AddInternal(currentSegmentSkinnable = new SmokeSkinnableDrawable(new OsuSkinComponentLookup(OsuSkinComponents.CursorSmoke), _ => new DefaultSmokeSegment()));
            lastTimeAdded = double.MinValue;

            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
        {
            if (e.Action != OsuAction.Smoke)
                return;

            currentSegment?.FinishDrawing(Time.Current);
            currentSegmentSkinnable = null;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            lastMousePosition = e.MousePosition;

            if (currentSegment == null)
                return base.OnMouseMove(e);

            bool shouldForcePoint = Time.Current - lastTimeAdded >= min_smoke_rate;

            if (currentSegment.AddPosition(lastMousePosition, Time.Current, shouldForcePoint))
                lastTimeAdded = Time.Current;

            return base.OnMouseMove(e);
        }

        private class SmokeSkinnableDrawable : SkinnableDrawable
        {
            public override bool RemoveWhenNotAlive => true;

            public override double LifetimeStart => Drawable.LifetimeStart;
            public override double LifetimeEnd => Drawable.LifetimeEnd;

            public SmokeSkinnableDrawable(ISkinComponentLookup lookup, Func<ISkinComponentLookup, Drawable>? defaultImplementation = null, ConfineMode confineMode = ConfineMode.NoScaling)
                : base(lookup, defaultImplementation, confineMode)
            {
            }
        }
    }
}
