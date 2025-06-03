using System;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Retry
{
    /// <summary>
    /// Extension methods for retry functionality.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Executes a <paramref name="method"/> using <paramref name="policy"/>
        /// and <paramref name="exceptionValidator"/>.
        /// </summary>
        /// <param name="policy">a retry policy to use</param>
        /// <param name="exceptionValidator">an exception validator to use</param>
        /// <param name="method">a method to execute</param>
        public static async Task ExecuteAsync(this IRetryPolicy policy,
            ITransientExceptionValidator exceptionValidator, Func<Task> method)
        {
            if (null == policy) throw new ArgumentNullException(nameof(policy));
            if (null == exceptionValidator) throw new ArgumentNullException(nameof(exceptionValidator));
            if (null == method) throw new ArgumentNullException(nameof(method));

            var currentRetry = 0;
            while (currentRetry < policy.RetryAttempts)
            {
                try
                {
                    await method();
                    return;
                }
                catch (Exception ex)
                {
                    currentRetry += 1;
                    if (currentRetry == policy.RetryAttempts || !exceptionValidator.IsTransient(ex))
                        throw;
                }
            }
        }
    }
}
