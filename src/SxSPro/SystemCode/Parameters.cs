﻿using System.Collections.Generic;

namespace SxSPro.SystemCode
{
    public static class Parameters
    {
        public static string DischargeQr => "QR";
        public static string StageHg => "HG";
        public static string RiverSectionArea => "RiverSectionArea";
        public static string RiverSectionWidth => "RiverSectionWidth";
        public static string WaterVelocityWv => "WV";
        public static string WaterTemp => "TW";
        public static string AirTemp => "TA";
        public static string Voltage => "VB";

        public static IReadOnlyList<string> DischargeSummaryParameterIds =>
            new List<string>
            {
                DischargeQr,
                StageHg,
                RiverSectionArea,
                RiverSectionWidth,
                WaterVelocityWv
            };
    }
}
