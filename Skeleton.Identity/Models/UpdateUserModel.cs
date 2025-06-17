namespace Skeleton.Identity.Models
{
    public class UpdateUserModel
    {
	    public Guid Id { get; set; }
	    public string FirstName { get; set; }
	    public string LastName { get; set; }
	    public string Email { get; set; }
	    public string PhoneNumber { get; set; }
		public bool IsActive { get; set; }
        public string DeactivatedReason { get; set; }
	    public Guid ModifiedByById { get; set; }
    }
}
