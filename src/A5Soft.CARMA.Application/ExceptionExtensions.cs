using A5Soft.CARMA.Application.Authorization.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// Resolves the class of an exception (business, not authenticated, transient, fault, cancellation) by searching
    /// its whole hierarchy, as an exception may be wrapped at any stage; and remembers that a boundary has logged it.
    /// </summary>
    /// <remarks>
    /// <para>Resolve in this order; the first match wins: <see cref="IsCancellation"/>,
    /// <see cref="ToNotAuthenticatedException"/>, <see cref="Domain.Extensions.ToBusinessException"/>,
    /// <see cref="Retry.Extensions.ToTransientException"/>; anything else is a fault.</para>
    /// <para>Each exception is logged once, at the first boundary it reaches (<see cref="IsLogged"/>).</para>
    /// </remarks>
    public static class ExceptionExtensions
    {
        private const string LoggedKey = "A5Soft.CARMA.Logged";
        private const string CorrelationIdKey = "A5Soft.CARMA.CorrelationId";


        /// <summary>
        /// Gets the exception and every exception it wraps, outermost first.
        /// </summary>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static IEnumerable<Exception> GetHierarchy(this Exception exception)
        {
            if (null == exception) yield break;
            yield return exception;

            var children = exception is AggregateException aggregate
                ? aggregate.InnerExceptions
                : (IEnumerable<Exception>)new[] { exception.InnerException };

            foreach (var child in children)
                foreach (var nested in child.GetHierarchy())
                    yield return nested;
        }

        /// <summary>
        /// Gets the outermost <see cref="NotAuthenticatedException"/> in the hierarchy; null if there is none.
        /// </summary>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static NotAuthenticatedException ToNotAuthenticatedException(this Exception exception)
            => exception.GetHierarchy().OfType<NotAuthenticatedException>().FirstOrDefault();

        /// <summary>
        /// Gets a value indicating whether the exception is a cancellation: the caller's token is cancelled and the
        /// hierarchy holds an <see cref="OperationCanceledException"/>. One without a cancelled token is a timeout,
        /// a fault unless the infrastructure has classified it.
        /// </summary>
        /// <param name="exception">the exception</param>
        /// <param name="cancellationToken">the caller's token</param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static bool IsCancellation(this Exception exception, CancellationToken cancellationToken)
            => cancellationToken.IsCancellationRequested
            && exception.GetHierarchy().Any(e => e is OperationCanceledException);

        /// <summary>
        /// Gets a value indicating whether a boundary has logged the exception, or one it wraps (or the server, for
        /// one the client data portal has re-created).
        /// </summary>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static bool IsLogged(this Exception exception)
            => exception.GetHierarchy().Any(e => e.Data.Contains(LoggedKey));

        /// <summary>
        /// Gets the id the exception, or one it wraps, was logged under; null if none.
        /// </summary>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static string GetCorrelationId(this Exception exception)
            => exception.GetHierarchy().Select(e => e.Data[CorrelationIdKey] as string).FirstOrDefault(id => null != id);

        /// <summary>
        /// Marks the exception as logged, so that no other boundary logs it again.
        /// </summary>
        /// <param name="exception">the exception</param>
        /// <param name="correlationId">the id it was logged under, if any</param>
        /// <returns>the exception</returns>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static TException MarkLogged<TException>(this TException exception, string correlationId = null)
            where TException : Exception
        {
            if (null == exception) throw new ArgumentNullException(nameof(exception));

            exception.Data[LoggedKey] = true;
            if (!string.IsNullOrEmpty(correlationId)) exception.Data[CorrelationIdKey] = correlationId;
            return exception;
        }
    }
}