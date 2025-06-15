using FluentValidation;
using FluentValidation.Validators;
using Skeleton.Core;
using Skeleton.API.Core.Validation;

namespace Skeleton.API.Core.Validation
{
    public static class ValidatorExtensions
    {
        /// <summary>
        /// Set Message and ErrorCode for the ValidationResult
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="rule"></param>
        /// <param name="problem"></param>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, TProperty> WithMessage<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, Problem problem)
        {
            return rule.WithMessage(problem.Message).WithErrorCode(problem.Code);
        }

        public static IRuleBuilderOptions<T, string?> IsDescription<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new MaximumLengthValidator<T>(ValidationConstants.DescriptionLength));
        }

        public static IRuleBuilderOptions<T, string?> IsName<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new MaximumLengthValidator<T>(ValidationConstants.NameLength)).SetValidator(new MinimumLengthValidator<T>(ValidationConstants.NameMinimumLength));
        }

        public static IRuleBuilderOptions<T, string?> IsFirstName<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new MaximumLengthValidator<T>(ValidationConstants.FirstNameLength)).SetValidator(new MinimumLengthValidator<T>(ValidationConstants.NameMinimumLength));
        }

        public static IRuleBuilderOptions<T, string?> IsLastName<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new MaximumLengthValidator<T>(ValidationConstants.LastNameLength)).SetValidator(new MinimumLengthValidator<T>(ValidationConstants.NameMinimumLength));
        }

        public static IRuleBuilderOptions<T, string?> IsEmailAddress<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new MaximumLengthValidator<T>(ValidationConstants.EmailAddressLength));
        }

        public static IRuleBuilderOptions<T, string?> IsIPAddress<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new MaximumLengthValidator<T>(ValidationConstants.IPAddressLength));
        }

        public static IRuleBuilderOptions<T, string?> IsPhoneNumber<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new MaximumLengthValidator<T>(ValidationConstants.PhoneNumberLength));
        }

        public static IRuleBuilderOptions<T, string?> IsUrl<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new MaximumLengthValidator<T>(ValidationConstants.UrlLength));
        }
    }
}
