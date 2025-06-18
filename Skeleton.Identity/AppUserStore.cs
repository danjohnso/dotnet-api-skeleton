using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Skeleton.Identity.Entities;

namespace Skeleton.Identity
{
    public class AppUserStore(AppIdentityContext ctx)
        : UserStore<User, Role, AppIdentityContext, Guid>(ctx)
    {
    }
}