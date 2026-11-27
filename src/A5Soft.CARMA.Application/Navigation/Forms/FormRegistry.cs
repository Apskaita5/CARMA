using A5Soft.CARMA.Application.IoC;
using A5Soft.CARMA.Application.IoC.Plugins;
using A5Soft.CARMA.Domain.UnitTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace A5Soft.CARMA.Application.Navigation.Forms
{
    /// <summary>
    /// Every form serving a use case (<see cref="ServesUseCaseAttribute"/>), of the app and of the plugins. Built by
    /// <see cref="IoCManagerBase"/> at startup and added to the container; the scope's form is chosen by the
    /// <see cref="IFormResolver"/>. Immutable.
    /// </summary>
    /// <remarks>Startup refuses a form serving something that is not a use case interface, a plugin's form for
    /// another plugin's use case, and two forms of the same owner (the app, or one plugin) for the same use case and
    /// kind.</remarks>
    public sealed class FormRegistry
    {
        private readonly Dictionary<(Type UseCase, bool IsDialog), IReadOnlyList<FormRegistration>> _forms;


        private FormRegistry(IReadOnlyList<FormRegistration> registrations, IReadOnlyList<Assembly> pluginUiAssemblies)
        {
            Registrations = registrations;
            PluginUiAssemblies = pluginUiAssemblies;
            _forms = registrations.GroupBy(r => (r.UseCaseType, r.IsDialog))
                .ToDictionary(g => g.Key, g => (IReadOnlyList<FormRegistration>)g.ToList());
        }


        /// <summary>Gets every registration.</summary>
        public IReadOnlyList<FormRegistration> Registrations { get; }

        /// <summary>
        /// Gets the plugins' assemblies holding UI: every UI companion, and any plugin library holding forms; in plugin
        /// identifier order. The GUI registers them where its framework finds pages, e.g. Blazor's
        /// <c>Router.AdditionalAssemblies</c> (<c>@inject FormRegistry Forms</c> in App.razor). The app's own
        /// assemblies are not included.
        /// </summary>
        public IReadOnlyList<Assembly> PluginUiAssemblies { get; }


        /// <summary>
        /// Gets the forms registered for a use case and kind: the app's and every plugin's.
        /// </summary>
        /// <param name="useCaseType">the use case interface</param>
        /// <param name="dialog">whether dialogs or pages</param>
        public IReadOnlyList<FormRegistration> GetCandidates(Type useCaseType, bool dialog = false)
        {
            if (null == useCaseType) throw new ArgumentNullException(nameof(useCaseType));
            return _forms.TryGetValue((useCaseType, dialog), out var forms) ? forms : Array.Empty<FormRegistration>();
        }


        /// <summary>
        /// Builds the registry from the scanned form types.
        /// </summary>
        /// <exception cref="InvalidOperationException">the message lists every violation</exception>
        internal static FormRegistry Create(IEnumerable<Type> appFormTypes,
            IEnumerable<(PluginAssembly Plugin, IEnumerable<Type> FormTypes)> pluginForms)
        {
            var violations = new List<ConventionViolation>();
            var registrations = new List<FormRegistration>();
            var plugins = pluginForms.ToList();
            var pluginAssemblies = new HashSet<Assembly>(plugins.Select(p => p.Plugin.Assembly));

            void Add(Type formType, PluginAssembly plugin)
            {
                foreach (var attribute in formType.GetCustomAttributes<ServesUseCaseAttribute>(false))
                {
                    var useCase = attribute.UseCaseType;
                    if (!useCase.IsInterface || !(useCase.IsDefined(typeof(UseCaseAttribute), false)
                        || useCase.IsDefined(typeof(LocalUseCaseAttribute), false)))
                    {
                        violations.Add(new ConventionViolation(formType, string.Empty,
                            $"serves {useCase.FullName}, which is not a use case interface"));
                        continue;
                    }

                    if (null != plugin && pluginAssemblies.Contains(useCase.Assembly) && useCase.Assembly != plugin.Assembly)
                    {
                        violations.Add(new ConventionViolation(formType, string.Empty,
                            $"serves {useCase.FullName} of another plugin: a plugin serves its own use cases and the app's"));
                        continue;
                    }

                    var registration = new FormRegistration(useCase, formType, attribute.AsDialog, plugin?.Id);

                    var same = registrations.FirstOrDefault(r => r.UseCaseType == useCase
                        && r.IsDialog == attribute.AsDialog && r.PluginId == registration.PluginId);
                    if (null != same) violations.Add(new ConventionViolation(formType, string.Empty,
                        $"serves {useCase.FullName} as {same.FormType.FullName} does already" +
                        $"{(attribute.AsDialog ? " as a dialog" : string.Empty)}"));
                    else registrations.Add(registration);
                }
            }

            foreach (var type in appFormTypes.Distinct()) Add(type, null);
            foreach (var (plugin, types) in plugins)
                foreach (var type in types.Distinct()) Add(type, plugin);

            if (violations.Count > 0) throw new InvalidOperationException(
                ConventionViolation.Format("The forms cannot be registered", violations));

            var pluginUiAssemblies = plugins.OrderBy(p => p.Plugin.Id, StringComparer.OrdinalIgnoreCase)
                .SelectMany(p => new[] { p.Plugin.Assembly, p.Plugin.UiAssembly }
                    .Where(a => null != a && (a == p.Plugin.UiAssembly || registrations.Any(r => r.FormType.Assembly == a))))
                .Distinct().ToList();

            return new FormRegistry(registrations, pluginUiAssemblies);
        }
    }
}
