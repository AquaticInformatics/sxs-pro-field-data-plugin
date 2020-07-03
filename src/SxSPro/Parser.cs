using System;
using System.Collections.Generic;
using System.IO;
using FieldDataPluginFramework;
using FieldDataPluginFramework.Context;
using FieldDataPluginFramework.Results;
using SxSPro.Mappers;
using SxSPro.Schema;

namespace SxSPro
{
    public class Parser
    {
        private readonly LocationInfo _location;
        private readonly IFieldDataResultsAppender _appender;
        private readonly ILog _logger;

        public Parser(LocationInfo location, IFieldDataResultsAppender appender, ILog logger)
        {
            _location = location;
            _appender = appender;
            _logger = logger;
        }

        public void Parse(XmlRoot xmlRoot)
        {
            if(xmlRoot?.Summary?.WinRiver_II_Section_by_Section_Summary == null)
                throw new ArgumentNullException(nameof(xmlRoot));

            var fieldVisitInfo = AppendMappedFieldVisitInfo(xmlRoot.Summary, _location);
            AppendMappedMeasurements(xmlRoot.Summary, fieldVisitInfo);
        }

        private FieldVisitInfo AppendMappedFieldVisitInfo(XmlRootSummary summary, LocationInfo locationInfo)
        {
            var config = new ConfigLoader(_appender).Load();
            var mapper = new FieldVisitMapper(config, summary, _location);
            var fieldVisitDetails = mapper.MapFieldVisitDetails();

            _logger.Info($"Successfully parsed one visit '{fieldVisitDetails.FieldVisitPeriod}' " +
                         $"for location '{locationInfo.LocationIdentifier}'");

            return _appender.AddFieldVisit(locationInfo, fieldVisitDetails);
        }

        private void AppendMappedMeasurements(XmlRootSummary xmlRootSummary, FieldVisitInfo fieldVisitInfo)
        {
            var dischargeActivityMapper = new DischargeActivityMapper(fieldVisitInfo);

            _appender.AddDischargeActivity(fieldVisitInfo, dischargeActivityMapper.Map(xmlRootSummary));

            var readingsMapper = new ReadingsMapper(fieldVisitInfo);
            foreach (var reading in readingsMapper.Map(xmlRootSummary, dischargeActivityMapper.IsMetric))
            {
                _appender.AddReading(fieldVisitInfo, reading);
            }
        }
    }
}
