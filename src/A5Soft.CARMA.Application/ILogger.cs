using A5Soft.CARMA.Domain;
using System;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Interface for a logger to use for the application.
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public interface ILogger
    {

        /// <summary>
        /// Logs a critical application exception.
        /// </summary>
        /// <param name="exception">an exception that caused the critical failure</param>
        /// <param name="args">any data that needs to be logged along the exception</param>
        void LogCritical(Exception exception, params object[] args);

        /// <summary>
        /// Logs a critical application exception.
        /// </summary>
        /// <param name="message">an event description to log</param>
        /// <param name="exception">an exception that caused the critical failure</param>
        /// <param name="args">any data that needs to be logged along the exception</param>
        void LogCritical(string message, Exception exception, params object[] args);

        /// <summary>
        /// Logs a critical application exception.
        /// </summary>
        /// <param name="message">an event description to log</param>
        /// <param name="args">any data that needs to be logged along the exception</param>
        void LogCritical(string message, params object[] args);

        /// <summary>
        /// Logs an application exception that is not critical.
        /// </summary>
        /// <param name="exception">an exception that caused the failure</param>
        /// <param name="args">any data that needs to be logged along the exception</param>
        void LogError(Exception exception, params object[] args);

        /// <summary>
        /// Logs an application exception that is not critical.
        /// </summary>
        /// <param name="message">an event description to log</param>
        /// <param name="exception">an exception that caused the failure</param>
        /// <param name="args">any data that needs to be logged along the exception</param>
        void LogError(string message, Exception exception, params object[] args);

        /// <summary>
        /// Logs an application exception that is not critical.
        /// </summary>
        /// <param name="message">an event description to log</param>
        /// <param name="args">any data that needs to be logged along the exception</param>
        void LogError(string message, params object[] args);

        /// <summary>
        /// Logs an application exception that could be handled by the application
        /// yet might indicate an application bug.
        /// </summary>
        /// <param name="exception">an exception to log</param>
        /// <param name="args">any data that needs to be logged along the exception</param>
        void LogWarning(Exception exception, params object[] args);

        /// <summary>
        /// Logs an application event that could be handled by the application
        /// yet might indicate an application bug.
        /// </summary>
        /// <param name="message">an event description to log</param>
        /// <param name="args">any data that needs to be logged along the event description</param>
        void LogWarning(string message, params object[] args);

        /// <summary>
        /// Logs an application event that could be of general interest, e.g. user loggen in.
        /// </summary>
        /// <param name="message">an event description to log</param>
        /// <param name="args">any data that needs to be logged along the event description</param>
        void LogInfo(string message, params object[] args);

        /// <summary>
        /// Logs an application exception (typically business logic related),
        /// that could be safely handled either by the application or by the user,
        /// yet could be useful for debugging. 
        /// </summary>
        /// <param name="exception">an exception to log</param>
        /// <param name="args">any data that needs to be logged along the exception</param>
        void LogDebug(Exception exception, params object[] args);

        /// <summary>
        /// Logs an application event that could be useful for debugging.
        /// </summary>
        /// <param name="message">an event description to log</param>
        /// <param name="args">any data that needs to be logged along the event description</param>
        void LogDebug(string message, params object[] args);

        /// <summary>
        /// Logs method entry data for debugging.
        /// </summary>
        /// <param name="ownerType">a type that declares the method</param>
        /// <param name="methodName">a name of the method</param>
        /// <param name="methodParams">parameter values that has been passed to the method</param>
        void LogMethodEntry(Type ownerType, string methodName, params object[] methodParams);

        /// <summary>
        /// Logs method (successful) exit data for debugging.
        /// </summary>
        /// <param name="ownerType">a type that declares the method</param>
        /// <param name="methodName">a name of the method</param>
        /// <param name="returnValue">a value returned by the method</param>
        void LogMethodExit(Type ownerType, string methodName, object returnValue = null);

        /// <summary>
        /// Logs a security related application exception (e.g. forged credentials),
        /// that could be safely handled either by the application or by the user,
        /// yet could be useful for managing app security. 
        /// </summary>
        /// <param name="exception">a security related exception to log</param>
        /// <param name="args">any data that needs to be logged along the exception</param>
        void LogSecurityIssue(Exception exception, params object[] args);

        /// <summary>
        /// Logs a security related event (e.g. failed login attempt)
        /// that could be safely handled either by the application or by the user,
        /// yet could be useful for managing app security.
        /// </summary>
        /// <param name="message">a security related event description to log</param>
        /// <param name="args">any data that needs to be logged along the event description</param>
        void LogSecurityIssue(string message, params object[] args);

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <typeparam name="TState">Begins a logical operation scope.</typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>A disposable object that ends the logical operation scope on dispose.</returns>
        IDisposable BeginScope<TState>(TState state);
    }
}
