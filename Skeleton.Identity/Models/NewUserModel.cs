namespace Skeleton.Identity.Models
{
	public class NewUserModel
	{
		public Guid Id { get; set; }
		public required string PasswordResetToken { get; set; }
	}
}
