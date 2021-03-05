using System;
using System.Collections.Concurrent;

namespace A5Soft.CARMA.Application.Authorization.Default
{
    /// <summary>
    /// Default implementation of IAuthorizationProvider using authorization attributes.
    /// </summary>
    public class DefaultAuthorizationProvider : IAuthorizationProvider
    {
        private static readonly ConcurrentDictionary<Type, UseCaseAuthorizer> _cache
            = new ConcurrentDictionary<Type, UseCaseAuthorizer>();

        private readonly ILogger _logger;


        public DefaultAuthorizationProvider(ILogger logger)
        {
            _logger = logger;
        }


        /// <inheritdoc cref="IAuthorizationProvider.GetAuthorizer" />
        public IUseCaseAuthorizer GetAuthorizer(Type useCaseType)
        {
            if (null == useCaseType) throw new ArgumentNullException(nameof(useCaseType));
            return GetDefaultAuthorizer(useCaseType, _logger);
        }

        /// <inheritdoc cref="IAuthorizationProvider.GetAuthorizer" />
        public IUseCaseAuthorizer GetAuthorizer<T>()
        {
            return GetDefaultAuthorizer<T>(_logger);
        }


        private static IUseCaseAuthorizer GetDefaultAuthorizer(Type useCaseType, ILogger logger)
        {
            if (null == useCaseType) throw new ArgumentNullException(nameof(useCaseType));
            return _cache.GetOrAdd(useCaseType, t => new UseCaseAuthorizer(t, logger));
        }

        private static IUseCaseAuthorizer GetDefaultAuthorizer<T>(ILogger logger)
        {
            return GetDefaultAuthorizer(typeof(T), logger);
        }

    }
}
