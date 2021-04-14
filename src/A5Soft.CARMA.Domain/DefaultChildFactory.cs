using A5Soft.CARMA.Domain.Rules;
using System;
using System.Linq;
using System.Reflection;

namespace A5Soft.CARMA.Domain
{
    /// <summary>
    /// a default implementation for <see cref="IChildFactory{TChild}"/> that uses either constructor with a
    /// <see cref="IValidationEngineProvider"/> or a parameterless constructor.
    /// </summary>
    /// <typeparam name="TChild"></typeparam>
    public class DefaultChildFactory<TChild> : IChildFactory<TChild>
    {
        private ConstructorInfo _constructor;
        private bool _isEmpty;


        /// <summary>
        /// Creates a new instance of child factory.
        /// </summary>
        /// <exception cref="InvalidOperationException">if TChild has neither IValidationEngineProvider nor empty constructor</exception>
        public DefaultChildFactory()
        {
            _constructor = GetConstructorInfo<TChild>(ref _isEmpty);

            if (null == _constructor) throw new InvalidOperationException(
                $"Type {typeof(TChild).FullName} has neither IValidationEngineProvider nor empty constructor.");
        }

        private DefaultChildFactory(ConstructorInfo constructor, bool isEmpty)
        {
            _constructor = constructor;
            _isEmpty = isEmpty;
        }


        /// <inheritdoc cref="IChildFactory{TChild}.CreateNew(IDomainObject, IValidationEngineProvider)"/>
        public TChild CreateNew(IDomainObject parent, IValidationEngineProvider validationEngineProvider)
        {
            if (_isEmpty) return (TChild)_constructor.Invoke(null);
            return (TChild) _constructor.Invoke(new object[] { validationEngineProvider });
        }


        /// <summary>
        /// if TC has either IValidationEngineProvider or empty constructor returns
        /// a new <see cref="IChildFactory{TC}"/> instance; otherwise returns null
        /// </summary>
        /// <typeparam name="TC">a type of children to create a factory for</typeparam>
        public static IChildFactory<TChild> NewOrNull()
        {
            bool isEmpty = true;
            var constructor = GetConstructorInfo<TChild>(ref isEmpty);

            if (null == constructor) return null;

            return new DefaultChildFactory<TChild>(constructor, isEmpty);
        }


        private static ConstructorInfo GetConstructorInfo<TC>(ref bool isEmpty)
        {
            var constructors = typeof(TC).GetConstructors();
            var constructor = constructors.FirstOrDefault(c => c.GetParameters().Length
                - c.GetParameters().Count(p => p.IsOptional) == 1
                && c.GetParameters().First().ParameterType == typeof(IValidationEngineProvider));
            isEmpty = (null == constructor);

            if (null == constructor) constructor = constructors.FirstOrDefault(
                c => c.GetParameters().Length == 0);
            if (null == constructor) constructor = constructors.FirstOrDefault(
                c => c.GetParameters().Count(p => !p.IsOptional) == 0);

            return constructor;
        }

    }
}
