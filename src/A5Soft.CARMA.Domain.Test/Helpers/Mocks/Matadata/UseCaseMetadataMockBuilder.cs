using A5Soft.CARMA.Domain.Metadata;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A5Soft.CARMA.Domain.Test.Helpers.Mocks.Matadata
{
    /// <summary>
    /// Fluent builder for creating IUseCaseMetadata mocks.
    /// </summary>
    public class UseCaseMetadataMockBuilder
    {
        private readonly Mock<IUseCaseMetadata> _mock;
        private Type _useCaseType;
        private string _buttonTitle;
        private string _menuTitle;
        private string _confirmationQuestion;
        private string _successMessage;
        private string _helpUri;

        public UseCaseMetadataMockBuilder(Type useCaseType)
        {
            _mock = new Mock<IUseCaseMetadata>();
            _useCaseType = useCaseType;
            _buttonTitle = useCaseType.Name;
            _menuTitle = useCaseType.Name;
        }

        public UseCaseMetadataMockBuilder ForType<T>()
        {
            _useCaseType = typeof(T);
            _buttonTitle = typeof(T).Name;
            _menuTitle = typeof(T).Name;
            return this;
        }

        public UseCaseMetadataMockBuilder WithButtonTitle(string buttonTitle)
        {
            _buttonTitle = buttonTitle;
            return this;
        }

        public UseCaseMetadataMockBuilder WithMenuTitle(string menuTitle)
        {
            _menuTitle = menuTitle;
            return this;
        }

        public UseCaseMetadataMockBuilder WithConfirmationQuestion(string confirmationQuestion)
        {
            _confirmationQuestion = confirmationQuestion;
            return this;
        }

        public UseCaseMetadataMockBuilder WithSuccessMessage(string successMessage)
        {
            _successMessage = successMessage;
            return this;
        }

        public UseCaseMetadataMockBuilder WithHelpUri(string helpUri)
        {
            _helpUri = helpUri;
            return this;
        }

        public IUseCaseMetadata Build()
        {
            _mock.Setup(x => x.UseCaseType).Returns(_useCaseType);
            _mock.Setup(x => x.GetButtonTitle()).Returns(_buttonTitle);
            _mock.Setup(x => x.GetMenuTitle()).Returns(_menuTitle);
            _mock.Setup(x => x.GetConfirmationQuestion()).Returns(_confirmationQuestion ?? string.Empty);
            _mock.Setup(x => x.GetSuccessMessage()).Returns(_successMessage ?? string.Empty);
            _mock.Setup(x => x.GetHelpUri()).Returns(_helpUri ?? string.Empty);

            return _mock.Object;
        }

        //public static implicit operator IUseCaseMetadata(UseCaseMetadataMockBuilder builder)
        //    => builder.Build();
    }
}
