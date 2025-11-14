using A5Soft.CARMA.Domain.Rules;
using Moq;
using System;
using System.Collections.Generic;

namespace A5Soft.CARMA.Domain.Test.Helpers.Mocks.Rules
{
    /// <summary>
    /// Fluent builder for creating IValidationEngineProvider mocks.
    /// </summary>
    public class ValidationEngineProviderMockBuilder
    {
        private readonly Mock<IValidationEngineProvider> _mock;
        private readonly Dictionary<Type, IValidationEngine> _engines = new();
        private IValidationEngine _defaultEngine;

        public ValidationEngineProviderMockBuilder()
        {
            _mock = new Mock<IValidationEngineProvider>();
        }

        /// <summary>
        /// Registers a validation engine for a specific entity type.
        /// </summary>
        public ValidationEngineProviderMockBuilder WithEngine(
            Type entityType,
            IValidationEngine engine)
        {
            _engines[entityType] = engine;
            return this;
        }

        /// <summary>
        /// Registers a validation engine for a specific entity type using a builder.
        /// </summary>
        public ValidationEngineProviderMockBuilder WithEngine<TEntity>(
            Action<ValidationEngineMockBuilder> configure = null)
        {
            var builder = new ValidationEngineMockBuilder();
            configure?.Invoke(builder);

            _engines[typeof(TEntity)] = builder.Build();
            return this;
        }

        /// <summary>
        /// Sets a default engine to return for any unregistered type.
        /// </summary>
        public ValidationEngineProviderMockBuilder WithDefaultEngine(IValidationEngine engine)
        {
            _defaultEngine = engine;
            return this;
        }

        /// <summary>
        /// Configures provider to return engines that always validate successfully.
        /// </summary>
        public ValidationEngineProviderMockBuilder AllValid()
        {
            _defaultEngine = new ValidationEngineMockBuilder().AllValid().Build();
            return this;
        }

        public IValidationEngineProvider Build()
        {
            // Setup GetValidationEngine(Type)
            _mock.Setup(x => x.GetValidationEngine(It.IsAny<Type>()))
                .Returns<Type>(type =>
                {
                    return _engines.TryGetValue(type, out var engine)
                        ? engine
                        : _defaultEngine;
                });

            // Setup GetValidationEngine<T>()
            _mock.Setup(x => x.GetValidationEngine<It.IsAnyType>())
                .Returns<Type>(type =>
                {
                    return _engines.TryGetValue(type, out var engine)
                        ? engine
                        : _defaultEngine;
                });

            return _mock.Object;
        }

        //public static implicit operator IValidationEngineProvider(
        //    ValidationEngineProviderMockBuilder builder)
        //    => builder.Build();
    }
}
