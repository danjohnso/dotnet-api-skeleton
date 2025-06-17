using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skeleton.Core.Extensions;
using Skeleton.Identity.Entities;
using Skeleton.Identity.Models;

namespace Skeleton.Identity
{
    public class UserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly AppUserManager _userManager;

        public UserService(ILogger<UserService> logger, AppUserManager userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<NewUserModel?> CreateAsync(CreateUserModel model)
        {
            NewUserModel? response = null;
            
            User user = new
            {
                model.Email,
                EmailConfirmed = false,
                model.FirstName,
                model.LastName,
                IsActive = true,
                LastPasswordChange = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                LockoutEnabled = true,
                UserName = model.Username,
                PhoneNumber = model.Phone,
                model.CreatedById,
                ModifiedById = model.CreatedById,
                TwoFactorEnabled = true
            };

            IdentityResult result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                response = new NewUserModel
                {
                    Id = user.Id,
                    PasswordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user)
                };
            }
            else
            {
                foreach (IdentityError error in result.Errors)
                {
                    _logger.LogError("[CreateAsync] Identity Error, code {Code} with message {Description}", error.Code, error.Description);
                }
            }

            return response;
        }

        public async Task<ExistingUserModel> ExistsAsync(string email)
        {
            User user = await _userManager.FindByEmailAsync(email);
            return new ExistingUserModel
            {
                Email = email,
                FirstName = user?.FirstName,
                Id = user?.Id,
                Initials = user?.Initials,
                LastName = user?.LastName,
                Phone = user?.PhoneNumber,
                Username = user?.UserName,
                HasEmail = user?.HasEmail ?? true //default to true since most users should have email...
            };
        }

        public async Task<List<ClaimsUserModel>> GetActiveAsync()
        {
            return await _userManager.Users
                                    .Where(x => x.IsActive && x.Id != Guid.Empty)
                                    .Select(x => new ClaimsUserModel
                                    {
                                        Id = x.Id,
                                        FirstName = x.FirstName,
                                        LastName = x.LastName,
                                        Email = x.Email,
                                        UserName = x.UserName
                                    })
                                    .ToListAsync();
        }

        public Task<ClaimsUserModel> GetActiveAsync(Guid id)
        {
            return _userManager.Users
                        .Where(x => x.IsActive && x.Id == id)
                        .Select(x => new ClaimsUserModel
                        {
                            Id = x.Id,
                            FirstName = x.FirstName,
                            LastName = x.LastName,
                            Email = x.Email,
                            UserName = x.UserName
                        })
                        .SingleAsync();
        }

        public async Task<List<UserLookupModel>> GetAvailableEmployeesAsync(Guid id, string tenantName)
        {
            return await _userManager.Users
                                    .Where(x => x.IsActive
                                            && x.Id != Guid.Empty //The System
                                            && x.Id != id //yourself
                                            && !x.Managers.Any(m => m.ManagerId == id && m.TenantName == tenantName) //already your employee
                                            && !x.Employees.Any(e => e.EmployeeId == id && e.TenantName == tenantName)) //your manager
                                    .Select(x => new UserLookupModel
                                    {
                                        Id = x.Id,
                                        DisplayName = x.FirstName + " " + x.LastName,
                                        UserName = x.UserName
                                    })
                                    .ToListAsync();
        }

        public async Task<List<UserLookupModel>> GetCurrentEmployeesAsync(Guid id, string tenantName)
        {
            return await _userManager.Users
                                    .Where(x => x.Managers.Any(m => m.ManagerId == id && m.TenantName == tenantName))
                                    .Select(x => new UserLookupModel
                                    {
                                        Id = x.Id,
                                        DisplayName = x.FirstName + " " + x.LastName,
                                        UserName = x.UserName
                                    })
                                    .ToListAsync();
        }

        public async Task<List<UserLookupModel>> GetShareableEmployeesAsync(Guid id, string tenantName)
        {
            return await _userManager.Users
                                    .Where(x => x.IsActive
                                                && x.Id != Guid.Empty //The System
                                                && x.Id != id //yourself
                                                && !x.Managers.Any(m => m.ManagerId == id && m.TenantName == tenantName)) //already your employee
                                    .Select(x => new UserLookupModel
                                    {
                                        Id = x.Id,
                                        DisplayName = x.FirstName + " " + x.LastName,
                                        UserName = x.UserName
                                    })
                                    .ToListAsync();
        }

        public async Task<UserStatusModel> GetStatusAsync(Guid id)
        {
            return await _userManager.Users
                                    .Where(x => x.Id == id)
                                    .Select(x => new UserStatusModel
                                    {
                                        HasEmail = x.HasEmail,
                                        IsActive = x.IsActive,
                                        Deactivated = x.Deactivated,
                                        DeactivatedReason = x.DeactivatedReason,
                                        IsEmailConfirmed = x.EmailConfirmed
                                    })
                                    .SingleAsync();
        }

        public async Task<ResendEmailInfoModel> GetResendNewUserEmailInfoAsync(Guid id)
        {
            User user = await _userManager.FindByIdAsync(id.ToString());
            string code = await _userManager.GeneratePasswordResetTokenAsync(user);

            ResendEmailInfoModel rm = new ResendEmailInfoModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsEmailConfirmed = user.EmailConfirmed,
                Username = user.UserName,
                PasswordResetToken = code,
            };

            return rm;
        }

        public async Task<bool> ResetPasswordAsync(Guid id, Guid actingUserId)
        {
            User? user = await _userManager.FindByIdAsync(id.ToString());            
            if (user == null)
            {
                _logger.LogError("[ResetPasswordAsync] User with ID {Id} not found", id);
                return false;
            }

            string code = await _userManager.GeneratePasswordResetTokenAsync(user);

            //TODO, get the random password generator?  or maybe we just use the token....
            string password = $"{user.UserName.ToLower()}{DateTime.UtcNow:MM-dd-yy}";

            IdentityResult resetResult = await _userManager.ResetPasswordAsync(user, code, password);
            if (!resetResult.Succeeded)
            {
                foreach (IdentityError error in resetResult.Errors)
                {
                    _logger.LogError("[ResetPasswordAsync] Reset Identity Error, code {Code} with message {Description}", error.Code, error.Description);
                }
            }
            else
            {
                user.LastPasswordChange = null;
                user.Modified = DateTime.UtcNow;
                user.ModifiedById = actingUserId;

                IdentityResult result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        _logger.LogError("[ResetPasswordAsync] Update Identity Error, code {Code} with message {Description}", error.Code, error.Description);
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }


        public Task<List<ClaimsUserModel>> SearchActiveAsync(string pattern)
        {
            return _userManager.Users
                                    .Where(x => x.IsActive
                                            && x.Id != Guid.Empty
                                            && (x.Email.Contains(pattern) || x.FirstName.Contains(pattern) ||
                                                x.LastName.Contains(pattern) || x.UserName.Contains(pattern))
                                    )
                                    .Select(x => new ClaimsUserModel
                                    {
                                        Id = x.Id,
                                        FirstName = x.FirstName,
                                        LastName = x.LastName,
                                        Email = x.Email,
                                        UserName = x.UserName
                                    })
                                    .ToListAsync();
        }

        public async Task<bool> UpdateAsync(UpdateUserModel model)
        {
            User user = await _userManager.Users.SingleAsync(x => x.Id == model.Id);
            
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            if (user.PhoneNumber.IsNotEqualTo(model.PhoneNumber))
            {
                if (await _userManager.Users.AnyAsync(x => x.PhoneNumber == model.PhoneNumber))
                {
                    throw new InvalidOperationException($"Phone number {model.PhoneNumber} is already in use by another user");
                }

                user.PhoneNumber = model.PhoneNumber;
            }

            if (user.Email.IsNotEqualTo(model.Email))
            {
                if (await _userManager.Users.AnyAsync(x => x.Email == model.Email))
                {
                    throw new InvalidOperationException($"Email address {model.Email} is already in use by another user");
                }

                user.Email = model.Email;
            }

            if (user.IsActive && !model.IsActive)
            {
                user.IsActive = false;
                user.Deactivated = DateTime.UtcNow;
                user.DeactivatedReason = model.DeactivatedReason;
            }
            else if (!user.IsActive && model.IsActive)
            {
                user.IsActive = true;
                user.Deactivated = null;
                user.DeactivatedReason = null;
            }

            user.Modified = DateTime.UtcNow;
            user.ModifiedById = model.ModifiedByById;

            IdentityResult result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    _logger.LogError("[UpdateAsync] Identity Error, code {Code} with message {Description}", error.Code, error.Description);
                }
            }

            return result.Succeeded;
        }
    }
}
