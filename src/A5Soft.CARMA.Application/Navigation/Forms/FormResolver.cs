using A5Soft.CARMA.Application.Authorization.Identity;
using A5Soft.CARMA.Application.IoC.Plugins;
using A5Soft.CARMA.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Application.Navigation.Forms
{
    /// <summary>
    /// The framework's <see cref="IFormResolver"/>.
    /// </summary>
    [DefaultServiceImplementation(typeof(IFormResolver))]
    public sealed class FormResolver : IFormResolver
    {
        private readonly IAuthenticationStateProvider _authentication;
        private readonly PluginCatalog _plugins;
        private readonly FormRegistry _forms;


        /// <summary>
        /// Creates the resolver of a scope.
        /// </summary>
        /// <param name="authentication">the scope's identity</param>
        /// <param name="plugins">the installed plugins</param>
        /// <param name="forms">the registered forms</param>
        public FormResolver(IAuthenticationStateProvider authentication, PluginCatalog plugins, FormRegistry forms)
        {
            _authentication = authentication ?? throw new ArgumentNullException(nameof(authentication));
            _plugins = plugins ?? throw new ArgumentNullException(nameof(plugins));
            _forms = forms ?? throw new ArgumentNullException(nameof(forms));
        }


        /// <inheritdoc />
        public Type GetForm(Type useCaseType, bool dialog = false)
        {
            var candidates = _forms.GetCandidates(useCaseType, dialog);
            if (candidates.Count < 1) return null;

            // a plugin of the scope first
            var scopePlugins = new HashSet<string>(_plugins.GetScopePlugins(_authentication.Identity).Select(p => p.Id),
                StringComparer.OrdinalIgnoreCase);
            var byPlugin = candidates.Where(c => null != c.PluginId && scopePlugins.Contains(c.PluginId)).ToList();

            if (byPlugin.Count > 1) throw new InvalidOperationException(
                $"Plugins {string.Join(" and ", byPlugin.Select(c => c.PluginId))} both serve {useCaseType.FullName}" +
                $"{(dialog ? " as a dialog" : string.Empty)}: assign at most one of them to a tenant.");
            if (byPlugin.Count == 1) return byPlugin[0].FormType;

            // then the app's
            return candidates.FirstOrDefault(c => null == c.PluginId)?.FormType;
        }

        /// <inheritdoc />
        public bool HasForm(Type useCaseType, bool dialog = false) => null != GetForm(useCaseType, dialog);
    }
}
