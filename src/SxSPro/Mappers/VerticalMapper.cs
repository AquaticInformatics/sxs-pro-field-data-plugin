using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FieldDataPluginFramework.DataModel;
using FieldDataPluginFramework.DataModel.ChannelMeasurements;
using FieldDataPluginFramework.DataModel.Meters;
using FieldDataPluginFramework.DataModel.Verticals;
using SxSPro.Helper;
using SxSPro.Schema;

namespace SxSPro.Mappers
{
    public class VerticalMapper
    {
        private readonly DateTimeInterval _measurementInterval;
        private readonly MeterCalibration _meterCalibration;

        public VerticalMapper(DateTimeInterval measurementInterval, MeterCalibration meterCalibration)
        {
            _measurementInterval = measurementInterval;
            _meterCalibration = meterCalibration;
        }

        public IEnumerable<Vertical> MapAll(XmlRootSummaryStation[] stations)
        {
            if (stations == null || !stations.Any())
                return new List<Vertical>();

            var lastStationId = $"{stations.Length}";

            return stations.Select(st => Map(st, lastStationId));
        }

        private Vertical Map(XmlRootSummaryStation station, string lastStationId)
        {
            return new Vertical
            {
                Segment = GetSegment(station),
                //TODO: may need to look at ice thickness to determine which condition:
                MeasurementConditionData = new OpenWaterData(),
                TaglinePosition = station.Dist.AsDouble(),
                SequenceNumber = int.Parse(station.id),
                MeasurementTime = GetMeasurementTime(station, lastStationId),
                VerticalType = VerticalType.MidRiver, //TODO: is this correct?
                EffectiveDepth = station.Depth.AsDouble(),
                VelocityObservation = GetVelocityObservation(station),
                FlowDirection = FlowDirectionType.Normal,
                Comments = AddUnMappedFieldsToVerticalComments(station)
            };
        }

        private Segment GetSegment(XmlRootSummaryStation station)
        {
            return new Segment
            {
                Area = station.Area.AsDouble(),
                Discharge = station.Stn_Q.AsDouble(),
                Velocity = station.Avg_V.AsDouble(),
                Width = station.Width.AsDouble(),
                TotalDischargePortion = station.Percent_Tot.AsDouble()
            };
        }

        private DateTimeOffset? GetMeasurementTime(XmlRootSummaryStation station, string lastStationId)
        {
            //station.Start_T is "-" at the first and last station. It should be safe to assume
            //that first has the start measurement time and last has the end time.

            var timeText = station.Start_T?.Trim();

            if (string.IsNullOrEmpty(timeText) || timeText == "-")
            {
                if (station.id == "1")
                {
                    return _measurementInterval.Start;
                }

                if (station.id == lastStationId)
                {
                    return _measurementInterval.End;
                }

                throw new ArgumentException($"'{station.Start_T}' is not a valid start time for station ID={station.id}");
            }

            if (!DateTime.TryParse(timeText, CultureInfo.InvariantCulture, DateTimeStyles.NoCurrentDateDefault, out var dateTime) || dateTime.Date != DateTime.MinValue)
                throw new ArgumentException($"'{station.Start_T}' is not a valid start time for station ID={station.id}");

            return new DateTimeOffset(_measurementInterval.Start.Date, _measurementInterval.Start.Offset)
                .Add(dateTime.TimeOfDay);
        }

        private VelocityObservation GetVelocityObservation(XmlRootSummaryStation station)
        {
            var observation = GetVelocityDepthObservation(station);
            return new VelocityObservation
            {
                MeterCalibration = _meterCalibration,
                DeploymentMethod = DeploymentMethodType.StationaryBoat,
                //TODO: This is required, but OneAtPointFive is not correct for ADCP measurement.
                VelocityObservationMethod = PointVelocityObservationType.OneAtPointFive,
                MeanVelocity = station.Avg_V.AsDouble(),
                Observations = { observation }
            };
        }

        private VelocityDepthObservation GetVelocityDepthObservation(XmlRootSummaryStation station)
        {
            //TODO: some fields are not applicable. eg. RevolutionCount.
            return new VelocityDepthObservation
            {
                Depth = station.ADCP_Depth.AsDouble(),
                ObservationInterval = station.Meas_T.AsDouble(),
                RevolutionCount = 0,
                Velocity = station.Avg_V.AsDouble()
            };
        }

        private string AddUnMappedFieldsToVerticalComments(XmlRootSummaryStation station)
        {
            return $"Dir.: {station.Dir_V}; " +
                   $"Coeff. of Var.: {station.CV};" +
                   $" Angle Corr. Coeff.: {station.Ang_Cf}";
        }
    }
}
