using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Skeleton.Identity.Entities;

namespace Skeleton.Identity
{
	public class AppSignInManager(AppUserManager userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<User> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<User>> logger, IAuthenticationSchemeProvider schemes) 
		: SignInManager<User>(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes)
    {
        public override async Task<bool> CanSignInAsync(User user)
		{
			bool canSignIn = await base.CanSignInAsync(user);
			if (canSignIn)
			{
				canSignIn = user.IsActive;
			}

			return canSignIn;
		}
	}
}
