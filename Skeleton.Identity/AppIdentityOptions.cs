using Microsoft.AspNetCore.Identity;

namespace Skeleton.Identity
{
    public static class AppIdentityOptions
    {
	    public static readonly int HashIterationCount = 100_000;

        public static readonly Action<IdentityOptions> DefaultIdentitySetup = (setup) =>
        {
            setup.Password.RequiredLength = 15;
            setup.Password.RequireNonAlphanumeric = false;
            setup.Password.RequireDigit = false;
            setup.Password.RequireLowercase = false;
            setup.Password.RequireUppercase = false;
            setup.Password.RequiredUniqueChars = 8;

            setup.User.RequireUniqueEmail = true;
            setup.User.AllowedUserNameCharacters = @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 -._@+\";

            setup.SignIn.RequireConfirmedEmail = false;

            setup.ClaimsIdentity.UserNameClaimType = "name";
            setup.ClaimsIdentity.UserIdClaimType = "sub";
            setup.ClaimsIdentity.RoleClaimType = "role";

            setup.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            setup.Lockout.MaxFailedAccessAttempts = 5;
            setup.Lockout.AllowedForNewUsers = true;
        };
    }
}
