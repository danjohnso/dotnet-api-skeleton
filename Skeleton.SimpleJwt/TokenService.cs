using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Skeleton.Identity;
using Skeleton.Identity.Entities;
using Skeleton.SimpleJwt.Extensions;
using Skeleton.SimpleJwt.Requests;
using Skeleton.SimpleJwt.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Skeleton.SimpleJwt
{
    public class TokenService
    {
        //cached token format: token:{TokenType}:{UserId}
        private readonly IMemoryCache _cache;
        private readonly ILogger<TokenService> _logger;

        private readonly AppUserManager _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly SimpleJwtOptions _jwtOptions;
        private readonly JwtSecurityTokenHandler _tokenHandler = new();
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly SigningCredentials _signingCredentials;

        public TokenService(ILogger<TokenService> logger, AppUserManager userManager, SignInManager<User> signInManager, IMemoryCache cache, IOptions<SimpleJwtOptions> jwtOptionsAccessor, IOptionsMonitor<JwtBearerOptions> jwtBearerOptions)
        {
            _cache = cache;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtOptions = jwtOptionsAccessor.Value;
            _tokenValidationParameters = jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters;

            SymmetricSecurityKey signingKey = new(Encoding.UTF8.GetBytes(_jwtOptions.CurrentSigningKey));
            _signingCredentials = new(signingKey, SecurityAlgorithms.HmacSha512);
        }

        public async Task<IResult> LoginAsync(LoginRequest request)
        {
            User? user = await _userManager.FindByEmailAsync(request.EmailAddress);
            if (user == null)
            {
                _logger.LogInformation("Login attempt with invalid email: {Email}", request.EmailAddress);
                return Results.Unauthorized();
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Inactive user attempted to log in: {Email}", request.EmailAddress);
                return Results.Unauthorized();
            }

            SignInResult result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                if (result.RequiresTwoFactor)
                {
                    _logger.LogInformation("MFA required: {Email}", request.EmailAddress);

                    //I think to make this stateless we need to generate a temp token with the email address
                    //save it to user tokens and then check for it in the MFA endpoint
                    string mfaToken = GenerateMfaTokenAsync(user);
                    await _userManager.SetAuthenticationTokenAsync(user, SimpleJwtConstants.Provider, SimpleJwtConstants.MfaLoginTokenType, mfaToken);

                    return Results.Ok(new MfaResponse { TwoFactorRequired = true });
                }

                _logger.LogWarning("Signin failed for {Email}: {Reason}", user.Email, result.IsLockedOut ? "Locked" : "Not Allowed");
                return Results.Unauthorized();
            }

            TokenResponse tokens = await GenerateTokensAsync(user);
            return Results.Ok(tokens);
        }

        public async Task<IResult> MfaAsync(MfaRequest request)
        {
            User? user = await _userManager.FindByEmailAsync(request.EmailAddress);
            if (user == null)
            {
                _logger.LogWarning("Request for invalid email at MFA endpoint: {Email}", request.EmailAddress);
                return Results.Unauthorized();
            }

            string? token = await _userManager.GetAuthenticationTokenAsync(user, SimpleJwtConstants.Provider, SimpleJwtConstants.MfaLoginTokenType);
            if (token is null)
            {
                _logger.LogInformation("No MFA token for user: {Email}", request.EmailAddress);
                return Results.Unauthorized();
            }

#warning might need to consider a mechanism to cleanup expired tokens thet never get attempted
            //cleanup the token after validation, its either valid or it gets used
            await _userManager.RemoveAuthenticationTokenAsync(user, SimpleJwtConstants.Provider, SimpleJwtConstants.MfaLoginTokenType);

            if (!await ValidateTokenAsync(token, SimpleJwtConstants.MfaLoginTokenType))
            {
                _logger.LogWarning("Invalid MFA token for user: {Email}", request.EmailAddress);
                return Results.Unauthorized();
            }

            SignInResult result = await _signInManager.TwoFactorSignInAsync(TokenOptions.DefaultAuthenticatorProvider, request.Code, isPersistent: false, rememberClient: false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Signin failed for {Email}: {Reason}", user.Email, result.IsLockedOut ? "Locked" : "Not Allowed");
                return Results.Unauthorized();
            }

            TokenResponse tokens = await GenerateTokensAsync(user);
            return Results.Ok(tokens);
        }

        public async Task<IResult> RefreshAsync(RefreshRequest request)
        {
            JwtSecurityToken? jwtToken = _tokenHandler.ReadJwtToken(request.RefreshToken);
            if (jwtToken == null)
            {
                _logger.LogInformation("Unable to read refresh token");
                return Results.Unauthorized();
            }

            string? subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (subClaim.IsWhiteSpace())
            {
                _logger.LogInformation("Unable to read 'sub' claim from refresh token");
                return Results.Unauthorized();
            }

            User? user = await _userManager.FindByIdAsync(subClaim);
            if (user is null)
            {
                _logger.LogInformation("Unable to find user for 'sub' claim from refresh token");
                return Results.Unauthorized();
            }

            if (!user.IsActive)
            {
                _logger.LogInformation("Inactive users cannot refresh: {Email}", user.Email);
                return Results.Unauthorized();
            }

            if (!await _signInManager.CanSignInAsync(user))
            {
                _logger.LogInformation("Account is not confirmed and cannot refresh: {Email}", user.Email);
                return Results.Unauthorized();
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Locked out users cannot refresh: {Email}", user.Email);
                return Results.Unauthorized();
            }

            if (!await ValidateRefreshTokenAsync(user, request.RefreshToken))
            {
                _logger.LogInformation("Refresh token is not valid: {Email}", user.Email);
                return Results.Unauthorized();
            }

            TokenResponse tokens = await GenerateTokensAsync(user);
            return Results.Ok(tokens);
        }

        public async Task<IResult> LogoutAsync(HttpContext context)
        {
            string? userId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (userId.IsWhiteSpace())
            {
                _logger.LogWarning("UserId not found on claims principal");
                return Results.Unauthorized();
            }

            User? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError("Unable to find user on logout: {UserId}", userId);
                return Results.Unauthorized();
            }

            await RevokeTokensAsync(user);
            return Results.Ok();
        }

        private async Task<TokenResponse> GenerateTokensAsync(User user)
        {
            // Should always regenerate refresh token with the access token to minimize risk of replay attacks.
            string accessToken = await GenerateAccessTokenAsync(user);
            string refreshToken = await GenerateRefreshTokenAsync(user);
            return new(accessToken, refreshToken);
        }

        private async Task RevokeTokensAsync(User user)
        {
            await _userManager.RemoveAuthenticationTokenAsync(user, SimpleJwtConstants.Provider, SimpleJwtConstants.RefreshTokenType);

            // Clear refresh token from cache
            string cacheKey = $"token:{SimpleJwtConstants.RefreshTokenType}:{user.Id}";
            _cache.Remove(cacheKey);
        }

        private async Task<bool> ValidateRefreshTokenAsync(User user, string refreshToken)
        {
            // Try cache first
            string cacheKey = $"token:{SimpleJwtConstants.RefreshTokenType}:{user.Id}";
            if (_cache.TryGetValue(cacheKey, out string? cachedTokenHash))
            {
                string inputTokenHash = refreshToken.Sha512();
                if (cachedTokenHash == inputTokenHash)
                {
                    return await ValidateTokenAsync(refreshToken, SimpleJwtConstants.RefreshTokenType);
                }
            }

            // Fallback to database if not in cache
            string? storedTokenHash = await _userManager.GetAuthenticationTokenAsync(user, SimpleJwtConstants.Provider, SimpleJwtConstants.RefreshTokenType);
            if (storedTokenHash == null)
            {
                return false;
            }

            string inputTokenHashDb = refreshToken.Sha512();
            if (storedTokenHash != inputTokenHashDb)
            {
                return false;
            }

            // If hash matches from DB, we still need to validate the token to make sure its still valid
            bool isValid = await ValidateTokenAsync(refreshToken, SimpleJwtConstants.RefreshTokenType);
            if (isValid)
            {
                // Update cache if token is valid
                _cache.Set(cacheKey, storedTokenHash, new MemoryCacheEntryOptions
                {
                    AbsoluteExpiration = DateTime.UtcNow.AddMinutes(_jwtOptions.RefreshTokenExpirationMinutes),
                    SlidingExpiration = TimeSpan.FromDays(1)
                });
            }

            return isValid;
        }

        private async Task<string> GenerateAccessTokenAsync(User user)
        {
            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //(JWT ID): Unique identifier; can be used to prevent the JWT from being replayed (allows a token to be used only once)
                new Claim(SimpleJwtConstants.TokenTypeClaim, SimpleJwtConstants.AccessTokenType)
            ];

            claims.AddRange(await _userManager.GetClaimsAsync(user));

            DateTime expires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes);
            JwtSecurityToken token = new(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: _signingCredentials
            );

            string tokenString = _tokenHandler.WriteToken(token);
            //not going to store or cache these since these are short lived
            return tokenString;
        }

        private string GenerateMfaTokenAsync(User user)
        {
            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //(JWT ID): Unique identifier; can be used to prevent the JWT from being replayed (allows a token to be used only once)
            ];

            //going to make this short lived
            DateTime expires = DateTime.UtcNow.AddMinutes(5);
            JwtSecurityToken token = new(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: _signingCredentials
            );

            string tokenString = _tokenHandler.WriteToken(token);
            //not going to store or cache these since these are short lived
            return tokenString;
        }

        private async Task<string> GenerateRefreshTokenAsync(User user)
        {
            List<Claim> claims =
            [
                new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //(JWT ID): Unique identifier; can be used to prevent the JWT from being replayed (allows a token to be used only once)
                new Claim(SimpleJwtConstants.TokenTypeClaim, SimpleJwtConstants.RefreshTokenType)
            ];

            DateTime expires = DateTime.UtcNow.AddMinutes(_jwtOptions.RefreshTokenExpirationMinutes);
            JwtSecurityToken token = new(
                 issuer: _jwtOptions.Issuer,
                 audience: _jwtOptions.Audience,
                 claims: claims,
                 expires: expires,
                 signingCredentials: _signingCredentials
             );

            string tokenString = _tokenHandler.WriteToken(token);

            //hash and save refresh token to database to survive app resets and allow revocation. Sliding cache for performance
            string tokenHash = tokenString.Sha512();
            await _userManager.SetAuthenticationTokenAsync(user, SimpleJwtConstants.Provider, SimpleJwtConstants.RefreshTokenType, tokenHash);
            _cache.Set($"token:{SimpleJwtConstants.RefreshTokenType}:{user.Id}", tokenHash, new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = expires,
                SlidingExpiration = TimeSpan.FromDays(1)
            });

            return tokenString;
        }

        private async Task<bool> ValidateTokenAsync(string token, string expectedTokenType)
        {
            TokenValidationResult tokenValidationResult = await _tokenHandler.ValidateTokenAsync(token, _tokenValidationParameters);
            if (!tokenValidationResult.IsValid)
            {
                _logger.LogDebug("Token validation failed: {@ValidationResult}", tokenValidationResult);
                return false;
            }

            return tokenValidationResult.Claims.Any(c => c.Key == SimpleJwtConstants.TokenTypeClaim && c.Value.ToString() == expectedTokenType);
        }
    }
}
