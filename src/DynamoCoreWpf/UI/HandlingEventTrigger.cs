using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Dynamo.UI.Views
{
    [Obsolete("This class will be removed. Do not reference it.")]

    public class HandlingEventTrigger : System.Windows.Interactivity.EventTrigger
    {
        protected override void OnEvent(System.EventArgs eventArgs)
        {
            var routedEventArgs = eventArgs as RoutedEventArgs;
            if (routedEventArgs != null)
                routedEventArgs.Handled = true;

            base.OnEvent(eventArgs);
        }
    }

    //forked from: http://www.mobilemotion.eu/?p=1822


    public class EventToCommand
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Commands", typeof(object), typeof(EventToCommand));

        public static ICommand[] GetCommands(DependencyObject obj)
        {
            return (ICommand[])obj.GetValue(CommandProperty);
        }

        public static void SetCommands(DependencyObject obj, string value)
        {
            var commands = parseCommandString(obj as FrameworkElement, value);
            obj.SetValue(CommandProperty, commands.ToArray());
        }

        public static readonly DependencyProperty EventProperty =
            DependencyProperty.RegisterAttached("Events", typeof(object), typeof(EventToCommand),
                new PropertyMetadata(EventPropertyChangedCallback));

        public static string[] GetEvents(DependencyObject obj)
        {
            return (string[])obj.GetValue(EventProperty);
        }

        public static void SetEvents(DependencyObject obj, string value)
        {
            //parse string "eventName1,eventName2";

            var eventNames = parseEventNames(value);
            obj.SetValue(EventProperty, eventNames);
        }

        public static void EventPropertyChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            string[] @events = (args.NewValue as string).Split(',');
            var eventInfos = @events.Select(x => obj.GetType().GetEvent(x));
            foreach (var eventInfo in eventInfos)
            {
                if (eventInfo != null)
                {
                    var methodInfo = typeof(EventToCommand).GetMethod("OnEvent", BindingFlags.Static | BindingFlags.NonPublic);
                    //TODO clear previous subscriptions?
                    eventInfo.GetAddMethod().Invoke(obj,
                        new object[] { Delegate.CreateDelegate(eventInfo.EventHandlerType, methodInfo) });
                }
            }
        }

        //TODO handle the events.
        private static void OnEvent(object sender, RoutedEventArgs e)
        {
            if (e != null)
            {
                e.Handled = true;

            }
            UIElement element = (UIElement)sender;
            if (element != null)
            {
                ICommand[] commands = parseCommandString(element as FrameworkElement,element.GetValue(CommandProperty) as string);
                var events =parseEventNames(element.GetValue(EventProperty) as string);
                //find which command to execute.
                //this is slow - there should be a better way, but unclear yet where to cache this data.

                var index = Array.IndexOf(events, e.RoutedEvent.Name);
                if (index > -1 && commands.Length > index)
                {
                    var command = commands[index];
                    if (command != null && command.CanExecute(null))
                    {
                        //need to pass the PortViewModel in some cases...
                        //TODO need to expose commandParamter dep prop
                        command.Execute((element as FrameworkElement).DataContext);

                       
                    }
                }
                
            }
        }

        private static ICommand[] parseCommandString(FrameworkElement obj, string commandString)
        {
            var commandNames = commandString.Split(',');
            var commands = commandNames.Select(x =>
            {
                //TODO this requires that the datacontext of the bound element is a portViewModel.
                //add assert/test/
                var dataContext = (obj as FrameworkElement).DataContext;
                return dataContext.GetType().GetProperty(x).GetValue(dataContext) as ICommand;
            });

            return commands.ToArray();

        }
        private static string[] parseEventNames(string events)
        {
            return events.Split(',');
        }

        //TODO dispose dep properties?

    }

}
