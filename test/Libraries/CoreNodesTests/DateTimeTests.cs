using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using WrappedDT = DSCore.DateTime;
using WrappedTS = DSCore.TimeSpan;

namespace DSCoreNodesTests
{
    [TestFixture]
    public static class DateTimeTests
    {
        static DateTime aDateTime = new DateTime(1000, 10, 3);
        static TimeSpan aTimeSpan = TimeSpan.FromDays(2.5);

        [Test]
        [Category("UnitTests")]
        public static void DateTimeWrappers()
        {
            Assert.AreEqual(DateTime.MinValue, WrappedDT.MinValue);
            Assert.AreEqual(DateTime.MaxValue, WrappedDT.MaxValue);
            Assert.AreEqual(new DateTime(1, 1, 1), WrappedDT.ByDate(1, 1, 1));
            Assert.AreEqual(new DateTime(1, 1, 1, 0, 0, 0, 0), WrappedDT.ByDateAndTime(1, 1, 1));
            Assert.AreEqual(aDateTime.Subtract(aTimeSpan), WrappedDT.SubtractTimeSpan(aDateTime, aTimeSpan));
            Assert.AreEqual(aDateTime.Add(aTimeSpan), WrappedDT.AddTimeSpan(aDateTime, aTimeSpan));
            Assert.AreEqual(DateTime.DaysInMonth(3000, 1), WrappedDT.DaysInMonth(3000, 1));
            Assert.AreEqual(aDateTime.IsDaylightSavingTime(), WrappedDT.IsDaylightSavingsTime(aDateTime));
            Assert.AreEqual(DateTime.IsLeapYear(1234), WrappedDT.IsLeapYear(1234));
            Assert.AreEqual(aDateTime, WrappedDT.FromString(aDateTime.ToString()));
            Assert.AreEqual(aDateTime.Date, WrappedDT.Date(aDateTime));
            Assert.AreEqual(aDateTime.DayOfYear, WrappedDT.DayOfYear(aDateTime));
            Assert.AreEqual(aDateTime.TimeOfDay, WrappedDT.TimeOfDay(aDateTime));
        }

        [Test, Category("UnitTests")]
        public static void DateTimeComponents()
        {
            Assert.AreEqual(
                new Dictionary<string, int>
                {
                    { "year", aDateTime.Year },
                    { "month", aDateTime.Month },
                    { "day", aDateTime.Day },
                    { "hour", aDateTime.Hour },
                    { "minute", aDateTime.Minute },
                    { "second", aDateTime.Second },
                    { "millisecond", aDateTime.Millisecond },
                },
                WrappedDT.Components(aDateTime));
        }
        [Test, Category("UnitTests")]
        public static void TestDateTime_Format()
        {
            Assert.AreEqual(aDateTime.ToString("dd MMM yyyy", CultureInfo.InvariantCulture), WrappedDT.Format(WrappedDT.FromString(aDateTime.ToString()), "dd MMM yyyy"));
        }

        [Test, Category("UnitTests")]
        public static void TimeSpanWrappers()
        {
            Assert.AreEqual(
                aDateTime.Add(aTimeSpan).Subtract(aDateTime),
                WrappedTS.ByDateDifference(aDateTime.Add(aTimeSpan), aDateTime));
            Assert.AreEqual(aDateTime.TimeOfDay, WrappedDT.TimeOfDay(aDateTime));
            Assert.AreEqual(aTimeSpan.Add(aTimeSpan), WrappedTS.Add(aTimeSpan, aTimeSpan));
            Assert.AreEqual(aTimeSpan.Subtract(aDateTime.TimeOfDay), WrappedTS.Subtract(aTimeSpan, aDateTime.TimeOfDay));
            Assert.AreEqual(aTimeSpan, WrappedTS.FromString(aTimeSpan.ToString()));
            Assert.AreEqual(aTimeSpan.TotalDays, WrappedTS.TotalDays(aTimeSpan));
            Assert.AreEqual(aTimeSpan.TotalHours, WrappedTS.TotalHours(aTimeSpan));
            Assert.AreEqual(aTimeSpan.TotalMinutes, WrappedTS.TotalMinutes(aTimeSpan));
            Assert.AreEqual(aTimeSpan.TotalSeconds, WrappedTS.TotalSeconds(aTimeSpan));
            Assert.AreEqual(aTimeSpan.TotalMilliseconds, WrappedTS.TotalMilliseconds(aTimeSpan));
        }

        [Test, Category("UnitTests")]
        public static void TimeSpanCreation()
        {
            Assert.AreEqual(aTimeSpan, WrappedTS.Create(hours: aTimeSpan.TotalHours));
        }

        [Test, Category("UnitTests")]
        public static void TimeSpanScaling()
        {
            Assert.AreEqual(TimeSpan.FromDays(aTimeSpan.TotalDays * 2), WrappedTS.Scale(aTimeSpan, 2));
        }

        [Test, Category("UnitTests")]
        public static void TimeSpanComponents()
        {
            Assert.AreEqual(
                new Dictionary<string, int>
                {
                    { "days", aTimeSpan.Days },
                    { "hours", aTimeSpan.Hours },
                    { "minutes", aTimeSpan.Minutes },
                    { "seconds", aTimeSpan.Seconds },
                    { "milliseconds", aTimeSpan.Milliseconds }
                },
                WrappedTS.Components(aTimeSpan));
        }
        [Test, Category("UnitTests")]
        public static void ZeroTimeSpan()
        {
            Assert.AreEqual(System.TimeSpan.Zero, DSCore.TimeSpan.Zero);
            
        }
        [Test, Category("UnitTests")]
        public static void MaxValueTimeSpan()
        {
            Assert.AreEqual(System.TimeSpan.MaxValue, DSCore.TimeSpan.MaxValue);
        }
        [Test, Category("UnitTests")]
        public static void NegateTimeSpan()
        {
            TimeSpan ts = System.TimeSpan.MaxValue;
            Assert.AreEqual(ts.Negate(), DSCore.TimeSpan.Negate(ts));
        }
        [Test, Category("UnitTests")]
        public static void MinValueTimeSpan()
        {
            Assert.AreEqual(System.TimeSpan.MinValue, DSCore.TimeSpan.MinValue);

        }
        [Test, Category("UnitTests")]
        public static void TestDateTime_Today()
        {
            Assert.AreEqual(System.DateTime.Today, DSCore.DateTime.Today);
        }
        [Test, Category("UnitTests")]
        public static void TestDateTime_DayOfWeek()
        {
            DateTime dt = new DateTime(2016, 6, 13);
            Assert.AreEqual("Monday", DSCore.DateTime.DayOfWeek(dt).ToString());
            dt = dt.AddDays(1);
            Assert.AreEqual("Tuesday", DSCore.DateTime.DayOfWeek(dt).ToString());
            dt = dt.AddDays(1);
            Assert.AreEqual("Wednesday", DSCore.DateTime.DayOfWeek(dt).ToString());
            dt = dt.AddDays(1);
            Assert.AreEqual("Thursday", DSCore.DateTime.DayOfWeek(dt).ToString());
            dt = dt.AddDays(1);
            Assert.AreEqual("Friday", DSCore.DateTime.DayOfWeek(dt).ToString());
            dt = dt.AddDays(1);
            Assert.AreEqual("Saturday", DSCore.DateTime.DayOfWeek(dt).ToString());
            dt = dt.AddDays(1);
            Assert.AreEqual("Sunday", DSCore.DateTime.DayOfWeek(dt).ToString());
        }
    }
}
