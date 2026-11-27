using System;

namespace A5Soft.CARMA.Application.Navigation.Forms
{
    /// <summary>
    /// A form serving a use case (<see cref="ServesUseCaseAttribute"/>), found by the startup scan.
    /// </summary>
    public sealed class FormRegistration
    {
        internal FormRegistration(Type useCaseType, Type formType, bool isDialog, string pluginId)
        {
            UseCaseType = useCaseType;
            FormType = formType;
            IsDialog = isDialog;
            PluginId = pluginId;
        }


        /// <summary>Gets the use case interface the form opens.</summary>
        public Type UseCaseType { get; }

        /// <summary>Gets the type of the form, page or dialog: the GUI creates it.</summary>
        public Type FormType { get; }

        /// <summary>Gets whether the form is a dialog.</summary>
        public bool IsDialog { get; }

        /// <summary>Gets the identifier of the plugin the form comes from; null for the app's own.</summary>
        public string PluginId { get; }


        /// <inheritdoc />
        public override string ToString()
            => $"{FormType.FullName} for {UseCaseType.FullName}{(IsDialog ? " (dialog)" : string.Empty)}" +
               $"{(null == PluginId ? string.Empty : $", plugin {PluginId}")}";
    }
}
