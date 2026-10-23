using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Test harness for <see cref="VirtualEventSession"/>: shows the token state live and lets
/// you force an expiry, then check that the session re-authenticates.
/// </summary>
public class VirtualEventSessionDebugPanel : MonoBehaviour
{
    [SerializeField] private Text tokenStatusText;
    [SerializeField] private Text resultText;

    [Tooltip("Development only: accept an expired TLS certificate from the API host. " +
             "Ignored in release and WebGL builds.")]
    [SerializeField] private bool allowExpiredCertificate;

    private void Awake()
    {
        VirtualEventSession.AllowExpiredCertificate = allowExpiredCertificate;
    }

    private void OnEnable()
    {
        VirtualEventSession.LoggedIn += HandleLoggedIn;
        VirtualEventSession.ReauthRequired += HandleReauthRequired;
    }

    private void OnDisable()
    {
        VirtualEventSession.LoggedIn -= HandleLoggedIn;
        VirtualEventSession.ReauthRequired -= HandleReauthRequired;
    }

    private void Update()
    {
        if (tokenStatusText != null)
            tokenStatusText.text = DescribeToken();
    }

    public void ForceExpire()
    {
        VirtualEventSession.InvalidateToken();
        SetResult("Token marked as expired.");
    }

    public void CheckToken()
    {
        StartCoroutine(CheckTokenRoutine());
    }

    private IEnumerator CheckTokenRoutine()
    {
        SetResult("Checking token...");

        bool valid = false;
        yield return VirtualEventSession.EnsureValidToken(v => valid = v);

        SetResult(valid
            ? "Token is valid."
            : "No valid token. Please log in again.");
    }

    private static string DescribeToken()
    {
        if (!VirtualEventSession.HasToken)
            return "Token: none";

        DateTimeOffset? expiresAt = VirtualEventSession.ExpiresAt;

        if (expiresAt == null)
            return "Token: present, expiry unknown (treated as expired)";

        TimeSpan remaining = expiresAt.Value - DateTimeOffset.UtcNow;
        string when = expiresAt.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");

        if (remaining <= TimeSpan.Zero)
            return $"Token: EXPIRED at {when}";

        string left = remaining.TotalHours >= 1
            ? $"{(int)remaining.TotalHours}h {remaining.Minutes}m"
            : $"{remaining.Minutes}m {remaining.Seconds}s";

        return $"Token: valid for {left} (expires {when})";
    }

    private void HandleLoggedIn(VirtualEventSession.LoginData data)
    {
        SetResult("Logged in. New token received.");
    }

    private void HandleReauthRequired()
    {
        SetResult("Re-authentication required: log in again.");
    }

    private void SetResult(string message)
    {
        if (resultText != null)
            resultText.text = message;
    }
}
