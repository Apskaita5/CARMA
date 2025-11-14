using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;
using Moq;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules
{
    /// <summary>
    /// Fluent builder for creating IValidationEngine mocks.
    /// </summary>
    public class ValidationEngineMockBuilder
    {
        private readonly Mock<IValidationEngine> _mock;
        private IEntityMetadata _entityMetadata;
        private readonly Dictionary<string, List<BrokenRule>> _propertyRules = new();
        private readonly Dictionary<string, string[]> _dependentProperties = new();
        private readonly List<BrokenRule> _entityLevelRules = new();

        public ValidationEngineMockBuilder()
        {
            _mock = new Mock<IValidationEngine>();
        }

        public ValidationEngineMockBuilder WithEntityMetadata(IEntityMetadata metadata)
        {
            _entityMetadata = metadata;
            return this;
        }

        /// <summary>
        /// Adds a validation rule for a specific property.
        /// </summary>
        public ValidationEngineMockBuilder WithPropertyRule(
            string propertyName,
            BrokenRule rule)
        {
            if (!_propertyRules.ContainsKey(propertyName))
                _propertyRules[propertyName] = new List<BrokenRule>();

            _propertyRules[propertyName].Add(rule);
            return this;
        }

        /// <summary>
        /// Adds a validation rule for a specific property using a builder.
        /// </summary>
        public ValidationEngineMockBuilder WithPropertyRule(
            string propertyName,
            Action<BrokenRuleBuilder> configure)
        {
            var builder = new BrokenRuleBuilder().ForProperty(propertyName);
            configure?.Invoke(builder);

            return WithPropertyRule(propertyName, builder.Build());
        }

        /// <summary>
        /// Adds an entity-level validation rule (not tied to a specific property).
        /// </summary>
        public ValidationEngineMockBuilder WithEntityRule(BrokenRule rule)
        {
            _entityLevelRules.Add(rule);
            return this;
        }

        /// <summary>
        /// Adds an entity-level validation rule using a builder.
        /// </summary>
        public ValidationEngineMockBuilder WithEntityRule(Action<BrokenRuleBuilder> configure)
        {
            var builder = new BrokenRuleBuilder();
            configure?.Invoke(builder);

            _entityLevelRules.Add(builder.Build());
            return this;
        }

        /// <summary>
        /// Defines dependent properties for validation cascading.
        /// </summary>
        public ValidationEngineMockBuilder WithDependentProperties(
            string propertyName,
            params string[] dependentProperties)
        {
            _dependentProperties[propertyName] = dependentProperties;
            return this;
        }

        /// <summary>
        /// Configures the engine to always pass validation (no broken rules).
        /// </summary>
        public ValidationEngineMockBuilder AllValid()
        {
            _propertyRules.Clear();
            _entityLevelRules.Clear();
            return this;
        }

        public IValidationEngine Build()
        {
            // Setup entity metadata
            _mock.Setup(x => x.EntityMetadata).Returns(_entityMetadata);

            // Setup GetBrokenRules for specific property (no dependents)
            _mock.Setup(x => x.GetBrokenRules(It.IsAny<object>(), It.IsAny<string>()))
                .Returns<object, string>((instance, propName) =>
                {
                    return _propertyRules.TryGetValue(propName, out var rules)
                        ? new List<BrokenRule>(rules)
                        : new List<BrokenRule>();
                });

            // Setup GetBrokenRules for specific property (with dependents option)
            _mock.Setup(x => x.GetBrokenRules(
                    It.IsAny<object>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns<object, string, bool>((instance, propName, includeDependents) =>
                {
                    var rules = new List<BrokenRule>();

                    // Add rules for the property itself
                    if (_propertyRules.TryGetValue(propName, out var propRules))
                        rules.AddRange(propRules);

                    // Add rules for dependent properties if requested
                    if (includeDependents && _dependentProperties.TryGetValue(propName, out var deps))
                    {
                        foreach (var dep in deps)
                        {
                            if (_propertyRules.TryGetValue(dep, out var depRules))
                                rules.AddRange(depRules);
                        }
                    }

                    return rules;
                });

            // Setup GetBrokenRules for entity level
            _mock.Setup(x => x.GetBrokenRules(It.IsAny<object>()))
                .Returns(new List<BrokenRule>(_entityLevelRules));

            // Setup GetAllBrokenRules (all rules for entity)
            _mock.Setup(x => x.GetAllBrokenRules(It.IsAny<object>()))
                .Returns(() =>
                {
                    var allRules = new List<BrokenRule>();
                    allRules.AddRange(_entityLevelRules);

                    foreach (var propRules in _propertyRules.Values)
                        allRules.AddRange(propRules);

                    return allRules;
                });

            // Setup GetDependentPropertyNames
            _mock.Setup(x => x.GetDependentPropertyNames(It.IsAny<string>()))
                .Returns<string>(propName =>
                {
                    return _dependentProperties.TryGetValue(propName, out var deps)
                        ? deps
                        : Array.Empty<string>();
                });

            return _mock.Object;
        }

        //public static implicit operator IValidationEngine(ValidationEngineMockBuilder builder)
        //    => builder.Build();
    }
}
