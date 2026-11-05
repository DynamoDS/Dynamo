using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// One run of the AI-touched halo's glow (DYN-11004, Figma node 20:338): a slow breathing glow
    /// that draws the eye without blinking, then fades out and leaves the badge.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Placed and modified nodes: fade in over 0.6 s, two 2 s breaths (100% to 65% to 100%), settle
    /// to 70%, hold until 13.4 s, fade out by 14.9 s. Nodes the AI Assistant only moved get the same
    /// timing without the breaths: fade in, hold, fade out. Their halo is already faint (40%), so a
    /// breath would barely show.
    /// </para>
    /// <para>
    /// Only the halo's opacity animates; its blur and size never change. The glow belongs to the
    /// node's view model, not its view, so a view that is rebuilt mid-glow picks up where it was,
    /// and one rebuilt after the fade-out shows no halo.
    /// </para>
    /// <para>
    /// Nodes touched by the same action (one burst of work on the UI thread) start 100 ms apart,
    /// in the order they were touched, up to 1 s in total.
    /// </para>
    /// </remarks>
    internal sealed class AIHaloGlow
    {
        #region Timeline (seconds from the start of the glow)

        /// <summary>
        /// End of the fade-in (0 to 100%).
        /// </summary>
        internal static readonly TimeSpan FadeInEnd = TimeSpan.FromSeconds(0.6);

        /// <summary>
        /// Length of one breath (100% to 65% to 100%), about 0.5 Hz.
        /// </summary>
        internal static readonly TimeSpan BreathLength = TimeSpan.FromSeconds(2.0);

        /// <summary>
        /// Number of breaths after the fade-in.
        /// </summary>
        internal const int BreathCount = 2;

        /// <summary>
        /// End of the breaths: 0.6 s + 2 x 2.0 s.
        /// </summary>
        internal static readonly TimeSpan BreathsEnd = FadeInEnd + TimeSpan.FromTicks(BreathLength.Ticks * BreathCount);

        /// <summary>
        /// End of the settle from 100% to 70%.
        /// </summary>
        internal static readonly TimeSpan SettleEnd = TimeSpan.FromSeconds(5.4);

        /// <summary>
        /// End of the steady hold; the fade-out starts here.
        /// </summary>
        internal static readonly TimeSpan HoldEnd = TimeSpan.FromSeconds(13.4);

        /// <summary>
        /// End of the fade-out: the glow is gone, only the badge remains.
        /// </summary>
        internal static readonly TimeSpan FadeOutEnd = TimeSpan.FromSeconds(14.9);

        /// <summary>
        /// Lowest point of a breath, as a share of the glow's strength.
        /// </summary>
        internal const double BreathLow = 0.65;

        /// <summary>
        /// Level of the steady hold, as a share of the glow's strength.
        /// </summary>
        internal const double HoldLevel = 0.70;

        /// <summary>
        /// Gap between the starts of nodes touched by the same action.
        /// </summary>
        internal static readonly TimeSpan StaggerStep = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Longest stagger delay; from the 11th node on, every node starts at this delay.
        /// </summary>
        internal static readonly TimeSpan MaxStagger = TimeSpan.FromSeconds(1);

        // CSS cubic-bezier(x1, y1, x2, y2) is WPF KeySpline(x1, y1, x2, y2).
        // Fade-in decelerates; breaths, settle and fade-out ease in and out.
        private static readonly KeySpline EaseOut = Frozen(new KeySpline(0, 0, 0.2, 1));
        private static readonly KeySpline EaseInOut = Frozen(new KeySpline(0.4, 0, 0.6, 1));

        #endregion

        #region Clock

        private static readonly Stopwatch stopwatch = Stopwatch.StartNew();

        /// <summary>
        /// The time used to place a glow on its timeline. Replaced by tests to control time; set it
        /// back to <see cref="DefaultClock"/> afterwards.
        /// </summary>
        internal static Func<TimeSpan> Now = DefaultClock;

        /// <summary>
        /// Time since Dynamo started, from a monotonic clock.
        /// </summary>
        internal static TimeSpan DefaultClock() => stopwatch.Elapsed;

        #endregion

        private AIHaloGlow(int batch, TimeSpan startedAt, TimeSpan delay, double strength, bool breathes)
        {
            this.batch = batch;
            StartedAt = startedAt;
            Delay = delay;
            Strength = strength;
            Breathes = breathes;
        }

        /// <summary>
        /// When the node was touched, on <see cref="Now"/>.
        /// </summary>
        internal TimeSpan StartedAt { get; }

        /// <summary>
        /// How long the glow waits before fading in, so nodes touched by the same action start one
        /// after another. Zero for the first node of an action.
        /// </summary>
        internal TimeSpan Delay { get; }

        /// <summary>
        /// Peak opacity of the halo: 1 for placed or modified nodes, 0.4 for moved ones.
        /// </summary>
        internal double Strength { get; }

        /// <summary>
        /// True for the breathing timeline; false for fade in, hold, fade out.
        /// </summary>
        internal bool Breathes { get; }

        /// <summary>
        /// From the touch to the end of the fade-out, including <see cref="Delay"/>.
        /// </summary>
        internal TimeSpan Duration => Delay + FadeOutEnd;

        /// <summary>
        /// Time since the node was touched.
        /// </summary>
        internal TimeSpan Elapsed
        {
            get
            {
                var elapsed = Now() - StartedAt;
                return elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
            }
        }

        /// <summary>
        /// True once the glow has faded out. The node then shows its badge only.
        /// </summary>
        internal bool IsFinished => Elapsed >= Duration;

        private readonly int batch;

        /// <summary>
        /// True while the action that started this glow is still running. Further changes to the node
        /// in that action keep this glow and its place in the stagger.
        /// </summary>
        internal bool IsInCurrentBatch
        {
            get
            {
                lock (staggerLock)
                {
                    return batchStartedAt != null && batch == batchId && Now() - batchStartedAt.Value <= MaxStagger;
                }
            }
        }

        /// <summary>
        /// Starts a glow for a node touched now. Nodes touched by the same action are staggered.
        /// </summary>
        /// <param name="strength">Peak opacity of the halo, from 0 to 1.</param>
        /// <param name="breathes">True for the breathing timeline (placed or modified nodes).</param>
        internal static AIHaloGlow Start(double strength, bool breathes)
        {
            var now = Now();
            var delay = NextStaggerDelay(now, out var batch);
            return new AIHaloGlow(batch, now, delay, strength, breathes);
        }

        /// <summary>
        /// This glow at a new strength, for example a moved node the same action then modified. Keeps
        /// its start time and its place in the stagger.
        /// </summary>
        internal AIHaloGlow WithStrength(double strength, bool breathes)
        {
            return new AIHaloGlow(batch, StartedAt, Delay, strength, breathes);
        }

        /// <summary>
        /// Builds the opacity animation for this glow, with time 0 at <see cref="StartedAt"/>.
        /// Values already include <see cref="Strength"/>.
        /// </summary>
        /// <param name="startOpacity">
        /// Opacity to start from. Zero for a new halo; the halo's current opacity when a node touched
        /// again restarts its glow, so the restart never dips to zero.
        /// </param>
        internal DoubleAnimationUsingKeyFrames CreateAnimation(double startOpacity = 0)
        {
            var animation = new DoubleAnimationUsingKeyFrames
            {
                Duration = new System.Windows.Duration(Duration),
                FillBehavior = FillBehavior.HoldEnd
            };
            var frames = animation.KeyFrames;
            var d = Delay;

            // Hold the start value while waiting for this node's turn in the stagger
            frames.Add(new DiscreteDoubleKeyFrame(startOpacity, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            if (d > TimeSpan.Zero)
            {
                frames.Add(new DiscreteDoubleKeyFrame(startOpacity, KeyTime.FromTimeSpan(d)));
            }

            // Fade in
            frames.Add(Spline(1.0, d + FadeInEnd, EaseOut));

            if (Breathes)
            {
                // Breaths: each is 1 s down to 65% and 1 s back up to 100%
                var half = TimeSpan.FromTicks(BreathLength.Ticks / 2);
                var time = d + FadeInEnd;
                for (var i = 0; i < BreathCount; i++)
                {
                    time += half;
                    frames.Add(Spline(BreathLow, time, EaseInOut));
                    time += half;
                    frames.Add(Spline(1.0, time, EaseInOut));
                }

                // Settle to the hold level
                frames.Add(Spline(HoldLevel, d + SettleEnd, EaseInOut));

                // Steady hold
                frames.Add(new LinearDoubleKeyFrame(HoldLevel * Strength, KeyTime.FromTimeSpan(d + HoldEnd)));
            }
            else
            {
                // Steady hold at full strength (no breaths, no settle)
                frames.Add(new LinearDoubleKeyFrame(Strength, KeyTime.FromTimeSpan(d + HoldEnd)));
            }

            // Fade out
            frames.Add(Spline(0, d + FadeOutEnd, EaseInOut));

            animation.Freeze();
            return animation;
        }

        private SplineDoubleKeyFrame Spline(double level, TimeSpan time, KeySpline easing)
        {
            return new SplineDoubleKeyFrame(level * Strength, KeyTime.FromTimeSpan(time), easing);
        }

        private static KeySpline Frozen(KeySpline spline)
        {
            spline.Freeze();
            return spline;
        }

        #region Stagger

        private static readonly object staggerLock = new object();
        private static TimeSpan? batchStartedAt;
        private static int batchSize;
        private static int batchId;

        /// <summary>
        /// The delay for the next node of the current action. The first touch opens a batch; it closes
        /// when the UI thread has finished the action's work, or after <see cref="MaxStagger"/> at the
        /// latest.
        /// </summary>
        private static TimeSpan NextStaggerDelay(TimeSpan now, out int batch)
        {
            lock (staggerLock)
            {
                if (batchStartedAt == null || now - batchStartedAt.Value > MaxStagger)
                {
                    batchStartedAt = now;
                    batchSize = 0;
                    CloseBatchWhenIdle(++batchId);
                }

                var delay = TimeSpan.FromTicks(Math.Min(StaggerStep.Ticks * batchSize, MaxStagger.Ticks));
                batchSize++;
                batch = batchId;
                return delay;
            }
        }

        private static void CloseBatchWhenIdle(int id)
        {
            // Background runs after the action's own work (Normal priority and above) is done
            var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.FromThread(Thread.CurrentThread);
            dispatcher?.BeginInvoke(DispatcherPriority.Background, new Action(() => EndBatch(id)));
        }

        private static void EndBatch(int id)
        {
            lock (staggerLock)
            {
                if (id == batchId)
                {
                    batchStartedAt = null;
                }
            }
        }

        /// <summary>
        /// Ends the current batch, so the next touched node starts without delay. For tests.
        /// </summary>
        internal static void EndBatch()
        {
            lock (staggerLock)
            {
                batchStartedAt = null;
            }
        }

        #endregion
    }
}
