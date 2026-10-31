# 08 – Identity and accounts

**Guest is the default.** Accounts are optional and come in Phase 5.

## Who does what

| Concern | Frontend (public) | Backend API (private) |
|---|---|---|
| OAuth handshake with providers (Google, Discord, Steam, Microsoft) | ✅ (callbacks must be public) | |
| Provider client secrets | ✅ | |
| Session cookie, antiforgery | ✅ | |
| User accounts, external login links | | ✅ |
| Password hashing and verification, lockout | | ✅ |
| Email confirmation / password reset tokens | | ✅ |
| Sending email (SMTP) | | ✅ |
| Database | **none** | ✅ (`identity` schema) |

- The backend uses **ASP.NET Core Identity Core** (`AddIdentityCore` + EF Core stores): `UserManager`, password hasher, lockout, token providers. No cookies.
- The frontend uses **cookie authentication** and the **external OAuth handlers**. It does not use Identity's `SignInManager` or Identity UI; account pages are our own Blazor pages calling backend auth endpoints through the BFF.
- **The frontend holds no database or SMTP credentials.**

## Modes

| Mode | Where progress lives | How to keep it |
|---|---|---|
| Guest (default) | Browser storage | Export file |
| Signed in (Phase 5) | Browser + backend database (synced through the frontend BFF) | Automatic sync, export still available |

## Sign-in options (Phase 5)

### External (preferred, passwordless)

| Provider | Package (frontend) | Email provided | Notes |
|---|---|---|---|
| Google | Built-in `Microsoft.AspNetCore.Authentication.Google` | Yes (with verified flag) | |
| Discord | `AspNet.Security.OAuth.Discord` | With `email` scope | Gamer favorite |
| Steam | `AspNet.Security.OpenId.Steam` | **No** (SteamID only) | OpenID 2.0 |
| Microsoft | Built-in `MicrosoftAccount` | Yes | Not the Xbox gamertag |
| Twitch (later) | `AspNet.Security.OAuth.Twitch` | With scope | |
| Epic (later) | OAuth via Epic Account Services | — | Needs brand review + verified domain |
| PlayStation | — | — | **Not possible.** No public web sign-in for third-party sites. |
| Nintendo | — | — | **Not possible.** |

For external accounts we store only the **login link** (provider + provider user ID) and a display name. No email.

### Email + password (only for non-external sign-up)

- **Email is the login name.** There is no separate username.
- Password is **hashed** by Identity in the backend (PBKDF2 with per-user salt). Never stored or logged in plain text.
- Email confirmation required before sign-in.
- Password reset by email.
- Lockout after repeated failures. Rate limiting on frontend auth pages and backend auth endpoints.
- Optional TOTP two-factor authentication.
- Local email testing: **Mailpit** container (backend network).

## Flows

All frontend → backend calls use the frontend's **service token** with the `auth` scope. Backend auth endpoints accept **only** that scope.

### External sign-in

1. Frontend runs the OAuth/OpenID challenge with the provider.
2. Frontend receives provider + provider user ID (+ name, + email if given).
3. Frontend → backend `POST /auth/external-login` → backend finds or creates the user, returns `uid` + security stamp.
4. Frontend issues the session cookie containing `uid`.

### Email/password sign-up

1. Frontend form → BFF → backend `POST /auth/register` (email, password).
2. Backend creates the user (hashes password), creates a confirmation token, sends the email.
3. User clicks the link (frontend URL) → frontend → backend `POST /auth/confirm-email`.

### Email/password sign-in

1. Frontend form → backend `POST /auth/password-sign-in`.
2. Backend verifies hash, lockout, confirmation, 2FA requirement. Returns `uid` (or "2FA required" / failure reason).
3. Frontend issues the session cookie.

### Session validation

- The frontend re-validates the session periodically (e.g. every 30 minutes, cookie `OnValidatePrincipal`) by asking the backend for the user's current **security stamp**.
- Password change, sign-out-everywhere or account deletion changes the stamp → old cookies stop working.

## Identity configuration notes (backend)

- `UserName`:
  - Email accounts: the email.
  - External accounts: a generated hidden value (e.g. GUID). Never shown.
- `Email` is `null` for external accounts.
- `RequireUniqueEmail = false` (otherwise Identity rejects null emails).
  Uniqueness is enforced by a **filtered unique index** on `normalized_email WHERE normalized_email IS NOT NULL`.
- Display name: provider name claim for external accounts, email for email accounts.
- User ID (`uid`): opaque, stable GUID. Never an email.

## Frontend session keys

- Cookies are protected with ASP.NET Core **Data Protection** keys.
- The frontend has no database, so keys are persisted to:
  - Local: a Docker volume.
  - Production: a key store decided in Phase 6 (e.g. mounted volume or blob storage + key vault).

## Guest → account migration

On first sign-in, if browser progress exists:
1. Ask: "Upload your local progress to your account?"
2. Upload as new playthrough(s). Never overwrite server data silently.

## Open decisions (decide in Phase 5)

1. **Account linking.** Can an email account add Google, or a Steam account add email/password?
   Simplest start: **no linking**, one sign-in method per account.
   If added later: link only while signed in. **Never auto-link by matching email** (account takeover risk).
2. **Recovery for external-only accounts.** No email means no recovery if the provider account is lost.
   Mitigation: regular export reminders.

## Privacy (GDPR)

- Account deletion: backend removes user, logins and progress, and changes the security stamp.
- Personal data export (JSON) from the backend through the BFF.
- Email addresses exist only in the backend `identity` schema. Never logged.
- Privacy policy lists providers used and data stored.
