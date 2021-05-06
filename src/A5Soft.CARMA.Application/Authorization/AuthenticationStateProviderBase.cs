using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.Authorization
{
    /// <summary>
    /// a base class for <see cref="IAuthenticationStateProvider"/> implementations.
    /// </summary>
    public abstract class AuthenticationStateProviderBase : IAuthenticationStateProvider, IDisposable
    {
        private Task<ClaimsIdentity> _authenticationStateTask;
        private CancellationTokenSource _loopCancellationTokenSource = new CancellationTokenSource();


        /// <inheritdoc cref="IAuthenticationStateProvider.IdentityChanged"/>
        public event IdentityChangedHandler IdentityChanged;

        /// <inheritdoc cref="IAuthenticationStateProvider.GetIdentityAsync"/>
        public Task<ClaimsIdentity> GetIdentityAsync()
        {
            if (null == _authenticationStateTask)
            {
                _authenticationStateTask = TryGetIdentityFromEnvironmentAsync();
                if (null != _authenticationStateTask)
                {
                    _loopCancellationTokenSource?.Cancel();
                    _loopCancellationTokenSource = new CancellationTokenSource();
                    _ = RevalidationLoop(_authenticationStateTask, _loopCancellationTokenSource.Token);
                }
            }

            return _authenticationStateTask ?? Task.FromResult(new ClaimsIdentity());
        }
          
        /// <inheritdoc cref="IAuthenticationStateProvider.NotifyIdentityChanged"/>
        public void NotifyIdentityChanged(ClaimsIdentity updatedIdentity)
        {
            if (null == updatedIdentity) throw new ArgumentNullException(nameof(updatedIdentity));
            _authenticationStateTask = Task.FromResult(updatedIdentity);

            IdentityChanged?.Invoke(_authenticationStateTask);

            _loopCancellationTokenSource?.Cancel();
            _loopCancellationTokenSource = new CancellationTokenSource();
            _ = RevalidationLoop(_authenticationStateTask, _loopCancellationTokenSource.Token);
        }


        /// <summary>
        /// Gets the interval between revalidation attempts.
        /// </summary>
        protected abstract TimeSpan RevalidationInterval { get; }

        /// <summary>
        /// Determines whether the <see cref="ClaimsIdentity"/> for the current user is still valid.
        /// </summary>
        /// <param name="identity">The current <see cref="ClaimsIdentity"/>.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> to observe while performing the operation.</param>
        /// <returns>A <see cref="Task"/> that resolves as true if the <paramref name="identity"/> is still valid,
        /// or false if it is not.</returns>
        protected abstract Task<bool> ValidateIdentityAsync(ClaimsIdentity identity, CancellationToken ct);

        /// <summary>
        /// Tries to get the current user identity from the app environment (e.g. cookie).
        /// </summary>
        /// <returns>current user identity (if any) from the app environment</returns>
        /// <remarks>Never throw exceptions, return null if failed</remarks>
        protected abstract Task<ClaimsIdentity> TryGetIdentityFromEnvironmentAsync();

        private async Task RevalidationLoop(Task<ClaimsIdentity> authenticationStateTask, CancellationToken ct)
        {
            try
            {
                var identity = await authenticationStateTask;
                if (identity.IsAuthenticated)
                {
                    while (!ct.IsCancellationRequested)
                    {
                        bool isValid;

                        try
                        {
                            await Task.Delay(RevalidationInterval, ct);
                            isValid = await ValidateIdentityAsync(identity, ct);
                        }
                        catch (TaskCanceledException tce)
                        {
                            // If it was our cancellation token, then this revalidation loop gracefully completes
                            // Otherwise, treat it like any other failure
                            if (tce.CancellationToken == ct)
                            {
                                break;
                            }
                            throw;
                        }

                        if (!isValid)
                        {
                            NotifyIdentityChanged(new ClaimsIdentity());
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NotifyIdentityChanged(new ClaimsIdentity()); 
            }
        }

        void IDisposable.Dispose()
        {
            _loopCancellationTokenSource?.Cancel();
            Dispose(disposing: true);
        }

        /// <inheritdoc />
        protected virtual void Dispose(bool disposing)
        {
        }

    }
}
