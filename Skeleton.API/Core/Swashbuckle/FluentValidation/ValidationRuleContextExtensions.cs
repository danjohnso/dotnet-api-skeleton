namespace Skeleton.API.Core.Swashbuckle.FluentValidation
{
    /// <summary>
    /// <see cref="ValidationRuleContext"/> extensions.
    /// </summary>
    public static class ValidationRuleContextExtensions
    {
        public static bool IsCollectionRule(this ValidationRuleContext ruleContext)
        {
            // CollectionPropertyRule<T, TElement> is also a PropertyRule.
            return ruleContext.ValidationRule.GetType().Name.StartsWith("CollectionPropertyRule");
        }

        public static ReflectionContext? GetReflectionContext(this ValidationRuleContext ruleContext)
        {
            var ruleMember = ruleContext.ValidationRule.Member;
            return ruleMember != null ? new ReflectionContext(type: ruleMember.ReflectedType, propertyInfo: ruleMember) : null;
        }
    }
}