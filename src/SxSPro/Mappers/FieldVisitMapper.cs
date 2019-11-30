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
        private readonly Config _config;

        public FieldVisitMapper(Config config, XmlRootSummary summary, LocationInfo location)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

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
            if (!DateTime.TryParseExact(_summary.Date, _config.DateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowWhiteSpaces, out var dateTime) || dateTime.Date == DateTime.MinValue)
            {
                throw new ArgumentException($"'{_summary.Date}' is an invalid date. Supported patterns are: {string.Join(", ", _config.DateFormats)}");
            }

            var date = dateTime.Date;

            var times = _summary
                .Start_End_Time
                .Split(TimeSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(ParseTime)
                .ToList();

            if (times.Count != 2)
                throw new ArgumentException($"Invalid start/end time: '{_summary.Start_End_Time}'");

            var startTime = times.First();
            var endTime = times.Last();

            var start = new DateTimeOffset(date, _location.UtcOffset).Add(startTime);
            var end = new DateTimeOffset(date, _location.UtcOffset).Add(endTime);

            if (end < start)
            {
                // Adjust for midnight measurement boundaries
                end = end.AddDays(1);
            }

            return new DateTimeInterval(start, end);
        }

        private static readonly char[] TimeSeparators = {'/'};

        private TimeSpan ParseTime(string text)
        {
            if (!DateTime.TryParseExact(text, _config.TimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var dateTime))
            {
                throw new ArgumentException($"'{text}' is an invalid time. Supported patterns are: {string.Join(", ", _config.TimeFormats)}");
            }

            return dateTime.TimeOfDay;
        }
    }
}
