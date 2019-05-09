using System;
using System.Globalization;
using System.Linq;
using FieldDataPluginFramework.Context;
using FieldDataPluginFramework.DataModel;
using SxSPro.Schema;

namespace SxSPro.Mappers
{
    public class FieldVisitMapper
    {
        private readonly XmlRootSummaryWinRiver_II_Section_by_Section_Summary _summary;
        private readonly LocationInfo _location;

        public FieldVisitMapper(XmlRootSummary summary, LocationInfo location)
        {
            _summary = summary?.WinRiver_II_Section_by_Section_Summary ?? 
                throw new ArgumentNullException(nameof(summary));

            _location = location ?? throw new ArgumentNullException(nameof(location));
        }

        public FieldVisitDetails MapFieldVisitDetails()
        {
            var visitPeriod = GetVisitTimePeriod();

            return new FieldVisitDetails(visitPeriod)
            { 
                Party = _summary.Party
            };
        }

        private DateTimeInterval GetVisitTimePeriod()
        {
            var date = DateTime.ParseExact(_summary.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            var times = _summary
                .Start_End_Time
                .Split(TimeSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(ParseTime)
                .ToList();

            if (times.Count != 2)
                throw new ArgumentException($"Invalid start/end time: '{_summary.Start_End_Time}'");

            var startTime = times.First();
            var endTime = times.Last();

            var start = new DateTimeOffset(date.Year, date.Month, date.Day,
                startTime.Hour,
                startTime.Minute,
                startTime.Second,
                _location.UtcOffset);

            var end = new DateTimeOffset(date.Year, date.Month, date.Day,
                endTime.Hour,
                endTime.Minute,
                endTime.Second,
                _location.UtcOffset);

            if (end < start)
            {
                // Adjust for midnight measurement boundaries
                end = end.AddDays(1);
            }

            return new DateTimeInterval(start, end);
        }

        private static readonly char[] TimeSeparators = {'/'};

        private DateTime ParseTime(string text)
        {
            if (DateTime.TryParseExact(text, SupportedTimeFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces, out var dateTime))
                return dateTime;

            throw new ArgumentException($"Invalid time: '{text}'");
        }

        private static readonly string[] SupportedTimeFormats =
        {
            "H:m:s",
            "H:m",
            "h:m:s tt",
            "h:m tt",
        };
    }
}
