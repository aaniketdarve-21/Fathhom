using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TrainingMode { Guided, Assessment }

public class SessionManager : MonoBehaviour
{
    public static SessionManager I;

    [Header("UI References")]
    public GameObject loginPanel;
    public TMP_InputField nameInput;
    public TMP_InputField passwordInput;
    public Button guidedButton;
    public Button assessmentButton;
    public TextMeshProUGUI errorText;

    [Header("Session Info (Read Only)")]
    public TrainingMode Mode { get; private set; } = TrainingMode.Guided;
    public string PlayerName { get; private set; } = "Guest";

    public event Action<TrainingMode> OnModeChosen;

    void Awake()
    {
        if (I == null) I = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (errorText) errorText.gameObject.SetActive(false);

        if (guidedButton) guidedButton.onClick.AddListener(() => TrySelectMode(TrainingMode.Guided));
        if (assessmentButton) assessmentButton.onClick.AddListener(() => TrySelectMode(TrainingMode.Assessment));
    }

    void TrySelectMode(TrainingMode mode)
    {
        string enteredName = nameInput ? nameInput.text.Trim() : "";
        string enteredPass = passwordInput ? passwordInput.text.Trim() : "";

        bool invalid = string.IsNullOrEmpty(enteredName) || string.IsNullOrEmpty(enteredPass);

        if (invalid)
        {
            if (errorText)
            {
                errorText.text = "⚠️ Please enter both Name and Password before proceeding.";
                errorText.color = Color.red;
                errorText.gameObject.SetActive(true);
            }

            if (nameInput && string.IsNullOrEmpty(enteredName)) nameInput.image.color = new Color(1f, 0.6f, 0.6f);
            if (passwordInput && string.IsNullOrEmpty(enteredPass)) passwordInput.image.color = new Color(1f, 0.6f, 0.6f);
            return;
        }

        if (nameInput) nameInput.image.color = Color.white;
        if (passwordInput) passwordInput.image.color = Color.white;
        if (errorText) errorText.gameObject.SetActive(false);

        PlayerName = enteredName;
        Mode = mode;

        if (loginPanel) loginPanel.SetActive(false);
        OnModeChosen?.Invoke(mode);
        Debug.Log($"[SessionManager] Login Success: {PlayerName} → {Mode}");
    }

    // ✅ small helper so other scripts can show/hide login cleanly
    public void ShowLogin(bool show)
    {
        if (loginPanel) loginPanel.SetActive(show);
        if (show && errorText) errorText.gameObject.SetActive(false);
    }
}
