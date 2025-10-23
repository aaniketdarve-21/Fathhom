using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SessionSummaryUI : MonoBehaviour
{
    [Header("Assign in Inspector (Title, Body, Footer)")]
    public TMP_Text titleText;
    public TMP_Text bodyText;
    public TMP_Text footerText;

    [Header("Buttons")]
    public Button okButton;          // hides and returns to login

    [Header("References")]
    public SessionManager session;   // show login again on OK

    void Awake()
    {
        if (okButton)
            okButton.onClick.AddListener(() =>
            {
                if (session) session.ShowLogin(true);
                gameObject.SetActive(false);
            });

        gameObject.SetActive(false); // hidden by default
    }

    public void ShowSummary(SessionLogger.SessionPayload p, string savedPath = null)
    {
        if (p == null || bodyText == null) return;

        if (titleText) titleText.text = "Session Summary";

        string startLocal = TryToLocal(p.startUtc);
        string endLocal = TryToLocal(p.endUtc);

        bodyText.text =
            $"👤 Player: {p.playerName}\n" +
            $"🎮 Mode: {p.mode}\n" +
            $"🕒 Start: {startLocal}\n" +
            $"🕕 End: {endLocal}\n" +
            $"⏳ Duration: {p.durationSec:F1} sec\n\n" +
            $"📍 Placements: {p.placements}\n" +
            $"🔒 Connects: {p.connects}\n" +
            $"💧 Flow Starts: {p.flowStarts}\n" +
            $"🛑 Flow Stops: {p.flowStops}\n" +
            $"🔄 Detaches: {p.detaches}";

        if (footerText)
            footerText.text = string.IsNullOrEmpty(savedPath)
                ? "Session data saved locally."
                : $"Saved to: {savedPath}";

        gameObject.SetActive(true);
    }

    static string TryToLocal(string iso)
    {
        if (System.DateTime.TryParse(iso, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
            return dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        return iso;
    }
}
