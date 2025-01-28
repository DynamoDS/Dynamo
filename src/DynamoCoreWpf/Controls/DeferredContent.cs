using System;
using System.Windows;
using System.Windows.Controls;

namespace Dynamo.Controls
{
    /// <summary>
    /// https://stackoverflow.com/a/26543731
    /// </summary>
    public class DeferredContent : ContentPresenter
    {
        public DataTemplate DeferredContentTemplate
        {
            get { return (DataTemplate)GetValue(DeferredContentTemplateProperty); }
            set { SetValue(DeferredContentTemplateProperty, value); }
        }

        public static readonly DependencyProperty DeferredContentTemplateProperty =
            DependencyProperty.Register("DeferredContentTemplate",
            typeof(DataTemplate), typeof(DeferredContent), null);

        public DeferredContent()
        {
            IsVisibleChanged += DeferredContent_IsVisibleChanged;
        }

        private void DeferredContent_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (true.Equals(e.NewValue))
            {
                IsVisibleChanged -= DeferredContent_IsVisibleChanged;
                Dispatcher.BeginInvoke(ShowDeferredContent);
            }
        }

        public void ShowDeferredContent()
        {
            if (DeferredContentTemplate != null)
            {
                base.Content = DeferredContentTemplate.LoadContent();
                RaiseDeferredContentLoaded();
            }
        }

        private void RaiseDeferredContentLoaded()
        {
            DeferredContentLoaded?.Invoke(this, new RoutedEventArgs());
        }

        public event EventHandler<RoutedEventArgs> DeferredContentLoaded;
    }
}
