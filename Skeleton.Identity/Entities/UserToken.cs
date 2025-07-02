using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Skeleton.Identity.Entities
{
    public class UserToken : IdentityUserToken<Guid>
    {
        public DateTime? Expiration { get; set; }
    }

    internal static class UserTokenMapping
    {
        public static EntityTypeBuilder<UserToken> Map(this EntityTypeBuilder<UserToken> entityBuilder)
        {
            entityBuilder.ToTable(nameof(UserToken));

            return entityBuilder;
        }
    }
}
