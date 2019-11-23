using System;
using System.Collections.Generic;

namespace Dynamo.UI.Views
{
    [Obsolete("This class will be removed.")]
    public class HandlingEventTrigger : System.Windows.Interactivity.EventTrigger
    {
        protected override void OnEvent(System.EventArgs eventArgs)
        {
            var routedEventArgs = eventArgs as System.Windows.RoutedEventArgs;
            if (routedEventArgs != null)
                routedEventArgs.Handled = true;

            base.OnEvent(eventArgs);
        }
    }

    /// <summary>
    /// Do not use reference this class if you are not writing dynamocorewpf code.
    /// </summary>
    public class EmbeddedHandlingEventTrigger : Dynamo.UI.Views.Microsoft.Xaml.Behaviors.EventTrigger
    {
        protected override void OnEvent(System.EventArgs eventArgs)
        {
            var routedEventArgs = eventArgs as System.Windows.RoutedEventArgs;
            if (routedEventArgs != null)
                routedEventArgs.Handled = true;

            base.OnEvent(eventArgs);
        }
    }


    //the following code is a subset of extracted code from microsoft.xaml.behaviors.wpf to isolate 
    //these types to keep them from causing xaml load exceptions in complex
    //plugin loading scenarios.
    //https://github.com/microsoft/XamlBehaviorsWpf
    //DO NOT MODIFY THE FOLLOWING CODE:

    //The only modifications made to this code were to ExceptionStringTable - this was made a static dictionary to avoid
    //generating resource assemblies. Only the exceptions referenced by the copied code were included.

    #region embedded microsoft.xaml.behaviors

    // Copyright (c) Microsoft. All rights reserved. 
    // Licensed under the MIT license. See LICENSE file in the project root for full license information. 
    namespace Microsoft.Xaml.Behaviors
    {
        using System;
        using System.Collections;
        using System.Collections.Generic;
        using System.Collections.ObjectModel;
        using System.Collections.Specialized;
        using System.ComponentModel;
        using System.Diagnostics;
        using System.Diagnostics.CodeAnalysis;
        using System.Globalization;
        using System.Reflection;
        using System.Windows;
        using System.Windows.Controls.Primitives;
        using System.Windows.Data;
        using System.Windows.Input;
        using System.Windows.Markup;
        using System.Windows.Media.Animation;


        /// <summary>
        /// Represents an object that can invoke actions conditionally.
        /// </summary>
        /// <typeparam name="T">The type to which this trigger can be attached.</typeparam>
        /// <remarks>
        ///		TriggerBase is the base class for controlling actions. Override OnAttached() and 
        ///		OnDetaching() to hook and unhook handlers on the AssociatedObject. You may 
        ///		constrain the types that a derived TriggerBase may be attached to by specifying 
        ///		the generic parameter. Call InvokeActions() to fire all Actions associated with 
        ///		this TriggerBase.
        ///	</remarks>
        public abstract class TriggerBase<T> : TriggerBase where T : DependencyObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TriggerBase&lt;T&gt;"/> class.
            /// </summary>
            protected TriggerBase()
                : base(typeof(T))
            {
            }

            /// <summary>
            /// Gets the object to which the trigger is attached.
            /// </summary>
            /// <value>The associated object.</value>
            protected new T AssociatedObject
            {
                get
                {
                    return (T)base.AssociatedObject;
                }
            }

            /// <summary>
            /// Gets the type constraint of the associated object.
            /// </summary>
            /// <value>The associated object type constraint.</value>
            protected sealed override Type AssociatedObjectTypeConstraint
            {
                get
                {
                    return base.AssociatedObjectTypeConstraint;
                }
            }
        }

        /// <summary>
        /// Argument passed to PreviewInvoke event. Assigning Cancelling to True will cancel the invoking of the trigger.
        /// </summary>
        /// <remarks>This is an infrastructure class. Behavior attached to a trigger base object can add its behavior as a listener to TriggerBase.PreviewInvoke.</remarks>
        public class PreviewInvokeEventArgs : EventArgs
        {
            public bool Cancelling { get; set; }
        }

        /// <summary>
        /// Represents an object that can invoke Actions conditionally.
        /// </summary>
        /// <remarks>This is an infrastructure class. Trigger authors should derive from Trigger&lt;T&gt; instead of this class.</remarks>
        [ContentProperty("Actions")]
        public abstract class TriggerBase :
            Animatable,
            IAttachedObject
        {
            private DependencyObject associatedObject;
            private Type associatedObjectTypeConstraint;

            private static readonly DependencyPropertyKey ActionsPropertyKey = DependencyProperty.RegisterReadOnly("Actions",
                                                                                                                typeof(TriggerActionCollection),
                                                                                                                typeof(TriggerBase),
                                                                                                                new FrameworkPropertyMetadata());

            public static readonly DependencyProperty ActionsProperty = ActionsPropertyKey.DependencyProperty;

            internal TriggerBase(Type associatedObjectTypeConstraint)
            {
                this.associatedObjectTypeConstraint = associatedObjectTypeConstraint;
                TriggerActionCollection newCollection = new TriggerActionCollection();
                this.SetValue(ActionsPropertyKey, newCollection);
            }

            /// <summary>
            /// Gets the object to which the trigger is attached.
            /// </summary>
            /// <value>The associated object.</value>
            protected DependencyObject AssociatedObject
            {
                get
                {
                    this.ReadPreamble();
                    return this.associatedObject;
                }
            }

            /// <summary>
            /// Gets the type constraint of the associated object.
            /// </summary>
            /// <value>The associated object type constraint.</value>
            protected virtual Type AssociatedObjectTypeConstraint
            {
                get
                {
                    this.ReadPreamble();
                    return this.associatedObjectTypeConstraint;
                }
            }

            /// <summary>
            /// Gets the actions associated with this trigger.
            /// </summary>
            /// <value>The actions associated with this trigger.</value>
            public TriggerActionCollection Actions
            {
                get
                {
                    return (TriggerActionCollection)this.GetValue(ActionsProperty);
                }
            }

            /// <summary>
            /// Event handler for registering to PreviewInvoke.
            /// </summary>
            public event EventHandler<PreviewInvokeEventArgs> PreviewInvoke;

            /// <summary>
            /// Invoke all actions associated with this trigger.
            /// </summary>
            /// <remarks>Derived classes should call this to fire the trigger.</remarks>
            protected void InvokeActions(object parameter)
            {
                if (this.PreviewInvoke != null)
                {
                    // Fire the previewInvoke event 
                    PreviewInvokeEventArgs previewInvokeEventArg = new PreviewInvokeEventArgs();
                    this.PreviewInvoke(this, previewInvokeEventArg);
                    // If a handler has cancelled the event, abort the invoke
                    if (previewInvokeEventArg.Cancelling == true)
                    {
                        return;
                    }
                }

                foreach (TriggerAction action in this.Actions)
                {
                    action.CallInvoke(parameter);
                }
            }

            /// <summary>
            /// Called after the trigger is attached to an AssociatedObject.
            /// </summary>
            protected virtual void OnAttached()
            {
            }

            /// <summary>
            /// Called when the trigger is being detached from its AssociatedObject, but before it has actually occurred.
            /// </summary>
            protected virtual void OnDetaching()
            {
            }

            /// <summary>
            /// Creates a new instance of the TriggerBase derived class.
            /// </summary>
            /// <returns>The new instance.</returns>
            protected override Freezable CreateInstanceCore()
            {
                Type classType = this.GetType();
                return (Freezable)Activator.CreateInstance(classType);
            }

            #region IAttachedObject Members

            /// <summary>
            /// Gets the associated object.
            /// </summary>
            /// <value>The associated object.</value>
            DependencyObject IAttachedObject.AssociatedObject
            {
                get
                {
                    return this.AssociatedObject;
                }
            }

            /// <summary>
            /// Attaches to the specified object.
            /// </summary>
            /// <param name="dependencyObject">The object to attach to.</param>
            /// <exception cref="InvalidOperationException">Cannot host the same trigger on more than one object at a time.</exception>
            /// <exception cref="InvalidOperationException">dependencyObject does not satisfy the trigger type constraint.</exception>
            public void Attach(DependencyObject dependencyObject)
            {
                if (dependencyObject != this.AssociatedObject)
                {
                    if (this.AssociatedObject != null)
                    {
                        throw new InvalidOperationException(EmbeddedStringTable.ExceptionStringTable["CannotHostTriggerMultipleTimesExceptionMessage"]);
                    }

                    // Ensure the type constraint is met
                    if (dependencyObject != null && !this.AssociatedObjectTypeConstraint.IsAssignableFrom(dependencyObject.GetType()))
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                                                                            EmbeddedStringTable.ExceptionStringTable["TypeConstraintViolatedExceptionMessage"],
                                                                            this.GetType().Name,
                                                                            dependencyObject.GetType().Name,
                                                                            this.AssociatedObjectTypeConstraint.Name));
                    }

                    this.WritePreamble();
                    this.associatedObject = dependencyObject;
                    this.WritePostscript();

                    this.Actions.Attach(dependencyObject);
                    this.OnAttached();
                }
            }

            /// <summary>
            /// Detaches this instance from its associated object.
            /// </summary>
            public void Detach()
            {
                this.OnDetaching();
                this.WritePreamble();
                this.associatedObject = null;
                this.WritePostscript();
                this.Actions.Detach();
            }

            #endregion
        }

        /// <summary>
        /// Represents a trigger that can listen to an element other than its AssociatedObject.
        /// </summary>
        /// <typeparam name="T">The type that this trigger can be associated with.</typeparam>
        /// <remarks>
        ///		EventTriggerBase extends TriggerBase to add knowledge of another object than the one it is attached to. 
        ///		This allows a user to attach a Trigger/Action pair to one element and invoke the Action in response to a 
        ///		change in another object somewhere else. Override OnSourceChanged to hook or unhook handlers on the source 
        ///		element, and OnAttached/OnDetaching for the associated element. The type of the Source element can be 
        ///		constrained by the generic type parameter. If you need control over the type of the 
        ///		AssociatedObject, set a TypeConstraintAttribute on your derived type.
        ///	</remarks>
        public abstract class EventTriggerBase<T> : EventTriggerBase where T : class
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EventTriggerBase&lt;T&gt;"/> class.
            /// </summary>
            protected EventTriggerBase()
                : base(typeof(T))
            {
            }

            /// <summary>
            /// Gets the resolved source. If <c ref="SourceName"/> is not set or cannot be resolved, defaults to AssociatedObject.
            /// </summary>
            /// <value>The resolved source object.</value>
            /// <remarks>In general, this property should be used in place of AssociatedObject in derived classes.</remarks>
            public new T Source
            {
                get
                {
                    return (T)base.Source;
                }
            }

            internal sealed override void OnSourceChangedImpl(object oldSource, object newSource)
            {
                base.OnSourceChangedImpl(oldSource, newSource);
                this.OnSourceChanged(oldSource as T, newSource as T);
            }

            /// <summary>
            /// Called when the source property changes.
            /// </summary>
            /// <remarks>Override this to hook functionality to and unhook functionality from the specified source, rather than the AssociatedObject.</remarks>
            /// <param name="oldSource">The old source.</param>
            /// <param name="newSource">The new source.</param>
            protected virtual void OnSourceChanged(T oldSource, T newSource)
            {
            }
        }

        /// <summary>
        /// Represents a trigger that can listen to an object other than its AssociatedObject.
        /// </summary>
        /// <remarks>This is an infrastructure class. Trigger authors should derive from EventTriggerBase&lt;T&gt; instead of this class.</remarks>
        public abstract class EventTriggerBase : TriggerBase
        {
            private Type sourceTypeConstraint;
            private bool isSourceChangedRegistered;
            private NameResolver sourceNameResolver;
            private MethodInfo eventHandlerMethodInfo;

            public static readonly DependencyProperty SourceObjectProperty = DependencyProperty.Register("SourceObject",
                                                                                                        typeof(object),
                                                                                                        typeof(EventTriggerBase),
                                                                                                        new PropertyMetadata(
                                                                                                            new PropertyChangedCallback(OnSourceObjectChanged)));


            public static readonly DependencyProperty SourceNameProperty = DependencyProperty.Register("SourceName",
                                                                                                        typeof(string),
                                                                                                        typeof(EventTriggerBase),
                                                                                                        new PropertyMetadata(
                                                                                                            new PropertyChangedCallback(OnSourceNameChanged)));

            /// <summary>
            /// Gets the type constraint of the associated object.
            /// </summary>
            /// <value>The associated object type constraint.</value>
            /// <remarks>Define a TypeConstraintAttribute on a derived type to constrain the types it may be attached to.</remarks>
            protected sealed override Type AssociatedObjectTypeConstraint
            {
                get
                {
                    AttributeCollection attributes = TypeDescriptor.GetAttributes(this.GetType());
                    TypeConstraintAttribute typeConstraintAttribute = attributes[typeof(TypeConstraintAttribute)] as TypeConstraintAttribute;

                    if (typeConstraintAttribute != null)
                    {
                        return typeConstraintAttribute.Constraint;
                    }
                    return typeof(DependencyObject);
                }
            }

            /// <summary>
            /// Gets the source type constraint.
            /// </summary>
            /// <value>The source type constraint.</value>
            protected Type SourceTypeConstraint
            {
                get
                {
                    return this.sourceTypeConstraint;
                }
            }

            /// <summary>
            /// Gets or sets the target object. If TargetObject is not set, the target will look for the object specified by TargetName. If an element referred to by TargetName cannot be found, the target will default to the AssociatedObject. This is a dependency property.
            /// </summary>
            /// <value>The target object.</value>
            public object SourceObject
            {
                get { return this.GetValue(SourceObjectProperty); }
                set { this.SetValue(SourceObjectProperty, value); }
            }

            /// <summary>
            /// Gets or sets the name of the element this EventTriggerBase listens for as a source. If the name is not set or cannot be resolved, the AssociatedObject will be used.  This is a dependency property.
            /// </summary>
            /// <value>The name of the source element.</value>
            public string SourceName
            {
                get { return (string)this.GetValue(SourceNameProperty); }
                set { this.SetValue(SourceNameProperty, value); }
            }

            /// <summary>
            /// Gets the resolved source. If <c ref="SourceName"/> is not set or cannot be resolved, defaults to AssociatedObject.
            /// </summary>
            /// <value>The resolved source object.</value>
            /// <remarks>In general, this property should be used in place of AssociatedObject in derived classes.</remarks>
            /// <exception cref="InvalidOperationException">The element pointed to by <c cref="Source"/> does not satisify the type constraint.</exception>
            public object Source
            {
                get
                {
                    object source = this.AssociatedObject;

                    if (this.SourceObject != null)
                    {
                        source = this.SourceObject;
                    }
                    else if (this.IsSourceNameSet)
                    {
                        source = this.SourceNameResolver.Object;
                        if (source != null && !this.SourceTypeConstraint.IsAssignableFrom(source.GetType()))
                        {
                            throw new InvalidOperationException(string.Format(
                                CultureInfo.CurrentCulture,
                                EmbeddedStringTable.ExceptionStringTable["RetargetedTypeConstraintViolatedExceptionMessage"],
                                this.GetType().Name,
                                source.GetType(),
                                this.SourceTypeConstraint,
                                "Source"));
                        }
                    }
                    return source;
                }
            }

            private NameResolver SourceNameResolver
            {
                get { return this.sourceNameResolver; }
            }

            private bool IsSourceChangedRegistered
            {
                get { return this.isSourceChangedRegistered; }
                set { this.isSourceChangedRegistered = value; }
            }

            private bool IsSourceNameSet
            {
                get
                {
                    return !string.IsNullOrEmpty(this.SourceName) || this.ReadLocalValue(SourceNameProperty) != DependencyProperty.UnsetValue;
                }
            }

            private bool IsLoadedRegistered
            {
                get;
                set;
            }

            internal EventTriggerBase(Type sourceTypeConstraint)
                : base(typeof(DependencyObject))
            {
                this.sourceTypeConstraint = sourceTypeConstraint;
                this.sourceNameResolver = new NameResolver();
                this.RegisterSourceChanged();
            }

            /// <summary>
            /// Specifies the name of the Event this EventTriggerBase is listening for.
            /// </summary>
            /// <returns></returns>
            [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "NikhilKo convinced us this was the right choice.")]
            protected abstract string GetEventName();

            /// <summary>
            /// Called when the event associated with this EventTriggerBase is fired. By default, this will invoke all actions on the trigger.
            /// </summary>
            /// <param name="eventArgs">The <see cref="System.EventArgs"/> instance containing the event data.</param>
            /// <remarks>Override this to provide more granular control over when actions associated with this trigger will be invoked.</remarks>
            protected virtual void OnEvent(EventArgs eventArgs)
            {
                this.InvokeActions(eventArgs);
            }

            private void OnSourceChanged(object oldSource, object newSource)
            {
                if (this.AssociatedObject != null)
                {
                    this.OnSourceChangedImpl(oldSource, newSource);
                }
            }

            /// <summary>
            /// Called when the source changes.
            /// </summary>
            /// <param name="oldSource">The old source.</param>
            /// <param name="newSource">The new source.</param>
            /// <remarks>This function should be overridden in derived classes to hook functionality to and unhook functionality from the changing source objects.</remarks>
            internal virtual void OnSourceChangedImpl(object oldSource, object newSource)
            {
                if (string.IsNullOrEmpty(this.GetEventName()))
                {
                    return;
                }
                // we'll handle loaded elsewhere
                if (string.Compare(this.GetEventName(), "Loaded", StringComparison.Ordinal) != 0)
                {
                    if (oldSource != null && this.SourceTypeConstraint.IsAssignableFrom(oldSource.GetType()))
                    {
                        this.UnregisterEvent(oldSource, this.GetEventName());
                    }

                    if (newSource != null && this.SourceTypeConstraint.IsAssignableFrom(newSource.GetType()))
                    {
                        this.RegisterEvent(newSource, this.GetEventName());
                    }
                }
            }

            /// <summary>
            /// Called after the trigger is attached to an AssociatedObject.
            /// </summary>
            protected override void OnAttached()
            {
                // We can't resolve element names using a Behavior, as it isn't a FrameworkElement.
                // Hence, if we are Hosted on a Behavior, we need to resolve against the Behavior's
                // Host rather than our own. See comment in TargetedTriggerAction.
                // TODO jekelly 6/20/08: Ideally we could do a namespace walk, but SL doesn't expose
                //						 a way to do this. This solution only looks one level deep. 
                //						 A Behavior with a Behavior attached won't work. This is OK
                //						 for now, but should consider a more general solution if needed.
                base.OnAttached();
                DependencyObject newHost = this.AssociatedObject;
                Behavior newBehavior = newHost as Behavior;
                FrameworkElement newHostElement = newHost as FrameworkElement;

                this.RegisterSourceChanged();
                if (newBehavior != null)
                {
                    newHost = ((IAttachedObject)newBehavior).AssociatedObject;
                    newBehavior.AssociatedObjectChanged += new EventHandler(OnBehaviorHostChanged);
                }
                else if (this.SourceObject != null || newHostElement == null)
                {
                    try
                    {
                        this.OnSourceChanged(null, this.Source);
                    }
                    catch (InvalidOperationException)
                    {
                        // If we're hosted on a DependencyObject, we don't have a name scope reference element.
                        // Hence, we'll fire off the source changed manually when we first attach. However, if we've
                        // been attached to something that doesn't meet the target type constraint, accessing Source
                        // will throw.
                    }
                }
                else
                {
                    this.SourceNameResolver.NameScopeReferenceElement = newHostElement;
                }

                bool isLoadedEvent = (string.Compare(this.GetEventName(), "Loaded", StringComparison.Ordinal) == 0);

                if (isLoadedEvent && newHostElement != null && !Interaction.IsElementLoaded(newHostElement))
                {
                    this.RegisterLoaded(newHostElement);
                }
            }

            /// <summary>
            /// Called when the trigger is being detached from its AssociatedObject, but before it has actually occurred.
            /// </summary>
            protected override void OnDetaching()
            {
                base.OnDetaching();
                Behavior oldBehavior = this.AssociatedObject as Behavior;
                FrameworkElement oldElement = this.AssociatedObject as FrameworkElement;

                try
                {
                    this.OnSourceChanged(this.Source, null);
                }
                catch (InvalidOperationException)
                {
                    // We fire off the source changed manually when we detach. However, if we've been attached to 
                    // something that doesn't meet the target type constraint, accessing Source will throw.
                }
                this.UnregisterSourceChanged();

                if (oldBehavior != null)
                {
                    oldBehavior.AssociatedObjectChanged -= new EventHandler(OnBehaviorHostChanged);
                }

                this.SourceNameResolver.NameScopeReferenceElement = null;

                bool isLoadedEvent = (string.Compare(this.GetEventName(), "Loaded", StringComparison.Ordinal) == 0);

                if (isLoadedEvent && oldElement != null)
                {
                    this.UnregisterLoaded(oldElement);
                }
            }

            private void OnBehaviorHostChanged(object sender, EventArgs e)
            {
                this.SourceNameResolver.NameScopeReferenceElement = ((IAttachedObject)sender).AssociatedObject as FrameworkElement;
            }

            private static void OnSourceObjectChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
            {
                EventTriggerBase eventTriggerBase = (EventTriggerBase)obj;
                object sourceNameObject = eventTriggerBase.SourceNameResolver.Object;
                if (args.NewValue == null)
                {
                    eventTriggerBase.OnSourceChanged(args.OldValue, sourceNameObject);
                }
                else
                {
                    if (args.OldValue == null && sourceNameObject != null)
                    {
                        eventTriggerBase.UnregisterEvent(sourceNameObject, eventTriggerBase.GetEventName());
                    }
                    eventTriggerBase.OnSourceChanged(args.OldValue, args.NewValue);
                }
            }

            private static void OnSourceNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
            {
                EventTriggerBase trigger = (EventTriggerBase)obj;
                trigger.SourceNameResolver.Name = (string)args.NewValue;
            }

            private void RegisterSourceChanged()
            {
                if (!this.IsSourceChangedRegistered)
                {
                    this.SourceNameResolver.ResolvedElementChanged += this.OnSourceNameResolverElementChanged;
                    this.IsSourceChangedRegistered = true;
                }
            }

            private void UnregisterSourceChanged()
            {
                if (this.IsSourceChangedRegistered)
                {
                    this.SourceNameResolver.ResolvedElementChanged -= this.OnSourceNameResolverElementChanged;
                    this.IsSourceChangedRegistered = false;
                }
            }

            private void OnSourceNameResolverElementChanged(object sender, NameResolvedEventArgs e)
            {
                if (this.SourceObject == null)
                {
                    this.OnSourceChanged(e.OldObject, e.NewObject);
                }
            }

            private void RegisterLoaded(FrameworkElement associatedElement)
            {
                Debug.Assert(this.eventHandlerMethodInfo == null);
                Debug.Assert(!this.IsLoadedRegistered, "Trying to register Loaded more than once.");
                if (!this.IsLoadedRegistered && associatedElement != null)
                {
                    associatedElement.Loaded += this.OnEventImpl;
                    this.IsLoadedRegistered = true;
                }
            }

            private void UnregisterLoaded(FrameworkElement associatedElement)
            {
                Debug.Assert(this.eventHandlerMethodInfo == null);
                if (this.IsLoadedRegistered && associatedElement != null)
                {
                    associatedElement.Loaded -= this.OnEventImpl;
                    this.IsLoadedRegistered = false;
                }
            }

            /// <exception cref="ArgumentException">Could not find eventName on the Target.</exception>
            private void RegisterEvent(object obj, string eventName)
            {
                Debug.Assert(this.eventHandlerMethodInfo == null && string.Compare(eventName, "Loaded", StringComparison.Ordinal) != 0);
                Type targetType = obj.GetType();
                EventInfo eventInfo = targetType.GetEvent(eventName);
                if (eventInfo == null)
                {
                    if (this.SourceObject != null)
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                                                                    EmbeddedStringTable.ExceptionStringTable["EventTriggerCannotFindEventNameExceptionMessage"],
                                                                    eventName,
                                                                    obj.GetType().Name));
                    }
                    else
                    {
                        return;
                    }
                }

                if (!EventTriggerBase.IsValidEvent(eventInfo))
                {
                    if (this.SourceObject != null)
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                                                                    EmbeddedStringTable.ExceptionStringTable["EventTriggerBaseInvalidEventExceptionMessage"],
                                                                    eventName,
                                                                    obj.GetType().Name));
                    }
                    else
                    {
                        return;
                    }
                }
                this.eventHandlerMethodInfo = typeof(EventTriggerBase).GetMethod("OnEventImpl", BindingFlags.NonPublic | BindingFlags.Instance);
                eventInfo.AddEventHandler(obj, Delegate.CreateDelegate(eventInfo.EventHandlerType, this, this.eventHandlerMethodInfo));
            }

            private static bool IsValidEvent(EventInfo eventInfo)
            {
                Type eventHandlerType = eventInfo.EventHandlerType;
                if (typeof(Delegate).IsAssignableFrom(eventInfo.EventHandlerType))
                {
                    MethodInfo invokeMethod = eventHandlerType.GetMethod("Invoke");
                    ParameterInfo[] parameters = invokeMethod.GetParameters();
                    return parameters.Length == 2 && typeof(object).IsAssignableFrom(parameters[0].ParameterType) && typeof(EventArgs).IsAssignableFrom(parameters[1].ParameterType);
                }
                return false;
            }

            private void UnregisterEvent(object obj, string eventName)
            {
                if (string.Compare(eventName, "Loaded", StringComparison.Ordinal) == 0)
                {
                    FrameworkElement frameworkElement = obj as FrameworkElement;
                    if (frameworkElement != null)
                    {
                        this.UnregisterLoaded(frameworkElement);
                    }
                }
                else
                {
                    this.UnregisterEventImpl(obj, eventName);
                }
            }

            private void UnregisterEventImpl(object obj, string eventName)
            {
                Type targetType = obj.GetType();
                Debug.Assert(this.eventHandlerMethodInfo != null || targetType.GetEvent(eventName) == null);

                if (this.eventHandlerMethodInfo == null)
                {
                    return;
                }

                EventInfo eventInfo = targetType.GetEvent(eventName);
                Debug.Assert(eventInfo != null, "Should not try to unregister an event that we successfully registered");
                eventInfo.RemoveEventHandler(obj, Delegate.CreateDelegate(eventInfo.EventHandlerType, this, this.eventHandlerMethodInfo));
                this.eventHandlerMethodInfo = null;
            }

            private void OnEventImpl(object sender, EventArgs eventArgs)
            {
                this.OnEvent(eventArgs);
            }

            internal void OnEventNameChanged(string oldEventName, string newEventName)
            {
                if (this.AssociatedObject != null)
                {
                    FrameworkElement associatedElement = this.Source as FrameworkElement;

                    if (associatedElement != null && string.Compare(oldEventName, "Loaded", StringComparison.Ordinal) == 0)
                    {
                        this.UnregisterLoaded(associatedElement);
                    }
                    else if (!string.IsNullOrEmpty(oldEventName))
                    {
                        this.UnregisterEvent(this.Source, oldEventName);
                    }
                    if (associatedElement != null && string.Compare(newEventName, "Loaded", StringComparison.Ordinal) == 0)
                    {
                        this.RegisterLoaded(associatedElement);
                    }
                    else if (!string.IsNullOrEmpty(newEventName))
                    {
                        this.RegisterEvent(this.Source, newEventName);
                    }
                }
            }
        }

        /// <summary>
        /// Provides data about which objects were affected when resolving a name change.
        /// </summary>
        internal sealed class NameResolvedEventArgs : EventArgs
        {
            private object oldObject;
            private object newObject;

            public object OldObject
            {
                get { return oldObject; }
            }

            public object NewObject
            {
                get { return newObject; }
            }

            public NameResolvedEventArgs(object oldObject, object newObject)
            {
                this.oldObject = oldObject;
                this.newObject = newObject;
            }
        }

        /// <summary>
        /// Helper class to handle the logic of resolving a TargetName into a Target element
        /// based on the context provided by a host element.
        /// </summary>
        internal sealed class NameResolver
        {
            private string name;
            private FrameworkElement nameScopeReferenceElement;

            /// <summary>
            /// Occurs when the resolved element has changed.
            /// </summary>
            public event EventHandler<NameResolvedEventArgs> ResolvedElementChanged;

            /// <summary>
            /// Gets or sets the name of the element to attempt to resolve.
            /// </summary>
            /// <value>The name to attempt to resolve.</value>
            public string Name
            {
                get { return this.name; }
                set
                {
                    // because the TargetName influences that Target returns, need to
                    // store the Target value before we change it so we can detect if
                    // it has actually changed
                    DependencyObject oldObject = this.Object;
                    this.name = value;
                    this.UpdateObjectFromName(oldObject);
                }
            }

            /// <summary>
            /// The resolved object. Will return the reference element if TargetName is null or empty, or if a resolve has not been attempted.
            /// </summary>
            public DependencyObject Object
            {
                get
                {
                    if (string.IsNullOrEmpty(this.Name) && this.HasAttempedResolve)
                    {
                        return this.NameScopeReferenceElement;
                    }
                    return this.ResolvedObject;
                }
            }

            /// <summary>
            /// Gets or sets the reference element from which to perform the name resolution.
            /// </summary>
            /// <value>The reference element.</value>
            public FrameworkElement NameScopeReferenceElement
            {
                get { return this.nameScopeReferenceElement; }
                set
                {
                    FrameworkElement oldHost = this.NameScopeReferenceElement;
                    this.nameScopeReferenceElement = value;
                    this.OnNameScopeReferenceElementChanged(oldHost);
                }
            }

            private FrameworkElement ActualNameScopeReferenceElement
            {
                get
                {
                    if (this.NameScopeReferenceElement == null || !Interaction.IsElementLoaded(this.NameScopeReferenceElement))
                    {
                        return null;
                    }
                    return GetActualNameScopeReference(this.NameScopeReferenceElement);
                }
            }

            private DependencyObject ResolvedObject
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a value indicating whether the reference element load is pending.
            /// </summary>
            /// <value>
            /// 	<c>True</c> if [pending reference element load]; otherwise, <c>False</c>.
            /// </value>
            /// <remarks>
            /// If the Host has not been loaded, the name will not be resolved.
            /// In that case, delay the resolution and track that fact with this property.
            /// </remarks>
            private bool PendingReferenceElementLoad
            {
                get;
                set;
            }

            private bool HasAttempedResolve
            {
                get;
                set;
            }

            private void OnNameScopeReferenceElementChanged(FrameworkElement oldNameScopeReference)
            {
                if (this.PendingReferenceElementLoad)
                {
                    oldNameScopeReference.Loaded -= new RoutedEventHandler(OnNameScopeReferenceLoaded);
                    this.PendingReferenceElementLoad = false;
                }
                this.HasAttempedResolve = false;
                this.UpdateObjectFromName(this.Object);
            }

            /// <summary>
            /// Attempts to update the resolved object from the name within the context of the namescope reference element.
            /// </summary>
            /// <param name="oldObject">The old resolved object.</param>
            /// <remarks>
            /// Resets the existing target and attempts to resolve the current TargetName from the
            /// context of the current Host. If it cannot resolve from the context of the Host, it will
            /// continue up the visual tree until it resolves. If it has not resolved it when it reaches
            /// the root, it will set the Target to null and write a warning message to Debug output.
            /// </remarks>
            private void UpdateObjectFromName(DependencyObject oldObject)
            {
                DependencyObject newObject = null;

                // clear the cache
                this.ResolvedObject = null;

                if (this.NameScopeReferenceElement != null)
                {
                    if (!Interaction.IsElementLoaded(this.NameScopeReferenceElement))
                    {
                        // We had a debug message here, but it seems like too common a scenario
                        this.NameScopeReferenceElement.Loaded += new RoutedEventHandler(OnNameScopeReferenceLoaded);
                        this.PendingReferenceElementLoad = true;
                        return;
                    }

                    // update the target
                    if (!string.IsNullOrEmpty(this.Name))
                    {
                        FrameworkElement namescopeElement = this.ActualNameScopeReferenceElement;
                        if (namescopeElement != null)
                        {
                            newObject = namescopeElement.FindName(this.Name) as DependencyObject;
                        }

                        if (newObject == null)
                        {
                            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, EmbeddedStringTable.ExceptionStringTable["UnableToResolveTargetNameWarningMessage"], this.Name));
                        }
                    }
                }
                this.HasAttempedResolve = true;
                // note that this.Object will be null if it doesn't resolve
                this.ResolvedObject = newObject;
                if (oldObject != this.Object)
                {
                    // this.Source may not be newTarget, if TargetName is null or empty
                    this.OnObjectChanged(oldObject, this.Object);
                }
            }

            private void OnObjectChanged(DependencyObject oldTarget, DependencyObject newTarget)
            {
                if (this.ResolvedElementChanged != null)
                {
                    this.ResolvedElementChanged(this, new NameResolvedEventArgs(oldTarget, newTarget));
                }
            }

            private FrameworkElement GetActualNameScopeReference(FrameworkElement initialReferenceElement)
            {
                Debug.Assert(Interaction.IsElementLoaded(initialReferenceElement));
                FrameworkElement nameScopeReference = initialReferenceElement;

                if (this.IsNameScope(initialReferenceElement))
                {
                    nameScopeReference = initialReferenceElement.Parent as FrameworkElement ?? nameScopeReference;
                }
                return nameScopeReference;
            }

            private bool IsNameScope(FrameworkElement frameworkElement)
            {
                FrameworkElement parentElement = frameworkElement.Parent as FrameworkElement;
                if (parentElement != null)
                {
                    // Logic behind this check is as follows:
                    // Resolves in Child Scope  |  Resolves in Parent Scope  |  Should use Parent as namescope?
                    //			no				|			no				 |			no
                    //			yes				|			no				 |			no
                    //			no				|			yes				 |			yes
                    //			yes				|			yes				 |			yes*
                    // * Note that if the resolved element is the same, it doesn't matter if we use the parent or child,
                    //   so we choose the parent. If they are different, we've found a name collision across namescopes,
                    //	 and our rule is to use the parent as the namescope in that case and discourage people from 
                    //	 getting into this state by disallowing creation of targeted types on Control XAML root elements.
                    // Hence, we only need to check if Name resolves in the parent scope to know if we need to use the parent.
                    object resolvedInParentScope = parentElement.FindName(this.Name);
                    return resolvedInParentScope != null;
                }
                return false;
            }

            private void OnNameScopeReferenceLoaded(object sender, RoutedEventArgs e)
            {
                this.PendingReferenceElementLoad = false;
                this.NameScopeReferenceElement.Loaded -= new RoutedEventHandler(OnNameScopeReferenceLoaded);
                this.UpdateObjectFromName(this.Object);
            }
        }

        /// <summary>
        /// An interface for an object that can be attached to another object.
        /// </summary>
        public interface IAttachedObject
        {
            /// <summary>
            /// Gets the associated object.
            /// </summary>
            /// <value>The associated object.</value>
            /// <remarks>Represents the object the instance is attached to.</remarks>
            DependencyObject AssociatedObject
            {
                get;
            }

            /// <summary>
            /// Attaches to the specified object.
            /// </summary>
            /// <param name="dependencyObject">The object to attach to.</param>
            void Attach(DependencyObject dependencyObject);

            /// <summary>
            /// Detaches this instance from its associated object.
            /// </summary>
            void Detach();
        }

        /// <summary>
        /// Represents an attachable object that encapsulates a unit of functionality.
        /// </summary>
        /// <typeparam name="T">The type to which this action can be attached.</typeparam>
        public abstract class TriggerAction<T> : TriggerAction where T : DependencyObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TriggerAction&lt;T&gt;"/> class.
            /// </summary>
            protected TriggerAction()
                : base(typeof(T))
            {
            }

            /// <summary>
            /// Gets the object to which this <see cref="Microsoft.Xaml.Behaviors.TriggerAction&lt;T&gt;"/> is attached.
            /// </summary>
            /// <value>The associated object.</value>
            protected new T AssociatedObject
            {
                get
                {
                    return (T)base.AssociatedObject;
                }
            }

            /// <summary>
            /// Gets the associated object type constraint.
            /// </summary>
            /// <value>The associated object type constraint.</value>
            protected sealed override Type AssociatedObjectTypeConstraint
            {
                get
                {
                    return base.AssociatedObjectTypeConstraint;
                }
            }
        }

        /// <summary>
        /// Represents an attachable object that encapsulates a unit of functionality.
        /// </summary>
        /// <remarks>This is an infrastructure class. Action authors should derive from TriggerAction&lt;T&gt; instead of this class.</remarks>
        [DefaultTrigger(typeof(UIElement), typeof(Microsoft.Xaml.Behaviors.EventTrigger), "MouseLeftButtonDown")]
        [DefaultTrigger(typeof(ButtonBase), typeof(Microsoft.Xaml.Behaviors.EventTrigger), "Click")]
        public abstract class TriggerAction :
            Animatable,
            IAttachedObject
        {
            private bool isHosted;
            private DependencyObject associatedObject;
            private Type associatedObjectTypeConstraint;

            public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled",
                                                                                                        typeof(bool),
                                                                                                        typeof(TriggerAction),
                                                                                                        new FrameworkPropertyMetadata(true));

            /// <summary>
            /// Gets or sets a value indicating whether this action will run when invoked. This is a dependency property.
            /// </summary>
            /// <value>
            /// 	<c>True</c> if this action will be run when invoked; otherwise, <c>False</c>.
            /// </value>
            public bool IsEnabled
            {
                get { return (bool)this.GetValue(TriggerAction.IsEnabledProperty); }
                set
                {
                    this.SetValue(TriggerAction.IsEnabledProperty, value);
                }
            }

            /// <summary>
            /// Gets the object to which this action is attached.
            /// </summary>
            /// <value>The associated object.</value>
            protected DependencyObject AssociatedObject
            {
                get
                {
                    this.ReadPreamble();
                    return this.associatedObject;
                }
            }

            /// <summary>
            /// Gets the associated object type constraint.
            /// </summary>
            /// <value>The associated object type constraint.</value>
            protected virtual Type AssociatedObjectTypeConstraint
            {
                get
                {
                    this.ReadPreamble();
                    return this.associatedObjectTypeConstraint;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether this instance is attached.
            /// </summary>
            /// <value><c>True</c> if this instance is attached; otherwise, <c>False</c>.</value>
            internal bool IsHosted
            {
                get
                {
                    this.ReadPreamble();
                    return this.isHosted;
                }
                set
                {
                    this.WritePreamble();
                    this.isHosted = value;
                    this.WritePostscript();
                }
            }

            internal TriggerAction(Type associatedObjectTypeConstraint)
            {
                this.associatedObjectTypeConstraint = associatedObjectTypeConstraint;
            }

            /// <summary>
            /// Attempts to invoke the action.
            /// </summary>
            /// <param name="parameter">The parameter to the action. If the action does not require a parameter, the parameter may be set to a null reference.</param>
            internal void CallInvoke(object parameter)
            {
                if (this.IsEnabled)
                {
                    this.Invoke(parameter);
                }
            }

            /// <summary>
            /// Invokes the action.
            /// </summary>
            /// <param name="parameter">The parameter to the action. If the action does not require a parameter, the parameter may be set to a null reference.</param>
            protected abstract void Invoke(object parameter);

            /// <summary>
            /// Called after the action is attached to an AssociatedObject.
            /// </summary>
            protected virtual void OnAttached()
            {
            }

            /// <summary>
            /// Called when the action is being detached from its AssociatedObject, but before it has actually occurred.
            /// </summary>
            protected virtual void OnDetaching()
            {
            }

            /// <summary>
            /// When implemented in a derived class, creates a new instance of the <see cref="T:System.Windows.Freezable"/> derived class.
            /// </summary>
            /// <returns>The new instance.</returns>
            protected override Freezable CreateInstanceCore()
            {
                Type classType = this.GetType();
                return (Freezable)Activator.CreateInstance(classType);
            }

            #region IAttachedObject Members

            /// <summary>
            /// Gets the associated object.
            /// </summary>
            /// <value>The associated object.</value>
            DependencyObject IAttachedObject.AssociatedObject
            {
                get
                {
                    return this.AssociatedObject;
                }
            }

            /// <summary>
            /// Attaches to the specified object.
            /// </summary>
            /// <param name="dependencyObject">The object to attach to.</param>
            /// <exception cref="InvalidOperationException">Cannot host the same TriggerAction on more than one object at a time.</exception>
            /// <exception cref="InvalidOperationException">dependencyObject does not satisfy the TriggerAction type constraint.</exception>
            public void Attach(DependencyObject dependencyObject)
            {
                if (dependencyObject != this.AssociatedObject)
                {
                    if (this.AssociatedObject != null)
                    {
                        throw new InvalidOperationException(EmbeddedStringTable.ExceptionStringTable["CannotHostTriggerActionMultipleTimesExceptionMessage"]);
                    }

                    // Ensure the type constraint is met
                    if (dependencyObject != null && !this.AssociatedObjectTypeConstraint.IsAssignableFrom(dependencyObject.GetType()))
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                                                                            EmbeddedStringTable.ExceptionStringTable["TypeConstraintViolatedExceptionMessage"],
                                                                            this.GetType().Name,
                                                                            dependencyObject.GetType().Name,
                                                                            this.AssociatedObjectTypeConstraint.Name));
                    }

                    this.WritePreamble();
                    this.associatedObject = dependencyObject;
                    this.WritePostscript();

                    this.OnAttached();
                }
            }

            /// <summary>
            /// Detaches this instance from its associated object.
            /// </summary>
            public void Detach()
            {
                this.OnDetaching();
                this.WritePreamble();
                this.associatedObject = null;
                this.WritePostscript();
            }

            #endregion
        }

        /// <summary>
        /// Provides design tools information about what <see cref="TriggerBase"/> to instantiate for a given action or command.
        /// </summary>
        [CLSCompliant(false)]
        [AttributeUsage(AttributeTargets.Class |
                        AttributeTargets.Property,
                        AllowMultiple = true)]
        [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "FxCop is complaining about our single parameter override")]
        public sealed class DefaultTriggerAttribute : Attribute
        {
            private Type targetType;
            private Type triggerType;
            private object[] parameters;

            /// <summary>
            /// Gets the type that this DefaultTriggerAttribute applies to.
            /// </summary>
            /// <value>The type this DefaultTriggerAttribute applies to.</value>
            public Type TargetType
            {
                get { return this.targetType; }
            }

            /// <summary>
            /// Gets the type of the <see cref="TriggerBase"/> to instantiate.
            /// </summary>
            /// <value>The type of the <see cref="TriggerBase"/> to instantiate.</value>
            public Type TriggerType
            {
                get { return this.triggerType; }
            }

            /// <summary>
            /// Gets the parameters to pass to the <see cref="TriggerBase"/> constructor.
            /// </summary>
            /// <value>The parameters to pass to the <see cref="TriggerBase"/> constructor.</value>
            public IEnumerable Parameters
            {
                get { return this.parameters; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="DefaultTriggerAttribute"/> class.
            /// </summary>
            /// <param name="targetType">The type this attribute applies to.</param>
            /// <param name="triggerType">The type of <see cref="TriggerBase"/> to instantiate.</param>
            /// <param name="parameter">A single argument for the specified <see cref="TriggerBase"/>.</param>
            /// <exception cref="ArgumentException"><c cref="triggerType"/> is not derived from TriggerBase.</exception>
            /// <remarks>This constructor is useful if the specifed <see cref="TriggerBase"/> has a single argument. The
            /// resulting code will be CLS compliant.</remarks>
            public DefaultTriggerAttribute(Type targetType, Type triggerType, object parameter) :
                this(targetType, triggerType, new object[] { parameter })
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="DefaultTriggerAttribute"/> class.
            /// </summary>
            /// <param name="targetType">The type this attribute applies to.</param>
            /// <param name="triggerType">The type of <see cref="TriggerBase"/> to instantiate.</param>
            /// <param name="parameters">The constructor arguments for the specified <see cref="TriggerBase"/>.</param>
            /// <exception cref="ArgumentException"><c cref="triggerType"/> is not derived from TriggerBase.</exception>
            public DefaultTriggerAttribute(Type targetType, Type triggerType, params object[] parameters)
            {
                if (!typeof(TriggerBase).IsAssignableFrom(triggerType))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                                                                EmbeddedStringTable.ExceptionStringTable["DefaultTriggerAttributeInvalidTriggerTypeSpecifiedExceptionMessage"],
                                                                triggerType.Name));
                }

                // todo jekelly: validate that targetType is a valid target for the trigger specified by triggerType

                this.targetType = targetType;
                this.triggerType = triggerType;
                this.parameters = parameters;
            }

            /// <summary>
            /// Instantiates this instance.
            /// </summary>
            /// <returns>The <see cref="TriggerBase"/> specified by the DefaultTriggerAttribute.</returns>
            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Activator.CreateInstance could be calling user code which we don't want to bring us down.")]
            public TriggerBase Instantiate()
            {
                object trigger = null;
                try
                {
                    trigger = Activator.CreateInstance(this.TriggerType, this.parameters);
                }
                catch
                {
                }
                return (TriggerBase)trigger;
            }
        }

        /// <summary>
        /// Represents a collection of actions with a shared AssociatedObject and provides change notifications to its contents when that AssociatedObject changes.
        /// </summary>
        public class TriggerActionCollection : AttachableCollection<TriggerAction>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TriggerActionCollection"/> class.
            /// </summary>
            /// <remarks>Internal, because this should not be inherited outside this assembly.</remarks>
            internal TriggerActionCollection()
            {
            }

            /// <summary>
            /// Called immediately after the collection is attached to an AssociatedObject.
            /// </summary>
            protected override void OnAttached()
            {
                foreach (TriggerAction action in this)
                {
                    Debug.Assert(action.IsHosted, "Action must be hosted if it is in the collection.");
                    action.Attach(this.AssociatedObject);
                }
            }

            /// <summary>
            /// Called when the collection is being detached from its AssociatedObject, but before it has actually occurred.
            /// </summary>
            protected override void OnDetaching()
            {
                foreach (TriggerAction action in this)
                {
                    Debug.Assert(action.IsHosted, "Action must be hosted if it is in the collection.");
                    action.Detach();
                }
            }

            /// <summary>
            /// Called when a new item is added to the collection.
            /// </summary>
            /// <param name="item">The new item.</param>
            internal override void ItemAdded(TriggerAction item)
            {
                if (item.IsHosted)
                {
                    throw new InvalidOperationException(EmbeddedStringTable.ExceptionStringTable["CannotHostTriggerActionMultipleTimesExceptionMessage"]);
                }
                if (this.AssociatedObject != null)
                {
                    item.Attach(this.AssociatedObject);
                }
                item.IsHosted = true;
            }

            /// <summary>
            /// Called when an item is removed from the collection.
            /// </summary>
            /// <param name="item">The removed item.</param>
            internal override void ItemRemoved(TriggerAction item)
            {
                Debug.Assert(item.IsHosted, "Item should hosted if it is being removed from a TriggerCollection.");
                if (((IAttachedObject)item).AssociatedObject != null)
                {
                    item.Detach();
                }
                item.IsHosted = false;
            }

            /// <summary>
            /// Creates a new instance of the TriggerActionCollection.
            /// </summary>
            /// <returns>The new instance.</returns>
            protected override Freezable CreateInstanceCore()
            {
                return new TriggerActionCollection();
            }
        }


        /// <summary>
        /// Specifies type constraints on the AssociatedObject of TargetedTriggerAction and EventTriggerBase.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        public sealed class TypeConstraintAttribute : Attribute
        {
            /// <summary>
            /// Gets the constraint type.
            /// </summary>
            /// <value>The constraint type.</value>
            public Type Constraint
            {
                get;
                private set;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TypeConstraintAttribute"/> class.
            /// </summary>
            /// <param name="constraint">The constraint type.</param>
            public TypeConstraintAttribute(Type constraint)
            {
                this.Constraint = constraint;
            }
        }

        /// <summary>
        /// Represents a collection of IAttachedObject with a shared AssociatedObject and provides change notifications to its contents when that AssociatedObject changes.
        /// </summary>
        public abstract class AttachableCollection<T> :
            FreezableCollection<T>,
            IAttachedObject where T : DependencyObject, IAttachedObject
        {
            private Collection<T> snapshot;
            private DependencyObject associatedObject;

            /// <summary>
            /// The object on which the collection is hosted.
            /// </summary>
            protected DependencyObject AssociatedObject
            {
                get
                {
                    this.ReadPreamble();
                    return this.associatedObject;
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AttachableCollection&lt;T&gt;"/> class.
            /// </summary>
            /// <remarks>Internal, because this should not be inherited outside this assembly.</remarks>
            internal AttachableCollection()
            {
                INotifyCollectionChanged notifyCollectionChanged = (INotifyCollectionChanged)this;
                notifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);

                this.snapshot = new Collection<T>();
            }

            /// <summary>
            /// Called immediately after the collection is attached to an AssociatedObject.
            /// </summary>
            protected abstract void OnAttached();

            /// <summary>
            /// Called when the collection is being detached from its AssociatedObject, but before it has actually occurred.
            /// </summary>
            protected abstract void OnDetaching();

            /// <summary>
            /// Called when a new item is added to the collection.
            /// </summary>
            /// <param name="item">The new item.</param>
            internal abstract void ItemAdded(T item);

            /// <summary>
            /// Called when an item is removed from the collection.
            /// </summary>
            /// <param name="item">The removed item.</param>
            internal abstract void ItemRemoved(T item);

            [Conditional("DEBUG")]
            private void VerifySnapshotIntegrity()
            {
                bool isValid = (this.Count == this.snapshot.Count);
                if (isValid)
                {
                    for (int i = 0; i < this.Count; i++)
                    {
                        if (this[i] != this.snapshot[i])
                        {
                            isValid = false;
                            break;
                        }
                    }
                }
                Debug.Assert(isValid, "ReferentialCollection integrity has been compromised.");
            }

            /// <exception cref="InvalidOperationException">Cannot add the instance to a collection more than once.</exception>
            private void VerifyAdd(T item)
            {
                if (this.snapshot.Contains(item))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, EmbeddedStringTable.ExceptionStringTable["DuplicateItemInCollectionExceptionMessage"], typeof(T).Name, this.GetType().Name));
                }
            }

            private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                switch (e.Action)
                {
                    // We fix the snapshot to mirror the structure, even if an exception is thrown. This may not be desirable.
                    case NotifyCollectionChangedAction.Add:
                        foreach (T item in e.NewItems)
                        {
                            try
                            {
                                this.VerifyAdd(item);
                                this.ItemAdded(item);
                            }
                            finally
                            {
                                this.snapshot.Insert(this.IndexOf(item), item);
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        foreach (T item in e.OldItems)
                        {
                            this.ItemRemoved(item);
                            this.snapshot.Remove(item);
                        }
                        foreach (T item in e.NewItems)
                        {
                            try
                            {
                                this.VerifyAdd(item);
                                this.ItemAdded(item);
                            }
                            finally
                            {
                                this.snapshot.Insert(this.IndexOf(item), item);
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (T item in e.OldItems)
                        {
                            this.ItemRemoved(item);
                            this.snapshot.Remove(item);
                        }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        foreach (T item in this.snapshot)
                        {
                            this.ItemRemoved(item);
                        }
                        this.snapshot = new Collection<T>();
                        foreach (T item in this)
                        {
                            this.VerifyAdd(item);
                            this.ItemAdded(item);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                    default:
                        Debug.Fail("Unsupported collection operation attempted.");
                        break;
                }
#if DEBUG
                this.VerifySnapshotIntegrity();
#endif
            }

            #region IAttachedObject Members

            /// <summary>
            /// Gets the associated object.
            /// </summary>
            /// <value>The associated object.</value>
            DependencyObject IAttachedObject.AssociatedObject
            {
                get
                {
                    return this.AssociatedObject;
                }
            }

            /// <summary>
            /// Attaches to the specified object.
            /// </summary>
            /// <param name="dependencyObject">The object to attach to.</param>
            /// <exception cref="InvalidOperationException">The IAttachedObject is already attached to a different object.</exception>
            public void Attach(DependencyObject dependencyObject)
            {
                if (dependencyObject != this.AssociatedObject)
                {
                    if (this.AssociatedObject != null)
                    {
                        throw new InvalidOperationException();
                    }

                    if (Interaction.ShouldRunInDesignMode || !(bool)this.GetValue(DesignerProperties.IsInDesignModeProperty))
                    {
                        this.WritePreamble();
                        this.associatedObject = dependencyObject;
                        this.WritePostscript();
                    }
                    this.OnAttached();
                }
            }

            /// <summary>
            /// Detaches this instance from its associated object.
            /// </summary>
            public void Detach()
            {
                this.OnDetaching();
                this.WritePreamble();
                this.associatedObject = null;
                this.WritePostscript();
            }

            #endregion
        }

        /// <summary>
        /// A trigger that listens for a specified event on its source and fires when that event is fired.
        /// </summary>
        public class EventTrigger : EventTriggerBase<object>
        {
            public static readonly DependencyProperty EventNameProperty = DependencyProperty.Register("EventName",
                                                                                                        typeof(string),
                                                                                                        typeof(EventTrigger),
                                                                                                        new FrameworkPropertyMetadata(
                                                                                                            "Loaded",
                                                                                                            new PropertyChangedCallback(OnEventNameChanged)));

            /// <summary>
            /// Initializes a new instance of the <see cref="EventTrigger"/> class.
            /// </summary>
            public EventTrigger()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="EventTrigger"/> class.
            /// </summary>
            /// <param name="eventName">Name of the event.</param>
            public EventTrigger(string eventName)
            {
                this.EventName = eventName;
            }

            /// <summary>
            /// Gets or sets the name of the event to listen for. This is a dependency property.
            /// </summary>
            /// <value>The name of the event.</value>
            [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
            public string EventName
            {
                get { return (string)this.GetValue(EventNameProperty); }
                set { this.SetValue(EventNameProperty, value); }
            }

            protected override string GetEventName()
            {
                return this.EventName;
            }

            private static void OnEventNameChanged(object sender, DependencyPropertyChangedEventArgs args)
            {
                ((EventTrigger)sender).OnEventNameChanged((string)args.OldValue, (string)args.NewValue);
            }
        }

        /// <summary>
        /// Static class that owns the Triggers and Behaviors attached properties. Handles propagation of AssociatedObject change notifications.
        /// </summary>
        public static class Interaction
        {
            /// <summary>
            /// Gets or sets a value indicating whether to run as if in design mode.
            /// </summary>
            /// <value>
            /// 	<c>True</c> if [should run in design mode]; otherwise, <c>False</c>.
            /// </value>
            /// <remarks>Not to be used outside unit tests.</remarks>
            internal static bool ShouldRunInDesignMode
            {
                get;
                set;
            }

            /// <summary>
            /// This property is used as the internal backing store for the public Triggers attached property.
            /// </summary>
            /// <remarks>
            /// This property is not exposed publicly. This forces clients to use the GetTriggers and SetTriggers methods to access the
            /// collection, ensuring the collection exists and is set before it is used.
            /// </remarks>
            private static readonly DependencyProperty TriggersProperty = DependencyProperty.RegisterAttached("ShadowTriggers",
                                                                                                                    typeof(TriggerCollection),
                                                                                                                    typeof(Interaction),
                                                                                                                    new FrameworkPropertyMetadata(
                                                                                                                        new PropertyChangedCallback(OnTriggersChanged)));

            // Note that the parts of the xml document comments must be together in the source, even in the presense of #ifs
            /// <summary>
            /// This property is used as the internal backing store for the public Behaviors attached property.
            /// </summary>
            /// <remarks>
            /// This property is not exposed publicly. This forces clients to use the GetBehaviors and SetBehaviors methods to access the
            /// collection, ensuring the collection exists and is set before it is used.
            /// </remarks>
            private static readonly DependencyProperty BehaviorsProperty = DependencyProperty.RegisterAttached("ShadowBehaviors",
                                                                                                                            typeof(BehaviorCollection),
                                                                                                                            typeof(Interaction),
                                                                                                                            new FrameworkPropertyMetadata(
                                                                                                                                new PropertyChangedCallback(OnBehaviorsChanged)));


            /// <summary>
            /// Gets the TriggerCollection containing the triggers associated with the specified object.
            /// </summary>
            /// <param name="obj">The object from which to retrieve the triggers.</param>
            /// <returns>A TriggerCollection containing the triggers associated with the specified object.</returns>
            public static TriggerCollection GetTriggers(DependencyObject obj)
            {
                TriggerCollection triggerCollection = (TriggerCollection)obj.GetValue(Interaction.TriggersProperty);
                if (triggerCollection == null)
                {
                    triggerCollection = new TriggerCollection();
                    obj.SetValue(Interaction.TriggersProperty, triggerCollection);
                }
                return triggerCollection;
            }

            /// <summary>
            /// Gets the <see cref="BehaviorCollection"/> associated with a specified object.
            /// </summary>
            /// <param name="obj">The object from which to retrieve the <see cref="BehaviorCollection"/>.</param>
            /// <returns>A <see cref="BehaviorCollection"/> containing the behaviors associated with the specified object.</returns>
            public static BehaviorCollection GetBehaviors(DependencyObject obj)
            {
                BehaviorCollection behaviorCollection = (BehaviorCollection)obj.GetValue(Interaction.BehaviorsProperty);
                if (behaviorCollection == null)
                {
                    behaviorCollection = new BehaviorCollection();
                    obj.SetValue(Interaction.BehaviorsProperty, behaviorCollection);
                }
                return behaviorCollection;
            }

            /// <exception cref="InvalidOperationException">Cannot host the same BehaviorCollection on more than one object at a time.</exception>
            private static void OnBehaviorsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
            {
                BehaviorCollection oldCollection = (BehaviorCollection)args.OldValue;
                BehaviorCollection newCollection = (BehaviorCollection)args.NewValue;

                if (oldCollection != newCollection)
                {
                    if (oldCollection != null && ((IAttachedObject)oldCollection).AssociatedObject != null)
                    {
                        oldCollection.Detach();
                    }

                    if (newCollection != null && obj != null)
                    {
                        if (((IAttachedObject)newCollection).AssociatedObject != null)
                        {
                            throw new InvalidOperationException(EmbeddedStringTable.ExceptionStringTable["CannotHostBehaviorCollectionMultipleTimesExceptionMessage"]);
                        }

                        newCollection.Attach(obj);
                    }
                }
            }

            /// <exception cref="InvalidOperationException">Cannot host the same TriggerCollection on more than one object at a time.</exception>
            private static void OnTriggersChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
            {
                TriggerCollection oldCollection = args.OldValue as TriggerCollection;
                TriggerCollection newCollection = args.NewValue as TriggerCollection;

                if (oldCollection != newCollection)
                {
                    if (oldCollection != null && ((IAttachedObject)oldCollection).AssociatedObject != null)
                    {
                        oldCollection.Detach();
                    }

                    if (newCollection != null && obj != null)
                    {
                        if (((IAttachedObject)newCollection).AssociatedObject != null)
                        {
                            throw new InvalidOperationException(EmbeddedStringTable.ExceptionStringTable["CannotHostTriggerCollectionMultipleTimesExceptionMessage"]);
                        }

                        newCollection.Attach(obj);
                    }
                }
            }

            /// <summary>
            /// A helper function to take the place of FrameworkElement.IsLoaded, as this property is not available in Silverlight.
            /// </summary>
            /// <param name="element">The element of interest.</param>
            /// <returns>True if the element has been loaded; otherwise, False.</returns>
            internal static bool IsElementLoaded(FrameworkElement element)
            {
                return element.IsLoaded;
            }
        }

        /// <summary>
        /// Encapsulates state information and zero or more ICommands into an attachable object.
        /// </summary>
        /// <typeparam name="T">The type the <see cref="Behavior&lt;T&gt;"/> can be attached to.</typeparam>
        /// <remarks>
        ///		Behavior is the base class for providing attachable state and commands to an object.
        ///		The types the Behavior can be attached to can be controlled by the generic parameter.
        ///		Override OnAttached() and OnDetaching() methods to hook and unhook any necessary handlers
        ///		from the AssociatedObject.
        ///	</remarks>
        public abstract class Behavior<T> : Behavior where T : DependencyObject
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Behavior&lt;T&gt;"/> class.
            /// </summary>
            protected Behavior()
                : base(typeof(T))
            {
            }

            /// <summary>
            /// Gets the object to which this <see cref="Behavior&lt;T&gt;"/> is attached.
            /// </summary>
            protected new T AssociatedObject
            {
                get { return (T)base.AssociatedObject; }
            }
        }

        /// <summary>
        /// Encapsulates state information and zero or more ICommands into an attachable object.
        /// </summary>
        /// <remarks>This is an infrastructure class. Behavior authors should derive from Behavior&lt;T&gt; instead of from this class.</remarks>
        public abstract class Behavior :
            Animatable,
            IAttachedObject
        {
            private Type associatedType;
            private DependencyObject associatedObject;

            internal event EventHandler AssociatedObjectChanged;

            /// <summary>
            /// The type to which this behavior can be attached.
            /// </summary>
            protected Type AssociatedType
            {
                get
                {
                    this.ReadPreamble();
                    return this.associatedType;
                }
            }

            /// <summary>
            /// Gets the object to which this behavior is attached.
            /// </summary>
            protected DependencyObject AssociatedObject
            {
                get
                {
                    this.ReadPreamble();
                    return this.associatedObject;
                }
            }

            internal Behavior(Type associatedType)
            {
                this.associatedType = associatedType;
            }

            /// <summary>
            /// Called after the behavior is attached to an AssociatedObject.
            /// </summary>
            /// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
            protected virtual void OnAttached()
            {
            }

            /// <summary>
            /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
            /// </summary>
            /// <remarks>Override this to unhook functionality from the AssociatedObject.</remarks>
            protected virtual void OnDetaching()
            {
            }

            protected override Freezable CreateInstanceCore()
            {
                Type classType = this.GetType();
                return (Freezable)Activator.CreateInstance(classType);
            }

            private void OnAssociatedObjectChanged()
            {
                if (this.AssociatedObjectChanged != null)
                {
                    this.AssociatedObjectChanged(this, new EventArgs());
                }
            }

            #region IAttachedObject Members

            /// <summary>
            /// Gets the associated object.
            /// </summary>
            /// <value>The associated object.</value>
            DependencyObject IAttachedObject.AssociatedObject
            {
                get
                {
                    return this.AssociatedObject;
                }
            }

            /// <summary>
            /// Attaches to the specified object.
            /// </summary>
            /// <param name="dependencyObject">The object to attach to.</param>
            /// <exception cref="InvalidOperationException">The Behavior is already hosted on a different element.</exception>
            /// <exception cref="InvalidOperationException">dependencyObject does not satisfy the Behavior type constraint.</exception>
            public void Attach(DependencyObject dependencyObject)
            {
                if (dependencyObject != this.AssociatedObject)
                {
                    if (this.AssociatedObject != null)
                    {
                        throw new InvalidOperationException(EmbeddedStringTable.ExceptionStringTable["CannotHostBehaviorMultipleTimesExceptionMessage"]);
                    }

                    // todo jekelly: what do we do if dependencyObject is null?

                    // Ensure the type constraint is met
                    if (dependencyObject != null && !this.AssociatedType.IsAssignableFrom(dependencyObject.GetType()))
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                                                                            EmbeddedStringTable.ExceptionStringTable["TypeConstraintViolatedExceptionMessage"],
                                                                            this.GetType().Name,
                                                                            dependencyObject.GetType().Name,
                                                                            this.AssociatedType.Name));
                    }

                    this.WritePreamble();
                    this.associatedObject = dependencyObject;
                    this.WritePostscript();
                    this.OnAssociatedObjectChanged();

                    this.OnAttached();
                }
            }

            /// <summary>
            /// Detaches this instance from its associated object.
            /// </summary>
            public void Detach()
            {
                this.OnDetaching();
                this.WritePreamble();
                this.associatedObject = null;
                this.WritePostscript();
                this.OnAssociatedObjectChanged();
            }

            #endregion
        }

        /// <summary>
        /// Represents a collection of behaviors with a shared AssociatedObject and provides change notifications to its contents when that AssociatedObject changes.
        /// </summary>
        public sealed class BehaviorCollection : AttachableCollection<Behavior>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="BehaviorCollection"/> class.
            /// </summary>
            /// <remarks>Internal, because this should not be inherited outside this assembly.</remarks>
            internal BehaviorCollection()
            {
            }

            /// <summary>
            /// Called immediately after the collection is attached to an AssociatedObject.
            /// </summary>
            protected override void OnAttached()
            {
                foreach (Behavior behavior in this)
                {
                    behavior.Attach(this.AssociatedObject);
                }
            }

            /// <summary>
            /// Called when the collection is being detached from its AssociatedObject, but before it has actually occurred.
            /// </summary>
            protected override void OnDetaching()
            {
                foreach (Behavior behavior in this)
                {
                    behavior.Detach();
                }
            }

            /// <summary>
            /// Called when a new item is added to the collection.
            /// </summary>
            /// <param name="item">The new item.</param>
            internal override void ItemAdded(Behavior item)
            {
                if (this.AssociatedObject != null)
                {
                    item.Attach(this.AssociatedObject);
                }
            }

            /// <summary>
            /// Called when an item is removed from the collection.
            /// </summary>
            /// <param name="item">The removed item.</param>
            internal override void ItemRemoved(Behavior item)
            {
                if (((IAttachedObject)item).AssociatedObject != null)
                {
                    item.Detach();
                }
            }

            /// <summary>
            /// Creates a new instance of the BehaviorCollection.
            /// </summary>
            /// <returns>The new instance.</returns>
            protected override Freezable CreateInstanceCore()
            {
                return new BehaviorCollection();
            }
        }

        //<summary>
        /// Represents a collection of triggers with a shared AssociatedObject and provides change notifications to its contents when that AssociatedObject changes.
        /// </summary>
        public sealed class TriggerCollection : AttachableCollection<TriggerBase>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TriggerCollection"/> class.
            /// </summary>
            /// <remarks>Internal, because this should not be inherited outside this assembly.</remarks>
            internal TriggerCollection()
            {
            }

            /// <summary>
            /// Called immediately after the collection is attached to an AssociatedObject.
            /// </summary>
            protected override void OnAttached()
            {
                foreach (TriggerBase trigger in this)
                {
                    trigger.Attach(this.AssociatedObject);
                }
            }

            /// <summary>
            /// Called when the collection is being detached from its AssociatedObject, but before it has actually occurred.
            /// </summary>
            protected override void OnDetaching()
            {
                foreach (TriggerBase trigger in this)
                {
                    trigger.Detach();
                }
            }

            /// <summary>
            /// Called when a new item is added to the collection.
            /// </summary>
            /// <param name="item">The new item.</param>
            internal override void ItemAdded(TriggerBase item)
            {
                if (this.AssociatedObject != null)
                {
                    item.Attach(this.AssociatedObject);
                }
            }

            /// <summary>
            /// Called when an item is removed from the collection.
            /// </summary>
            /// <param name="item">The removed item.</param>
            internal override void ItemRemoved(TriggerBase item)
            {
                if (((IAttachedObject)item).AssociatedObject != null)
                {
                    item.Detach();
                }
            }

            /// <summary>
            /// Creates a new instance of the <see cref="TriggerCollection"/>.
            /// </summary>
            /// <returns>The new instance.</returns>
            protected override Freezable CreateInstanceCore()
            {
                return new TriggerCollection();
            }

        }

        /// <summary>
        /// Executes a specified ICommand when invoked.
        /// </summary>
        public sealed class InvokeCommandAction : TriggerAction<DependencyObject>
        {
            private string commandName;

            public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(InvokeCommandAction), null);
            public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(InvokeCommandAction), null);
            public static readonly DependencyProperty EventArgsConverterProperty = DependencyProperty.Register("EventArgsConverter", typeof(IValueConverter), typeof(InvokeCommandAction), new PropertyMetadata(null));
            public static readonly DependencyProperty EventArgsConverterParameterProperty = DependencyProperty.Register("EventArgsConverterParameter", typeof(object), typeof(InvokeCommandAction), new PropertyMetadata(null));
            public static readonly DependencyProperty EventArgsParameterPathProperty = DependencyProperty.Register("EventArgsParameterPath", typeof(string), typeof(InvokeCommandAction), new PropertyMetadata(null));

            /// <summary>
            /// Gets or sets the name of the command this action should invoke.
            /// </summary>
            /// <value>The name of the command this action should invoke.</value>
            /// <remarks>This property will be superseded by the Command property if both are set.</remarks>
            public string CommandName
            {
                get
                {
                    this.ReadPreamble();
                    return this.commandName;
                }
                set
                {
                    if (this.CommandName != value)
                    {
                        this.WritePreamble();
                        this.commandName = value;
                        this.WritePostscript();
                    }
                }
            }

            /// <summary>
            /// Gets or sets the command this action should invoke. This is a dependency property.
            /// </summary>
            /// <value>The command to execute.</value>
            /// <remarks>This property will take precedence over the CommandName property if both are set.</remarks>
            public ICommand Command
            {
                get { return (ICommand)this.GetValue(CommandProperty); }
                set { this.SetValue(CommandProperty, value); }
            }

            /// <summary>
            /// Gets or sets the command parameter. This is a dependency property.
            /// </summary>
            /// <value>The command parameter.</value>
            /// <remarks>This is the value passed to ICommand.CanExecute and ICommand.Execute.</remarks>
            public object CommandParameter
            {
                get { return this.GetValue(InvokeCommandAction.CommandParameterProperty); }
                set { this.SetValue(InvokeCommandAction.CommandParameterProperty, value); }
            }

            /// <summary>
            /// Gets or sets the IValueConverter that is used to convert the EventArgs passed to the Command as a parameter.
            /// </summary>
            /// <remarks>If the <see cref="Command"/> or <see cref="EventArgsParameterPath"/> properties are set, this property is ignored.</remarks>
            public IValueConverter EventArgsConverter
            {
                get { return (IValueConverter)GetValue(EventArgsConverterProperty); }
                set { SetValue(EventArgsConverterProperty, value); }
            }

            /// <summary>
            /// Gets or sets the parameter that is passed to the EventArgsConverter.
            /// </summary>
            public object EventArgsConverterParameter
            {
                get { return (object)GetValue(EventArgsConverterParameterProperty); }
                set { SetValue(EventArgsConverterParameterProperty, value); }
            }

            /// <summary>
            /// Gets or sets the parameter path used to extract a value from an <see cref= "EventArgs" /> property to pass to the Command as a parameter.
            /// </summary>
            /// <remarks>If the <see cref="Command"/> propert is set, this property is ignored.</remarks>
            public string EventArgsParameterPath
            {
                get { return (string)GetValue(EventArgsParameterPathProperty); }
                set { SetValue(EventArgsParameterPathProperty, value); }
            }

            /// <summary>
            /// Specifies whether the EventArgs of the event that triggered this action should be passed to the Command as a parameter.
            /// </summary>
            /// <remarks>If the <see cref="Command"/>, <see cref="EventArgsParameterPath"/>, or <see cref="EventArgsConverter"/> properties are set, this property is ignored.</remarks>
            public bool PassEventArgsToCommand { get; set; }

            /// <summary>
            /// Invokes the action.
            /// </summary>
            /// <param name="parameter">The parameter to the action. If the action does not require a parameter, the parameter may be set to a null reference.</param>
            protected override void Invoke(object parameter)
            {
                if (this.AssociatedObject != null)
                {
                    ICommand command = this.ResolveCommand();

                    if (command != null)
                    {
                        object commandParameter = this.CommandParameter;

                        //if no CommandParameter has been provided, let's check the EventArgsParameterPath
                        if (commandParameter == null && !string.IsNullOrWhiteSpace(this.EventArgsParameterPath))
                        {
                            commandParameter = GetEventArgsPropertyPathValue(parameter);
                        }

                        //next let's see if an event args converter has been supplied
                        if (commandParameter == null && this.EventArgsConverter != null)
                        {
                            commandParameter = this.EventArgsConverter.Convert(parameter, typeof(object), EventArgsConverterParameter, CultureInfo.CurrentCulture);
                        }

                        //last resort, let see if they want to force the event args to be passed as a parameter
                        if (commandParameter == null && this.PassEventArgsToCommand)
                        {
                            commandParameter = parameter;
                        }

                        if (command.CanExecute(commandParameter))
                        {
                            command.Execute(commandParameter);
                        }
                    }
                    else
                    {
                        Debug.WriteLine(EmbeddedStringTable.ExceptionStringTable["CommandDoesNotExistOnBehaviorWarningMessage"], this.CommandName, this.AssociatedObject.GetType().Name);
                    }
                }
            }

            private object GetEventArgsPropertyPathValue(object parameter)
            {
                object commandParameter;
                object propertyValue = parameter;
                string[] propertyPathParts = EventArgsParameterPath.Split('.');
                foreach (string propertyPathPart in propertyPathParts)
                {
                    PropertyInfo propInfo = propertyValue.GetType().GetProperty(propertyPathPart);
                    propertyValue = propInfo.GetValue(propertyValue, null);
                }

                commandParameter = propertyValue;
                return commandParameter;
            }

            private ICommand ResolveCommand()
            {
                ICommand command = null;

                if (this.Command != null)
                {
                    command = this.Command;
                }
                else if (this.AssociatedObject != null)
                {
                    // todo jekelly 06/09/08: we could potentially cache some or all of this information if needed, updating when AssociatedObject changes
                    Type associatedObjectType = this.AssociatedObject.GetType();
                    PropertyInfo[] typeProperties = associatedObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    foreach (PropertyInfo propertyInfo in typeProperties)
                    {
                        if (typeof(ICommand).IsAssignableFrom(propertyInfo.PropertyType))
                        {
                            if (string.Equals(propertyInfo.Name, this.CommandName, StringComparison.Ordinal))
                            {
                                command = (ICommand)propertyInfo.GetValue(this.AssociatedObject, null);
                            }
                        }
                    }
                }

                return command;
            }
        }

    }

    #endregion

    internal static class EmbeddedStringTable {
        public static Dictionary<string, string> ExceptionStringTable =
              new Dictionary<string, string>()
              {
           {"GoToStateActionTargetHasNoStateGroups","Target {0} does not define any VisualStateGroups." },
            {"UnsupportedRemoveTargetExceptionMessage","The target of the RemoveElementAction is not supported." },
            {"ChangePropertyActionCannotFindPropertyNameExceptionMessage","Cannot find a property named '{0}' on type '{1}'" },
            {"ChangePropertyActionCannotSetValueExceptionMessage","Cannot assign value of type '{0}' to property '{1}' of type '{2}'. The '{1}' property can be assigned only values of type '{2}'." },
            {"ChangePropertyActionPropertyIsReadOnlyExceptionMessage","Property '{0}' defined by type '{1}' does not expose a set method and therefore cannot be modified." },
            {"ChangePropertyActionCannotIncrementAnimatedPropertyChangeExceptionMessage","The Increment property cannot be set to True if the Duration property is set." },

            {"CannotHostTriggerMultipleTimesExceptionMessage","An instance of a trigger cannot be attached to more than one object at a time."},
            {"TypeConstraintViolatedExceptionMessage","Cannot attach type '{0}' to type '{1}'. Instances of type '{0}' can only be attached to objects of type '{2}'." },
            {"RetargetedTypeConstraintViolatedExceptionMessage","An object of type '{0}' cannot have a {3} property of type '{1}'. Instances of type '{0}' can have only a {3} property of type '{2}'."},
            {"EventTriggerCannotFindEventNameExceptionMessage","Cannot find an event named '{0}' on type '{1}'."},
            {"EventTriggerBaseInvalidEventExceptionMessage","The event '{0}' on type '{1}' has an incompatible signature. Make sure the event is public and satisfies the EventHandler delegate."},
            {"CannotHostBehaviorMultipleTimesExceptionMessage","An instance of a Behavior cannot be attached to more than one object at a time."},
            {"DuplicateItemInCollectionExceptionMessage","Cannot add the same instance of '{0}' to a '{1}' more than once."},
                  {"UnableToResolveTargetNameWarningMessage","Unable to resolve TargetName '{0}'." },
                  {"CannotHostTriggerActionMultipleTimesExceptionMessage", "Cannot host an instance of a TriggerAction in multiple TriggerCollections simultaneously. Remove it from one TriggerCollection before adding it to another."},
                  {"DefaultTriggerAttributeInvalidTriggerTypeSpecifiedExceptionMessage","'{0}' is not a valid type for the TriggerType parameter. Make sure '{0}' derives from TriggerBase." },
                  {"CannotHostBehaviorCollectionMultipleTimesExceptionMessage","Cannot set the same BehaviorCollection on multiple objects." },
                  {"CannotHostTriggerCollectionMultipleTimesExceptionMessage","Cannot set the same TriggerCollection on multiple objects." },
                  { "CommandDoesNotExistOnBehaviorWarningMessage","The command '{0}' does not exist or is not publicly exposed on {1}."}

              };
    }
}

