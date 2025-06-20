using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Skeleton.Core.Extensions;
using Skeleton.Identity.Entities;
using Skeleton.Identity.Enums;
using System.Security.Cryptography;

namespace Skeleton.Identity
{
    public class AppUserManager(
        IHttpContextAccessor _contextAccessor,
        IUserStore<User> _userStore,
        IOptions<IdentityOptions> _optionsAccessor,
        IPasswordHasher<User> _passwordHasher,
        IEnumerable<IUserValidator<User>> _userValidators,
        IEnumerable<IPasswordValidator<User>> _passwordValidators,
        ILookupNormalizer _keyNormalizer,
        IdentityErrorDescriber _errors,
        IServiceProvider _services,
        ILogger<UserManager<User>> _logger
    ) : UserManager<User>(_userStore, _optionsAccessor, _passwordHasher, _userValidators, _passwordValidators, _keyNormalizer, _errors, _services, _logger)
    {
        private const int PasswordHistoryLimit = 3;
        internal const string UnknownUserId = "Anonymous";
        protected internal new AppUserStore Store => (AppUserStore)base.Store;

        #region - Overrides -

        public override Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return GeneratePasswordResetTokenAsync(user, UnknownUserId);
        }

        public override async Task<IdentityResult> AccessFailedAsync(User user)
        {
            IdentityResult result = await base.AccessFailedAsync(user);

            string ipAddress = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            Store.Context.AuditEvents.Add(new AuditEvent() { UserId = user.Id, EventType = AuditEventType.FailedLogin, IPAddress = ipAddress, TriggeredBy = UnknownUserId });

            if (await IsLockedOutAsync(user))
            {
                Store.Context.AuditEvents.Add(new AuditEvent() { UserId = user.Id, EventType = AuditEventType.LockedOut, IPAddress = ipAddress, TriggeredBy = UnknownUserId });
            }

            await Store.Context.SaveChangesAsync();

            return result;
        }

        public override async Task<bool> CheckPasswordAsync(User user, string password)
        {
            bool isSuccess = await base.CheckPasswordAsync(user, password);

            if (isSuccess)
            {
                string ipAddress = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

                Store.Context.AuditEvents.Add(new AuditEvent() { UserId = user.Id, EventType = AuditEventType.Login, IPAddress = ipAddress, TriggeredBy = user.Id.ToString() });
                await Store.Context.SaveChangesAsync();
            }

            return isSuccess;
        }

        public override async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
        {
            string ipAddress = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            if (IsPreviousPassword(user, newPassword))
            {
                Store.Context.AuditEvents.Add(new AuditEvent() { UserId = user.Id, EventType = AuditEventType.InvalidPasswordReset, IPAddress = ipAddress, TriggeredBy = user.Id.ToString(), Message = "Attempted to use a previous password" });
                await Store.Context.SaveChangesAsync();
                return IdentityResult.Failed(new IdentityError { Description = "Cannot reuse old password" });
            }

            IdentityResult result = await base.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                result = await AddToPreviousPasswordsAsync(user, PasswordHasher.HashPassword(user, newPassword), ipAddress, true);
            }
            else
            {
                foreach (IdentityError error in result.Errors)
                {
                    Store.Context.AuditEvents.Add(new AuditEvent() { UserId = user.Id, EventType = AuditEventType.InvalidPasswordReset, IPAddress = ipAddress, TriggeredBy = user.Id.ToString(), Message = error.Description });
                }

                await Store.Context.SaveChangesAsync();
            }

            return result;
        }

        public override async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
        {
            string ipAddress = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            if (IsPreviousPassword(user, newPassword))
            {
                Store.Context.AuditEvents.Add(new AuditEvent() { UserId = user.Id, EventType = AuditEventType.InvalidPasswordReset, IPAddress = ipAddress, TriggeredBy = UnknownUserId, Message = "Attempted to use a previous password" });
                await Store.Context.SaveChangesAsync();
                return IdentityResult.Failed(new IdentityError { Description = "Cannot reuse old password" });
            }

            IdentityResult result = await base.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                result = await AddToPreviousPasswordsAsync(user, PasswordHasher.HashPassword(user, newPassword), ipAddress, true);
            }
            else
            {
                foreach (IdentityError error in result.Errors)
                {
                    Store.Context.AuditEvents.Add(new AuditEvent() { UserId = user.Id, EventType = AuditEventType.InvalidPasswordReset, IPAddress = ipAddress, TriggeredBy = user.Id.ToString(), Message = error.Description });
                }

                await Store.Context.SaveChangesAsync();
            }

            return result;
        }

        public override async Task<IdentityResult> CreateAsync(User user)
        {
            IdentityResult result = await base.CreateAsync(user);

            //only applies if we have a hash (local accounts)
            if (result.Succeeded && user.PasswordHash.IsNotWhiteSpace())
            {
                string ipAddress = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                result = await AddToPreviousPasswordsAsync(user, user.PasswordHash, ipAddress, false);
            }

            return result;
        }

        #endregion

        #region - Extensions -

        /// <summary>
        /// Audit eventing for reset password token
        /// </summary>
        /// <param name="user"></param>
        /// <param name="actingUser">Should be an User.Id if available, otherwise 'Anonymous'</param>
        /// <returns></returns>
        public async Task<string> GeneratePasswordResetTokenAsync(User user, string actingUser = UnknownUserId)
        {
            string token = await base.GeneratePasswordResetTokenAsync(user);

            string ipAddress = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            user.AuditEvents.Add(new AuditEvent() { UserId = user.Id, EventType = AuditEventType.NewPasswordLinkRequested, IPAddress = ipAddress, TriggeredBy = actingUser });

            await UpdateAsync(user);

            return token;
        }

        public async Task<User?> GetUserNameReminderAsync(string email, string actingUser = UnknownUserId)
        {
            User? user = Store.Users.FirstOrDefault(x => x.Email == email);
            if (user != null)
            {
                string ipAddress = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                user.AuditEvents.Add(new AuditEvent() { UserId = user.Id, EventType = AuditEventType.UserNameReminderRequested, IPAddress = ipAddress, TriggeredBy = actingUser });
                await UpdateAsync(user);
            }

            return user;
        }

        /// <summary>
        /// Generates a random password that passes validation
        /// </summary>
        /// <returns></returns>
        public async Task<string> CreateRandomPasswordAsync(User user)
        {
            int minLength = Options.Password.RequiredLength;

            const string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789!@#$^*<>_-";
            const string allowedLowerChars = "abcdefghijkmnopqrstuvwxyz";
            const string allowedUpperChars = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
            const string allowedNumeralChars = "0123456789";
            const string allowedSpecialChars = "!@#$^*<>_-";

            char[] chars = RandomNumberGenerator.GetItems<char>(allowedChars, minLength);

            foreach (IPasswordValidator<User> validator in PasswordValidators)
            {
                bool changeMade = false;
                IdentityResult result = await validator.ValidateAsync(this, user, chars.ToString());
                while (!result.Succeeded)
                {
                    if (result.Errors.Contains(ErrorDescriber.PasswordRequiresDigit()))
                    {
                        chars[RandomNumberGenerator.GetInt32(minLength)] = allowedNumeralChars[RandomNumberGenerator.GetInt32(allowedNumeralChars.Length)];
                        changeMade = true;
                    }

                    if (result.Errors.Contains(ErrorDescriber.PasswordRequiresLower()))
                    {
                        chars[RandomNumberGenerator.GetInt32(minLength)] = allowedLowerChars[RandomNumberGenerator.GetInt32(allowedLowerChars.Length)];
                        changeMade = true;
                    }

                    if (result.Errors.Contains(ErrorDescriber.PasswordRequiresNonAlphanumeric()))
                    {
                        chars[RandomNumberGenerator.GetInt32(minLength)] = allowedSpecialChars[RandomNumberGenerator.GetInt32(allowedSpecialChars.Length)];
                        changeMade = true;
                    }

                    if (result.Errors.Contains(ErrorDescriber.PasswordRequiresUpper()))
                    {
                        chars[RandomNumberGenerator.GetInt32(minLength)] = allowedUpperChars[RandomNumberGenerator.GetInt32(allowedUpperChars.Length)];
                        changeMade = true;
                    }

                    if (result.Errors.Contains(ErrorDescriber.PasswordRequiresUniqueChars(Options.Password.RequiredUniqueChars)))
                    {
                        chars[RandomNumberGenerator.GetInt32(minLength)] = allowedChars[RandomNumberGenerator.GetInt32(allowedChars.Length)];
                        changeMade = true;
                    }

                    if (!changeMade)
                    {
                        // If no change was made, we are stuck in an infinite loop
                        throw new InvalidOperationException("Unable to generate a valid password.");
                    }

                    result = await validator.ValidateAsync(this, user, new string(chars));
                }
            }

            return new string(chars);
        }

        #endregion

        #region - Private Helpers -

        private bool IsPreviousPassword(User user, string newPassword)
        {
            return user.PreviousPasswords
                    .OrderByDescending(x => x.Created)
                    .Select(x => x.PasswordHash)
                    .Take(PasswordHistoryLimit)
                    .Any(x => PasswordHasher.VerifyHashedPassword(user, x, newPassword) != PasswordVerificationResult.Failed);
        }

        private async Task<IdentityResult> AddToPreviousPasswordsAsync(User user, string passwordHash, string ipAddress, bool updateLastChanged)
        {
            if (updateLastChanged)
            {
                user.LastPasswordChange = DateTime.UtcNow;
                user.AuditEvents.Add(new AuditEvent() { UserId = user.Id, EventType = AuditEventType.PasswordReset, IPAddress = ipAddress, TriggeredBy = user.Id.ToString() });
            }

            user.PreviousPasswords.Add(new PreviousPassword() { PasswordHash = passwordHash, UserId = user.Id });

            return await UpdateAsync(user);
        }

        #endregion
    }
}
