using A5Soft.CARMA.Domain.Math;
using A5Soft.CARMA.Domain.Metadata;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace A5Soft.CARMA.Domain.Rules.DataAnnotations.CommonRules
{
    /// <summary>
    /// A base class for numeric range (double of int only) validation.
    /// Implementation shall set ErrorMessageResourceType in the constructor and override
    /// abstract methods to provide different resource property names for different types of errors.
    /// </summary>
    public abstract class RangeAttribute : System.ComponentModel.DataAnnotations.RangeAttribute,
        IPropertyValidationAttribute
    {

        /// <summary>
        /// Gets or sets a value indicating severity of broken rule.
        /// </summary>
        public RuleSeverity Severity { get; set; } = RuleSeverity.Error;

        /// <summary>
        /// Gets or sets significant digits to compare double values (default - 2).
        /// </summary>
        public int Digits { get; set; } = 2;


        /// <summary>
        /// Creates a new range attribute.
        /// </summary>
        /// <param name="minimum">minimum allowed value</param>
        /// <param name="maximum">maximum allowed value</param>
        protected RangeAttribute(double minimum, double maximum) : base(minimum, maximum)
        {
            this.ErrorMessageResourceName = ErrorMessageResourceNameForRange;
        }

        /// <summary>
        /// Creates a new range attribute.
        /// </summary>
        /// <param name="minimum">minimum allowed value</param>
        /// <param name="maximum">maximum allowed value</param>
        protected RangeAttribute(int minimum, int maximum) : base(minimum, maximum)
        {
            this.ErrorMessageResourceName = ErrorMessageResourceNameForRange;
        }

        /// <summary>
        /// Creates a new range attribute.
        /// </summary>
        /// <param name="threshold">threshold value (either min or max subject to isMin parameter)</param>
        /// <param name="isMin">whether the threshold value is minimum allowed value (otherwise - max)</param>
        protected RangeAttribute(double threshold, bool isMin)
            : base(isMin ? threshold : double.MinValue, isMin ? double.MaxValue : threshold)
        {
            if (isMin)
            {
                this.ErrorMessageResourceName = ErrorMessageResourceNameForMin;
            }
            else
            {
                this.ErrorMessageResourceName = ErrorMessageResourceNameForMax;
            }
        }

        /// <summary>
        /// Creates a new range attribute.
        /// </summary>
        /// <param name="threshold">threshold value (either min or max subject to isMin parameter)</param>
        /// <param name="isMin">whether the threshold value is minimum allowed value (otherwise - max)</param>
        protected RangeAttribute(int threshold, bool isMin)
            : base(isMin ? threshold : int.MinValue, isMin ? int.MaxValue : threshold)
        {
            if (isMin)
            {
                this.ErrorMessageResourceName = ErrorMessageResourceNameForMin;
            }
            else
            {
                this.ErrorMessageResourceName = ErrorMessageResourceNameForMax;
            }
        }


        /// <summary>
        /// Provide resource property name for error message if value is out of allowed range.
        /// Parameter sequence is: localized property name, min value, max value.
        /// </summary>
        protected abstract string ErrorMessageResourceNameForRange { get; }

        /// <summary>
        /// Provide resource property name for error message if value is smaller than allowed.
        /// Parameter sequence is: localized property name, min value, max value.
        /// </summary>
        protected abstract string ErrorMessageResourceNameForMin { get; }

        /// <summary>
        /// Provide resource property name for error message if value is greater than allowed.
        /// Parameter sequence is: localized property name, min value, max value.
        /// </summary>
        protected abstract string ErrorMessageResourceNameForMax { get; }


        /// <inheritdoc cref="System.ComponentModel.DataAnnotations.ValidationAttribute.IsValid" />
        public override bool IsValid(object value)
        {
            if (Severity != RuleSeverity.Error) return true;
            return IsValidInternal(value as string);
        }

        /// <inheritdoc cref="IPropertyValidationAttribute.GetValidationResult" />
        public BrokenRule GetValidationResult(object instance, IPropertyMetadata propInfo,
            IEnumerable<IPropertyMetadata> relatedProps)
        {
            if (IsValidInternal(propInfo.GetValue(instance))) return null;

            return new BrokenRule(this.GetType().FullName, propInfo.Name, string.Format(
                CultureInfo.CurrentCulture, this.ErrorMessageString, propInfo.GetDisplayName(), 
                Minimum, Maximum), Severity);
        }


        private bool IsValidInternal(object value)
        {
            if (null == value) return true;

            if (value is int intValue)
            {
                return intValue <= (int)this.Maximum && intValue >= (int)this.Minimum;
            }
            else if (value is double dblValue)
            {
                return dblValue.GreaterThan((double)this.Minimum, Digits) 
                       && ((double)this.Maximum).GreaterThan(dblValue, Digits);
            }
            else
            {
                throw new NotSupportedException(
                    $"Value type {value.GetType().FullName} is not supported by RangeAttribute.");
            }
        }

    }
}
