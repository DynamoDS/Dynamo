using System;
using System.Collections.Generic;
using System.Globalization;

using Autodesk.DesignScript.Runtime;

namespace DSCore
{
    /// <summary>
    ///     Object representing a specific Date and Time.
    /// </summary>
    public static class DateTime
    {
        /// <summary>
        ///     The earliest date and time that can be represented.
        /// </summary>
        public static System.DateTime MinValue
        {
            get { return System.DateTime.MinValue; }
        }

        /// <summary>
        ///     The latest date and time that can be represented.
        /// </summary>
        public static System.DateTime MaxValue
        {
            get { return System.DateTime.MaxValue; }
        }

        /// <summary>
        ///     The current system date and time.
        /// </summary>
        public static System.DateTime Now
        {
            get { return System.DateTime.Now; }
        }

        /// <summary>
        ///     The current system date, at midnight.
        /// </summary>
        public static System.DateTime Today
        {
            get { return System.DateTime.Today; }
        }

        /// <summary>
        ///     Creates a new DateTime at an exact date.
        /// </summary>
        /// <param name="year">Exact year (1-9999)</param>
        /// <param name="month">Exact month (1-12)</param>
        /// <param name="day">Exact day (1-[days in month])</param>
        public static System.DateTime ByDate(int year, int month, int day)
        {
            return new System.DateTime(year, month, day);
        }

        /// <summary>
        ///     Creates a new DateTime at an exact date and time.
        /// </summary>
        /// <param name="year">Exact year (1-9999)</param>
        /// <param name="month">Exact month (1-12)</param>
        /// <param name="day">Exact day (1-[days in month])</param>
        /// <param name="hour">Exact hour (0-23)</param>
        /// <param name="minute">Exact minute (0-59)</param>
        /// <param name="second">Exact second (0-59)</param>
        /// <param name="millisecond">Exact millisecond (0-999)</param>
        public static System.DateTime ByDateAndTime(
            int year, int month, int day, int hour=0, int minute=0, int second=0, int millisecond=0)
        {
            return new System.DateTime(year, month, day, hour, minute, second, millisecond);
        }

        /// <summary>
        ///     Subtracts a TimeSpan from a DateTime, yielding a new DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="timeSpan">Amount of time to subtract.</param>
        public static System.DateTime Subtract(System.DateTime dateTime, System.TimeSpan timeSpan)
        {
            return dateTime.Add(-timeSpan);
        }

        /// <summary>
        ///     Takes the difference between two DateTimes, yielding a TimeSpan.
        /// </summary>
        /// <param name="dateTime1">Starting DateTime.</param>
        /// <param name="dateTime2">Ending DateTime.</param>
        /// <returns></returns>
        public static System.TimeSpan Substract(
            System.DateTime dateTime1, System.DateTime dateTime2)
        {
            return TimeSpan.ByDateDifference(dateTime1, dateTime2);
        }

        /// <summary>
        ///     Subtracts an inexact number of days from a DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="days">Amount of time to subtract, represented as days.</param>
        public static System.DateTime SubtractDays(System.DateTime dateTime, double days)
        {
            return dateTime.AddDays(-days);
        }

        /// <summary>
        ///     Subtracts an inexact number of hours from a DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="hrs">Amount of time to subtract, represented as hours.</param>
        public static System.DateTime SubtractHours(System.DateTime dateTime, double hrs)
        {
            return dateTime.AddHours(-hrs);
        }

        /// <summary>
        ///     Subtracts an inexact number of milliseconds from a DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="ms">Amount of time to subtract, represented as milliseconds.</param>
        public static System.DateTime SubtractMilliseconds(System.DateTime dateTime, double ms)
        {
            return dateTime.AddMilliseconds(-ms);
        }

        /// <summary>
        ///     Subtracts an inexact number of minutes from a DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="mins">Amount of time to subtract, represented as minutes.</param>
        public static System.DateTime SubtractMinutes(System.DateTime dateTime, double mins)
        {
            return dateTime.AddMinutes(-mins);
        }

        /// <summary>
        ///     Subtracts an exact number of months from a DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="months">Amount of months to subtract.</param>
        public static System.DateTime SubtractMonths(System.DateTime dateTime, int months)
        {
            return dateTime.AddMonths(-months);
        }

        /// <summary>
        ///     Subtracts an inexact number of seconds from a DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="secs">Amount of time to subtract, represented as seconds.</param>
        public static System.DateTime SubtractSeconds(System.DateTime dateTime, double secs)
        {
            return dateTime.AddSeconds(-secs);
        }

        /// <summary>
        ///     Subtracts an exact number of years from a DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="yrs">Amount of years to subtract.</param>
        public static System.DateTime SubtractYears(System.DateTime dateTime, int yrs)
        {
            return dateTime.AddYears(-yrs);
        }

        /// <summary>
        ///     Adds a TimeSpan to a DateTime, yielding a new DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="timeSpan">Amount of time to add.</param>
        public static System.DateTime Add(System.DateTime dateTime, System.TimeSpan timeSpan)
        {
            return dateTime.Add(timeSpan);
        }

        /// <summary>
        ///     Adds an inexact number of days to a DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="days">Amount of time to add, represented as days.</param>
        public static System.DateTime AddDays(System.DateTime dateTime, double days)
        {
            return dateTime.AddDays(days);
        }

        /// <summary>
        ///     Adds an inexact number of hours to a DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="hrs">Amount of time to add, represented as hours.</param>
        public static System.DateTime AddHours(System.DateTime dateTime, double hrs)
        {
            return dateTime.AddHours(hrs);
        }

        /// <summary>
        ///     Adds an inexact number of milliseconds to a DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="ms">Amount of time to add, represented as milliseconds.</param>
        public static System.DateTime AddMilliseconds(System.DateTime dateTime, double ms)
        {
            return dateTime.AddMilliseconds(ms);
        }

        /// <summary>
        ///     Adds an inexact number of minutes to a DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="mins">Amount of time to add, represented as minutes.</param>
        public static System.DateTime AddMinutes(System.DateTime dateTime, double mins)
        {
            return dateTime.AddMinutes(mins);
        }

        /// <summary>
        ///     Adds an exact number of months to a DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="months">Amount of months to add.</param>
        public static System.DateTime AddMonths(System.DateTime dateTime, int months)
        {
            return dateTime.AddMonths(months);
        }

        /// <summary>
        ///     Adds an inexact number of secons to a DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="secs">Amount of time to add, represented as seconds.</param>
        public static System.DateTime AddSeconds(System.DateTime dateTime, double secs)
        {
            return dateTime.AddSeconds(secs);
        }

        /// <summary>
        ///     Adds an exact number of years to a DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="yrs">Amount of years to add.</param>
        public static System.DateTime AddYears(System.DateTime dateTime, int yrs)
        {
            return dateTime.AddYears(yrs);
        }

        /// <summary>
        ///     Calculates how many days are in the given month of the given year.
        /// </summary>
        /// <param name="year">Exact year (1-9999)</param>
        /// <param name="month">Exact month (1-12)</param>
        public static int DaysInMonth(int year, int month)
        {
            return System.DateTime.DaysInMonth(year, month);
        }

        /// <summary>
        ///     Determines if it is Daylight Savings Time at the given DateTime.
        /// </summary>
        /// <param name="dateTime">A DateTime.</param>
        public static bool IsDaylightSavingsTime(System.DateTime dateTime)
        {
            return dateTime.IsDaylightSavingTime();
        }

        /// <summary>
        ///     Determines if the given year is a leap year.
        /// </summary>
        /// <param name="year">Exact year (1-9999)</param>
        public static bool IsLeapYear(int year)
        {
            return System.DateTime.IsLeapYear(year);
        }

        /// <summary>
        ///     Attempts to parse a DateTime from a string.
        /// </summary>
        /// <param name="str">String representation of a DateTime.</param>
        public static System.DateTime FromString(string str)
        {
            return System.DateTime.Parse(str, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Extracts only the date from a DateTime. Time components are set to 0.
        /// </summary>
        /// <param name="dateTime">A DateTime.</param>
        public static System.DateTime Date(System.DateTime dateTime)
        {
            return dateTime.Date;
        }

        /// <summary>
        ///     Extracts the individual components of a DateTime.
        /// </summary>
        /// <param name="dateTime">A DateTime.</param>
        [MultiReturn("year", "month", "day", "hour", "minute", "second", "millisecond")]
        public static Dictionary<string, int> Components(System.DateTime dateTime)
        {
            return new Dictionary<string, int>
            {
                { "year", dateTime.Year },
                { "month", dateTime.Month },
                { "day", dateTime.Day },
                { "hour", dateTime.Hour },
                { "minute", dateTime.Minute },
                { "second", dateTime.Second },
                { "millisecond", dateTime.Millisecond },
            };
        }

        /// <summary>
        ///     Gets the Day of the Week from a given DateTime.
        /// </summary>
        /// <param name="dateTime">A DateTime.</param>
        public static DayOfWeek DayOfWeek(System.DateTime dateTime)
        {
            switch (dateTime.DayOfWeek)
            {
                case System.DayOfWeek.Sunday:
                    return DSCore.DayOfWeek.Sunday;
                case System.DayOfWeek.Monday:
                    return DSCore.DayOfWeek.Monday;
                case System.DayOfWeek.Tuesday:
                    return DSCore.DayOfWeek.Tuesday;
                case System.DayOfWeek.Wednesday:
                    return DSCore.DayOfWeek.Wednesday;
                case System.DayOfWeek.Thursday:
                    return DSCore.DayOfWeek.Thursday;
                case System.DayOfWeek.Friday:
                    return DSCore.DayOfWeek.Friday;
                case System.DayOfWeek.Saturday:
                    return DSCore.DayOfWeek.Saturday;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static int DayOfYear(System.DateTime dateTime)
        {
            return dateTime.DayOfYear;
        }
    }

    /// <summary>
    ///     Days of the Week
    /// </summary>
    public enum DayOfWeek
    {
        Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday
    }

    /// <summary>
    ///     Object representing an elapsed period of time, with no specific start or end date.
    /// </summary>
    public static class TimeSpan
    {
        /// <summary>
        ///     Yields a new TimeSpan calculated from the time difference between two DateTimes.
        /// </summary>
        /// <param name="date1">Starting DateTime.</param>
        /// <param name="date2">Ending DateTime.</param>
        public static System.TimeSpan ByDateDifference(System.DateTime date1, System.DateTime date2)
        {
            return date1.Subtract(date2);
        }

        /// <summary>
        ///     Yields a new TimeSpan representing the amount of time passed since midnight of the
        ///     given DateTime.
        /// </summary>
        /// <param name="dateTime">A DateTime.</param>
        public static System.TimeSpan TimeOfDay(System.DateTime dateTime)
        {
            return dateTime.TimeOfDay;
        }

        /// <summary>
        ///     A TimeSpan representing an elapsed time of Zero.
        /// </summary>
        public static System.TimeSpan Zero { get { return System.TimeSpan.Zero; } }

        /// <summary>
        ///     The largest TimeSpan that can be represented.
        /// </summary>
        public static System.TimeSpan MaxValue { get { return System.TimeSpan.MaxValue; } }

        /// <summary>
        ///     The smallest TimeSpan that can be represented.
        /// </summary>
        public static System.TimeSpan MinValue { get { return System.TimeSpan.MaxValue; } }

        /// <summary>
        ///     Creates a new TimeSpan from an exact span of time.
        /// </summary>
        /// <param name="days">Days spanned.</param>
        /// <param name="hours">Hours spanned.</param>
        /// <param name="minutes">Minutes spanned.</param>
        /// <param name="seconds">Seconds spanned.</param>
        /// <param name="milliseconds">Milliseconds spanned.</param>
        public static System.TimeSpan Create(
            int days=0, int hours=0, int minutes=0, int seconds=0, int milliseconds=0)
        {
            return new System.TimeSpan(days, hours, minutes, seconds, milliseconds);
        }

        /// <summary>
        ///     Adds two TimeSpans.
        /// </summary>
        /// <param name="timeSpan1">A TimeSpan.</param>
        /// <param name="timeSpan2">A TimeSpan.</param>
        public static System.TimeSpan Add(System.TimeSpan timeSpan1, System.TimeSpan timeSpan2)
        {
            return timeSpan1.Add(timeSpan2);
        }

        /// <summary>
        ///     Subtracts two TimeSpans.
        /// </summary>
        /// <param name="timeSpan1">A TimeSpan.</param>
        /// <param name="timeSpan2">A TimeSpan.</param>
        /// <returns></returns>
        public static System.TimeSpan Subtract(System.TimeSpan timeSpan1, System.TimeSpan timeSpan2)
        {
            return timeSpan1.Subtract(timeSpan2);
        }

        /// <summary>
        ///     Creates a new TimeSpan from an inexact number of days spanned.
        /// </summary>
        /// <param name="days">Amount of time spanned, represented as days.</param>
        public static System.TimeSpan FromDays(double days)
        {
            return System.TimeSpan.FromDays(days);
        }

        /// <summary>
        ///     Creates a new TimeSpan from an inexact number of hours spanned.
        /// </summary>
        /// <param name="hours">Amount of time spanned, represented as hours.</param>
        public static System.TimeSpan FromHours(double hours)
        {
            return System.TimeSpan.FromHours(hours);
        }

        /// <summary>
        ///     Creates a new TimeSpan from an inexact number of minutes spanned.
        /// </summary>
        /// <param name="mins">Amount of time spanned, represented as minutes.</param>
        public static System.TimeSpan FromMinutes(double mins)
        {
            return System.TimeSpan.FromMinutes(mins);
        }

        /// <summary>
        ///     Creates a new TimeSpan from an inexact number of seconds spanned.
        /// </summary>
        /// <param name="secs">Amount of time spanned, represented as seconds.</param>
        public static System.TimeSpan FromSeconds(double secs)
        {
            return System.TimeSpan.FromSeconds(secs);
        }

        /// <summary>
        ///     Creates a new TimeSpan from an inexact number of milliseconds spanned.
        /// </summary>
        /// <param name="ms">Amount of time spanned, represented as milliseconds.</param>
        public static System.TimeSpan FromMilliseconds(double ms)
        {
            return System.TimeSpan.FromMilliseconds(ms);
        }

        /// <summary>
        ///     Attempts to parse a TimeSpan from a string.
        /// </summary>
        /// <param name="str">String representation of a TimeSpan.</param>
        public static System.TimeSpan FromString(string str)
        {
            return System.TimeSpan.Parse(str, CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Extracts the individual components of a TimeSpan.
        /// </summary>
        /// <param name="timeSpan">A TimeSpan.</param>
        [MultiReturn("days", "hours", "minutes", "seconds", "milliseconds")]
        public static Dictionary<string, int> Components(System.TimeSpan timeSpan)
        {
            return new Dictionary<string, int>
            {
                { "days", timeSpan.Days },
                { "hours", timeSpan.Hours },
                { "minutes", timeSpan.Minutes },
                { "seconds", timeSpan.Seconds },
                { "milliseconds", timeSpan.Milliseconds }
            };
        }

        /// <summary>
        ///     Converts the total amount of time represented by a TimeSpan to an
        ///     inexact number of days.
        /// </summary>
        /// <param name="timeSpan">A TimeSpan.</param>
        public static double TotalDays(System.TimeSpan timeSpan)
        {
            return timeSpan.TotalDays;
        }

        /// <summary>
        ///     Converts the total amount of time represented by a TimeSpan to an
        ///     inexact number of hours.
        /// </summary>
        /// <param name="timeSpan">A TimeSpan.</param>
        public static double TotalHours(System.TimeSpan timeSpan)
        {
            return timeSpan.TotalHours;
        }

        /// <summary>
        ///     Converts the total amount of time represented by a TimeSpan to an
        ///     inexact number of minutes.
        /// </summary>
        /// <param name="timeSpan">A TimeSpan.</param>
        public static double TotalMinutes(System.TimeSpan timeSpan)
        {
            return timeSpan.TotalMinutes;
        }

        /// <summary>
        ///     Converts the total amount of time represented by a TimeSpan to an
        ///     inexact number of seconds.
        /// </summary>
        /// <param name="timeSpan">A TimeSpan.</param>
        public static double TotalSeconds(System.TimeSpan timeSpan)
        {
            return timeSpan.TotalSeconds;
        }

        /// <summary>
        ///     Converts the total amount of time represented by a TimeSpan to an
        ///     inexact number of milliseconds.
        /// </summary>
        /// <param name="timeSpan">A TimeSpan.</param>
        public static double TotalMilliseconds(System.TimeSpan timeSpan)
        {
            return timeSpan.TotalMilliseconds;
        }
    }
}
