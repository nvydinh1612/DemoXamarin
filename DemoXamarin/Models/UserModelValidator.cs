using FluentValidation;

namespace DemoXamarin.Models
{
    public class UserModelValidator : AbstractValidator<UserModel>
    {
        public UserModelValidator()
        {
            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }
}
