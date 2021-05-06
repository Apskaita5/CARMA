using System;
using A5Soft.CARMA.Domain.Rules;
using Moq;
using Xunit;

namespace A5Soft.CARMA.Domain.Test.ValidationState
{
    public class ValidationStateTest
    {
        private IValidationEngineProvider _validationEngineProvider;


        public ValidationStateTest()
        {
            _validationEngineProvider = new MockValidationEngineProvider(
                (e) => e.AddNotNullRules());
        }


        [Fact]
        public void TestSimpleValidationState()
        {
           var instance = new SimpleDomainEntityBase(_validationEngineProvider);
           Assert.True(instance.IsValid, "Is invalid after init");
           instance.CheckRules();
           Assert.True(!instance.IsValid, "Is valid after check rules");
           Assert.True(instance.BrokenRules.ErrorCount == 3, $"Expected error count 3 got {instance.BrokenRules.ErrorCount}");
           instance.GroupName = "kljkjlk";
           Assert.True(instance.BrokenRules.ErrorCount == 2, $"Expected error count 2 got {instance.BrokenRules.ErrorCount}");
            instance.MaxTenants = 2;
            Assert.True(instance.BrokenRules.ErrorCount == 1, $"Expected error count 1 got {instance.BrokenRules.ErrorCount}");
            instance.MaxUsers = 2;
           Assert.True(instance.IsValid, "Is still invalid after all prop set");
           instance.GroupName = "";
           Assert.True(!instance.IsValid, "Is still valid after prop set to invalid value");
        }

        [Fact]
        public void TestParentValidationState()
        {
            var instance = new ParentDomainEntityBase(_validationEngineProvider);
            Assert.True(instance.IsValid, "Is invalid after init");
            instance.CheckRules();
            Assert.True(!instance.IsValid, "Is valid after check rules");
            Assert.True(instance.BrokenRules.ErrorCount == 3, $"Expected error count 3 got {instance.BrokenRules.ErrorCount}");
            instance.GroupName = "kljkjlk";
            Assert.True(instance.BrokenRules.ErrorCount == 2, $"Expected error count 2 got {instance.BrokenRules.ErrorCount}");
            instance.MaxTenants = 2;
            Assert.True(instance.BrokenRules.ErrorCount == 1, $"Expected error count 1 got {instance.BrokenRules.ErrorCount}");
            instance.MaxUsers = 2;
            Assert.True(instance.IsSelfValid, "Is still invalid after all prop set");
            Assert.True(!instance.IsValid, "Is valid while child is not");
            instance.ChildEntity.IntProperty = 1;
            instance.ChildEntity.StringProperty = "jkhkj";
            Assert.True(instance.IsValid, "Is invalid after child prop set");
            instance.ChildEntity.StringProperty = "";
            var res = instance.IsValid;
            Assert.True(!instance.IsValid, "Is valid after child prop set to invalid value");
        }

    }
}
