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
            var builder = new ValidationEngineMockBuilder(typeof(TEntity));
            configure?.Invoke(builder);

            _engines[typeof(TEntity)] = builder.Build();
            return this;
        }

        /// <summary>
        /// Configures provider to return engines that always validate successfully.
        /// </summary>
        public ValidationEngineProviderMockBuilder WithAllValid(Type forType)
        {
            _engines[forType] = new ValidationEngineMockBuilder(forType).AllValid().Build();
            return this;
        }

        public IValidationEngineProvider Build()
        {
            try
            {
                // Setup GetValidationEngine(Type)
                _mock.Setup(x => x.GetValidationEngine(It.IsAny<Type>()))
                    .Returns<Type>(type =>
                    {
                        return _engines.TryGetValue(type, out var engine)
                            ? engine
                            : throw new NotImplementedException($"Engine for type {type.FullName} is not set up.");
                    });

                // Setup GetValidationEngine<T>()
                //_mock.Setup(x => x.GetValidationEngine<It.IsAnyType>())
                //    .Returns<Type>(type =>
                //    {
                //        return _engines.TryGetValue(type, out var engine)
                //            ? engine
                //            : _defaultEngine;
                //    });

                return _mock.Object;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        //public static implicit operator IValidationEngineProvider(
        //    ValidationEngineProviderMockBuilder builder)
        //    => builder.Build();
    }
}
