using Windows.ApplicationModel;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

namespace CSharpAnalytics.Sample.Windows8
{
    public sealed partial class OptionsFlyout
    {
        private const int ContentAnimationOffset = 100;

        public OptionsFlyout()
        {
            InitializeComponent();

            if (!DesignMode.DesignModeEnabled)
            {
                FlyoutContent.Transitions = new TransitionCollection {
                    new EntranceThemeTransition {
                        FromHorizontalOffset = (SettingsPane.Edge == SettingsEdgeLocation.Right) ? ContentAnimationOffset : (ContentAnimationOffset*-1)
                    }
                };
            }
        }

        private void OnBackButton(object sender, RoutedEventArgs e)
        {
            var parent = (Popup)Parent;
            if (parent != null)
                parent.IsOpen = false;

            if (ApplicationView.Value != ApplicationViewState.Snapped)
                SettingsPane.Show();
        }
    }

    public static class Flyout
    {
        public enum FlyoutWidth
        {
            Regular = 346,
            Wide = 646
        };

        private static Popup popup;

        public static void Open<T>(FlyoutWidth flyoutWidth)
            where T : UserControl, new()
        {
            var windowBounds = Window.Current.Bounds;

            popup = new Popup {
                IsLightDismissEnabled = true,
                Height = windowBounds.Height,
                Width = (double) flyoutWidth,
                ChildTransitions = new TransitionCollection {
                    new PaneThemeTransition {
                        Edge = (SettingsPane.Edge == SettingsEdgeLocation.Right ? EdgeTransitionLocation.Right : EdgeTransitionLocation.Left)
                    }
                },
            };

            Window.Current.Activated += OnWindowActivated;

            popup.Child = new T { Width = popup.Width, Height = popup.Height };
            popup.SetValue(Canvas.LeftProperty, SettingsPane.Edge == SettingsEdgeLocation.Right ? (windowBounds.Width - popup.Width) : 0);
            popup.SetValue(Canvas.TopProperty, 0);
            popup.Closed += PopupOnClosed;
            popup.IsOpen = true;
        }

        private static void PopupOnClosed(object sender, object o)
        {
            Window.Current.Activated -= OnWindowActivated;
            Close();
        }

        public static void Close()
        {
            var safePopup = popup;
            if (safePopup != null)
            {
                safePopup.IsOpen = false;
                popup.Closed -= PopupOnClosed;
                popup = null;
            }
        }

        private static void OnWindowActivated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
                Close();
        }
    }
}