using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media.Animation;

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
    /// The glow clock (DYN-11005, Figma node 7:989) makes sure changes made off-screen are not
    /// missed. A new glow waits until the node is on screen (<see cref="Begin"/>). Nodes that come
    /// on screen together start 100 ms apart, in the order they were touched, up to 1 s in total.
    /// Hovering the node holds the glow at its current value (<see cref="Hold"/>,
    /// <see cref="Release"/>); only time while not held counts. A node touched again while glowing
    /// restarts when the next on-screen check finds it on screen, from its current strength
    /// (<see cref="Retouch"/>, <see cref="Restart"/>). If it has left the screen, the glow waits
    /// again until the node is back (<see cref="WaitAgain"/>), so a later change is not missed
    /// either. Esc on a hovered node ends the glow and keeps the badge (<see cref="Dismiss"/>).
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
        /// Gap between the starts of nodes that come on screen together.
        /// </summary>
        internal static readonly TimeSpan StaggerStep = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Longest stagger delay; from the 11th node that comes on screen together on, every node
        /// starts at this delay.
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
        internal static Func<TimeSpan> Now { get; set; } = DefaultClock;

        /// <summary>
        /// Time since Dynamo started, from a monotonic clock.
        /// </summary>
        internal static TimeSpan DefaultClock() => stopwatch.Elapsed;

        #endregion

        #region State

        private static long lastTouchOrder;

        // The node's view model may be updated off the UI thread (DynamoMCP edits), while hover,
        // Esc and the on-screen check run on it. Every field below is read and written under this lock.
        private readonly object sync = new object();

        private double strength;
        private bool breathes;
        private TimeSpan delay;
        private bool isStarted;
        private bool isHeld;
        private bool isDismissed;
        private bool isRestartPending;
        private int version;

        private int startOpacityVersion = -1;
        private double startOpacity;

        // Time on the timeline that has already counted, and since when the clock has been running
        private TimeSpan countedBefore;
        private TimeSpan? runningSince;

        private AIHaloGlow(double strength, bool breathes)
        {
            this.strength = strength;
            this.breathes = breathes;
            TouchOrder = Interlocked.Increment(ref lastTouchOrder);
        }

        /// <summary>
        /// Creates a glow for a node the AI Assistant has just touched. It waits until the node is on
        /// screen; see <see cref="Begin"/>.
        /// </summary>
        /// <param name="strength">Peak opacity of the halo, from 0 to 1.</param>
        /// <param name="breathes">True for the breathing timeline (placed or modified nodes).</param>
        internal static AIHaloGlow Create(double strength, bool breathes)
        {
            return new AIHaloGlow(strength, breathes);
        }

        /// <summary>
        /// The order in which glows were created. Nodes that come on screen together are staggered
        /// in this order, which is the order the AI Assistant touched them.
        /// </summary>
        internal long TouchOrder { get; }

        /// <summary>
        /// Peak opacity of the halo: 1 for placed or modified nodes, 0.4 for moved ones.
        /// </summary>
        internal double Strength
        {
            get { lock (sync) return strength; }
        }

        /// <summary>
        /// True for the breathing timeline; false for fade in, hold, fade out.
        /// </summary>
        internal bool Breathes
        {
            get { lock (sync) return breathes; }
        }

        /// <summary>
        /// How long the glow waits before fading in, so nodes that come on screen together start one
        /// after another. Zero until the glow begins, for the first node of a group, and after a restart.
        /// </summary>
        internal TimeSpan Delay
        {
            get { lock (sync) return delay; }
        }

        /// <summary>
        /// Changes each time the glow's timeline starts again or stops to wait (<see cref="Begin"/>,
        /// <see cref="Restart"/>, <see cref="WaitAgain"/>), so a view knows to build a new animation.
        /// Holding, releasing and <see cref="Retouch"/> do not change it.
        /// </summary>
        internal int Version
        {
            get { lock (sync) return version; }
        }

        /// <summary>
        /// True while the glow waits for the node to come on screen.
        /// </summary>
        internal bool IsWaiting
        {
            get { lock (sync) return !isStarted && !isDismissed; }
        }

        /// <summary>
        /// True when the node was touched again while glowing, until the next on-screen check
        /// restarts the glow or sends it back to waiting. The current glow keeps playing meanwhile.
        /// </summary>
        internal bool IsRestartPending
        {
            get { lock (sync) return isRestartPending; }
        }

        /// <summary>
        /// True once the node has been on screen and the clock has started.
        /// </summary>
        internal bool IsStarted
        {
            get { lock (sync) return isStarted; }
        }

        /// <summary>
        /// True while the glow is held at its current value, because the node is hovered.
        /// </summary>
        internal bool IsHeld
        {
            get { lock (sync) return isHeld; }
        }

        /// <summary>
        /// True once Esc has ended the glow. The badge stays.
        /// </summary>
        internal bool IsDismissed
        {
            get { lock (sync) return isDismissed; }
        }

        /// <summary>
        /// From the start of the clock to the end of the fade-out, including <see cref="Delay"/>.
        /// </summary>
        internal TimeSpan Duration
        {
            get { lock (sync) return delay + FadeOutEnd; }
        }

        /// <summary>
        /// Time on the glow's timeline: time since the clock started (or restarted), not counting
        /// time spent held. Zero while waiting.
        /// </summary>
        internal TimeSpan Elapsed
        {
            get { lock (sync) return ElapsedUnlocked(); }
        }

        /// <summary>
        /// True once the glow has faded out or been dismissed. The node then shows its badge only.
        /// A glow with a restart pending is not finished, even if its current timeline has ended:
        /// the newer change still has to be seen.
        /// </summary>
        internal bool IsFinished
        {
            get
            {
                lock (sync)
                {
                    return isDismissed
                        || (isStarted && !isRestartPending && ElapsedUnlocked() >= delay + FadeOutEnd);
                }
            }
        }

        /// <summary>
        /// True while the halo is on screen: the clock has started and the glow has not finished.
        /// </summary>
        internal bool IsGlowing
        {
            get
            {
                lock (sync)
                {
                    return isStarted && !isDismissed && ElapsedUnlocked() < delay + FadeOutEnd;
                }
            }
        }

        private TimeSpan ElapsedUnlocked()
        {
            var elapsed = countedBefore;
            if (runningSince.HasValue)
            {
                var running = Now() - runningSince.Value;
                if (running > TimeSpan.Zero) elapsed += running;
            }
            return elapsed < TimeSpan.Zero ? TimeSpan.Zero : elapsed;
        }

        /// <summary>
        /// The opacity this timeline version starts from: set by the first view to play it, reused by any rebuilt view.
        /// </summary>
        internal double StartOpacityFor(int forVersion, double currentOpacity)
        {
            lock (sync)
            {
                if (startOpacityVersion != forVersion)
                {
                    startOpacityVersion = forVersion;
                    startOpacity = currentOpacity;
                }
                return startOpacity;
            }
        }

        #endregion

        #region Clock control

        /// <summary>
        /// Starts the clock when the node first comes on screen. Does nothing if it has already
        /// started or was dismissed. A held glow starts held, and runs once released.
        /// </summary>
        /// <param name="staggerDelay">This node's place in the group that came on screen together; see <see cref="StaggerDelay"/>.</param>
        /// <returns>True if the clock started.</returns>
        internal bool Begin(TimeSpan staggerDelay)
        {
            lock (sync)
            {
                if (isStarted || isDismissed) return false;

                isStarted = true;
                delay = staggerDelay < TimeSpan.Zero ? TimeSpan.Zero : staggerDelay;
                countedBefore = TimeSpan.Zero;
                runningSince = isHeld ? (TimeSpan?)null : Now();
                version++;
                return true;
            }
        }

        /// <summary>
        /// Holds the glow at its current value while the node is hovered. A waiting glow is marked
        /// as held, so it starts held if the node comes on screen under the mouse.
        /// </summary>
        /// <returns>True if this changed the glow.</returns>
        internal bool Hold()
        {
            lock (sync)
            {
                if (isHeld) return false;

                isHeld = true;
                if (runningSince.HasValue)
                {
                    countedBefore = ElapsedUnlocked();
                    runningSince = null;
                }
                return true;
            }
        }

        /// <summary>
        /// Lets a held glow run again from where it was held.
        /// </summary>
        /// <returns>True if this changed the glow.</returns>
        internal bool Release()
        {
            lock (sync)
            {
                if (!isHeld) return false;

                isHeld = false;
                if (isStarted && !isDismissed && !runningSince.HasValue)
                {
                    runningSince = Now();
                }
                return true;
            }
        }

        /// <summary>
        /// The AI Assistant touched the node again while this glow is waiting or running. The glow
        /// takes the new strength, for example a moved node that is then modified. A waiting glow
        /// keeps waiting. A running glow keeps playing and is marked <see cref="IsRestartPending"/>;
        /// the next on-screen check then calls <see cref="Restart"/> if the node is on screen, or
        /// <see cref="WaitAgain"/> if it has left.
        /// </summary>
        /// <param name="newStrength">Peak opacity of the halo, from 0 to 1.</param>
        /// <param name="newBreathes">True for the breathing timeline (placed or modified nodes).</param>
        internal void Retouch(double newStrength, bool newBreathes)
        {
            lock (sync)
            {
                if (isDismissed) return;

                strength = newStrength;
                breathes = newBreathes;
                if (isStarted)
                {
                    isRestartPending = true;
                }
            }
        }

        /// <summary>
        /// Starts a glow with a restart pending again from the beginning, without a stagger delay,
        /// because its node is on screen. The view starts it from the halo's current strength, so it
        /// never dips to zero. A held glow restarts held.
        /// </summary>
        /// <returns>True if the glow restarted.</returns>
        internal bool Restart()
        {
            lock (sync)
            {
                if (!isRestartPending || isDismissed) return false;

                isRestartPending = false;
                delay = TimeSpan.Zero;
                countedBefore = TimeSpan.Zero;
                runningSince = isHeld ? (TimeSpan?)null : Now();
                version++;
                return true;
            }
        }

        /// <summary>
        /// Sends a glow with a restart pending back to waiting, because its node has left the
        /// screen. The halo goes away, and the glow starts again, with the usual stagger, the next
        /// time the node comes on screen (<see cref="Begin"/>).
        /// </summary>
        /// <returns>True if the glow is waiting again.</returns>
        internal bool WaitAgain()
        {
            lock (sync)
            {
                if (!isRestartPending || isDismissed) return false;

                isRestartPending = false;
                isStarted = false;
                delay = TimeSpan.Zero;
                countedBefore = TimeSpan.Zero;
                runningSince = null;
                version++;
                return true;
            }
        }

        /// <summary>
        /// Ends the glow (Esc on the hovered node). The badge stays; a view rebuilt later shows no halo.
        /// </summary>
        /// <returns>True if the glow was on screen and is now dismissed.</returns>
        internal bool Dismiss()
        {
            lock (sync)
            {
                if (!isStarted || isDismissed || ElapsedUnlocked() >= delay + FadeOutEnd) return false;

                countedBefore = ElapsedUnlocked();
                runningSince = null;
                isDismissed = true;
                isRestartPending = false;
                return true;
            }
        }

        #endregion

        /// <summary>
        /// Builds the opacity animation for this glow, with time 0 at the start of its clock.
        /// Values already include <see cref="Strength"/>.
        /// </summary>
        /// <param name="startOpacity">
        /// Opacity to start from. Zero for a new halo; the halo's current opacity when a running glow
        /// restarts (<see cref="Restart"/>), so the halo never dips to zero.
        /// </param>
        internal DoubleAnimationUsingKeyFrames CreateAnimation(double startOpacity = 0)
        {
            TimeSpan d;
            double s;
            bool b;
            lock (sync)
            {
                d = delay;
                s = strength;
                b = breathes;
            }

            var animation = new DoubleAnimationUsingKeyFrames
            {
                Duration = new System.Windows.Duration(d + FadeOutEnd),
                FillBehavior = FillBehavior.HoldEnd
            };
            var frames = animation.KeyFrames;

            // Hold the start value while waiting for this node's turn in the stagger
            frames.Add(new DiscreteDoubleKeyFrame(startOpacity, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            if (d > TimeSpan.Zero)
            {
                frames.Add(new DiscreteDoubleKeyFrame(startOpacity, KeyTime.FromTimeSpan(d)));
            }

            // Fade in
            frames.Add(Spline(s, 1.0, d + FadeInEnd, EaseOut));

            if (b)
            {
                // Breaths: each is 1 s down to 65% and 1 s back up to 100%
                var half = TimeSpan.FromTicks(BreathLength.Ticks / 2);
                var time = d + FadeInEnd;
                for (var i = 0; i < BreathCount; i++)
                {
                    time += half;
                    frames.Add(Spline(s, BreathLow, time, EaseInOut));
                    time += half;
                    frames.Add(Spline(s, 1.0, time, EaseInOut));
                }

                // Settle to the hold level
                frames.Add(Spline(s, HoldLevel, d + SettleEnd, EaseInOut));

                // Steady hold
                frames.Add(new LinearDoubleKeyFrame(HoldLevel * s, KeyTime.FromTimeSpan(d + HoldEnd)));
            }
            else
            {
                // Steady hold at full strength (no breaths, no settle)
                frames.Add(new LinearDoubleKeyFrame(s, KeyTime.FromTimeSpan(d + HoldEnd)));
            }

            // Fade out
            frames.Add(Spline(s, 0, d + FadeOutEnd, EaseInOut));

            animation.Freeze();
            return animation;
        }

        private static SplineDoubleKeyFrame Spline(double strength, double level, TimeSpan time, KeySpline easing)
        {
            return new SplineDoubleKeyFrame(level * strength, KeyTime.FromTimeSpan(time), easing);
        }

        private static KeySpline Frozen(KeySpline spline)
        {
            spline.Freeze();
            return spline;
        }

        /// <summary>
        /// The delay for a node's place in a group that came on screen together: 100 ms per place,
        /// up to 1 s.
        /// </summary>
        /// <param name="place">0 for the first node of the group.</param>
        internal static TimeSpan StaggerDelay(int place)
        {
            if (place <= 0) return TimeSpan.Zero;
            return TimeSpan.FromTicks(Math.Min(StaggerStep.Ticks * place, MaxStagger.Ticks));
        }
    }
}
