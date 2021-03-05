using System;

namespace A5Soft.CARMA.Domain.Math
{
    public static class Extensions
    {

        private static readonly double doubleRoundLimit = 1e16d;

        private const int maxRoundingDigits = 15;

        // This table is required for the Round function which can specify the number of digits to round to
        private static readonly double[] roundPower10Double = new double[] {
            1E0, 1E1, 1E2, 1E3, 1E4, 1E5, 1E6, 1E7, 1E8,
            1E9, 1E10, 1E11, 1E12, 1E13, 1E14, 1E15
        };


        /// <summary>
        /// Returns a double value rounded using accounting algorithm.
        /// </summary>
        /// <param name="value">the value to round</param>
        /// <param name="digits">the rounding order</param>
        /// <exception cref="ArgumentOutOfRangeException">Round order should be between 0 and 20.</exception>
        /// <remarks>Implements custom AwayFromZero rounding for double values.
        /// Used for consistent rounding behaviour.</remarks>
        public static double AccountingRound(this double value, int digits)
        {
            if (digits < 0 || digits > maxRoundingDigits)
                throw new ArgumentOutOfRangeException(nameof(digits), digits,
                    $"Round order value {digits} is invalid, should be in range 0 to {maxRoundingDigits}.");

            var sign = System.Math.Sign(value);
            value = System.Math.Abs(value);

            if (value < doubleRoundLimit)
            {
                value *= roundPower10Double[digits];
                var floor = System.Math.Floor(value);
                if ((floor + 0.5).GreaterThan(value))
                {
                    return sign * floor / roundPower10Double[digits];
                }
                return sign * (floor + 1.0) / roundPower10Double[digits];
            }

            return sign * value;
        }

        /// <summary>
        /// Returns a decimal value rounded using accounting algorithm.
        /// </summary>
        /// <param name="value">the value to round</param>
        /// <param name="digits">the rounding order</param>
        /// <exception cref="ArgumentOutOfRangeException">Round order should be between 0 and 15.</exception>
        /// <remarks>Equivalent to Decimal.Round(value, digits, MidpointRounding.AwayFromZero).
        /// Used for consistent rounding behaviour.</remarks>
        public static decimal AccountingRound(this decimal value, int digits)
        {
            if (digits < 0 || digits > 15)
                throw new ArgumentOutOfRangeException(nameof(digits), digits,
                    $"Round order value {digits} is invalid, should be in range 0 to 15.");

            return Decimal.Round(value, digits, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Compares double values for equality using double.Epsilon.
        /// </summary>
        /// <param name="value">the first value to compare</param>
        /// <param name="valueToCompare">the second value to compare</param>
        public static bool EqualsTo(this double value, double valueToCompare)
        {
            return !((System.Math.Abs(value - valueToCompare) > double.Epsilon));
        }

        /// <summary>
        /// Compares double values for equality using the epsilon provided.
        /// </summary>
        /// <param name="value">the first value to compare</param>
        /// <param name="valueToCompare">the second value to compare</param>
        /// <param name="epsilon">the max value diff to ignore</param>
        public static bool EqualsTo(this double value, double valueToCompare, double epsilon)
        {
            return !((System.Math.Abs(value - valueToCompare) > epsilon));
        }

        /// <summary>
        /// Compares double values for equality using the round order provided.
        /// </summary>
        /// <param name="value">the first value to compare</param>
        /// <param name="valueToCompare">the second value to compare</param>
        /// <param name="roundOrder">the round order applicable</param>
        public static bool EqualsTo(this double value, double valueToCompare, int roundOrder)
        {
            return !((System.Math.Abs(value - valueToCompare) > 
                (1.0 / System.Math.Pow(10.0, roundOrder + 1))));
        }

        /// <summary>
        /// Compares double values for the first value to be greater than the second using double.Epsilon.
        /// </summary>
        /// <param name="value">the first value to compare</param>
        /// <param name="valueToCompare">the second value to compare</param>
        public static bool GreaterThan(this double value, double valueToCompare)
        {
            return ((value - valueToCompare) > double.Epsilon);
        }

        /// <summary>
        /// Compares double values for the first value to be greater than the second using the epsilon provided.
        /// </summary>
        /// <param name="value">the first value to compare</param>
        /// <param name="valueToCompare">the second value to compare</param>
        /// <param name="epsilon">the max value diff to ignore</param>
        public static bool GreaterThan(this double value, double valueToCompare, double epsilon)
        {
            return ((value - valueToCompare) > epsilon);
        }

        /// <summary>
        /// Compares double values for the first value to be greater than the second using the round order provided.
        /// </summary>
        /// <param name="value">the first value to compare</param>
        /// <param name="valueToCompare">the second value to compare</param>
        /// <param name="roundOrder">the round order applicable</param>
        public static bool GreaterThan(this double value, double valueToCompare, int roundOrder)
        {
            return ((value - valueToCompare) > (1.0 / System.Math.Pow(10.0, roundOrder + 1)));
        }

    }
}
