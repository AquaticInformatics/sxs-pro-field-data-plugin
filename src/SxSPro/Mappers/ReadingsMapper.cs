using System.Collections.Generic;
using System.Globalization;
using FieldDataPluginFramework.Context;
using FieldDataPluginFramework.DataModel;
using FieldDataPluginFramework.DataModel.Readings;
using SxSPro.Schema;

namespace SxSPro.Mappers
{
    public class ReadingsMapper
    {
        private readonly FieldVisitInfo _fieldVisitInfo;

        public ReadingsMapper(FieldVisitInfo fieldVisitInfo)
        {
            _fieldVisitInfo = fieldVisitInfo;
        }

        public List<Reading> Map(XmlRootSummary sxsSummary, bool isMetric)
        {
            var readings = new List<Reading>();

            AddTemperatureReading(readings, isMetric, "TW", sxsSummary.WinRiver_II_Section_by_Section_Summary.Water_Temp);
            AddTemperatureReading(readings, isMetric, "TA", sxsSummary.WinRiver_II_Section_by_Section_Summary.Air_Temp);

            return readings;
        }

        private void AddTemperatureReading(List<Reading> readings, bool isMetric, string parameterId, string value)
        {
            if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var temperature))
                return;

            readings.Add(new Reading(parameterId, isMetric ? "degC" : "degF", temperature)
            {
                DateTimeOffset = _fieldVisitInfo.StartDate
            });
        }
    }
}
