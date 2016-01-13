using System;
using System.Collections.Generic;

namespace Dynamo.Utilities
{
    /// <summary>
    ///     Object containing an instance of one of two potential types, labeled "Left" or "Right".
    /// </summary>
    /// <typeparam name="TLeft">Type of a potential "Left" value.</typeparam>
    /// <typeparam name="TRight">Type of a potential "Right" value.</typeparam>
    public interface IEither<TLeft, TRight>
    {
        /// <summary>
        ///     If the IEither(TLeft, TRight) instance contains a Right value, project the Right value into
        ///     a new IEither(TLeft, TNewRight). If the instance contains a Left value, just return the instance.
        /// </summary>
        /// <typeparam name="TNewRight">The new type of a potential Right value.</typeparam>
        /// <param name="selector">Function used to project a Right value.</param>
        IEither<TLeft, TNewRight> BindRight<TNewRight>(Func<TRight, IEither<TLeft, TNewRight>> selector);
        
        /// <summary>
        ///     If the IEither(TLeft, TRight) instance contains a Left value, project the Left value into
        ///     a new IEither(TNewLeft, TRight). If the instance contains a Right value, just return the instance.
        /// </summary>
        /// <typeparam name="TNewLeft">The new type of a potential Left value.</typeparam>
        /// <param name="selector">Function used to project a Left value.</param>
        IEither<TNewLeft, TRight> BindLeft<TNewLeft>(Func<TLeft, IEither<TNewLeft, TRight>> selector);
        
        /// <summary>
        ///     Create a new IEither(TNewLeft, TNewRight) instance by passing the value the a conversion
        ///     function. Which function is selected depends on whether it is a Right or Left value.
        /// </summary>
        /// <typeparam name="TNewLeft">The new type of a potential Left value.</typeparam>
        /// <typeparam name="TNewRight">The new type of a potential Right value.</typeparam>
        /// <param name="leftSelector">Function used to convert a Left value.</param>
        /// <param name="rightSelector">Function used to convert a Right value.</param>
        IEither<TNewLeft, TNewRight> Select<TNewLeft, TNewRight>(
            Func<TLeft, TNewLeft> leftSelector, Func<TRight, TNewRight> rightSelector);
        
        /// <summary>
        ///     Produces a new value of type T using one of the given functions, based on whether the
        ///     IEither(TLeft, TRight) contains a Left or Right value.
        /// </summary>
        /// <typeparam name="T">Type of objects produced by either match case.</typeparam>
        /// <param name="leftCase">Function used to create a result from a Left value.</param>
        /// <param name="rightCase">Function used to create a result from a Right value.</param>
        T Match<T>(Func<TLeft, T> leftCase, Func<TRight, T> rightCase);
        
        /// <summary>
        ///     Attempts to get a Left value.
        /// </summary>
        TLeft LeftValue { get; }
        
        /// <summary>
        ///     Attempts to get a Right value.
        /// </summary>
        TRight RightValue { get; }
        
        /// <summary>
        ///     Specifies if this instance contains a Left value. If true, it does. If false,
        ///     it contains a Right value.
        /// </summary>
        bool IsLeft { get; }
    }

    /// <summary>Utility methods for working with Either instances.</summary>
    public static class Either
    {
        /// <summary>
        ///     Creates a new IEither(TLeft, TRight) instance containing a Left value.
        /// </summary>
        /// <typeparam name="TLeft">Type of the Left value.</typeparam>
        /// <typeparam name="TRight">Type of a potential Right value.</typeparam>
        /// <param name="value">Left value to be stored in the new IEither(TLeft, TRight) instance.</param>
        public static IEither<TLeft, TRight> Left<TLeft, TRight>(TLeft value)
        {
            return new _Left<TLeft, TRight>(value);
        }

        /// <summary>
        ///     Creates a new IEither(TLeft, TRight) instance containing a Right value.
        /// </summary>
        /// <typeparam name="TLeft">Type of a potential Left value.</typeparam>
        /// <typeparam name="TRight">Type of the Right value.</typeparam>
        /// <param name="value">Right value to be stored in the new IEither(TLeft, TRight) instance.</param>
        public static IEither<TLeft, TRight> Right<TLeft, TRight>(TRight value)
        {
            return new _Right<TLeft, TRight>(value);
        }
        
        /// <summary>
        ///     Return an IEither(TNewLeft, TRight) instance by either passing the contained Left value
        ///     to a conversion function, or propagating the Right value.
        /// </summary>
        /// <typeparam name="TOldLeft">The original type of a potential Left value.</typeparam>
        /// <typeparam name="TNewLeft">The new type of a potential Left value.</typeparam>
        /// <typeparam name="TRight">The type of a potential Right value.</typeparam>
        /// <param name="either">An IEither(TOldLeft, TRight) instance.</param>
        /// <param name="selector">Function used to convert a Left value.</param>
        public static IEither<TNewLeft, TRight> SelectLeft<TOldLeft, TNewLeft, TRight>(
            this IEither<TOldLeft, TRight> either, Func<TOldLeft, TNewLeft> selector)
        {
            return either.Select(selector, right => right);
        }

        /// <summary>
        ///     Return an IEither(TLeft, TNewRight) instance by either passing the contained Right value
        ///     to a conversion function, or propagating the Left value.
        /// </summary>
        /// <typeparam name="TLeft">The type of a potential Left value.</typeparam>
        /// <typeparam name="TOldRight">The original type of a potential Right value.</typeparam>
        /// <typeparam name="TNewRight">The new type of a potential Right value.</typeparam>
        /// <param name="either">An IEither(TLeft, TOldRight) instance.</param>
        /// <param name="selector">Function used to convert a Right value.</param>
        public static IEither<TLeft, TNewRight> SelectRight<TLeft, TOldRight, TNewRight>(
            this IEither<TLeft, TOldRight> either, Func<TOldRight, TNewRight> selector)
        {
            return either.Select(left => left, selector);
        }

        /// <summary>
        ///     Performs an given Action based on whether the IEither(TLeft, TRight) contains a Left or
        ///     Right value.
        /// </summary>
        /// <typeparam name="TLeft">The type of a potential Left value.</typeparam>
        /// <typeparam name="TRight">The type of a potential Right value.</typeparam>
        /// <param name="either">An IEither(TLeft, TRight) instance.</param>
        /// <param name="leftAction">Action used for a Left value.</param>
        /// <param name="rightAction">Action used for a Right value.</param>
        public static void Match<TLeft, TRight>(
            this IEither<TLeft, TRight> either, Action<TLeft> leftAction, Action<TRight> rightAction)
        {
            either.Match<object>(
                left =>
                {
                    leftAction(left);
                    return null; // We're not using the result of the match, so just return null.
                },
                right =>
                {
                    rightAction(right);
                    return null;
                });
        }

        // Left values
        private sealed class _Left<T, T1> : IEither<T, T1>
        {
            private readonly T value;

            public _Left(T value)
            {
                this.value = value;
            }

            public IEither<T, U> BindRight<U>(Func<T1, IEither<T, U>> selector)
            {
                return new _Left<T, U>(value);
            }

            public IEither<U, T1> BindLeft<U>(Func<T, IEither<U, T1>> selector)
            {
                return selector(value);
            }

            public IEither<U, V> Select<U, V>(Func<T, U> leftSelector, Func<T1, V> rightSelector)
            {
                return new _Left<U, V>(leftSelector(value));
            }

            public U Match<U>(Func<T, U> leftCase, Func<T1, U> rightCase)
            {
                return leftCase(value);
            }

            public T LeftValue
            {
                get { return value; }
            }

            public T1 RightValue
            {
                get
                {
                    throw new InvalidOperationException("Cannot get LeftValue from Right Either type.");
                }
            }

            public bool IsLeft { get { return true; } }
        }

        // Right values
        private sealed class _Right<T, T1> : IEither<T, T1>
        {
            private readonly T1 value;

            public _Right(T1 value)
            {
                this.value = value;
            }

            public IEither<T, U> BindRight<U>(Func<T1, IEither<T, U>> selector)
            {
                return selector(value);
            }

            public IEither<U, T1> BindLeft<U>(Func<T, IEither<U, T1>> selector)
            {
                return new _Right<U, T1>(value);
            }

            public IEither<U, V> Select<U, V>(Func<T, U> leftSelector, Func<T1, V> rightSelector)
            {
                return new _Right<U, V>(rightSelector(value));
            }

            public U Match<U>(Func<T, U> leftCase, Func<T1, U> rightCase)
            {
                return rightCase(value);
            }

            public T1 RightValue
            {
                get { return value; }
            }

            public bool IsLeft
            {
                get { return false; }
            }

            public T LeftValue
            {
                get
                {
                    throw new InvalidOperationException("Cannot get RightValue from Left Either type.");
                }
            }
        }
    }

    /// <summary>
    ///     Object that either contains a single value, or is empty.
    /// </summary>
    /// <typeparam name="T">Type of value potentially contained within.</typeparam>
    public interface IOption<out T>
    {
        /// <summary>
        ///     If the IOption(T) instance contains a value, project the value into a new IOption(U)
        ///     instance using the given function. Otherwise, propagate the empty option.
        /// </summary>
        /// <typeparam name="U">New type of value potentially contained within.</typeparam>
        /// <param name="selector">Function used to project a value.</param>
        IOption<U> Bind<U>(Func<T, IOption<U>> selector);
        
        /// <summary>
        ///     Produces a new value of type U using one of the given functions, based on whether the
        ///     IOption(T) contains a value or not.
        /// </summary>
        /// <typeparam name="U">Type of object produced by either match case.</typeparam>
        /// <param name="someCase">Function used to create a result from a value.</param>
        /// <param name="noneCase">Function used to create a result from no value.</param>
        U Match<U>(Func<T, U> someCase, Func<U> noneCase);
        
        /// <summary>
        ///     Attempts to get a contained value.
        /// </summary>
        T Value { get; }
    }

    /// <summary>Utility method for working with Option instances.</summary>
    public static class Option
    {
        /// <summary>
        ///     Creates a new IOption(T) instance containing a value.
        /// </summary>
        /// <typeparam name="T">Type of the value contained in the new Option.</typeparam>
        /// <param name="value">Value to be stored in the new IOption(T) instance.</param>
        public static IOption<T> Some<T>(T value)
        {
            return new _Some<T>(value);
        }

        /// <summary>
        ///     Creates an empty Option.
        /// </summary>
        /// <typeparam name="T">Type of the value potentially contained.</typeparam>
        public static IOption<T> None<T>()
        {
            return _None<T>.Instance;
        }

        /// <summary>
        ///     Creates a new IOption(U) from an IOption(T) by converting the potentially contained
        ///     value, using a given conversion function.
        /// </summary>
        /// <typeparam name="T">Type of the value originally contained.</typeparam>
        /// <typeparam name="U">Type of the new value potentially contained.</typeparam>
        /// <param name="option">An option to Select over.</param>
        /// <param name="selector">A function used to convert the potential value.</param>
        public static IOption<U> Select<T, U>(this IOption<T> option, Func<T, U> selector)
        {
            return option.Bind(x => Some(selector(x)));
        }

        /// <summary>
        ///     Determines if the given Option contains a value or not.
        /// </summary>
        /// <typeparam name="T">Type of the value potentially contained.</typeparam>
        /// <param name="option">Option instance to check for a value.</param>
        public static bool HasValue<T>(this IOption<T> option)
        {
            return option is _Some<T>;
        }
        
        // Non-empty options.
        private class _Some<T> : IOption<T>, IEquatable<_Some<T>>
        {
            private readonly T value;

            public _Some(T value)
            {
                this.value = value;
            }

            public IOption<U> Bind<U>(Func<T, IOption<U>> selector)
            {
                return selector(value);
            }

            public U Match<U>(Func<T, U> someCase, Func<U> noneCase)
            {
                return someCase(value);
            }

            public T Value
            {
                get { return value; }
            }

            public bool Equals(_Some<T> other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return EqualityComparer<T>.Default.Equals(value, other.value);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((_Some<T>)obj);
            }

            public override int GetHashCode()
            {
                return EqualityComparer<T>.Default.GetHashCode(value);
            }

            public static bool operator ==(_Some<T> left, _Some<T> right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(_Some<T> left, _Some<T> right)
            {
                return !Equals(left, right);
            }
        }
        
        // Empty options.
        private class _None<T> : IOption<T>, IEquatable<_None<T>>
        {
            public static bool operator ==(_None<T> left, _None<T> right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(_None<T> left, _None<T> right)
            {
                return !Equals(left, right);
            }

            public static readonly IOption<T> Instance = new _None<T>();

            public IOption<U> Bind<U>(Func<T, IOption<U>> selector)
            {
                return _None<U>.Instance;
            }

            public U Match<U>(Func<T, U> someCase, Func<U> noneCase)
            {
                return noneCase();
            }

            public T Value
            {
                get { throw new InvalidOperationException("None type has no value."); }
            }

            public bool Equals(_None<T> other)
            {
                return this == other;
            }
        }
    }
}
