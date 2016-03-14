using System;
using System.Collections.Generic;
using System.Globalization;

using Autodesk.DesignScript.Runtime;
using Dynamo.Extensions;

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
        /// <returns name="dateTime">DateTime</returns>
        public static System.DateTime MinValue
        {
            get { return System.DateTime.MinValue; }
        }

        /// <summary>
        ///     The latest date and time that can be represented.
        /// </summary>
        /// <returns name="dateTime">DateTime</returns>
        public static System.DateTime MaxValue
        {
            get { return System.DateTime.MaxValue; }
        }

        /// <summary>
        ///     The current system date and time.
        /// </summary>
        /// <returns name="dateTime">DateTime</returns>
        [CanUpdatePeriodicallyAttribute(true)]
        public static System.DateTime Now
        {
            get { return System.DateTime.Now; }
        }

        /// <summary>
        ///     The current system date, with time set at midnight.
        /// </summary>
        /// <returns name="dateTime">DateTime</returns>
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
        /// <returns name="dateTime">DateTime</returns>
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
        /// <returns name="dateTime">DateTime</returns>
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
        /// <returns name="dateTime">DateTime</returns>
        public static System.DateTime SubtractTimeSpan(System.DateTime dateTime, System.TimeSpan timeSpan)
        {
            return dateTime.Subtract(timeSpan);
        }

        /// <summary>
        ///     Adds a TimeSpan to a DateTime, yielding a new DateTime.
        /// </summary>
        /// <param name="dateTime">Starting DateTime.</param>
        /// <param name="timeSpan">Amount of time to add.</param>
        /// <returns name="dateTime">DateTime</returns>
        public static System.DateTime AddTimeSpan(System.DateTime dateTime, System.TimeSpan timeSpan)
        {
            return dateTime.Add(timeSpan);
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
        /// <returns name="dateTime">DateTime</returns>
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
        /// <param name="dateTime">A DateTime object.</param>
        /// <returns name="dayOfWeek">Day of the week</returns>
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

        /// <summary>
        ///     Returns the day of the year (0-366)
        /// </summary>
        /// <param name="dateTime">A DateTime.</param>
        public static int DayOfYear(System.DateTime dateTime)
        {
            return dateTime.DayOfYear;
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
    }

    /// <summary>
    ///     Days of the Week
    /// </summary>
    public enum DayOfWeek
    {
        [EnumDescription("EnumDateOfWeekSunday", typeof(Properties.Resources))]Sunday,
        [EnumDescription("EnumDateOfWeekMonday", typeof(Properties.Resources))]Monday,
        [EnumDescription("EnumDateOfWeekTuesday", typeof(Properties.Resources))]Tuesday,
        [EnumDescription("EnumDateOfWeekWednesday", typeof(Properties.Resources))]Wednesday,
        [EnumDescription("EnumDateOfWeekThursday", typeof(Properties.Resources))]Thursday,
        [EnumDescription("EnumDateOfWeekFriday", typeof(Properties.Resources))]Friday,
        [EnumDescription("EnumDateOfWeekSaturday", typeof(Properties.Resources))]Saturday
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
        /// <returns name="timeSpan">TimeSpan</returns>
        public static System.TimeSpan ByDateDifference(System.DateTime date1, System.DateTime date2)
        {
            return date1.Subtract(date2);
        }

        /// <summary>
        ///     A TimeSpan representing an elapsed time of Zero.
        /// </summary>
        /// <returns name="timeSpan">TimeSpan</returns>
        public static System.TimeSpan Zero { get { return System.TimeSpan.Zero; } }

        /// <summary>
        ///     The largest TimeSpan that can be represented.
        /// </summary>
        /// <returns name="timeSpan">TimeSpan</returns>
        public static System.TimeSpan MaxValue { get { return System.TimeSpan.MaxValue; } }

        /// <summary>
        ///     The smallest TimeSpan that can be represented.
        /// </summary>
        /// <returns name="timeSpan">TimeSpan</returns>
        public static System.TimeSpan MinValue { get { return System.TimeSpan.MaxValue; } }

        /// <summary>
        ///     Creates a new TimeSpan from a span of time.
        /// </summary>
        /// <param name="days">Days spanned.</param>
        /// <param name="hours">Hours spanned.</param>
        /// <param name="minutes">Minutes spanned.</param>
        /// <param name="seconds">Seconds spanned.</param>
        /// <param name="milliseconds">Milliseconds spanned.</param>
        /// <returns name="timeSpan">TimeSpan</returns>
        public static System.TimeSpan Create(
            double days = 0, double hours = 0, double minutes = 0, double seconds = 0, double milliseconds = 0)
        {
            return System.TimeSpan.FromMilliseconds(milliseconds) 
                + System.TimeSpan.FromSeconds(seconds)
                + System.TimeSpan.FromMinutes(minutes) 
                + System.TimeSpan.FromHours(hours)
                + System.TimeSpan.FromDays(days);
        }

        /// <summary>
        ///     Multiplies a TimeSpan by a scaling factor.
        /// </summary>
        /// <param name="timeSpan">A TimeSpan.</param>
        /// <param name="scaleFactor">
        /// Amount to scale the TimeSpan. For example, a scaling factor of 2 will yield
        /// double the amount of time spanned.
        /// </param>
        /// <returns name="timeSpan">TimeSpan</returns>
        public static System.TimeSpan Scale(System.TimeSpan timeSpan, double scaleFactor)
        {
            return System.TimeSpan.FromMilliseconds(timeSpan.TotalMilliseconds*scaleFactor);
        }

        /// <summary>
        ///     Negates a TimeSpan.
        /// </summary>
        /// <param name="timeSpan">A TimeSpan.</param>
        /// <returns name="timeSpan">TimeSpan</returns>
        public static System.TimeSpan Negate(System.TimeSpan timeSpan)
        {
            return timeSpan.Negate();
        }

        /// <summary>
        ///     Adds two TimeSpans.
        /// </summary>
        /// <param name="timeSpan1">A TimeSpan.</param>
        /// <param name="timeSpan2">A TimeSpan.</param>
        /// <returns name="timeSpan">TimeSpan</returns>
        public static System.TimeSpan Add(System.TimeSpan timeSpan1, System.TimeSpan timeSpan2)
        {
            return timeSpan1.Add(timeSpan2);
        }

        /// <summary>
        ///     Subtracts two TimeSpans.
        /// </summary>
        /// <param name="timeSpan1">A TimeSpan.</param>
        /// <param name="timeSpan2">A TimeSpan.</param>
        /// <returns name="timeSpan">TimeSpan</returns>
        public static System.TimeSpan Subtract(System.TimeSpan timeSpan1, System.TimeSpan timeSpan2)
        {
            return timeSpan1.Subtract(timeSpan2);
        }

        /// <summary>
        ///     Attempts to parse a TimeSpan from a string.
        /// </summary>
        /// <param name="str">String representation of a TimeSpan.</param>
        /// <returns name="timeSpan">TimeSpan</returns>
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
