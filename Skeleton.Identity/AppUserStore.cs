using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Skeleton.Identity.Entities;

namespace Skeleton.Identity
{
    public class AppUserStore(AppIdentityContext ctx)
        : UserOnlyStore<User, AppIdentityContext, Guid, IdentityUserClaim<Guid>, IdentityUserLogin<Guid>, UserToken>(ctx)
    {
    }
}