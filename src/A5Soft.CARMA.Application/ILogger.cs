using System;
using System.Collections.Generic;
using System.Text;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Interface for a logger to use for the application.
    /// </summary>
    public interface ILogger
    {

        void LogCritical(Exception exception, params object[] args);

        void LogError(Exception exception, params object[] args);

        void LogWarning(Exception exception, params object[] args);

        void LogWarning(string message, params object[] args);

        void LogDebug(Exception exception, params object[] args);

        void LogDebug(string message, params object[] args);

        void LogMethodEntry(Type ownerType, string methodName, params object[] methodParams);

        void LogMethodExit(Type ownerType, string methodName, object returnValue = null);

    }
}
