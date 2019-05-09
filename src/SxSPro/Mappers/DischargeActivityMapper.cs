using FieldDataPluginFramework.Context;
using FieldDataPluginFramework.DataModel;
using FieldDataPluginFramework.DataModel.ChannelMeasurements;
using FieldDataPluginFramework.DataModel.DischargeActivities;
using FieldDataPluginFramework.DataModel.Meters;
using FieldDataPluginFramework.Units;
using SxSPro.Helper;
using SxSPro.Schema;
using SxSPro.SystemCode;
using SxSSummary = SxSPro.Schema.XmlRootSummaryWinRiver_II_Section_by_Section_Summary;

namespace SxSPro.Mappers
{
    internal class DischargeActivityMapper
    {
        private readonly FieldVisitInfo _fieldVisitInfo;

        public DischargeActivityMapper(FieldVisitInfo fieldVisitInfo)
        {
            _fieldVisitInfo = fieldVisitInfo;
        }

        public DischargeActivity Map(XmlRootSummary xmlRootSummary)
        {
            var sxsSummary = xmlRootSummary.WinRiver_II_Section_by_Section_Summary;
            var unitSystem = sxsSummary.Units_of_Measure == "Metric"
                ? Units.MetricUnitSystem
                : Units.ImperialUnitSystem;

            var dischargeActivity = CreateDischargeActivityWithSummary(sxsSummary, unitSystem);

            SetDischargeSection(dischargeActivity, sxsSummary, xmlRootSummary.Station, unitSystem);

            return dischargeActivity;
        }

        private DischargeActivity CreateDischargeActivityWithSummary(SxSSummary sxsSummary, UnitSystem unitSystem)
        {
            var factory = new DischargeActivityFactory(unitSystem);

            //Discharge summary:
            var measurementPeriod = GetMeasurementPeriod();
            var dischargeActivity = factory.CreateDischargeActivity(measurementPeriod, sxsSummary.Total_Q.AsDouble());

            dischargeActivity.Comments = sxsSummary.Comments;
            dischargeActivity.Party = _fieldVisitInfo.Party;

            //Mean gage height: 
            AddMeanGageHeight(dischargeActivity, sxsSummary.Stage, unitSystem);

            return dischargeActivity;
        }

        private DateTimeInterval GetMeasurementPeriod()
        {
            return new DateTimeInterval(_fieldVisitInfo.StartDate, _fieldVisitInfo.EndDate);
        }

        private void AddMeanGageHeight(DischargeActivity dischargeActivity, decimal stage, UnitSystem unitSystem)
        {
            var measurement = new Measurement(stage.AsDouble(), unitSystem.DistanceUnitId); 
            var gageHeightMeasurement = new GageHeightMeasurement(measurement, _fieldVisitInfo.StartDate);

            dischargeActivity.GageHeightMeasurements.Add(gageHeightMeasurement);
        }

        private void SetDischargeSection(DischargeActivity dischargeActivity, SxSSummary sxsSummary, 
            XmlRootSummaryStation[] stations, UnitSystem unitSystem)
        {
            var dischargeSection = CreateDischargeSectionWithDescription(dischargeActivity, sxsSummary, unitSystem);
            var meterCalibration = new MeterCalibrationMapper().Map(sxsSummary);

            SetMappedVerticals(dischargeSection, meterCalibration, stations);

            SetChannelObservations(dischargeSection, sxsSummary, unitSystem);

            dischargeActivity.ChannelMeasurements.Add(dischargeSection);
        }

        private ManualGaugingDischargeSection CreateDischargeSectionWithDescription(DischargeActivity dischargeActivity,
            XmlRootSummaryWinRiver_II_Section_by_Section_Summary sxsSummary, UnitSystem unitSystem)
        {
            var factory = new ManualGaugingDischargeSectionFactory(unitSystem);
            var manualGaugingDischarge = factory.CreateManualGaugingDischargeSection(
                dischargeActivity.MeasurementPeriod, sxsSummary.Total_Q.AsDouble());

            //Party: 
            manualGaugingDischarge.Party = dischargeActivity.Party;

            //Discharge method default to mid-section:
            var dischargeMethod = sxsSummary.Q_Method == "Mean-section"
                ? DischargeMethodType.MeanSection
                : DischargeMethodType.MidSection;

            manualGaugingDischarge.DischargeMethod = dischargeMethod;

            return manualGaugingDischarge;
        }

        private void SetChannelObservations(ManualGaugingDischargeSection dischargeSection, SxSSummary sxsSummary,
            UnitSystem unitSystem)
        {
            //River area:
            dischargeSection.AreaUnitId = unitSystem.AreaUnitId;
            dischargeSection.AreaValue = sxsSummary.Meas_Area.AsDouble();

            //Width:
            
            dischargeSection.WidthValue = sxsSummary.Meas_Width.AsDouble();

            //Velocity:
            dischargeSection.VelocityUnitId = unitSystem.VelocityUnitId;
            dischargeSection.VelocityAverageValue = sxsSummary.Mean_Vel.AsDouble();
        }

        private void SetMappedVerticals(ManualGaugingDischargeSection manualGaugingDischarge, MeterCalibration meterCalibration,
            XmlRootSummaryStation[] stations)
        {
            var verticals = new VerticalMapper(GetMeasurementPeriod(), meterCalibration).MapAll(stations);

            foreach (var vertical in verticals)
            {
                manualGaugingDischarge.Verticals.Add(vertical);
            }
        }
    }
}
