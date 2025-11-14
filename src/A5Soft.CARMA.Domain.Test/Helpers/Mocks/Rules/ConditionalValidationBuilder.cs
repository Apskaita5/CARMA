using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Rules;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules
{
    /// <summary>
    /// Builder for creating conditional validation scenarios.
    /// </summary>
    public class ConditionalValidationBuilder
    {
        private readonly ValidationEngineMockBuilder _builder;
        private readonly Dictionary<Func<object, bool>, List<BrokenRule>> _conditions = new();

        public ConditionalValidationBuilder()
        {
            _builder = new ValidationEngineMockBuilder();
        }

        public ConditionalValidationBuilder WithEntityMetadata(IEntityMetadata metadata)
        {
            _builder.WithEntityMetadata(metadata);
            return this;
        }

        /// <summary>
        /// Adds rules that apply when condition is true.
        /// </summary>
        public ConditionalValidationBuilder When(
            Func<object, bool> condition,
            params BrokenRule[] rules)
        {
            _conditions[condition] = rules.ToList();
            return this;
        }

        /// <summary>
        /// Builds a validation engine that evaluates conditions.
        /// </summary>
        public IValidationEngine Build()
        {
            var mock = new Mock<IValidationEngine>();

            // Setup to evaluate conditions
            mock.Setup(x => x.GetAllBrokenRules(It.IsAny<object>()))
                .Returns<object>(instance =>
                {
                    var rules = new List<BrokenRule>();

                    foreach (var condition in _conditions)
                    {
                        if (condition.Key(instance))
                            rules.AddRange(condition.Value);
                    }

                    return rules;
                });

            return mock.Object;
        }
    }
}
