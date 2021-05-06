using System;
using System.Security.Claims;
using System.Threading.Tasks;
using A5Soft.CARMA.Application.Authorization;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Domain;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Application
{
    /// <summary>
    /// A base use case for a file upload operation with options.
    /// </summary>
    /// <typeparam name="TOptions">a type of options (must be json serializable,
    /// i.e. either a primitive or a POCO)</typeparam>
    public abstract class UploadFileWithOptionsUseCaseBase<TOptions> : AuthorizedUseCaseBase
    {
        /// <inheritdoc />
        protected UploadFileWithOptionsUseCaseBase(IAuthenticationStateProvider authenticationStateProvider, 
            IAuthorizationProvider authorizer, IClientDataPortal dataPortal, 
            IValidationEngineProvider validationProvider, IMetadataProvider metadataProvider, ILogger logger) 
            : base(authenticationStateProvider, authorizer, dataPortal, validationProvider, metadataProvider, logger) { }


        /// <summary>
        /// Uploads a file using the options specified.
        /// </summary>
        /// <param name="options">options that affect file save behaviour,
        /// e.g. to associate the file with some domain entity</param>
        /// <param name="file">a file to upload</param>
        public async Task UploadFileAsync(FileContent file, TOptions options)
        {
            if (file.IsNull()) throw new ArgumentNullException(nameof(file));

            Logger.LogMethodEntry(this.GetType(), nameof(UploadFileAsync), options);

            if (DataPortal.IsRemote)
            {
                try
                {
                    await BeforeDataPortalAsync(file, options);
                    await DataPortal.InvokeAsync(this.GetType(), file, options, await GetIdentityAsync());
                    await AfterDataPortalAsync(file, options);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                    throw;
                }

                Logger.LogMethodExit(this.GetType(), nameof(UploadFileAsync));

                return;
            }

            if (Authorizer.AuthorizationImplementedForParam<TOptions>())
                Authorizer.IsAuthorized(await GetIdentityAsync(), options, true);
            else await CanInvokeAsync(true);

            try
            {
                await SaveAsync(file, options);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                throw;
            }

            Logger.LogMethodExit(this.GetType(), nameof(UploadFileAsync));
        }

        /// <summary>
        /// Implement this method to save the uploaded file.
        /// If required, implement any post save actions as well, e.g. resetting cache for relevant lookups,
        /// sending notifications etc.
        /// </summary>
        /// <param name="file">a file to save</param>
        /// <param name="options">options that affect file save behaviour,
        /// e.g. to associate the file with some domain entity</param>
        /// <remarks>At this stage user has already been authorized
        /// and the file param is guaranteed to be not null.
        /// The options param is NOT guaranteed to be not null (as it could be a valid option).
        /// This method is always executed on server side (if data portal is configured).</remarks>
        protected abstract Task SaveAsync(FileContent file, TOptions options);

        /// <summary>
        /// Implement this method for any actions that should be taken before remote invocation.
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task BeforeDataPortalAsync(FileContent file, TOptions options)
            => Task.CompletedTask;

        /// <summary>
        /// Implement this method for any actions that should be taken after
        /// a successful remote invocation. (e.g. clear local cache)
        /// Only invoked if a remote data portal is used. 
        /// </summary>
        protected virtual Task AfterDataPortalAsync(FileContent file, TOptions options)
            => Task.CompletedTask;


        /// <summary>
        /// Gets metadata for the upload options.
        /// Returns null if the upload options is not a class or an interface.
        /// </summary>
        public IEntityMetadata GetOptionsMetadata()
        {
            var criteriaType = typeof(TOptions);
            if (!criteriaType.IsInterface && !criteriaType.IsClass) return null;
            if (criteriaType == typeof(string)) return null;
            return MetadataProvider.GetEntityMetadata(criteriaType);
        }

        /// <summary>
        /// Validates upload options (as a POCO object) and returns a broken rules collection
        /// that can be used to determine whether the options are valid and what are the
        /// broken rules (if invalid).
        /// </summary>
        /// <param name="options">upload options to validate</param>
        /// <remarks>Override this method in order to implement custom validation
        /// or disable validation by returning a new (empty) <see cref="BrokenRulesCollection"/>.</remarks>
        public virtual BrokenRulesCollection Validate(TOptions options)
        {
            return ValidationProvider.ValidatePoco(options);
        }

    }
}
