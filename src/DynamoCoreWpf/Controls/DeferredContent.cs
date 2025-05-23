using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using Dynamo.Graph.Nodes;
using Dynamo.ViewModels;
using static Dynamo.ViewModels.SearchViewModel;

namespace Dynamo.Controls
{
    /// <summary>
    /// https://stackoverflow.com/a/26543731
    /// </summary>
    public sealed class DeferredContent : ContentPresenter
    {
        private static readonly DeferredContentRenderPriority Priority = new();
        private bool isUnloaded = false;

        public static void Focus(double x, double y) => Priority.Focus(x, y);
        public static void ClearDeferredQueue() => Priority.ClearDeferredQueue();
        public static event Action<int> TotalDeferredNodeLoadedCountChanged;

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
            Unloaded += OnUnloaded;
            DataContextChanged += DeferredContent_DataContextChanged;
        }

        private void DeferredContent_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (true.Equals(e.NewValue))
            {
                IsVisibleChanged -= DeferredContent_IsVisibleChanged;
                //if (DataContext == null)
                //{
                //    DataContextChanged += DeferredContent_DataContextChanged;
                //    return;
                //}
                ////Dispatcher.BeginInvoke(ShowDeferredContent, DispatcherPriority.Background);
                //Priority.Enqueue(this);
            }
        }

        private void DeferredContent_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext == null) return;

            DataContextChanged -= DeferredContent_DataContextChanged;

            if (Priority.TotalDeferredNodeLoadedCount > 0)
            {
                //deferred node load has already started any node placed after that should
                //be able to skip the queue and load immediately
                Dispatcher.InvokeAsync(ShowDeferredContent, DispatcherPriority.Render);
                return;
            }
            if (DataContext is NodeViewModel nvm)
            {
                if (base.Content is Border bdr)
                {
                    bdr.Height = nvm.NodeModel.Height;
                    bdr.Width = nvm.NodeModel.Width;
                }
                //if (nvm.NodeModel.Height != 100 && nvm.NodeModel.Width != 100)
                //{
                //    Height = nvm.NodeModel.Height;
                //    Width = nvm.NodeModel.Width;
                //}
                //var size = new[] { nvm.NodeModel.Width, nvm.NodeModel.Height };
                //if (nvm.SetModelSizeCommand.CanExecute(size)) //can execute is always false at this point
                //{
                //    nvm.SetModelSizeCommand.Execute(size);
                //}
            }
            //put size as recieved from view model/model , ht and wd are set in node model, but first need to serialize that then load it
            //Height = 180;
            //Width = 180;
            Priority.Enqueue(this);
        }

        public void ShowDeferredContent()
        {
            if (isUnloaded) return;
            if (DeferredContentTemplate != null)
            {
                base.Content = DeferredContentTemplate.LoadContent();
                //if (base.Content is NodeView nv)
                //{
                //    nv.Height = Height;
                //    nv.Width = Width;
                //}
                RaiseDeferredContentLoaded(base.Content);
            }
        }

        public static void ShowDeferredContentBatch(List<DeferredContent> controls)
        {
            foreach (var control in controls)
            {
                if (control != null && !control.isUnloaded && control.DeferredContentTemplate != null)
                {
                    control.Content = control.DeferredContentTemplate.LoadContent();
                    control.RaiseDeferredContentLoaded(control.Content);
                }
            }
        }

        private void RaiseDeferredContentLoaded(object loaded)
        {
            if (loaded is NodeView)
            {
                Priority.RegisterLoadedCountChanged();
            }
            DeferredContentLoaded?.Invoke(this, new RoutedEventArgs());
        }

        protected void OnUnloaded(object sender, RoutedEventArgs e)
        {
            //removing an item from a queue involves dequing and re-queing the complete list,
            //so we will just use this flag to skip elements which were deleted before they were loaded.
            isUnloaded = true;
            Unloaded -= OnUnloaded;
        }

        public event EventHandler<RoutedEventArgs> DeferredContentLoaded;

        private class DeferredContentRenderPriority
        {
            private bool isRunning = false;
            private Point canvasCenter;
            private readonly List<(DeferredContent, DispatcherOperation)> scheduled = [];
            private readonly List<(List<DeferredContent>, DispatcherOperation)> scheduledBatch = [];
            private readonly PriorityQueue<DeferredContent, double> pending =
                new PriorityQueue<DeferredContent, double>();
            internal int TotalDeferredNodeLoadedCount
            {
                get;
                private set;
            } = 0;
            const int BatchSize = 5;

            internal void RegisterLoadedCountChanged(bool isReset = false)
            {
                TotalDeferredNodeLoadedCount = isReset ? 0 : TotalDeferredNodeLoadedCount + 1;
                var handler = DeferredContent.TotalDeferredNodeLoadedCountChanged;
                handler?.Invoke(TotalDeferredNodeLoadedCount);
            }

            internal void ClearDeferredQueue()
            {
                isRunning = false;
                RegisterLoadedCountChanged(true);
                scheduled.Clear();
                scheduledBatch.Clear();
                pending.Clear();
            }

            private double GetDistance(NodeViewModel vm)
            {
                var dx = canvasCenter.X - vm.X;
                var dy = canvasCenter.Y - vm.Y;
                return dx * dx + dy * dy;
            }

            internal void Enqueue(DeferredContent control)
            {
                if (control == null || control.DataContext is not NodeViewModel nvm) return;

                var distSquared = GetDistance(nvm);
                pending.Enqueue(control, distSquared);

                if (!isRunning)
                {
                    isRunning = true;
                    control.Dispatcher.InvokeAsync(SchedulePendingControlBatch, DispatcherPriority.ContextIdle);
                    //control.Dispatcher.InvokeAsync(SchedulePendingControl, DispatcherPriority.ContextIdle);
                }
            }

            internal void Focus(double x, double y)
            {
                isRunning = false;
                canvasCenter = new Point(x, y);

                // Use object pooling to reduce allocations
                //var toReschedule = System.Buffers.ArrayPool<DeferredContent>.Shared.Rent(pending.Count + scheduled.Count);

                var arrSize = pending.Count;
                if(scheduledBatch.Count > 0)
                {
                    arrSize += (scheduledBatch.Count - 1) * BatchSize + scheduledBatch.LastOrDefault().Item1.Count;
                }
                var toReschedule = System.Buffers.ArrayPool<DeferredContent>.Shared.Rent(arrSize);
                int rescheduleCount = 0;

                //add pending items to the reschedule list and clear
                if (pending.Count > 0)
                {
                    foreach (var item in pending.UnorderedItems)
                    {
                        toReschedule[rescheduleCount++] = item.Element;
                    }
                    pending.Clear();
                }

                //add scheduled items to the reschedule list and clear
                //foreach (var (control, task) in scheduled)
                //{
                //    if (task.Status != DispatcherOperationStatus.Completed && task.Abort())
                //    {
                //        toReschedule[rescheduleCount++] = control;
                //    }
                //}
                //scheduled.Clear();

                foreach (var (batch, task) in scheduledBatch)
                {
                    if (task.Status != DispatcherOperationStatus.Completed && task.Abort())
                    {
                        foreach (var control in batch)
                        {
                            toReschedule[rescheduleCount++] = control;
                        }
                    }
                }
                scheduledBatch.Clear();

                //recalculate the priority queue based on the current focus
                for (int i = 0; i < rescheduleCount; i++)
                {
                    Enqueue(toReschedule[i]);
                }

                System.Buffers.ArrayPool<DeferredContent>.Shared.Return(toReschedule, clearArray: true);
            }

            internal void SchedulePendingControl()
            {
                while (isRunning && pending.TryDequeue(out var control, out _))
                {
                    scheduled.Add((control, control.Dispatcher.InvokeAsync(control.ShowDeferredContent, DispatcherPriority.Background)));
                }

                isRunning = false;
            }

            internal void SchedulePendingControlBatch()
            {
                while (isRunning && pending.Count > 0)
                {
                    var batch = new List<DeferredContent>(BatchSize);

                    for (int i = 0; i < BatchSize && pending.TryDequeue(out var control, out _); i++)
                    {
                        batch.Add(control);
                    }

                    if (batch.Count > 0)
                    {
                        // Schedule the batch for processing
                        var op = batch[0].Dispatcher.InvokeAsync(
                            () => ShowDeferredContentBatch(batch),
                            DispatcherPriority.Background);

                        // You can track the first control for the operation, or all if needed
                        scheduledBatch.Add((batch, op));
                    }
                }

                isRunning = false;
            }
        }
    }
}
