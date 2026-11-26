using A5Soft.CARMA.Application.Authorization.Authorizers;
using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain;
using System.Collections.Generic;
using System.Threading;
using System;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base class for all use cases that are authorized by the attributes of their interface: the app's context
    /// and identity, typed.
    /// </summary>
    /// <remarks>A remote run is authorized by the server data portal; a local one by
    /// <see cref="BeforeLocalRunAsync"/>. The client does not check a remote call in advance: the server decides,
    /// with the current identity.</remarks>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    public abstract class AuthorizedUseCaseBase<TAppContext, TIdentity> : RemoteUseCaseBase<TAppContext>, IAuthorizedUseCase
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        private IUseCaseAuthorizer _authorizer;


        /// <inheritdoc />
        protected AuthorizedUseCaseBase(TAppContext context) : base(context){ }


        /// <summary>Gets the use case's authorization rules, read from its interface.</summary>
        protected IUseCaseAuthorizer Authorizer
            => _authorizer ?? (_authorizer = Context.AuthorizationProvider.GetAuthorizer(GetType()));

        /// <summary>
        /// Gets the scope's identity; null when anonymous, which only a use case marked
        /// <see cref="Authorization.AllowAnonymousAttribute"/> sees.
        /// </summary>
        /// <exception cref="InvalidOperationException">the scope's identity is of another type than
        /// <typeparamref name="TIdentity"/>: the scope was initialized with an identity that is not the app's</exception>
        protected TIdentity Identity
        {
            get
            {
                var identity = Context.Authentication.Identity;
                if (identity is TIdentity typed) return typed;
                if (identity.IsAnonymous) return null;

                throw new InvalidOperationException($"The scope's identity is {identity.GetType().FullName}, " +
                    $"not {typeof(TIdentity).FullName}: initialize the scope with the app's identity type.");
            }
        }


        /// <inheritdoc />
        public bool CanInvoke(bool throwOnNotAuthorized = false)
        {
            if (throwOnNotAuthorized) Authorizer.EnsureCanInvoke(Context.Authentication.Identity);
            return Authorizer.CanInvoke(Context.Authentication.Identity);
        }


        /// <summary>
        /// Authorizes a local run: makes sure that the identity is current, then checks it against the rules.
        /// </summary>
        /// <param name="ct">a token to cancel the operation</param>
        protected override Task BeforeLocalRunAsync(CancellationToken ct)
            => Context.Authentication.AuthorizeAsync(Authorizer, ct);

        /// <summary>
        /// Fetches a lookup list for this use case through the context's <see cref="ILookupService"/>. The use
        /// case's interface must list the lookup in its <see cref="UseCaseAttribute.LookupTypes"/>.
        /// </summary>
        /// <typeparam name="TLookup">the type of the lookup</typeparam>
        /// <typeparam name="TEntity">the type of the entity the lookup refers to</typeparam>
        /// <param name="cancellationToken">a token to cancel the fetch</param>
        /// <returns>the lookup list, read only: it may be cached and shared</returns>
        protected async Task<IReadOnlyList<TLookup>> FetchLookupAsync<TLookup, TEntity>(
            CancellationToken cancellationToken = default)
            where TLookup : LookupBase<TEntity>
            where TEntity : class, IDomainEntity<TEntity>
        {
            var lookups = Context.Lookups ?? throw new InvalidOperationException(
                $"{GetType().Name} requests a lookup, but the application context has no {nameof(ILookupService)}.");

            var result = await lookups.FetchAsync<TLookup, TEntity>(GetType(), cancellationToken)
                .ConfigureAwait(false);

            return result?.AsReadOnly();
        }
    }
}