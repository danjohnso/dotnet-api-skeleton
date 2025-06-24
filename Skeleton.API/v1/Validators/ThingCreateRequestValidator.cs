using FluentValidation;
using Skeleton.Core;
using Skeleton.API.Core.Validation;
using Skeleton.API.v1.Requests;

namespace Skeleton.API.v1.Validators
{
    public class ThingCreateRequestValidator : AbstractValidator<ThingCreateRequest>
    {
        public ThingCreateRequestValidator() {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(Problems.FieldRequired)
                .IsName().WithMessage(Problems.FieldMaxLength);

            RuleFor(x => x.ModelNumber)
               .MaximumLength(50).WithMessage(Problems.FieldMaxLength);
        }
    }
}
