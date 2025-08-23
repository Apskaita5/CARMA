using System;
using System.Diagnostics;
using System.Text;

namespace A5Soft.CARMA.Application
{
    public static class Extensions
    {
        /// <summary>
        /// Gets an extensive description of an exception.
        /// </summary>
        /// <param name="ex">an exception to get a description for</param>
        /// <returns>an extensive description of an exception</returns>
        [DebuggerHidden]
        [DebuggerStepThrough]
        internal static string GetFullDescription(this Exception ex)
        {
            if (null == ex) return string.Empty;

            var builder = new StringBuilder();
            ex.AddInstanceDescription(builder);
            if (null != ex.InnerException) ex.InnerException.AddFullDescription(builder);

            return builder.ToString();
        }

        private static void AddFullDescription(this Exception ex, StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine("------Internal Exception------");
            builder.AppendLine();
            ex.AddInstanceDescription(builder);
            if (null != ex.InnerException) ex.InnerException.AddFullDescription(builder);
        }

        private static void AddInstanceDescription(this Exception ex, StringBuilder builder)
        {
            builder.AppendLine(ex.Message);
            builder.AppendLine($"Exception type: {ex.GetType().Name}");
            builder.AppendLine($"Target site: {ex.TargetSite?.Name}");
            builder.AppendLine("Stack trace:");
            builder.AppendLine(ex.StackTrace);
        }
    }
}
