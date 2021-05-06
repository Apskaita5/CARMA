using System;
using System.Collections.Generic;
using System.Text;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain.Test
{
    public class MockValidationEngineProvider : IValidationEngineProvider
    {
        private Action<MockValidationEngine> _setup;

        public MockValidationEngineProvider(Action<MockValidationEngine> setupOptions)
        {
            _setup = setupOptions;
        }

        public IValidationEngine GetValidationEngine(Type entityType)
        {
            var result = new MockValidationEngine(entityType);
            _setup?.Invoke(result);
            return result;
        }

        public IValidationEngine GetValidationEngine<T>() where T : class
        {
            return GetValidationEngine(typeof(T));
        }
    }
}
