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
                .NotEmpty().WithMessage(Problems.FieldRequired.Format(nameof(ThingCreateRequest.Name)))
                .IsName().WithMessage(Problems.FieldMaxLength.Format(nameof(ThingCreateRequest.Name), ValidationConstants.NameLength));

            RuleFor(x => x.ModelNumber)
               .MaximumLength(50).WithMessage(Problems.FieldMaxLength.Format(nameof(ThingCreateRequest.ModelNumber), 50));
        }
    }
}
