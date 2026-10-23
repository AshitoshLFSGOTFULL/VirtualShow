using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class VirtualEventLogin : MonoBehaviour
{
    [Header("Login UI")]
    [SerializeField] private InputField emailInput;
    [SerializeField] private InputField passwordInput;
    [SerializeField] private UnityEngine.UI.Text statusText;

    [Header("Scene Attrib")] 
    [SerializeField] private int SceneIndex;

    private void OnEnable()
    {
        VirtualEventSession.ReauthRequired += HandleReauthRequired;
    }

    private void OnDisable()
    {
        VirtualEventSession.ReauthRequired -= HandleReauthRequired;
    }

    public void Login()
    {
        SetStatus("Logging in...");

        StartCoroutine(VirtualEventSession.Login(
            emailInput.text.Trim(), passwordInput.text,
            OnLoginSuccess,
            error => SetStatus(error)));
    }

    // Hooked up to the Logout button.
    public void OnLogoutClicked()
    {
        Logout();

        if (passwordInput != null)
            passwordInput.text = "";

        SetStatus("Logged out.");
    }

    private void OnLoginSuccess(VirtualEventSession.LoginData data)
    {
        Debug.Log("Authenticated as " + data.user.email);
        Debug.Log("User ID: " + data.user.id);

        SetStatus("Welcome, " + data.user.name);
        GameManager.s_Instance.SetLoginResponse(data);
        // TODO: Open your virtual event or load the next scene.
        SceneManager.LoadSceneAsync(SceneIndex);
    }

    // The token expired and could not be renewed silently (e.g. after an app restart).
    private void HandleReauthRequired()
    {
        SetStatus("Your session has expired. Please log in again.");
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    public static string GetToken()
    {
        return VirtualEventSession.Token;
    }

    public static void Logout()
    {
        VirtualEventSession.Logout();
    }
}
