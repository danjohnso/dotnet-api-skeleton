using System;
using System.Collections.Generic;

namespace Skeleton.Identity.Models
{
	public class CreateUserModel
    {
	    public string FirstName { get; set; }
	    public string LastName { get; set; }
	    public string Username { get; set; }
        //Inactive email users
        public bool HasEmail { get; set; }
	    public string Email { get; set; }
	    public string Phone { get; set; }
		public Guid CreatedById { get; set; }
    }
}
