namespace Skeleton.Identity.Enums
{
    public enum AuditEventType
    {
        Login = 1,
        FailedLogin = 2,
        LockedOut = 3,
        Unlocked = 4,
        NewPasswordLinkRequested = 5,
        PasswordReset = 6,
        Activated = 7,
        Deactivated = 8,
        UserNameReminderRequested = 9,
        InvalidPasswordReset = 10,
		UserNameChange = 11
    }
}