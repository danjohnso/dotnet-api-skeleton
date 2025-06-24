using FluentValidation;
using Skeleton.Core;
using Skeleton.API.Core.Validation;
using Skeleton.API.v1.Requests;

namespace Skeleton.API.v1.Validators
{
    public class ThingUpdateRequestValidator : AbstractValidator<ThingUpdateRequest>
    {
        public ThingUpdateRequestValidator() {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(Problems.FieldRequired)
                .IsName().WithMessage(Problems.FieldMaxLength);

            RuleFor(x => x.RowVersion)
                .NotNull().WithMessage(Problems.FieldRequired);
        }
    }
}
