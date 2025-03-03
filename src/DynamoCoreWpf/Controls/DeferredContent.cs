using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Dynamo.ViewModels;

namespace Dynamo.Controls
{
    /// <summary>
    /// https://stackoverflow.com/a/26543731
    /// </summary>
    public class DeferredContent : ContentPresenter
    {
        public static void Focus(double x, double y) => Priority.Focus(x, y);
        private static DeferredContentRenderPriority Priority = new();

        public DataTemplate DeferredContentTemplate
        {
            get => (DataTemplate)GetValue(DeferredContentTemplateProperty);
            set => SetValue(DeferredContentTemplateProperty, value);
        }

        public static readonly DependencyProperty DeferredContentTemplateProperty =
            DependencyProperty.Register(nameof(DeferredContentTemplate),
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
                if (DataContext == null)
                {
                    DataContextChanged += DeferredContent_DataContextChanged;
                    return;
                }
                //Dispatcher.BeginInvoke(ShowDeferredContent, DispatcherPriority.Background);
                Priority.Enqueue(this);
            }
        }

        private void DeferredContent_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DataContextChanged -= DeferredContent_DataContextChanged;
            // new node added,jump the queue
            Dispatcher.BeginInvoke(ShowDeferredContent, DispatcherPriority.Render);
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

        private class DeferredContentRenderPriority
        {
            private bool isRunning = false;
            private Point canvasCenter = default;
            private readonly List<(DeferredContent, DispatcherOperation)> scheduled = [];
            private readonly PriorityQueue<DeferredContent, double> pending =
                new PriorityQueue<DeferredContent, double>();

            public void Enqueue(DeferredContent control)
            {
                if (control.DataContext is not NodeViewModel nvm) return;

                var dist = (canvasCenter - new Point(nvm.X, nvm.Y)).Length;
                pending.Enqueue(control, dist);

                if (!isRunning)
                {
                    isRunning = true;
                    control.Dispatcher.BeginInvoke(SchedulePending, DispatcherPriority.ContextIdle);
                }
            }

            public void Focus(double x, double y)
            {
                isRunning = false;
                canvasCenter = new Point(x, y);

                var toReschedule = new List<DeferredContent>();
                if (pending.Count > 0)
                {
                    toReschedule.AddRange(pending.UnorderedItems.Select(i => i.Element));
                    pending.Clear();
                }

                foreach (var (control, task) in scheduled)
                {
                    if (task.Status != DispatcherOperationStatus.Completed && task.Abort())
                    {
                        toReschedule.Add(control);
                    }
                }
                scheduled.Clear();

                foreach (var control in toReschedule)
                {
                    Enqueue(control);
                }
            }

            public void SchedulePending()
            {
                while (isRunning && pending.TryDequeue(out var control, out _))
                {
                    scheduled.Add((control, control.Dispatcher.BeginInvoke(control.ShowDeferredContent, DispatcherPriority.Background)));
                }

                isRunning = false;
            }
        }
    }
}
