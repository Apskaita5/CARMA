using A5Soft.CARMA.Application.Authorization.Identity;
using System;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Application.UseCases.Files
{
    /// <summary>
    /// A file upload, e.g. an import: the parameter carries the file(s) as <see cref="Domain.Files.FileContent"/>.
    /// </summary>
    /// <remarks>
    /// <para>The caller keeps owning the files it sends. On the server, the use case gets the files once they
    /// have all arrived and passed the checks; they are deleted after the call, so the use case copies what it
    /// keeps.</para>
    /// <para>Only <see cref="GetLoggedArgument"/> reaches the log, never the files.</para>
    /// </remarks>
    /// <typeparam name="TAppContext">the app's application context interface</typeparam>
    /// <typeparam name="TIdentity">the app's identity type</typeparam>
    /// <typeparam name="TParameter">the type of the parameter, carrying the file(s)</typeparam>
    /// <typeparam name="TResult">the type of the result, e.g. a summary of the import</typeparam>
    public abstract class UploadFileUseCaseBase<TAppContext, TIdentity, TParameter, TResult> :
        AuthorizedUseCaseBase<TAppContext, TIdentity>
        where TAppContext : class, IApplicationContext
        where TIdentity : CarmaIdentity
    {
        /// <inheritdoc />
        protected UploadFileUseCaseBase(TAppContext context) : base(context) { }


        /// <summary>
        /// Uploads the file(s).
        /// </summary>
        /// <param name="parameter">the parameter, carrying the file(s)</param>
        /// <returns>the result</returns>
        public Task<TResult> UploadAsync(TParameter parameter)
        {
            if (null == parameter) throw new ArgumentNullException(nameof(parameter));

            return RunAsync(dp => dp.FetchAsync<TParameter, TResult>(GetType(), parameter),
                () => DoUploadAsync(parameter), GetLoggedArgument(parameter));
        }


        /// <summary>
        /// Gets what is logged of the parameter, e.g. the file names; null (the default) for nothing.
        /// </summary>
        /// <param name="parameter">the parameter</param>
        protected virtual object GetLoggedArgument(TParameter parameter) => null;

        /// <summary>
        /// Processes the file(s). The parameter comes from the client: do not trust it.
        /// </summary>
        /// <param name="parameter">the parameter, carrying the file(s)</param>
        protected abstract Task<TResult> DoUploadAsync(TParameter parameter);
    }
}
