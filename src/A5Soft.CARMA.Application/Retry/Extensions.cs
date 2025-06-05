using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Retry
{
    /// <summary>
    /// Extension methods for retry functionality.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Executes a <paramref name="method"/> using <paramref name="policy"/>.
        /// </summary>
        /// <param name="policy">a retry policy to use</param>
        /// <param name="method">a method to execute</param>
        /// <remarks><paramref name="method"/> is expected to throw <see cref="TransiantException"/>
        /// if the error cause is transiant</remarks>
        public static async Task ExecuteAsync(this IRetryPolicy policy,
            Func<Task> method, CancellationToken ct = default)
        {
            if (null == policy) throw new ArgumentNullException(nameof(policy));
            if (null == method) throw new ArgumentNullException(nameof(method));

            var currentRetry = 0;
            while (currentRetry <= policy.RetryAttempts)
            {
                try
                {
                    if (ct.IsCancellationRequested) throw new TaskCanceledException();
                    await method();
                    return;
                }
                catch (Exception ex)
                {
                    currentRetry += 1;
                    if (currentRetry > policy.RetryAttempts || ct.IsCancellationRequested
                        || !ex.IsTransiantException()) throw;
                    await Task.Delay(policy.GetDelayForRetry(currentRetry), ct);
                }
            }
        }

        /// <summary>
        /// Executes a <paramref name="method"/> using <paramref name="policy"/>
        /// and <paramref name="validator"/>.
        /// </summary>
        /// <param name="policy">a retry policy to use</param>
        /// <param name="validator">a validator to use</param>
        /// <param name="method">a method to execute</param>
        public static async Task ExecuteAsync(this IRetryPolicy policy,
            ITransientExceptionValidator validator, Func<Task> method,
            CancellationToken ct = default)
        {
            if (null == policy) throw new ArgumentNullException(nameof(policy));
            if (null == method) throw new ArgumentNullException(nameof(method));
            if (null == validator) throw new ArgumentNullException(nameof(validator));

            await policy.ExecuteAsync(async () =>
            {
                try
                {
                    await method();
                }
                catch (Exception ex)
                {
                    if (ex.IsTransiantException(validator)) throw new TransiantException(ex);
                    throw;
                }
            }, ct);
        }

        /// <summary>
        /// Traverses the exception hierarchy and returns true if there is any
        /// <see cref="TransiantException"/> in the exception hierarchy.
        /// </summary>
        /// <param name="ex"></param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static bool IsTransiantException(this Exception ex)
        {
            if (null == ex) throw new ArgumentNullException(nameof(ex));

            if (ex is TransiantException bex) return true;

            if (ex is AggregateException aex)
            {
                foreach (var e in aex.InnerExceptions)
                {
                    var result = e.IsTransiantException();
                    if (result) return true;
                }
            }
            else if (null != ex.InnerException)
            {
                return ex.InnerException.IsTransiantException();
            }

            return false;
        }

        /// <summary>
        /// Traverses the exception hierarchy and returns the first exception of type
        /// <see cref="TransiantException"/>. If no such exception, returns null.
        /// </summary>
        /// <param name="ex"></param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static TransiantException ToTransiantException(this Exception ex)
        {
            if (null == ex) throw new ArgumentNullException(nameof(ex));

            if (ex is TransiantException bex) return bex;

            if (ex is AggregateException aex)
            {
                foreach (var e in aex.InnerExceptions)
                {
                    var result = e.ToTransiantException();
                    if (null != result) return result;
                }
            }
            else if (null != ex.InnerException)
            {
                return ex.InnerException.ToTransiantException();
            }

            return null;
        }

        /// <summary>
        /// Traverses the exception hierarchy and returns true if there is any
        /// transient error in the exception hierarchy.
        /// </summary>
        /// <param name="ex">an exception to inspect</param>
        /// <param name="validator">a validator to use</param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static bool IsTransiantException(this Exception ex, ITransientExceptionValidator validator)
        {
            if (null == ex) throw new ArgumentNullException(nameof(ex));
            if (null == validator) throw new ArgumentNullException(nameof(validator));

            if (validator.IsTransient(ex)) return true;

            if (ex is AggregateException aex)
            {
                foreach (var e in aex.InnerExceptions)
                {
                    var result = e.IsTransiantException(validator);
                    if (result) return true;
                }
            }
            else if (null != ex.InnerException)
            {
                return ex.InnerException.IsTransiantException(validator);
            }

            return false;
        }
    }
}
