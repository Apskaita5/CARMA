using System;

namespace A5Soft.CARMA.Application.Navigation
{
    /// <summary>
    /// Marks a form or page of the GUI (never a control) as the one that opens a use case: the GUI opens the use case
    /// with the form the <see cref="IFormResolver"/> gives for it. Read by the startup scan from the app's assemblies
    /// and from the plugins and their UI companions (<see cref="FormRegistry"/>).
    /// </summary>
    /// <remarks>
    /// <para>One form may serve several use cases (e.g. create and update). A use case has at most one form of the app
    /// per kind (page or dialog).</para>
    /// <para>A plugin's form for an app use case overrides the app's for the tenants the plugin is assigned to. It must
    /// keep the use case's contract: the same navigation parameters and no more permissions than the use case requires.
    /// A plugin's form for its own use case is the only one for it.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ServesUseCaseAttribute : Attribute
    {
        /// <summary>
        /// Marks the form as the one that opens a use case.
        /// </summary>
        /// <param name="useCaseType">the use case interface, remote or local</param>
        public ServesUseCaseAttribute(Type useCaseType)
        {
            UseCaseType = useCaseType ?? throw new ArgumentNullException(nameof(useCaseType));
        }


        /// <summary>Gets the use case interface the form opens.</summary>
        public Type UseCaseType { get; }

        /// <summary>
        /// Gets or sets whether the form is a dialog, shown over the current form and returning a result, rather than
        /// a page or window of its own. A use case may have both. Default: false.
        /// </summary>
        public bool AsDialog { get; set; }
    }
}
