﻿namespace Skeleton.Business.Models
{
    public class ProfileModel
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string EmailAddress { get; set; } = "";
    }
}
