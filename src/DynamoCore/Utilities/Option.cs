using System;

namespace Dynamo.Utilities
{
    public interface IEither<TLeft, TRight>
    {
        IEither<TLeft, U> BindRight<U>(Func<TRight, IEither<TLeft, U>> selector);
        IEither<U, TRight> BindLeft<U>(Func<TLeft, IEither<U, TRight>> selector);
        IEither<U, V> Select<U, V>(Func<TLeft, U> leftSelector, Func<TRight, V> rightSelector);
        U Match<U>(Func<TLeft, U> leftCase, Func<TRight, U> rightCase);
        TLeft LeftValue { get; }
        TRight RightValue { get; }
    }

    public static class Either
    {
        public static IEither<TLeft, TRight> Left<TLeft, TRight>(TLeft value)
        {
            return new _Left<TLeft, TRight>(value);
        }

        public static IEither<TLeft, TRight> Right<TLeft, TRight>(TRight value)
        {
            return new _Right<TLeft, TRight>(value);
        }

        public static IEither<U, TRight> SelectLeft<TLeft, U, TRight>(
            this IEither<TLeft, TRight> either, Func<TLeft, U> selector)
        {
            return either.Select(selector, right => right);
        }

        public static IEither<TLeft, U> SelectLeft<TLeft, TRight, U>(
            this IEither<TLeft, TRight> either, Func<TRight, U> selector)
        {
            return either.Select(left => left, selector);
        }

        private class _Left<T, T1> : IEither<T, T1>
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
        }

        private class _Right<T, T1> : IEither<T, T1>
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

            public T LeftValue
            {
                get
                {
                    throw new InvalidOperationException("Cannot get RightValue from Left Either type.");
                }
            }
        }
    }


    public interface IOption<out T>
    {
        IOption<U> Bind<U>(Func<T, IOption<U>> selector);
        U Match<U>(Func<T, U> someCase, Func<U> noneCase);
        T Value { get; }
    }

    public static class Option
    {
        public static IOption<T> Some<T>(T value)
        {
            return new _Some<T>(value);
        }

        public static IOption<T> None<T>()
        {
            return _None<T>.Instance;
        }

        public static IOption<U> Select<T, U>(this IOption<T> option, Func<T, U> selector)
        {
            return option.Bind(x => Some(selector(x)));
        }

        private class _Some<T> : IOption<T>
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
        }

        private class _None<T> : IOption<T>
        {
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
        }
    }
}
