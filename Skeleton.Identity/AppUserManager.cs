using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Skeleton.Identity.Entities;
using Skeleton.Identity.Enums;
using System.Security.Cryptography;

namespace Skeleton.Identity
{
    public class AppUserManager : UserManager<User>
    {
        private const int PASSWORD_HISTORY_LIMIT = 3;
        private readonly IHttpContextAccessor _contextAccessor;

        public AppUserManager(IHttpContextAccessor contextAccessor, IUserStore<User> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<User> passwordHasher, IEnumerable<IUserValidator<User>> userValidators, IEnumerable<IPasswordValidator<User>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<User>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _contextAccessor = contextAccessor;
        }

        #region - Overrides -

        public override Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return GeneratePasswordResetTokenAsync(user, "Anonymous");
        }

        public override async Task<IdentityResult> AccessFailedAsync(User user)
        {
            IdentityResult result = await base.AccessFailedAsync(user);

            string ipAddress = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            AppUserStore store = (AppUserStore)Store;
            store.Context.AuditEvents.Add(AuditEvent.CreateAuditEvent(user.Id, AuditEventType.FailedLogin, ipAddress));

            if (await IsLockedOutAsync(user))
            {
                store.Context.AuditEvents.Add(AuditEvent.CreateAuditEvent(user.Id, AuditEventType.LockedOut, ipAddress));
            }

            await store.Context.SaveChangesAsync();

            return result;
        }

        public override async Task<bool> CheckPasswordAsync(User user, string password)
        {
            bool isSuccess = await base.CheckPasswordAsync(user, password);

            if (isSuccess)
            {
                string ipAddress = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

                AppUserStore store = (AppUserStore)Store;
                store.Context.AuditEvents.Add(AuditEvent.CreateAuditEvent(user.Id, AuditEventType.Login, ipAddress, user.Id.ToString()));
                await store.Context.SaveChangesAsync();
            }

            return isSuccess;
        }

        public override async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
        {
            AppUserStore store = (AppUserStore)Store;
            if (IsPreviousPassword(user, newPassword))
            {
                store.Context.AuditEvents.Add(AuditEvent.CreateAuditEvent(user.Id, AuditEventType.InvalidPasswordReset, _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(), user.Id.ToString(), "Attempted to use a previous password"));
                await store.Context.SaveChangesAsync();
                return IdentityResult.Failed(new IdentityError { Description = "Cannot reuse old password" });
            }

            IdentityResult result = await base.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                result = await AddToPreviousPasswordsAsync(user, PasswordHasher.HashPassword(user, newPassword), true);
            }
            else
            {
                foreach (IdentityError error in result.Errors)
                {
                    store.Context.AuditEvents.Add(AuditEvent.CreateAuditEvent(user.Id, AuditEventType.InvalidPasswordReset, _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(), user.Id.ToString(), error.Description));
                }

                await store.Context.SaveChangesAsync();
            }

            return result;
        }

        public override async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
        {
            AppUserStore store = (AppUserStore)Store;
            if (IsPreviousPassword(user, newPassword))
            {
                store.Context.AuditEvents.Add(AuditEvent.CreateAuditEvent(user.Id, AuditEventType.InvalidPasswordReset, _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(), message: "Attempted to use a previous password"));
                await store.Context.SaveChangesAsync();
                return IdentityResult.Failed(new IdentityError { Description = "Cannot reuse old password" });
            }

            IdentityResult result = await base.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                result = await AddToPreviousPasswordsAsync(user, PasswordHasher.HashPassword(user, newPassword), true);
            }
            else
            {
                foreach (IdentityError error in result.Errors)
                {
                    store.Context.AuditEvents.Add(AuditEvent.CreateAuditEvent(user.Id, AuditEventType.InvalidPasswordReset, _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(), user.Id.ToString(), error.Description));
                }

                await store.Context.SaveChangesAsync();
            }

            return result;
        }

        public override async Task<IdentityResult> CreateAsync(User user)
        {
            IdentityResult result = await base.CreateAsync(user);

            //only applies if we have a hash (local accounts)
            if (result.Succeeded && !string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                result = await AddToPreviousPasswordsAsync(user, user.PasswordHash, false);
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
        public async Task<string> GeneratePasswordResetTokenAsync(User user, string actingUser = "Anonymous")
        {
            string token = await base.GeneratePasswordResetTokenAsync(user);

            user.AuditEvents.Add(AuditEvent.CreateAuditEvent(user.Id, AuditEventType.NewPasswordLinkRequested, _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(), actingUser));

            await UpdateAsync(user);

            return token;
        }

        public async Task<User> GetUsernameReminderAsync(string email, string actingUser = "Anonymous")
        {
            AppUserStore store = (AppUserStore)Store;
            User? user = store.Users.FirstOrDefault(x => x.Email == email);
            if (user != null)
            {
                user.AuditEvents.Add(AuditEvent.CreateAuditEvent(user.Id, AuditEventType.UsernameReminderRequested, _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(), actingUser));
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

            //char[] chars = new char[minLength];


            string chars = RandomNumberGenerator.GetString(allowedChars, minLength);
            ReadOnlySpan<char> charsSpan = chars.AsSpan();

            foreach (IPasswordValidator<User> validator in PasswordValidators)
            {
                IdentityResult result = await validator.ValidateAsync(this, user, chars);
                while (!result.Succeeded)
                {
                    if (result.Errors.Contains(ErrorDescriber.PasswordRequiresDigit()) && allowedNumeralChars.IndexOfAny(charsSpan) == -1)
                    {
                        chars[rd.Next(0, minLength)] = allowedNumeralChars[rd.Next(0, allowedNumeralChars.Length)];
                    }

                    if (result.Errors.Contains(ErrorDescriber.PasswordRequiresLower()) && allowedLowerChars.IndexOfAny(chars) == -1)
                    {
                        chars[rd.Next(0, minLength)] = allowedLowerChars[rd.Next(0, allowedLowerChars.Length)];
                    }

                    if (result.Errors.Contains(ErrorDescriber.PasswordRequiresNonAlphanumeric()) && allowedSpecialChars.IndexOfAny(chars) == -1)
                    {
                        chars[rd.Next(0, minLength)] = allowedSpecialChars[rd.Next(0, allowedSpecialChars.Length)];
                    }

                    if (result.Errors.Contains(ErrorDescriber.PasswordRequiresUpper()) && allowedUpperChars.IndexOfAny(chars) == -1)
                    {
                        chars[rd.Next(0, minLength)] = allowedUpperChars[rd.Next(0, allowedUpperChars.Length)];
                    }

                    if (result.Errors.Contains(ErrorDescriber.PasswordRequiresUniqueChars(Options.Password.RequiredUniqueChars)))
                    {
                        chars[rd.Next(0, minLength)] = allowedChars[rd.Next(0, allowedChars.Length)];
                    }

                    result = await validator.ValidateAsync(this, user, new string(chars));
                }
            }

            return new string(chars);
        }

        #endregion

        #region - Private Helpers -

        private Task<bool> IsPreviousPassword(User user, string newPassword)
        {
            return user.PreviousPasswords
                    .OrderByDescending(x => x.Created)
                    .Select(x => x.PasswordHash)
                    .Take(PASSWORD_HISTORY_LIMIT)
                    .AnyAsync(x => PasswordHasher.VerifyHashedPassword(user, x, newPassword) != PasswordVerificationResult.Failed);
        }

        private async Task<IdentityResult> AddToPreviousPasswordsAsync(User user, string password, bool updateLastChanged)
        {
            if (updateLastChanged)
            {
                user.LastPasswordChange = DateTime.UtcNow;
                user.AuditEvents.Add(AuditEvent.CreateAuditEvent(user.Id, AuditEventType.PasswordReset, _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(), user.Id.ToString()));
            }

            user.PreviousPasswords.Add(new PreviousPassword(password, user.Id));

            return await UpdateAsync(user);
        }

        #endregion
    }
}
