using A5Soft.CARMA.Application.Authorization.Authorizers;
using A5Soft.CARMA.Application.Retry;
using A5Soft.CARMA.Domain;
using System;
using System.Threading;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// The use case boundary: classifies a failure, logs it once and translates a transient one into a business
    /// exception. Used by the use case bases and the lookup service, in both branches.
    /// </summary>
    internal static class UseCaseBoundary
    {
        /// <summary>
        /// Handles a failure at the boundary.
        /// </summary>
        /// <param name="exception">the failure</param>
        /// <param name="cancellationToken">the caller's token</param>
        /// <param name="logger">the logger</param>
        /// <param name="factory">the app's business exception factory</param>
        /// <param name="source">the use case or service, for the log</param>
        /// <param name="method">the method, for the log</param>
        /// <returns>the exception to throw: the same one (rethrow it), or the business exception a transient
        /// failure becomes</returns>
        public static Exception Handle(Exception exception, CancellationToken cancellationToken, ILogger logger,
            IBusinessExceptionFactory factory, Type source, string method)
        {
            // 5: the caller has stopped waiting
            if (exception.IsCancellation(cancellationToken)) return exception;

            var logged = exception.IsLogged();

            // 2 and 1: the user's to resolve
            if (null != exception.ToNotAuthenticatedException() || null != exception.ToBusinessException())
            {
                if (logged) return exception;

                if (exception.ToBusinessException() is NotAuthorizedExceptionBase
                    && null == exception.ToNotAuthenticatedException())
                    logger?.LogSecurityIssue(exception);
                else logger?.LogInfo($"{source.Name}.{method}: {exception.GetType().Name}", exception);   // the full tree

                return exception.MarkLogged();
            }

            // 3: wrapped into 1, so that the user reads it and retry logic still finds it
            var transient = exception.ToTransiantException();
            if (null != transient)
            {
                if (!logged) logger?.LogWarning(exception);

                var business = factory?.ServiceUnavailable(transient, exception) ?? throw new InvalidOperationException(
                    $"{nameof(IBusinessExceptionFactory)}.{nameof(IBusinessExceptionFactory.ServiceUnavailable)} " +
                    "has returned no exception.", exception);
                return business.MarkLogged(exception.GetCorrelationId());
            }

            // 4: handled by no one
            if (!logged)
            {
                var correlationId = Guid.NewGuid().ToString("N");
                logger?.LogError($"{source.Name}.{method} has failed, correlation id {correlationId}.", exception);
                exception.MarkLogged(correlationId);
            }

            return exception;
        }
    }
}
