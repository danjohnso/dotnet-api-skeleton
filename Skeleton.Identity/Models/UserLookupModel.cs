using System;

namespace Skeleton.Identity.Models
{
	public class UserLookupModel
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
    }
}
