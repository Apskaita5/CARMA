using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using A5Soft.CARMA.Domain.Metadata;
using A5Soft.CARMA.Domain.Reflection;
using A5Soft.CARMA.Domain.Rules;

namespace A5Soft.CARMA.Domain.Test
{
    public class MockValidationEngine : IValidationEngine
    {
        public MockValidationEngine(Type type)
        {
            EntityMetadata = new MockEntityMetadata(type);
        }


        public List<KeyValuePair<string, Func<object, string, BrokenRule>>> Rules { get; } = new List<KeyValuePair<string, Func<object, string, BrokenRule>>>();

        public IEntityMetadata EntityMetadata { get; }

        public List<BrokenRule> GetAllBrokenRules(object instance)
        {
            var result = new List<BrokenRule>();
            foreach (var metadataProperty in EntityMetadata.Properties)
            {
                result.AddRange(GetBrokenRules(instance, metadataProperty.Key));
            }
            return result;
        }

        public List<BrokenRule> GetBrokenRules(object instance, string propName)
        {
            var prop = EntityMetadata.Properties
                .First(p => p.Key == propName);
            var value = prop.Value.GetValue(instance);

            var result = new List<BrokenRule>();
            foreach (var rule in Rules.Where(r => r.Key == propName).Select(r => r.Value))
            {
                var broken = rule(value, propName);
                if (null != broken) result.Add(broken);
            }

            return result;
        }

        public List<BrokenRule> GetBrokenRules(object instance, string propName, bool includeDependentProps)
        {
            return GetBrokenRules(instance, propName);
        }

        public List<BrokenRule> GetBrokenRules(object instance)
        {
            return new List<BrokenRule>();
        }

        public string[] GetDependentPropertyNames(string propName)
        {
            return new string[]{};
        }


        public void AddNotNullRules()
        {
            foreach (var metadataProperty in EntityMetadata.Properties)
            {
                var rule = new KeyValuePair<string, Func<object, string, BrokenRule>>(metadataProperty.Key, (value, name) =>
                {
                    if (value is string strValue && strValue.IsNullOrWhiteSpace())
                    {
                        return new BrokenRule("mock name", name, $"Value required for {name}.", RuleSeverity.Error);
                    }
                    if (value is int intValue && intValue < 1)
                    {
                        return new BrokenRule("mock name", name, $"Value required for {name}.", RuleSeverity.Error);
                    }

                    return null;
                });

                Rules.Add(rule);
            }
        }
    }
}
