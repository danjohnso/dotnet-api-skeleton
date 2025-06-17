namespace Skeleton.Identity.Models
{
	public class ResendEmailInfoModel
	{
		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string Username { get; set; }

		public string Email { get; set; }

		public bool IsEmailConfirmed { get; set; }

		public string PasswordResetToken { get; set; }
	}
}
