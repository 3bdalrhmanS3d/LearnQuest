## AuthController

**Base route:** `api/auth`&#x20;
**Authorization:** Public (no `[Authorize]` on controller; endpoints accessible anonymously, except where a valid token/cookie is required and validated manually).

### Endpoints

| Method | Route                       | Description                               | Request DTO                        | Response DTO                                  | Success                                 | Errors                                                                                                                                                        |
| ------ | --------------------------- | ----------------------------------------- | ---------------------------------- | --------------------------------------------- | --------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| POST   | `/signup`                   | Register a new user & send verification   | `SignupRequestDto`                 | `SecureAuthResponse`                          | **200 OK**<br/>`VERIFICATION_CODE_SENT` | **400 Bad Request**<br/>`INVALID_REQUEST` (model errors)<br/>`PASSWORD_REQUIREMENTS_NOT_MET` (weak password)                                                  |
| POST   | `/verify-account`           | Verify account with code                  | `VerifyAccountRequestDto`          | `SecureAuthResponse`                          | **200 OK**<br/>`OPERATION_SUCCESSFUL`   | **400 Bad Request**<br/>`INVALID_REQUEST` (model errors)<br/>`VERIFICATION_EXPIRED` (expired/invalid code)                                                    |
| GET    | `/verify-account/{token}`   | Verify account via token in URL           | –                                  | `SecureAuthResponse`                          | **200 OK**<br/>`OPERATION_SUCCESSFUL`   | **400 Bad Request**<br/>`INVALID_TOKEN` (missing/empty token)<br/>`VERIFICATION_EXPIRED`                                                                      |
| POST   | `/resend-verification-code` | Resend email verification code            | `ResendVerificationCodeRequestDto` | `SecureAuthResponse`                          | **200 OK**<br/>`VERIFICATION_CODE_SENT` | **400 Bad Request**<br/>`INVALID_REQUEST` (model errors)                                                                                                      |
| POST   | `/signin`                   | Authenticate & issue tokens               | `SigninRequestDto`                 | `SecureAuthResponse<SigninResponseDto>`       | **200 OK**<br/>`OPERATION_SUCCESSFUL`   | **400 Bad Request**<br/>`INVALID_REQUEST` (model errors)<br/>**423 Locked** `ACCOUNT_LOCKED` (locked account)<br/>**401 Unauthorized** `INVALID_CREDENTIALS`  |
| POST   | `/refresh-token`            | Refresh JWT using refresh token           | `RefreshTokenRequestDto`           | `SecureAuthResponse<RefreshTokenResponseDto>` | **200 OK**<br/>`OPERATION_SUCCESSFUL`   | **400 Bad Request** `INVALID_REQUEST` (model errors)<br/>**401 Unauthorized** `INVALID_TOKEN`                                                                 |
| POST   | `/auto-login`               | Login using auto-login token              | `AutoLoginRequestDto`              | `SecureAuthResponse<AutoLoginResponseDto>`    | **200 OK**<br/>`OPERATION_SUCCESSFUL`   | **400 Bad Request** `INVALID_TOKEN` (blank token)<br/>**401 Unauthorized** `INVALID_TOKEN`                                                                    |
| POST   | `/auto-login-from-cookie`   | Login from cookie-stored auto-login token | –                                  | `SecureAuthResponse<AutoLoginResponseDto>`    | **200 OK**<br/>`OPERATION_SUCCESSFUL`   | **400 Bad Request** `INVALID_TOKEN` (no cookie)                                                                                                               |
| POST   | `/forget-password`          | Initiate password reset (send email)      | `ForgetPasswordRequestDto`         | `SecureAuthResponse`                          | **200 OK**<br/>`OPERATION_SUCCESSFUL`   | **400 Bad Request** `INVALID_REQUEST` (model errors)                                                                                                          |
| POST   | `/reset-password`           | Reset password with verification code     | `ResetPasswordRequestDto`          | `SecureAuthResponse`                          | **200 OK**<br/>`OPERATION_SUCCESSFUL`   | **400 Bad Request** `INVALID_REQUEST` (model errors)<br/>`PASSWORD_REQUIREMENTS_NOT_MET` (weak new password)<br/>`INVALID_TOKEN` (expired/invalid code)       |
| POST   | `/logout`                   | Revoke JWT & clear auto-login cookie      | – (Authorization header)           | `SecureAuthResponse`                          | **200 OK**<br/>`LOGOUT_SUCCESSFUL`      | **400 Bad Request** `INVALID_TOKEN` (missing/invalid header)<br/>**500 Internal Server Error** `INVALID_REQUEST` (service failure)                            |

### Response Envelope

All responses use the standard envelope:

```json
{
  "errorCode": "string",
  "message": "string",
  "data": { /* T or null */ }
}
```

### DTO Definitions

#### Request DTOs

Source: **AutoLoginRequestDto.cs**&#x20;

```csharp
public class SignupRequestDto {
  [Required][StringLength(50)]       public string FirstName { get; set; }
  [Required][StringLength(50)]       public string LastName { get; set; }
  [Required][EmailAddress]           public string EmailAddress { get; set; }
  [Required]                         public string Password { get; set; }
  [Required][Compare("Password")]    public string UserConfPassword { get; set; }
}

public class VerifyAccountRequestDto {
  [Required][StringLength(6,MinimumLength=6)] public string VerificationCode { get; set; }
  [Required][EmailAddress]                   public string Email { get; set; }
}

public class ResendVerificationCodeRequestDto {
  [Required][EmailAddress] public string Email { get; set; }
}

public class SigninRequestDto {
  [Required][EmailAddress] public string Email { get; set; }
  [Required]               public string Password { get; set; }
  public bool               RememberMe { get; set; }
}

public class RefreshTokenRequestDto {
  [Required] public string OldRefreshToken { get; set; }
}

public class AutoLoginRequestDto {
  [Required] public string AutoLoginToken { get; set; }
}

public class ForgetPasswordRequestDto {
  [Required][EmailAddress] public string Email { get; set; }
}

public class ResetPasswordRequestDto {
  [Required][EmailAddress]         public string Email { get; set; }
  [Required][StringLength(6,MinimumLength=6)] public string Code { get; set; }
  [Required][MinLength(6)]         public string NewPassword { get; set; }
}
```

#### Response DTOs

Source: **AutoLoginResponseDto.cs**&#x20;

```csharp
public class SigninResponseDto {
  public string Token { get; set; }
  public DateTime Expiration { get; set; }
  public string Role { get; set; }
  public string RefreshToken { get; set; }
  public int    UserId { get; set; }
  public string? AutoLoginToken { get; set; }
}

public class RefreshTokenResponseDto {
  public string Token { get; set; }
  public DateTime Expiration { get; set; }
  public string RefreshToken { get; set; }
  public int?   UserId { get; internal set; }
}

public class AutoLoginResponseDto {
  public string Token { get; set; }
  public DateTime Expiration { get; set; }
  public string Role { get; set; }
}
```
