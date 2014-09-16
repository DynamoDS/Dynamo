using System;

namespace Dynamo.Utilities
{
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
            return new Some<T>(value);
        }

        public static IOption<T> None<T>()
        {
            return Utilities.None<T>.Instance;
        }

        public static IOption<U> Select<T, U>(this IOption<T> option, Func<T, U> selector)
        {
            return option.Bind(x => Some(selector(x)));
        }
    }

    class Some<T> : IOption<T>
    {
        private readonly T value;

        public Some(T value)
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

        public T Value { get { return value; } }
    }

    class None<T> : IOption<T>
    {
        public static IOption<T> Instance = new None<T>();
        
        public IOption<U> Bind<U>(Func<T, IOption<U>> selector)
        {
            return None<U>.Instance;
        }

        public U Match<U>(Func<T, U> someCase, Func<U> noneCase)
        {
            return noneCase();
        }

        public T Value
        {
            get
            {
                throw new InvalidOperationException("None type has no value.");
            }
        }
    }
}
