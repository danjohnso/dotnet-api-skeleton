using Microsoft.AspNetCore.Identity;
using Skeleton.Core.Extensions;
using System.Text.RegularExpressions;

namespace Skeleton.Identity
{
    public partial class RepeatingCharacterPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
    {
        private readonly Regex _repeating = RepeatingCharacters();

        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string? password)
        {
            return Task.FromResult(
                password.IsNotWhiteSpace()
                    ? _repeating.IsMatch(password) 
                        ? IdentityResult.Failed(new IdentityError
                            {
                                Code = "RepeatingChar",
                                Description = "Passwords cannot have more than 2 repeating characters."
                            }) 
                        : IdentityResult.Success
                    : IdentityResult.Failed(new IdentityError
                    {
                        Code = "Null",
                        Description = "Passwords cannot be null."
                    }));
        }

        [GeneratedRegex(@"(.)\1{2,}")]
        private static partial Regex RepeatingCharacters();
    }
}