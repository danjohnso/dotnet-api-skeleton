using System;

namespace Skeleton.Identity.Models
{
    public class UserStatusModel
    {
        public bool HasEmail { get; set; }
        public bool IsActive { get; set; }
        public DateTime? Deactivated { get; set; }
        public string DeactivatedReason { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }
}
