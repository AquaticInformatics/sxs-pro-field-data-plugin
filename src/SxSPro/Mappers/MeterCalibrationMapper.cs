using System;
using FieldDataPluginFramework.DataModel.Meters;
using SxSSummary = SxSPro.Schema.XmlRootSummaryWinRiver_II_Section_by_Section_Summary;

namespace SxSPro.Mappers
{
    public class MeterCalibrationMapper
    {
        public MeterCalibration Map(SxSSummary sxsSummary)
        {
            return new MeterCalibration
            {
                Manufacturer = "N/A", //Required
                FirmwareVersion = sxsSummary.Firmware_Version,
                SerialNumber = sxsSummary.ADCP_Serial_No,
                MeterType = MeterType.Adcp,
                SoftwareVersion = sxsSummary.Software_Version,
                Model = sxsSummary.Instrument?.Trim(),
                Configuration = ComposeConfiguration(sxsSummary)
            };
        }

        private string ComposeConfiguration(SxSSummary sxsSummary)
        {
            var newLine = Environment.NewLine;
            return $"Water Mode: {sxsSummary.Water_Mode}{newLine}" +
                   $"ADCP Depth: {sxsSummary.ADCP_Depth}{newLine}" +
                   $"Blank: {sxsSummary.Blank}{newLine}" +
                   $"# Cells: {sxsSummary.Num_Cells}{newLine}" +
                   $"Cell Size: {sxsSummary.Cell_Size}";
        }
    }
}
