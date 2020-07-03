using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using FieldDataPluginFramework.Results;
using ServiceStack;

namespace SxSPro
{
    public class ConfigLoader
    {
        private Dictionary<string,string> Settings { get; }

        public ConfigLoader(IFieldDataResultsAppender appender)
        {
            Settings = appender.GetPluginConfigurations();
        }

        public Config Load()
        {
            if (!Settings.TryGetValue(nameof(Config), out var jsonText) || string.IsNullOrWhiteSpace(jsonText))
                return Sanitize(new Config());

            try
            {
                return Sanitize(jsonText.FromJson<Config>());
            }
            catch (SerializationException exception)
            {
                throw new ArgumentException($"Invalid Config JSON:\b{jsonText}", exception);
            }
        }

        private Config Sanitize(Config config)
        {
            config.DateFormats = SanitizeList(config.DateFormats, "M/d/yyyy", "M-d-yyyy", "yyyy/M/d", "yyyy-M-d");
            config.TimeFormats = SanitizeList(config.TimeFormats, "h:m tt", "h:m:s tt");

            foreach (var pattern in config.DateFormats)
            {
                ThrowIfInvalidDateFormat(pattern);
            }

            var leadingMonthPatterns = config.DateFormats.Where(IsLeadingMonthPattern).ToList();
            var leadingDayPatterns = config.DateFormats.Where(IsLeadingDayPattern).ToList();

            if (leadingMonthPatterns.Any() && leadingDayPatterns.Any())
            {
                throw new ArgumentException($"Ambiguous {nameof(config.DateFormats)}: Day-then-month patterns ({string.Join(", ", leadingDayPatterns)}) conflict with Month-then-day patterns ({string.Join(", ", leadingMonthPatterns)})");
            }

            foreach (var pattern in config.TimeFormats)
            {
                ThrowIfInvalidTimeFormat(pattern);
            }

            return config;
        }

        private static string[] SanitizeList(IList<string> list, params string[] defaultIfEmpty)
        {
            if (defaultIfEmpty == null || defaultIfEmpty.Length == 0)
                throw new ArgumentException("Can't be empty", nameof(defaultIfEmpty));

            if (list == null)
            {
                list = new List<string>();
            }

            list = list
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            if (!list.Any())
            {
                foreach (var s in defaultIfEmpty)
                {
                    list.Add(s);
                }
            }

            return list.ToArray();
        }

        private bool IsLeadingMonthPattern(string pattern)
        {
            return pattern.StartsWith("M");
        }

        private bool IsLeadingDayPattern(string pattern)
        {
            return pattern.StartsWith("d");
        }

        private void ThrowIfInvalidDateFormat(string pattern)
        {
            ThrowIfInvalidFormat("DateFormat", pattern, s => s.Contains("m"), "Only use uppercase 'M' (months), not lowercase 'm' (minutes)");
            ThrowIfInvalidFormat("DateFormat", pattern, s => !s.Contains("yy"), "A 'yyyy' or 'yy' (year) pattern is missing.");
            ThrowIfInvalidFormat("DateFormat", pattern, s => !s.Contains("M"), "A 'M' (month) pattern is missing.");
            ThrowIfInvalidFormat("DateFormat", pattern, s => !s.Contains("d"), "A 'd' (ray) pattern is missing.");
        }

        private void ThrowIfInvalidTimeFormat(string pattern)
        {
            ThrowIfInvalidFormat("TimeFormat", pattern, s => s.Contains("M"), "Only use lowercase 'm' (minutes), not uppercase 'M' (months)");
            ThrowIfInvalidFormat("TimeFormat", pattern, s => !s.Contains("m"), "A 'm' (minutes) pattern is missing.");
            ThrowIfInvalidFormat("TimeFormat", pattern, s => !(s.Contains("h") && s.Contains("tt")) && !s.Contains("H"), "A 'H' (24-hour) or 'h' and 'tt' (12 hour plus AM/PM) pattern is missing.");
            ThrowIfInvalidFormat("TimeFormat", pattern, s => s.Contains("H") && s.Contains("tt"), "Can't mix 'H' (24 hour) with 'tt' (AM/PM)");
            ThrowIfInvalidFormat("TimeFormat", pattern, s => s.Contains("h") && !s.Contains("tt"), "Only us 'h' (12-hour) with 'tt' (AM/PM)");
        }

        private void ThrowIfInvalidFormat(string formatName, string pattern, Func<string, bool> invalidPredicate, string message)
        {
            if (invalidPredicate(pattern))
                throw new ArgumentException($"'{pattern}' is not a valid {formatName} string. {message}");
        }
    }
}
