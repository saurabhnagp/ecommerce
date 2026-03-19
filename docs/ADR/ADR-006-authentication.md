# ADR-006: Authentication Strategy

## Status
**Accepted**

## Date
2024-12-19

## Context

AmCart requires authentication for:
- Customer accounts (registration, login)
- Admin access to dashboard
- API protection for authenticated endpoints
- Social login (Google, Facebook)
- Guest checkout capability
- Mobile app support (future)

Security requirements:
- Protect against common attacks (CSRF, XSS, session hijacking)
- Support for password reset
- Account verification via email
- Rate limiting on auth endpoints

## Decision

We will implement **JWT-based authentication** with the following approach:

### Authentication Methods

| Method | Flow | Verification |
|--------|------|--------------|
| Email/Password Registration | Register form → Email verification → Account activated | Email link (24h expiry) |
| Email/Password Login | Login form → JWT tokens | Credentials validation |
| Google OAuth 2.0 | Google Sign-In → Callback → Auto-create account | Google ID token |
| Facebook OAuth 2.0 | Facebook Login → Callback → Auto-create account | Facebook access token |

### Token Strategy

| Token Type | Storage | Lifetime | Purpose |
|------------|---------|----------|---------|
| Access Token | Memory/Header | 15 minutes | API authorization |
| Refresh Token | HTTP-only cookie | 7 days | Token renewal |
| Email Verification Token | Database | 24 hours | Account activation |
| Password Reset Token | Database | 1 hour | Password recovery |

---

## User Registration Flow

### Email/Password Registration

```
┌─────────────────────────────────────────────────────────────────┐
│                  User Registration Flow                          │
│                                                                  │
│  1. User fills registration form                                 │
│     POST /api/v1/auth/register                                   │
│     {                                                            │
│       "email": "user@example.com",                               │
│       "password": "SecurePass123!",                              │
│       "firstName": "John",                                       │
│       "lastName": "Doe",                                         │
│       "phone": "+91-9876543210"                                  │
│     }                                                            │
│                                                                  │
│  2. Server creates unverified account                            │
│     - User record created (IsEmailVerified = false)              │
│     - Generate email verification token (GUID)                   │
│     - Store token with 24h expiry                                │
│                                                                  │
│  3. Verification email sent                                      │
│     Subject: "Verify your AmCart account"                        │
│     Link: https://amcart.com/verify-email?token=<token>         │
│                                                                  │
│  4. User clicks verification link                                │
│     GET /api/v1/auth/verify-email?token=<token>                 │
│                                                                  │
│  5. Server verifies token and activates account                  │
│     - Validate token exists and not expired                      │
│     - Set IsEmailVerified = true                                 │
│     - Delete verification token                                  │
│     - Send welcome email                                         │
│                                                                  │
│  6. User can now login with email/password                       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Registration Form Fields

| Field | Required | Validation |
|-------|----------|------------|
| Email | Yes | Valid email format, unique |
| Password | Yes | Min 8 chars, uppercase, lowercase, number, special char |
| First Name | Yes | 2-50 characters |
| Last Name | Yes | 2-50 characters |
| Phone | No | Valid phone format |
| Accept Terms | Yes | Must be true |

---

## Social Login Flow

### Google OAuth 2.0 Login

```
┌─────────────────────────────────────────────────────────────────┐
│                    Google OAuth 2.0 Flow                         │
│                                                                  │
│  1. User clicks "Sign in with Google" button                     │
│     Redirect to Google OAuth consent screen                      │
│     https://accounts.google.com/o/oauth2/v2/auth                 │
│     ?client_id=<GOOGLE_CLIENT_ID>                                │
│     &redirect_uri=https://amcart.com/auth/google/callback        │
│     &scope=openid email profile                                  │
│     &response_type=code                                          │
│                                                                  │
│  2. User authorizes AmCart on Google                             │
│     Google redirects back with authorization code                │
│                                                                  │
│  3. Server exchanges code for tokens                             │
│     POST https://oauth2.googleapis.com/token                     │
│     Receives: access_token, id_token                             │
│                                                                  │
│  4. Server validates Google ID token                             │
│     Extract: email, name, picture, sub (Google ID)               │
│                                                                  │
│  5. Server creates or links account                              │
│     IF user exists with email:                                   │
│       - Link Google ID to existing account                       │
│     ELSE:                                                        │
│       - Create new user (IsEmailVerified = true)                 │
│       - Set AuthProvider = "Google"                              │
│                                                                  │
│  6. Server generates JWT tokens                                  │
│     Return access token + set refresh token cookie               │
│                                                                  │
│  7. Redirect to dashboard/home                                   │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Facebook OAuth 2.0 Login

```
┌─────────────────────────────────────────────────────────────────┐
│                   Facebook OAuth 2.0 Flow                        │
│                                                                  │
│  1. User clicks "Sign in with Facebook" button                   │
│     Redirect to Facebook OAuth dialog                            │
│     https://www.facebook.com/v18.0/dialog/oauth                  │
│     ?client_id=<FACEBOOK_APP_ID>                                 │
│     &redirect_uri=https://amcart.com/auth/facebook/callback      │
│     &scope=email,public_profile                                  │
│                                                                  │
│  2. User authorizes AmCart on Facebook                           │
│     Facebook redirects back with authorization code              │
│                                                                  │
│  3. Server exchanges code for access token                       │
│     GET https://graph.facebook.com/v18.0/oauth/access_token      │
│                                                                  │
│  4. Server fetches user profile                                  │
│     GET https://graph.facebook.com/me                            │
│     ?fields=id,email,first_name,last_name,picture                │
│                                                                  │
│  5. Server creates or links account                              │
│     IF user exists with email:                                   │
│       - Link Facebook ID to existing account                     │
│     ELSE:                                                        │
│       - Create new user (IsEmailVerified = true)                 │
│       - Set AuthProvider = "Facebook"                            │
│                                                                  │
│  6. Server generates JWT tokens                                  │
│     Return access token + set refresh token cookie               │
│                                                                  │
│  7. Redirect to dashboard/home                                   │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Login Flow (Email/Password)

```
┌─────────────────────────────────────────────────────────────────┐
│                    Email/Password Login Flow                     │
│                                                                  │
│  1. User submits credentials                                     │
│     POST /api/v1/auth/login                                      │
│     { "email": "user@example.com", "password": "SecurePass123!" }│
│                                                                  │
│  2. Server validates credentials                                 │
│     - Check user exists                                          │
│     - Check IsEmailVerified = true                               │
│     - Verify password hash (bcrypt)                              │
│     - Check account not locked                                   │
│                                                                  │
│  3. On success: Generate tokens                                  │
│     - Access Token (JWT, 15 min)                                 │
│     - Refresh Token (stored in Redis, 7 days)                    │
│                                                                  │
│  4. Response                                                     │
│     - Access Token in response body                              │
│     - Refresh Token in HTTP-only cookie                          │
│                                                                  │
│  5. On failure: Return error                                     │
│     - Increment failed login attempts                            │
│     - Lock account after 5 failed attempts (30 min)              │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## JWT Payload

```json
{
  "sub": "user-uuid",
  "email": "user@example.com",
  "name": "John Doe",
  "role": "customer",
  "provider": "Local",
  "email_verified": true,
  "iat": 1703001600,
  "exp": 1703002500,
  "iss": "amcart",
  "aud": "amcart-api"
}
```

### Password Security

- Hashing: bcrypt with cost factor 12
- Minimum length: 8 characters
- Requirements: uppercase, lowercase, number, special character
- Password history: Last 5 passwords cannot be reused

## Consequences

### Positive

- **Stateless**: JWTs don't require server-side session storage
- **Scalable**: Any service can validate tokens independently
- **Mobile Ready**: Works well with mobile apps
- **Microservices**: Easy to pass between services
- **Performance**: No database lookup for validation

### Negative

- **Token Revocation**: Cannot easily revoke active tokens
- **Token Size**: JWTs larger than session IDs
- **Complexity**: More complex than session-based auth

### Mitigations

| Challenge | Mitigation |
|-----------|------------|
| Revocation | Short access token lifetime, blacklist in Redis |
| Token Size | Keep claims minimal |
| Complexity | Use established libraries (Microsoft.AspNetCore.Authentication.JwtBearer) |

## Implementation

### User Entity

```csharp
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string? PasswordHash { get; set; }  // Null for social login only users
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? Phone { get; set; }
    public string Role { get; set; } = "Customer";
    
    // Email Verification
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiry { get; set; }
    
    // Password Reset
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    
    // Social Login
    public AuthProvider AuthProvider { get; set; } = AuthProvider.Local;
    public string? GoogleId { get; set; }
    public string? FacebookId { get; set; }
    public string? ProfilePictureUrl { get; set; }
    
    // Security
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public enum AuthProvider
{
    Local,
    Google,
    Facebook
}
```

### User Registration

```csharp
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher _passwordHasher;

    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken ct)
    {
        // Check if email already exists
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existingUser != null)
            return AuthResult.Failure("Email already registered");

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLower(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            IsEmailVerified = false,
            EmailVerificationToken = Guid.NewGuid().ToString("N"),
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
            AuthProvider = AuthProvider.Local,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, ct);

        // Send verification email
        var verificationLink = $"https://amcart.com/verify-email?token={user.EmailVerificationToken}";
        await _emailService.SendEmailVerificationAsync(user.Email, user.FirstName, verificationLink);

        return AuthResult.Success("Registration successful. Please check your email to verify your account.");
    }
}
```

### Email Verification

```csharp
public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public async Task<AuthResult> Handle(VerifyEmailCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByVerificationTokenAsync(request.Token, ct);
        
        if (user == null)
            return AuthResult.Failure("Invalid verification token");
            
        if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            return AuthResult.Failure("Verification token has expired. Please request a new one.");
            
        if (user.IsEmailVerified)
            return AuthResult.Failure("Email already verified");

        // Activate account
        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        
        await _userRepository.UpdateAsync(user, ct);

        // Send welcome email
        await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);

        return AuthResult.Success("Email verified successfully. You can now login.");
    }
}
```

### Google OAuth Login

```csharp
public class GoogleAuthHandler : IRequestHandler<GoogleAuthCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly GoogleAuthSettings _settings;

    public async Task<AuthResult> Handle(GoogleAuthCommand request, CancellationToken ct)
    {
        // Exchange authorization code for tokens
        var tokenResponse = await ExchangeCodeForTokensAsync(request.Code);
        
        // Validate and decode Google ID token
        var payload = await ValidateGoogleIdTokenAsync(tokenResponse.IdToken);
        
        // Find or create user
        var user = await _userRepository.GetByEmailAsync(payload.Email, ct);
        
        if (user == null)
        {
            // Create new user from Google profile
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = payload.Email,
                FirstName = payload.GivenName ?? payload.Name?.Split(' ').FirstOrDefault() ?? "",
                LastName = payload.FamilyName ?? payload.Name?.Split(' ').LastOrDefault() ?? "",
                IsEmailVerified = true,  // Google verified the email
                AuthProvider = AuthProvider.Google,
                GoogleId = payload.Subject,
                ProfilePictureUrl = payload.Picture,
                CreatedAt = DateTime.UtcNow
            };
            await _userRepository.AddAsync(user, ct);
        }
        else
        {
            // Link Google account if not already linked
            if (string.IsNullOrEmpty(user.GoogleId))
            {
                user.GoogleId = payload.Subject;
                user.ProfilePictureUrl ??= payload.Picture;
                await _userRepository.UpdateAsync(user, ct);
            }
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, ct);

        // Generate JWT tokens
        var tokens = _tokenService.GenerateTokens(user);
        
        return AuthResult.Success(tokens);
    }

    private async Task<GoogleTokenResponse> ExchangeCodeForTokensAsync(string code)
    {
        using var client = new HttpClient();
        var response = await client.PostAsync("https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = _settings.ClientId,
                ["client_secret"] = _settings.ClientSecret,
                ["redirect_uri"] = _settings.RedirectUri,
                ["grant_type"] = "authorization_code"
            }));

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GoogleTokenResponse>(content)!;
    }
}
```

### Facebook OAuth Login

```csharp
public class FacebookAuthHandler : IRequestHandler<FacebookAuthCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly FacebookAuthSettings _settings;

    public async Task<AuthResult> Handle(FacebookAuthCommand request, CancellationToken ct)
    {
        // Exchange code for access token
        var accessToken = await ExchangeCodeForAccessTokenAsync(request.Code);
        
        // Get user profile from Facebook
        var profile = await GetFacebookProfileAsync(accessToken);
        
        // Find or create user
        var user = await _userRepository.GetByEmailAsync(profile.Email, ct);
        
        if (user == null)
        {
            // Create new user from Facebook profile
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = profile.Email,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                IsEmailVerified = true,  // Facebook verified the email
                AuthProvider = AuthProvider.Facebook,
                FacebookId = profile.Id,
                ProfilePictureUrl = profile.Picture?.Data?.Url,
                CreatedAt = DateTime.UtcNow
            };
            await _userRepository.AddAsync(user, ct);
        }
        else
        {
            // Link Facebook account if not already linked
            if (string.IsNullOrEmpty(user.FacebookId))
            {
                user.FacebookId = profile.Id;
                user.ProfilePictureUrl ??= profile.Picture?.Data?.Url;
                await _userRepository.UpdateAsync(user, ct);
            }
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, ct);

        // Generate JWT tokens
        var tokens = _tokenService.GenerateTokens(user);
        
        return AuthResult.Success(tokens);
    }

    private async Task<FacebookProfile> GetFacebookProfileAsync(string accessToken)
    {
        using var client = new HttpClient();
        var response = await client.GetStringAsync(
            $"https://graph.facebook.com/me?fields=id,email,first_name,last_name,picture&access_token={accessToken}");
        return JsonSerializer.Deserialize<FacebookProfile>(response)!;
    }
}
```

### JWT Token Service

```csharp
public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;
    private readonly IConnectionMultiplexer _redis;

    public AuthTokens GenerateTokens(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        
        // Store refresh token in Redis
        _redis.GetDatabase().StringSet(
            $"refresh:{refreshToken}",
            user.Id.ToString(),
            TimeSpan.FromDays(7));

        return new AuthTokens
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 900 // 15 minutes
        };
    }

    private string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("provider", user.AuthProvider.ToString()),
            new Claim("email_verified", user.IsEmailVerified.ToString().ToLower()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### JWT Validation in Services

```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Google and Facebook OAuth
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["OAuth:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["OAuth:Google:ClientSecret"]!;
    })
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["OAuth:Facebook:AppId"]!;
        options.AppSecret = builder.Configuration["OAuth:Facebook:AppSecret"]!;
    });
```

## Alternatives Considered

### 1. Session-based Authentication

**Pros:**
- Simpler implementation
- Easy to revoke sessions
- Smaller cookie size

**Cons:**
- Requires session storage
- Scaling challenges
- Not ideal for microservices

**Why Rejected:** Not suitable for stateless microservices.

### 2. OAuth 2.0 with Separate Identity Server

**Pros:**
- Industry standard
- Supports SSO
- Highly secure

**Cons:**
- Added complexity
- Additional service to maintain
- Overkill for current needs

**Why Rejected:** Complexity not justified; can adopt later if needed.

### 3. API Keys

**Pros:**
- Simple
- Works for machine-to-machine

**Cons:**
- Not suitable for user authentication
- Cannot carry user context

**Why Rejected:** Designed for service-to-service, not user auth.

## API Endpoints

### Registration & Verification

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/v1/auth/register` | Register new user | No |
| GET | `/api/v1/auth/verify-email` | Verify email with token | No |
| POST | `/api/v1/auth/resend-verification` | Resend verification email | No |

### Login & Session

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/v1/auth/login` | Login with email/password | No |
| POST | `/api/v1/auth/refresh` | Refresh access token | Cookie |
| POST | `/api/v1/auth/logout` | Logout (invalidate refresh token) | Yes |

### Social Login

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/v1/auth/google` | Redirect to Google OAuth | No |
| GET | `/api/v1/auth/google/callback` | Google OAuth callback | No |
| GET | `/api/v1/auth/facebook` | Redirect to Facebook OAuth | No |
| GET | `/api/v1/auth/facebook/callback` | Facebook OAuth callback | No |

### Password Management

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/v1/auth/forgot-password` | Request password reset | No |
| POST | `/api/v1/auth/reset-password` | Reset password with token | No |
| POST | `/api/v1/auth/change-password` | Change password (logged in) | Yes |

---

## Email Templates

### Verification Email

```html
Subject: Verify your AmCart account

Hi {FirstName},

Welcome to AmCart! Please verify your email address by clicking the button below:

[Verify Email Address]

This link will expire in 24 hours.

If you didn't create an account, please ignore this email.
```

### Welcome Email (After Verification)

```html
Subject: Welcome to AmCart! 🎉

Hi {FirstName},

Your email has been verified and your AmCart account is now active!

You can now:
✓ Browse our latest products
✓ Add items to your wishlist
✓ Place orders with fast checkout

[Start Shopping]

Happy shopping!
The AmCart Team
```

---

## Security Measures

| Measure | Implementation |
|---------|----------------|
| Password Hashing | bcrypt, cost 12 |
| Token Signing | HMAC-SHA256 |
| Transport Security | HTTPS required |
| CSRF Protection | SameSite cookies |
| XSS Prevention | HTTP-only cookies for refresh token |
| Brute Force | Rate limiting (10 req/min on auth), account lockout (5 attempts) |
| Token Revocation | Redis blacklist |
| Email Verification | Required for email/password registration |
| Social Login | OAuth 2.0 with state parameter (CSRF protection) |
| Password Policy | Min 8 chars, complexity requirements, history check |

---

## Configuration

### appsettings.json

```json
{
  "Jwt": {
    "Secret": "your-256-bit-secret-key-here",
    "Issuer": "amcart",
    "Audience": "amcart-api",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "OAuth": {
    "Google": {
      "ClientId": "your-google-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-google-client-secret",
      "RedirectUri": "https://amcart.com/auth/google/callback"
    },
    "Facebook": {
      "AppId": "your-facebook-app-id",
      "AppSecret": "your-facebook-app-secret",
      "RedirectUri": "https://amcart.com/auth/facebook/callback"
    }
  },
  "Email": {
    "VerificationTokenExpiryHours": 24,
    "PasswordResetTokenExpiryHours": 1
  },
  "Security": {
    "MaxFailedLoginAttempts": 5,
    "LockoutDurationMinutes": 30
  }
}
```

---

## References

- [JWT.io](https://jwt.io)
- [OWASP Authentication Cheatsheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [ASP.NET Core JWT Authentication](https://docs.microsoft.com/aspnet/core/security/authentication)
- [Google OAuth 2.0](https://developers.google.com/identity/protocols/oauth2)
- [Facebook Login](https://developers.facebook.com/docs/facebook-login/)

