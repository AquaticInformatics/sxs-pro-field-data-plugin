using System;

namespace SxSPro.Helper
{
    public static class DecimalExtensions
    {
        public static double AsDouble(this Decimal value)
        {
            return Decimal.ToDouble(value);
        }
    }
}
