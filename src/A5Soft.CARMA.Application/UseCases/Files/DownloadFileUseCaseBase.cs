using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Domain.Files;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Files
{
    /// <summary>
    /// A file download: a report, an export. Does not change state, so it can be cancelled. Interface method:
    /// <c>Task&lt;FileContent&gt; DownloadAsync(TParameter parameter,
    /// CancellationToken cancellationToken = default)</c>.
    /// </summary>
    /// <remarks>The caller disposes the file. A remote download is streamed as it arrives.</remarks>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TParameter">the type of the parameter, e.g. the report's criteria</typeparam>
    public abstract class DownloadFileUseCaseBase<TAppContext, TIdentity, TParameter>
        : AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected DownloadFileUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <param name="parameter">the parameter</param>
        /// <param name="cancellationToken">a token to cancel the download</param>
        /// <returns>the file; the caller disposes it</returns>
        public Task<FileContent> DownloadAsync(TParameter parameter, CancellationToken cancellationToken = default)
        {
            if (null == parameter) throw new ArgumentNullException(nameof(parameter));

            return RunReadOnlyAsync((dp, ct) => dp.FetchAsync<TParameter, FileContent>(GetType(), parameter, ct),
                ct => DoDownloadAsync(parameter, ct), cancellationToken, parameter);
        }


        /// <summary>
        /// Creates the file content. The parameter comes from the client: do not trust it.
        /// </summary>
        /// <param name="parameter">the parameter</param>
        /// <param name="cancellationToken">a token to cancel the operation</param>
        protected abstract Task<FileContent> DoDownloadAsync(TParameter parameter, CancellationToken cancellationToken);
    }
}
