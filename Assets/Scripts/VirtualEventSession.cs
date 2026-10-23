using System;
using System.Collections;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Holds the virtual event API session: the bearer token, its expiry, and the logic
/// to log in again when the token has expired (or the server rejects it with 401).
///
/// Use <see cref="SendAuthorized"/> / <see cref="Get"/> for every authenticated call.
/// They check the expiry before sending, and retry once after re-authenticating if the
/// server still answers 401 (e.g. token revoked, or the device clock is off).
///
/// Re-authentication is silent while the credentials are still in memory. They are never
/// written to disk, so after an app restart with an expired token the session raises
/// <see cref="ReauthRequired"/> and the UI must ask the user to log in again.
/// </summary>
public static class VirtualEventSession
{
    private const string LoginUrl =
        "https://msp.virtualevents.my/api/v1/virtual-environment/login";

    private const string ApiHost = "msp.virtualevents.my";
    private const string CertificateDomain = "virtualevents.my";

    // API key required by the login endpoint ("Invalid or missing API key" without it).
    // The login then returns the user token that authorizes later calls.
    // NOTE: this is compiled into the build, so anyone with the build can extract it.
    private const string ApiKey = "vbe_SCp4Z3NViwtNb3OBQ7MkJRGevzqOCyhaZn6PzGbWXYCfD0ot";

    private const string TokenKey = "AuthToken";
    private const string TokenTypeKey = "AuthTokenType";
    private const string ExpiryKey = "AuthTokenExpiresAt";

    // Treat the token as expired slightly early so it can't lapse mid-request.
    private static readonly TimeSpan ExpirySkew = TimeSpan.FromSeconds(30);

    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    public class LoginResponse
    {
        public LoginData data;
    }

    [Serializable]
    public class LoginData
    {
        public string token;
        public string token_type;
        public string expires_at;
        public User user;
    }

    [Serializable]
    public class User
    {
        public int id;
        public string name;
        public string email;
        public string[] roles;
        public string photo_url;
        public string initials;
        public string avatar_color;
    }

    /// <summary>Raised after every successful login, including silent re-authentication.</summary>
    public static event Action<LoginData> LoggedIn;

    /// <summary>
    /// Raised when the session can't be renewed without the user: no credentials in
    /// memory, or the server rejected the stored ones.
    /// </summary>
    public static event Action ReauthRequired;

    /// <summary>
    /// Development only, off by default: accept an invalid (e.g. expired) TLS certificate from
    /// <see cref="ApiHost"/>. It only takes effect in the Editor and development builds, never
    /// in release builds or WebGL players (the browser does the TLS there), and never for
    /// any other host. Play mode in the Editor works even when the build target is WebGL.
    /// </summary>
    public static bool AllowExpiredCertificate { get; set; }

    private static string email;
    private static string password;
    private static bool loginInProgress;
    private static LoginData currentLogin;

    // ---- State -------------------------------------------------------------------

    public static string Token => PlayerPrefs.GetString(TokenKey, "");

    public static string TokenType
    {
        get
        {
            string type = PlayerPrefs.GetString(TokenTypeKey, "");
            return string.IsNullOrEmpty(type) ? "Bearer" : type;
        }
    }

    public static bool HasToken => !string.IsNullOrEmpty(Token);

    public static bool HasCredentials =>
        !string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password);

    /// <summary>Token expiry, or null if unknown.</summary>
    public static DateTimeOffset? ExpiresAt
    {
        get
        {
            string raw = PlayerPrefs.GetString(ExpiryKey, "");
            return TryParseExpiry(raw, out DateTimeOffset parsed) ? parsed : (DateTimeOffset?)null;
        }
    }

    /// <summary>True when there is no token, no known expiry, or the expiry has passed.</summary>
    public static bool IsExpired => IsExpiredAt(TimeSpan.Zero);

    /// <summary>Like <see cref="IsExpired"/> but includes a small safety margin.</summary>
    public static bool IsExpiringSoon => IsExpiredAt(ExpirySkew);

    private static bool IsExpiredAt(TimeSpan margin)
    {
        if (!HasToken) return true;

        DateTimeOffset? expiresAt = ExpiresAt;
        // An unknown expiry is treated as expired so we re-authenticate rather than guess.
        return expiresAt == null || DateTimeOffset.UtcNow + margin >= expiresAt.Value;
    }

    /// <summary>
    /// Marks the current token as expired so the next authorized request logs in again.
    /// Also useful for testing the re-authentication path.
    /// </summary>
    public static void InvalidateToken()
    {
        PlayerPrefs.SetString(ExpiryKey, "");
        PlayerPrefs.Save();
    }

    public static void Logout()
    {
        email = null;
        password = null;
        currentLogin = null;

        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.DeleteKey(TokenTypeKey);
        PlayerPrefs.DeleteKey(ExpiryKey);
        PlayerPrefs.Save();
    }

    // ---- Login -------------------------------------------------------------------

    public static IEnumerator Login(
        string loginEmail, string loginPassword,
        Action<LoginData> onSuccess, Action<string> onError)
    {
        if (loginInProgress)
        {
            // Another coroutine is already logging in; share its result.
            while (loginInProgress) yield return null;

            if (HasToken && !IsExpired) onSuccess?.Invoke(currentLogin);
            else onError?.Invoke("Login failed.");
            yield break;
        }

        loginInProgress = true;

        try
        {
            string json = JsonUtility.ToJson(
                new LoginRequest { email = loginEmail, password = loginPassword });

            using (UnityWebRequest request = new UnityWebRequest(LoginUrl, "POST"))
            {
                request.uploadHandler =
                    new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Accept", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + ApiKey);
                ApplyCertificatePolicy(request);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(
                        $"Login failed: {request.responseCode} {request.error}\n" +
                        request.downloadHandler.text);

                    // The server rejected the credentials: stop retrying with them.
                    if (request.responseCode == 401 || request.responseCode == 422)
                    {
                        email = null;
                        password = null;
                    }

                    onError?.Invoke("Login failed. Please try again.");
                    yield break;
                }

                LoginResponse response = null;

                try
                {
                    response = JsonUtility.FromJson<LoginResponse>(
                        request.downloadHandler.text);
                }
                catch (ArgumentException) { }

                if (response == null || response.data == null ||
                    string.IsNullOrEmpty(response.data.token))
                {
                    onError?.Invoke("Invalid server response.");
                    yield break;
                }

                email = loginEmail;
                password = loginPassword;
                SaveToken(response.data);

                Debug.Log("Login successful. Token expires at " + response.data.expires_at);

                LoggedIn?.Invoke(response.data);
                onSuccess?.Invoke(response.data);
            }
        }
        finally
        {
            loginInProgress = false;
        }
    }

    private static void SaveToken(LoginData data)
    {
        currentLogin = data;

        PlayerPrefs.SetString(TokenKey, data.token);
        PlayerPrefs.SetString(TokenTypeKey, data.token_type ?? "");

        // The API returns a local time with an offset (e.g. +08:00); store it as UTC.
        string expiry = TryParseExpiry(data.expires_at, out DateTimeOffset parsed)
            ? parsed.UtcDateTime.ToString("o", CultureInfo.InvariantCulture)
            : "";

        if (expiry.Length == 0)
            Debug.LogWarning("Could not parse expires_at: " + data.expires_at);

        PlayerPrefs.SetString(ExpiryKey, expiry);
        PlayerPrefs.Save();
    }

    private static bool TryParseExpiry(string raw, out DateTimeOffset result)
    {
        result = default;

        return !string.IsNullOrEmpty(raw) && DateTimeOffset.TryParse(
            raw, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal, out result);
    }

    // ---- Authenticated requests --------------------------------------------------

    /// <summary>
    /// Makes sure there is a usable token, logging in again with the in-memory credentials
    /// if it has (nearly) expired. Calls <paramref name="done"/> with whether a token is usable.
    /// </summary>
    public static IEnumerator EnsureValidToken(Action<bool> done)
    {
        if (HasToken && !IsExpiringSoon)
        {
            done(true);
            yield break;
        }

        if (!HasCredentials)
        {
            // Nothing to renew with; a token that hasn't strictly expired is still usable.
            bool usable = HasToken && !IsExpired;
            if (!usable) ReauthRequired?.Invoke();

            done(usable);
            yield break;
        }

        Debug.Log("Token expired or about to expire. Re-authenticating...");

        bool loggedIn = false;
        yield return Login(email, password, _ => loggedIn = true, null);

        if (!loggedIn && !HasCredentials)
            ReauthRequired?.Invoke(); // credentials were rejected

        done(loggedIn || (HasToken && !IsExpired));
    }

    /// <summary>
    /// Sends an authenticated request. <paramref name="createRequest"/> is called for each
    /// attempt (a UnityWebRequest can't be re-sent) and must not set the Authorization header.
    /// </summary>
    public static IEnumerator SendAuthorized(
        Func<UnityWebRequest> createRequest,
        Action<string> onSuccess, Action<string> onError)
    {
        for (int attempt = 0; attempt < 2; attempt++)
        {
            bool valid = false;
            yield return EnsureValidToken(v => valid = v);

            if (!valid)
            {
                onError?.Invoke("Session expired. Please log in again.");
                yield break;
            }

            string tokenUsed = Token;

            using (UnityWebRequest request = createRequest())
            {
                request.SetRequestHeader("Authorization", TokenType + " " + tokenUsed);
                request.SetRequestHeader("Accept", "application/json");
                ApplyCertificatePolicy(request);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke(request.downloadHandler.text);
                    yield break;
                }

                if (request.responseCode == 401 && attempt == 0)
                {
                    // Rejected despite looking valid (revoked, clock skew). Force a re-login,
                    // unless another request has already replaced the token.
                    if (Token == tokenUsed)
                        InvalidateToken();

                    continue;
                }

                if (request.responseCode == 401)
                    ReauthRequired?.Invoke();

                onError?.Invoke($"Request failed: HTTP {request.responseCode} {request.error}");
                yield break;
            }
        }
    }

    // ---- Development-only certificate bypass -------------------------------------

    private static void ApplyCertificatePolicy(UnityWebRequest request)
    {
        if (!AllowExpiredCertificate) return;

#if UNITY_EDITOR || (DEVELOPMENT_BUILD && !UNITY_WEBGL)
        if (!string.Equals(request.uri.Host, ApiHost, StringComparison.OrdinalIgnoreCase))
            return;

        request.certificateHandler = new ApiHostCertificateHandler();
        request.disposeCertificateHandlerOnDispose = true;
#else
        Debug.LogWarning(
            "AllowExpiredCertificate is ignored outside the Editor and development builds.");
#endif
    }

#if UNITY_EDITOR || (DEVELOPMENT_BUILD && !UNITY_WEBGL)
    // Skips chain/expiry validation, but still requires the certificate to be issued for
    // the API's domain. Only attached to requests to ApiHost.
    private class ApiHostCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            try
            {
                using (X509Certificate2 certificate = new X509Certificate2(certificateData))
                {
                    string name = certificate.GetNameInfo(X509NameType.DnsName, false) ?? "";

                    bool expected =
                        string.Equals(name, ApiHost, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(name, CertificateDomain, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(name, "*." + CertificateDomain, StringComparison.OrdinalIgnoreCase);

                    if (!expected)
                    {
                        Debug.LogWarning("Rejected certificate for unexpected host: " + name);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Could not read server certificate: " + e.Message);
                return false;
            }

            Debug.LogWarning(
                "TLS certificate validation is DISABLED for " + ApiHost + " (development only).");
            return true;
        }
    }
#endif

    public static IEnumerator Get(
        string url, Action<string> onSuccess, Action<string> onError)
    {
        return SendAuthorized(() => UnityWebRequest.Get(url), onSuccess, onError);
    }
}
